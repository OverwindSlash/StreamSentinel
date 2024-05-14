using LibVLCSharp.Shared;
using OpenCvSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisDefinitions;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.MediaLoader;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MediaLoader.Vlc
{
    public class VlcVideoLoader : IVideoLoader, IDisposable
    {
        private string _deviceId;

        private IntPtr memoryMappedFile;
        private IntPtr memoryMappedView;
        private LibVLC libvlc;
        private MediaPlayer mediaPlayer;
        private const uint BytePerPixel = 4;
        private uint Pitch;
        private uint Lines;

        public string Uri { get; private set; } = "demo uri";
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        private MemoryMappedFile CurrentMappedFile;
        private MemoryMappedViewAccessor CurrentMappedViewAccessor;
        private ConcurrentQueue<(MemoryMappedFile file, MemoryMappedViewAccessor accessor)> FilesToProcess = new ConcurrentQueue<(MemoryMappedFile file, MemoryMappedViewAccessor accessor)>();
        private long FrameCounter = 0;

        private bool isRunning = false;

        private readonly IConcurrentBoundedQueue<Frame> _frameBuffer;
        private VideoSpecs _videoSpecs;
        private bool _isOpen = false;

        private int _skipFrame = 8;
        public VlcVideoLoader(string deivceId, int bufferSize): this(deivceId, bufferSize,1920, 1080)
        {
            
        }
        public VlcVideoLoader(string deviceId, int bufferSize, uint width =1920, uint height=1080)
        {
            _deviceId = deviceId;
            _frameBuffer = new ConcurrentBoundedQueue<Frame>(bufferSize);

            // this will load the native libvlc library (if needed, depending on the platform).
            Core.Initialize();
            Width = width;
            Height = height;
            _videoSpecs = new VideoSpecs(string.Empty, 0, 0, 0, 0);

        }


        public VideoSpecs Specs => _videoSpecs;

        public string DeviceId => _deviceId;

        public int MediaWidth => (int)Width;

        public int MediaHeight => (int)Height;

        public bool IsOpened => _isOpen;

        public bool IsInPlaying => isRunning;

        public int BufferedFrameCount => _frameBuffer.Count;
        public int BufferedMaxOccupied => _frameBuffer.MaxOccupied;

        private void CreatePlayer(string uri, uint width, uint height)
        {
            Uri = uri;
            Width = width;
            Height = height;

            // Create a new LibVLC instance
            libvlc = new LibVLC();

            // Create a new media player
            mediaPlayer = new MediaPlayer(libvlc);

            // Set the media player's media to the RTSP stream URL
            mediaPlayer.Media = new Media(libvlc, new Uri(uri));

            mediaPlayer.Media.AddOption(":network-caching=333");
            mediaPlayer.Media.AddOption(":clock-jitter=0");
            mediaPlayer.Media.AddOption(":clock-syncro=0");
            mediaPlayer.Media.AddOption(":no-audio");


            Pitch = Align(width * BytePerPixel);
            Lines = Align(height);

            // Set the size and format of the video here.
            mediaPlayer.SetVideoFormat("RV32", width, height, Pitch);
            mediaPlayer.SetVideoCallbacks(Lock, null, Display);


            uint Align(uint size)
            {
                if (size % 32 == 0)
                {
                    return size;
                }

                return ((size / 32) + 1) * 32;// Align on the next multiple of 32
            }

            _videoSpecs = new VideoSpecs(uri, (int)Width, (int)Height,25, 0);
        }
        public void Close()
        {
            mediaPlayer.Stop();
        }

        public void Dispose()
        {
            mediaPlayer.Dispose();
            libvlc.Dispose();
        }

        public void Open(string uri)
        {
            CreatePlayer(uri, Width, Height);
            _frameBuffer.Clear();
            _isOpen = true;
        }

        public void Play(int stride = 1, bool debugMode = false, int debugFrameCount = 0)
        {
            _skipFrame = stride;
            mediaPlayer.Play();
            //while (!mediaPlayer.IsPlaying)
            //{
            //    Thread.Sleep(100);
            //    continue;
            //}
            isRunning = true;
            Process();
        }

        public Frame RetrieveFrame()
        {
            return _frameBuffer.Dequeue();
        }

        public async Task<Frame> RetrieveFrameAsync()
        {
            return await _frameBuffer.DequeueAsync();
        }

        public void Stop()
        {
            isRunning = false;
        }
        private void Process()
        {
            var frameNumber = 0;
            //await Task.Factory.StartNew(async () =>
            //{
                while (isRunning)
                {
                    if (!mediaPlayer.IsPlaying)
                    {
                        mediaPlayer.Stop();
                        var res = mediaPlayer.Play();
                        Trace.TraceInformation($"Connecting...");
                        //Task.Delay(TimeSpan.FromSeconds(3));
                        Thread.Sleep(1000);
                    }
                    if (FilesToProcess.TryDequeue(out var file))
                    {
                        using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Bgra32>((int)(Pitch / BytePerPixel), (int)Lines))
                        using (var sourceStream = file.file.CreateViewStream())
                        {
                            var mg = image.GetPixelMemoryGroup();

                            for (int i = 0; i < mg.Count; i++)
                            {
                                sourceStream.Read(MemoryMarshal.AsBytes(mg[i].Span));
                            }

                            image.Mutate(ctx => ctx.Crop((int)Width, (int)Height));
                            var memoryStream = new MemoryStream();
                            image.SaveAsBmp(memoryStream);
                            // TODO: convert to opencv mat and insert to queue
                            var bitmap = new Bitmap(memoryStream);
                            var mat = bitmap.ToMat();
                            var frame = new Frame(_deviceId, frameNumber, 0, mat);
                            _frameBuffer.Enqueue(frame);
                            //Trace.TraceInformation($"=============================vlc queue: {_frameBuffer.Count}");

                        }
                        file.accessor.Dispose();
                        file.file.Dispose();
                        frameNumber++;
                    }
                    else
                    {
                        //await Task.Delay(TimeSpan.FromSeconds(3));
                        //FrameFailedCounter++;
                        //Trace.TraceWarning($"FrameFailedCounter: {FrameFailedCounter}");
                    }
                }
            //});

                Close();
        }
        private IntPtr Lock(IntPtr opaque, IntPtr planes)
        {
            CurrentMappedFile = MemoryMappedFile.CreateNew(null, Pitch * Lines);
            CurrentMappedViewAccessor = CurrentMappedFile.CreateViewAccessor();
            Marshal.WriteIntPtr(planes, CurrentMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle());
            return IntPtr.Zero;
        }

        private void Display(IntPtr opaque, IntPtr picture)
        {
            if (FrameCounter % _skipFrame == 0)
            {
                FilesToProcess.Enqueue((CurrentMappedFile, CurrentMappedViewAccessor));
                CurrentMappedFile = null;
                CurrentMappedViewAccessor = null;
            }
            else
            {
                CurrentMappedViewAccessor.Dispose();
                CurrentMappedFile.Dispose();
                CurrentMappedFile = null;
                CurrentMappedViewAccessor = null;
            }
            FrameCounter++;
        }
    }
}
