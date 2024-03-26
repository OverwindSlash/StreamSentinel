using MediaLoader.OpenCV;

namespace MediaLoader.Tests;

public class OpenCVMediaLoaderTests
{
    [Test]
    public void Test_ProvideAndConsume_InDifferentThread()
    {
        int bufferSize = 100;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        // Consumer
        Task.Run(() =>
        {
            int frameCount = 0;
            int frameInterval = (int)(1000 / loader.Specs.Fps);
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = loader.RetrieveFrame();
                if (frame == null)
                {
                    continue;
                }

                // Cv2.ImShow("test", frame.Scene);
                // Cv2.WaitKey(frameInterval);
                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(151));
        });

        // Provider
        Task.Run(() => { loader.Play(); });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public void Test_ProvideAndConsumeAsync_InDifferentThread()
    {
        int bufferSize = 100;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        // Consumer
        Task.Run(async () =>
        {
            int frameCount = 0;
            int frameInterval = (int)(1000 / loader.Specs.Fps);
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = await loader.RetrieveFrameAsync();
                if (frame == null)
                {
                    continue;
                }

                // Cv2.ImShow("test", frame.Scene);
                // Cv2.WaitKey(frameInterval);
                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(151));
        });

        // Provider
        Task.Run(() => { loader.Play(); });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public void Test_Open_VideoFile()
    {
        int bufferSize = 100;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        var specs = loader.Specs;
        Assert.IsNotNull(specs);
        Assert.That(specs.Uri, Is.EqualTo(@"Video\video1.avi"));
        Assert.That(specs.Width, Is.EqualTo(902));
        Assert.That(specs.Height, Is.EqualTo(666));
        Assert.That(specs.Fps, Is.EqualTo(30.0f).Within(0.4));
        Assert.That(specs.FrameCount, Is.EqualTo(151));
    }

    [Test]
    public async Task Test_Play_VideoFile_QueueNotFull()
    {
        int bufferSize = 200;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        Task.Run(() => { loader.Play(); });

        Task.Run(() =>
        {
            int frameCount = 0;
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = loader.RetrieveFrame();
                Assert.IsNotNull(frame);
                Assert.That(frame.FrameId, Is.EqualTo(frameCount + 1));

                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(loader.Specs.FrameCount));
        });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public async Task Test_Play_VideoFile_QueueFull()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        Task.Run(() =>
        {
            int stride = 1;
            loader.Play(stride, true, bufferSize);
        });

        Task.Run(async () =>
        {
            int frameCount = 0;
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = await loader.RetrieveFrameAsync();
                Assert.IsNotNull(frame);
                Assert.That(frame.FrameId, Is.EqualTo(frameCount + 1));

                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(bufferSize));
        });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public async Task Test_Play_VideoFile_QueueFull_PlusOne()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        Task.Run(() =>
        {
            int stride = 1;
            loader.Play(stride, true, bufferSize + 1);
        });

        Task.Run(async () =>
        {
            int frameCount = 0;
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = await loader.RetrieveFrameAsync();
                Assert.IsNotNull(frame);
                Assert.That(frame.FrameId, Is.EqualTo(frameCount + 1));

                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(bufferSize + 1));
        });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public async Task Test_Play_VideoFile_QueueFullTwice()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        Task.Run(() =>
        {
            int stride = 1;
            loader.Play(stride, true, bufferSize * 2);
        });

        Task.Run(async () =>
        {
            int frameCount = 0;
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = await loader.RetrieveFrameAsync();
                Assert.IsNotNull(frame);
                Assert.That(frame.FrameId, Is.EqualTo(frameCount + 1));

                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(bufferSize * 2));
        });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public async Task Test_Play_VideoFile_QueueFullTwice_PlusOne()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        Task.Run(() =>
        {
            int stride = 1;
            loader.Play(stride, true, bufferSize * 2 + 1);
        });

        Task.Run(async () =>
        {
            int frameCount = 0;
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = await loader.RetrieveFrameAsync();
                Assert.IsNotNull(frame);
                Assert.That(frame.FrameId, Is.EqualTo(frameCount + 1));

                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(bufferSize * 2 + 1));
        });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public async Task Test_Play_VideoFile_Stride2()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        int stride = 2;
        Task.Run(() => { loader.Play(stride, true, bufferSize); });

        Task.Run(async () =>
        {
            int frameCount = 0;
            while (loader.BufferedFrameCount != 0 || loader.IsOpened)
            {
                using var frame = await loader.RetrieveFrameAsync();
                Assert.IsNotNull(frame);
                Assert.That(frame.FrameId, Is.EqualTo(frameCount * stride + 1));

                frameCount++;
            }

            Assert.That(frameCount, Is.EqualTo(bufferSize / 2));
        });

        // Keep thread running until video ends.
        while (loader.BufferedFrameCount != 0 || loader.IsOpened)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"Max Occupied:{loader.BufferedMaxOccupied}");
    }

    [Test]
    public async Task Test_Play_VideoFile_QueueFull_CheckTimeOffset()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"Video\video1.avi");

        loader.Play(1, true, bufferSize);

        var frameCount = loader.BufferedFrameCount;

        var firstFrame = await loader.RetrieveFrameAsync();
        for (int i = 1; i < frameCount - 1; i++)
        {
            await loader.RetrieveFrameAsync();
        }

        var lastFrame = await loader.RetrieveFrameAsync();

        var elapsedTime = lastFrame.TimeStamp - firstFrame.TimeStamp;
        var diff = Math.Abs(lastFrame.OffsetMilliSec - (long)elapsedTime.TotalMilliseconds);

        Assert.That(diff <= 10);
    }

    [Test]
    public async Task Test_Play_RtspStream()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"rtsp://stream.strba.sk:1935/strba/VYHLAD_JAZERO.stream");

        loader.Play(1, true, bufferSize);

        var bufferedFrameCount = loader.BufferedFrameCount;
        for (int i = 1; i <= bufferedFrameCount; i++)
        {
            var frame = await loader.RetrieveFrameAsync();
            Assert.IsNotNull(frame);
            Assert.That(frame.FrameId, Is.EqualTo(i));
        }
    }

    [Test]
    public async Task Test_Play_RtspStream_CheckTimeOffset()
    {
        int bufferSize = 50;
        using var loader = new OpenCVMediaLoader("tempId", bufferSize);
        loader.Open(@"rtsp://stream.strba.sk:1935/strba/VYHLAD_JAZERO.stream");

        loader.Play(1, true, bufferSize);

        var bufferedFrameCount = loader.BufferedFrameCount;

        var firstFrame = await loader.RetrieveFrameAsync();
        for (int i = 1; i < bufferedFrameCount - 1; i++)
        {
            await loader.RetrieveFrameAsync();
        }

        var lastFrame = await loader.RetrieveFrameAsync();

        var elapsedTime = lastFrame.TimeStamp - firstFrame.TimeStamp;
        var diff = Math.Abs(lastFrame.OffsetMilliSec - (long)elapsedTime.TotalMilliseconds);

        Console.WriteLine($"Time difference:{diff}ms");

        Assert.That(diff <= 50);
    }
}