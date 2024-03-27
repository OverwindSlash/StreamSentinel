using System.Diagnostics;
using NSubstitute;
using OpenCvSharp;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Core.Tests
{
    public class VideoFrameBufferTests
    {
        [Test]
        public void TestCreateVideoFrameBuffer()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            Assert.That(buffer.Count, Is.EqualTo(0));
            Assert.That(buffer.MaxCapacity, Is.EqualTo(bufferSize));
            Assert.That(buffer.MaxOccupied, Is.EqualTo(0));
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueOne()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            var frame = Substitute.For<Frame>("tempId", 1, 0, new Mat());

            buffer.Enqueue(frame);

            Assert.That(buffer.Count, Is.EqualTo(1));
            Assert.That(buffer.MaxCapacity, Is.EqualTo(bufferSize));
            Assert.That(buffer.MaxOccupied, Is.EqualTo(1));

            frame.DidNotReceive().Dispose();
        }

        [Test]
        public void TestVideoFrameBuffer_FullEnqueue()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            List<Frame> frames = new List<Frame>();
            for (int i = 1; i <= bufferSize; i++)
            {
                var frame = Substitute.For<Frame>("tempId", i, 40 * i, new Mat());
                frames.Add(frame);

                buffer.Enqueue(frame);
            }

            Assert.That(buffer.Count, Is.EqualTo(10));
            Assert.That(buffer.MaxCapacity, Is.EqualTo(bufferSize));
            Assert.That(buffer.MaxOccupied, Is.EqualTo(10));

            foreach (var frame in frames)
            {
                frame.DidNotReceive().Dispose();
            }
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueExceedOne()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            List<Frame> frames = new List<Frame>();
            for (int i = 1; i <= bufferSize + 1; i++)
            {
                var frame = Substitute.For<Frame>("tempId", i, 40 * i, new Mat());
                frames.Add(frame);

                buffer.Enqueue(frame);
            }

            Assert.That(buffer.Count, Is.EqualTo(10));
            Assert.That(buffer.MaxCapacity, Is.EqualTo(bufferSize));
            Assert.That(buffer.MaxOccupied, Is.EqualTo(11));

            frames[0].Received().Dispose();
            for (int i = 1; i < bufferSize + 1; i++)
            {
                frames[i].DidNotReceive().Dispose();
            }
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueMoreThan1x()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            List<Frame> frames = new List<Frame>();
            for (int i = 1; i <= bufferSize * 2; i++)
            {
                var frame = Substitute.For<Frame>("tempId", i, 40 * i, new Mat());
                frames.Add(frame);

                buffer.Enqueue(frame);
            }

            Assert.That(buffer.Count, Is.EqualTo(10));
            Assert.That(buffer.MaxCapacity, Is.EqualTo(bufferSize));
            Assert.That(buffer.MaxOccupied, Is.EqualTo(11));

            for (int i = 0; i < bufferSize; i++)
            {
                frames[i].Received().Dispose();
            }

            for (int i = bufferSize; i < bufferSize * 2; i++)
            {
                frames[i].DidNotReceive().Dispose();
            }
        }

        [Test]
        public void TestVideoFrameBuffer_Dequeue()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            List<Frame> frames = new List<Frame>();
            for (int i = 1; i <= bufferSize; i++)
            {
                var frame = Substitute.For<Frame>("tempId", i, 40 * i, new Mat());
                frames.Add(frame);

                buffer.Enqueue(frame);
            }

            var result = buffer.Dequeue();

            Assert.That(buffer.Count, Is.EqualTo(9));
            Assert.That(buffer.MaxCapacity, Is.EqualTo(bufferSize));
            Assert.That(buffer.MaxOccupied, Is.EqualTo(10));

            foreach (var frame in frames)
            {
                frame.DidNotReceive().Dispose();
            }
        }

        [Test]
        public void TestDequeueWait()
        {
            int bufferSize = 10;
            var buffer = new VideoFrameBuffer(bufferSize);

            List<Task> tasks = new List<Task>();

            var task1 = Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                var result = buffer.Dequeue();
                stopwatch.Stop();

                Assert.That(result, !Is.Null);
                Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(500));
            });
            tasks.Add(task1);

            var task2 = Task.Run(() =>
            {
                Thread.Sleep(500);
                var frame = Substitute.For<Frame>("tempId", 1, 0, new Mat());

                buffer.Enqueue(frame);
            });
            tasks.Add(task2);

            Task.WaitAll(tasks.ToArray());
        }
    }
}
