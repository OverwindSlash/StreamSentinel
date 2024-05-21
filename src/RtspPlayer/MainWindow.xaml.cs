using OpenCvSharp;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StreamSentinel.Pipeline;
using Microsoft.Extensions.Configuration;
using StreamSentinel.Eventbus;
namespace RtspPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private uint width = 1920;
        private uint height = 1080;

        private WriteableBitmap _writeableBitmap;
        private Int32Rect _dirtyRect;

        private WriteableBitmap _writeableBitmap2;
        private Int32Rect _dirtyRect2;

        private long frameCounter = 0;
        Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            _writeableBitmap = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
            _dirtyRect = new Int32Rect(0, 0, 1920, 1080);
            LeftImage.Source = _writeableBitmap;

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, true)
                .Build();

            var pipeline = new AnalysisPipeline(config);
            pipeline.FrameReceived += FrameReceivedEvent;

            _writeableBitmap2 = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
            _dirtyRect2 = new Int32Rect(0, 0, 1920, 1080);
            RightImage.Source = _writeableBitmap2;

            IConfiguration config2 = new ConfigurationBuilder()
                .AddJsonFile("settings2.json", true, true)
                .Build();
            var pipeline2 = new AnalysisPipeline(config2);
            pipeline2.FrameReceived += FrameReceivedEvent2;

            pipeline.Run();
            Thread.Sleep(5000);

            pipeline2.Run();
        }

        private void FrameReceivedEvent( Mat mat, string pipeline)
        {
            //var frame = (Bitmap)bitmap.Clone();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(frame);
                //if (frameCounter++ % 3 == 0)
                //{
                    
                //}
                
                _writeableBitmap.Lock();
                try
                {
                    _writeableBitmap.WritePixels(_dirtyRect, mat.Data, mat.Height * (int)mat.Step(), (int)mat.Step());
                }
                catch (Exception ex)
                {
                    Trace.TraceError(pipeline, ex);
                }
                finally
                {
                    _writeableBitmap.Unlock();
                }
            }));
        }

        private void FrameReceivedEvent2(Mat mat, string pipeline)
        {
            //var frame = (Bitmap)bitmap.Clone();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(frame);
                //if (frameCounter++ % 3 == 0)
                //{

                //}

                _writeableBitmap2.Lock();
                try
                {
                    _writeableBitmap2.WritePixels(_dirtyRect2, mat.Data, mat.Height * (int)mat.Step(), (int)mat.Step());
                }
                catch (Exception ex)
                {
                    Trace.TraceError(pipeline, ex);
                }
                finally
                {
                    _writeableBitmap2.Unlock();
                }
            }));
        }



        //private async void OnStartClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {

        //        vlcVideoProvider?.Stop();
        //        vlcVideoProvider?.Dispose();
        //        vlcVideoProvider = new VlcVideoProvider(UriTextBox.Text, width, height);
        //        vlcVideoProvider.ImageChangeEvent += VlcVideoProvider_ImageChangeEvent;

        //        await vlcVideoProvider.Start().ConfigureAwait(false);
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        //private void OnStopClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        vlcVideoProvider?.Stop();
        //        vlcVideoProvider?.Dispose();
        //        vlcVideoProvider = null;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}



        //private static void Draw(OpenCvSharp.Mat image, int trackId, double upX, double upY, double width, double height)
        //{
        //    //label formating
        //    var label = $"{trackId}";
        //    //Console.WriteLine($"confidence {confidence * 100:0.00}% {label}");
        //    //draw result
        //    image.Rectangle(new OpenCvSharp.Point(upX, upY), new OpenCvSharp.Point(upX + width, upY + height), new OpenCvSharp.Scalar(0, 255, 0), 2);
        //    OpenCvSharp.Cv2.PutText(image, label, new OpenCvSharp.Point(upX, upY - 5),
        //        OpenCvSharp.HersheyFonts.HersheyTriplex, 0.5, OpenCvSharp.Scalar.Red);
        //}

    }
}