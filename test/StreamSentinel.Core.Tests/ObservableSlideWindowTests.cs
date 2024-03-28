using NSubstitute;
using NSubstitute.Core.Arguments;
using OpenCvSharp;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events;

namespace StreamSentinel.Core.Tests
{
    public class ObservableSlideWindowTests
    {
        [Test]
        public void TestVideoFrameBuffer_EnqueueOne()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            // frame 1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            slideWindow.AddNewFrame(frame1);

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(1));

            var car2Frames = slideWindow.GetFramesContainObjectId("car:2");
            Assert.That(car2Frames.Count, Is.EqualTo(0));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(true));

            var car2IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:2");
            Assert.That(car2IsUnderTracking, Is.EqualTo(false));
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueFull()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            // frame 1: car1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            // frame 2: car1, person1
            var frame2 = new Frame("tempId", 2, 40, new Mat());
            frame2.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                },

                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 3: person1
            var frame3 = new Frame("tempId", 3, 80, new Mat());
            frame3.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            slideWindow.AddNewFrame(frame1);
            slideWindow.AddNewFrame(frame2);
            slideWindow.AddNewFrame(frame3);

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(2));

            var person1Frames = slideWindow.GetFramesContainObjectId("person:1");
            Assert.That(person1Frames.Count, Is.EqualTo(2));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(true));

            var person1IsUnderTracking = slideWindow.IsObjIdUnderTracking("person:1");
            Assert.That(person1IsUnderTracking, Is.EqualTo(true));
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueMoreThan1()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            // frame 1: car1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            // frame 2: car1, person1
            var frame2 = new Frame("tempId", 2, 40, new Mat());
            frame2.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                },

                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 3: person1
            var frame3 = new Frame("tempId", 3, 80, new Mat());
            frame3.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 4: person1
            var frame4 = new Frame("tempId", 4, 120, new Mat());
            frame4.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            slideWindow.AddNewFrame(frame1);
            slideWindow.AddNewFrame(frame2);
            slideWindow.AddNewFrame(frame3);
            slideWindow.AddNewFrame(frame4);

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(1));

            var person1Frames = slideWindow.GetFramesContainObjectId("person:1");
            Assert.That(person1Frames.Count, Is.EqualTo(3));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(true));

            var person1IsUnderTracking = slideWindow.IsObjIdUnderTracking("person:1");
            Assert.That(person1IsUnderTracking, Is.EqualTo(true));
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueMoreThan2()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            // frame 1: car1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            // frame 2: car1, person1
            var frame2 = new Frame("tempId", 2, 40, new Mat());
            frame2.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                },

                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 3: person1
            var frame3 = new Frame("tempId", 3, 80, new Mat());
            frame3.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 4: person1
            var frame4 = new Frame("tempId", 4, 120, new Mat());
            frame4.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 5: person1, car2
            var frame5 = new Frame("tempId", 5, 160, new Mat());
            frame5.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                },

                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            slideWindow.AddNewFrame(frame1);
            slideWindow.AddNewFrame(frame2);
            slideWindow.AddNewFrame(frame3);
            slideWindow.AddNewFrame(frame4);
            slideWindow.AddNewFrame(frame5);

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(0));

            var person1Frames = slideWindow.GetFramesContainObjectId("person:1");
            Assert.That(person1Frames.Count, Is.EqualTo(3));

            var car2Frames = slideWindow.GetFramesContainObjectId("car:2");
            Assert.That(car2Frames.Count, Is.EqualTo(1));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(false));

            var person1IsUnderTracking = slideWindow.IsObjIdUnderTracking("person:1");
            Assert.That(person1IsUnderTracking, Is.EqualTo(true));

            var car2IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:2");
            Assert.That(car2IsUnderTracking, Is.EqualTo(true));
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueMoreThan3()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            // frame 1: car1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            // frame 2: car1, person1
            var frame2 = new Frame("tempId", 2, 40, new Mat());
            frame2.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                },

                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 3: person1
            var frame3 = new Frame("tempId", 3, 80, new Mat());
            frame3.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 4: person1
            var frame4 = new Frame("tempId", 4, 120, new Mat());
            frame4.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 5: person1, car2
            var frame5 = new Frame("tempId", 5, 160, new Mat());
            frame5.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                },

                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            // frame 6: car2
            var frame6 = new Frame("tempId", 5, 160, new Mat());
            frame6.DetectedObjects = new List<DetectedObject>()
            {
                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            slideWindow.AddNewFrame(frame1);
            slideWindow.AddNewFrame(frame2);
            slideWindow.AddNewFrame(frame3);
            slideWindow.AddNewFrame(frame4);
            slideWindow.AddNewFrame(frame5);
            slideWindow.AddNewFrame(frame6);

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(0));

            var person1Frames = slideWindow.GetFramesContainObjectId("person:1");
            Assert.That(person1Frames.Count, Is.EqualTo(2));

            var car2Frames = slideWindow.GetFramesContainObjectId("car:2");
            Assert.That(car2Frames.Count, Is.EqualTo(2));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(false));

            var person1IsUnderTracking = slideWindow.IsObjIdUnderTracking("person:1");
            Assert.That(person1IsUnderTracking, Is.EqualTo(true));

            var car2IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:2");
            Assert.That(car2IsUnderTracking, Is.EqualTo(true));
        }

        [Test]
        public void TestVideoFrameBuffer_EnqueueWithNoObject()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            // frame 1: car1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            // frame 2: car1, person1
            var frame2 = new Frame("tempId", 2, 40, new Mat());
            frame2.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                },

                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 3: person1
            var frame3 = new Frame("tempId", 3, 80, new Mat());
            frame3.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 4: person1
            var frame4 = new Frame("tempId", 4, 120, new Mat());
            frame4.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 5: person1, car2
            var frame5 = new Frame("tempId", 5, 160, new Mat());
            frame5.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                },

                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            // frame 6: car2
            var frame6 = new Frame("tempId", 6, 200, new Mat());
            frame6.DetectedObjects = new List<DetectedObject>()
            {
                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            // frame 7: 
            var frame7 = new Frame("tempId", 7, 240, new Mat());
            frame7.DetectedObjects = new List<DetectedObject>();

            // frame 8: 
            var frame8 = new Frame("tempId", 7, 280, new Mat());
            frame8.DetectedObjects = new List<DetectedObject>();

            slideWindow.AddNewFrame(frame1);
            slideWindow.AddNewFrame(frame2);
            slideWindow.AddNewFrame(frame3);
            slideWindow.AddNewFrame(frame4);
            slideWindow.AddNewFrame(frame5);
            slideWindow.AddNewFrame(frame6);
            slideWindow.AddNewFrame(frame7);
            slideWindow.AddNewFrame(frame8);

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(0));

            var person1Frames = slideWindow.GetFramesContainObjectId("person:1");
            Assert.That(person1Frames.Count, Is.EqualTo(0));

            var car2Frames = slideWindow.GetFramesContainObjectId("car:2");
            Assert.That(car2Frames.Count, Is.EqualTo(1));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(false));

            var person1IsUnderTracking = slideWindow.IsObjIdUnderTracking("person:1");
            Assert.That(person1IsUnderTracking, Is.EqualTo(false));

            var car2IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:2");
            Assert.That(car2IsUnderTracking, Is.EqualTo(true));
        }

        [Test]
        public void TestVideoFrameBuffer_ObserverInvoke()
        {
            int windowSize = 3;
            var slideWindow = new ObservableSlideWindow(windowSize);

            var frameExpiredObserver = Substitute.For<IObserver<FrameExpiredEvent>>();
            var objectExpiredObserver = Substitute.For<IObserver<ObjectExpiredEvent>>();
            slideWindow.Subscribe(frameExpiredObserver);
            slideWindow.Subscribe(objectExpiredObserver);

            // frame 1: car1
            var frame1 = new Frame("tempId", 1, 0, new Mat());
            frame1.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                }
            };

            // frame 2: car1, person1
            var frame2 = new Frame("tempId", 2, 40, new Mat());
            frame2.DetectedObjects = new List<DetectedObject>()
            {
                // object 1
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 1
                    }
                },

                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 3: person1
            var frame3 = new Frame("tempId", 3, 80, new Mat());
            frame3.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 4: person1
            var frame4 = new Frame("tempId", 4, 120, new Mat());
            frame4.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                }
            };

            // frame 5: person1, car2
            var frame5 = new Frame("tempId", 5, 160, new Mat());
            frame5.DetectedObjects = new List<DetectedObject>()
            {
                // object 2
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "person",
                        TrackingId = 1
                    }
                },

                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            // frame 6: car2
            var frame6 = new Frame("tempId", 6, 200, new Mat());
            frame6.DetectedObjects = new List<DetectedObject>()
            {
                // object 3
                new DetectedObject()
                {
                    Bbox = new BoundingBox()
                    {
                        Label = "car",
                        TrackingId = 2
                    }
                }
            };

            // frame 7: 
            var frame7 = new Frame("tempId", 7, 240, new Mat());
            frame7.DetectedObjects = new List<DetectedObject>();

            // frame 8: 
            var frame8 = new Frame("tempId", 7, 280, new Mat());
            frame8.DetectedObjects = new List<DetectedObject>();

            slideWindow.AddNewFrame(frame1);
            slideWindow.AddNewFrame(frame2);
            slideWindow.AddNewFrame(frame3);

            slideWindow.AddNewFrame(frame4);
            frameExpiredObserver.Received().OnNext(Arg.Is<FrameExpiredEvent>(e => e.FrameId == 1));

            slideWindow.AddNewFrame(frame5);
            frameExpiredObserver.Received().OnNext(Arg.Is<FrameExpiredEvent>(e => e.FrameId == 2));
            objectExpiredObserver.Received().OnNext(Arg.Is<ObjectExpiredEvent>(e => e.Id == "car:1"));

            slideWindow.AddNewFrame(frame6);
            frameExpiredObserver.Received().OnNext(Arg.Is<FrameExpiredEvent>(e => e.FrameId == 3));

            slideWindow.AddNewFrame(frame7);
            frameExpiredObserver.Received().OnNext(Arg.Is<FrameExpiredEvent>(e => e.FrameId == 4));

            slideWindow.AddNewFrame(frame8);
            frameExpiredObserver.Received().OnNext(Arg.Is<FrameExpiredEvent>(e => e.FrameId == 5));
            objectExpiredObserver.Received().OnNext(Arg.Is<ObjectExpiredEvent>(e => e.Id == "person:1"));

            var car1Frames = slideWindow.GetFramesContainObjectId("car:1");
            Assert.That(car1Frames.Count, Is.EqualTo(0));

            var person1Frames = slideWindow.GetFramesContainObjectId("person:1");
            Assert.That(person1Frames.Count, Is.EqualTo(0));

            var car2Frames = slideWindow.GetFramesContainObjectId("car:2");
            Assert.That(car2Frames.Count, Is.EqualTo(1));

            var car1IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:1");
            Assert.That(car1IsUnderTracking, Is.EqualTo(false));

            var person1IsUnderTracking = slideWindow.IsObjIdUnderTracking("person:1");
            Assert.That(person1IsUnderTracking, Is.EqualTo(false));

            var car2IsUnderTracking = slideWindow.IsObjIdUnderTracking("car:2");
            Assert.That(car2IsUnderTracking, Is.EqualTo(true));
        }
    }
}
