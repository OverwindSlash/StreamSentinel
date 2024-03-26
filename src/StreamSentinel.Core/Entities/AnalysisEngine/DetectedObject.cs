using System;
using System.Drawing;
using System.Text.Json.Serialization;
using OpenCvSharp;

namespace StreamSentinel.Entities.AnalysisEngine;

public class DetectedObject : IDisposable
{
    public string Id => $"{Label}:{TrackingId}";
    public string DeviceId { get; set; }
    public long FrameId { get; set; }
    public DateTime TimeStamp { get; set; }

    public BoundingBox Bbox { get; set; }

    public int LabelId => Bbox.LabelId;
    public string Label => Bbox.Label;
    public long TrackingId => Bbox.TrackingId;

    public Mat Snapshot { get; set; }

    public bool IsUnderAnalysis { get; set; }

    [JsonIgnore]
    public int CenterX => Bbox.CenterX;
    [JsonIgnore]
    public int CenterY => Bbox.CenterY;
    [JsonIgnore]
    public int BottomCenterX => Bbox.BottomCenterX;
    [JsonIgnore]
    public int BottomCenterY => Bbox.BottomCenterY;
    [JsonIgnore]
    public int BottomLeftX => Bbox.BottomLeftX;
    [JsonIgnore]
    public int BottomLeftY => Bbox.BottomLeftY;
    [JsonIgnore]
    public int BottomRightX => Bbox.BottomRightX;
    [JsonIgnore]
    public int BottomRightY => Bbox.BottomRightY;
    [JsonIgnore]
    public RectangleF TrackingRectangle => new(Bbox.X, Bbox.Y, Bbox.Width, Bbox.Height);

    public void Dispose()
    {
        Snapshot?.Dispose();
    }
}