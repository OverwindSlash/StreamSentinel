using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace StreamSentinel.Entities.AnalysisEngine;

public class Frame : IDisposable
{
    public string DeviceId { get; set; }

    public long FrameId { get; }

    public long OffsetMilliSec { get; set; }

    public DateTime TimeStamp { get; }

    public Mat Scene { get; set; }

    public List<DetectedObject> DetectedObjects { get; set; }

    public Frame(string deviceId, long frameId, long offsetMilliSec, Mat scene)
    {
        DeviceId = deviceId;
        FrameId = frameId;
        OffsetMilliSec = offsetMilliSec;
        TimeStamp = DateTime.Now;
        Scene = scene;
        DetectedObjects = new List<DetectedObject>();
    }

    public void Dispose()
    {
        Scene?.Dispose();
        foreach (DetectedObject detectedObject in DetectedObjects)
        {
            detectedObject.Dispose();
        }
    }
}