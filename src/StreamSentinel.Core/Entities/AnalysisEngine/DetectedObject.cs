using System;
using System.Drawing;
using System.Text.Json.Serialization;
using OpenCvSharp;

namespace StreamSentinel.Entities.AnalysisEngine;

public class DetectedObject : IDisposable, IPrediction
{
    public string Id => $"{Label}:{TrackingId}";
    public string DeviceId { get; set; }
    public long FrameId { get; set; }
    public DateTime TimeStamp { get; set; }

    public BoundingBox Bbox { get; set; }

    public int LabelId => Bbox.LabelId;
    public string Label => Bbox.Label;
    public long TrackingId => Bbox.TrackingId;
    public float Confidence => Bbox.Confidence;

    public Mat Snapshot { get; set; }

    public bool IsUnderAnalysis { get; set; }

    [JsonIgnore]
    public int X => Bbox.X;
    [JsonIgnore]
    public int Y => Bbox.Y;
    [JsonIgnore]
    public int Width => Bbox.Width;
    [JsonIgnore]
    public int Height => Bbox.Height;
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

    [JsonIgnore]
    public DetectionObjectType DetectionObjectType => (DetectionObjectType)(LabelId + 5);
    [JsonIgnore]
    public Rectangle CurrentBoundingBox => new(Bbox.X, Bbox.Y, Bbox.Width, Bbox.Height);
    //[JsonIgnore]
    //public float Confidence => Bbox.Confidence;
    [JsonIgnore]
    public int TrackId
    {
        get
        { return (int)TrackingId; }
        set
        {
            Bbox.TrackingId = (uint)value;
        }
    }

    public void Dispose()
    {
        Snapshot?.Dispose();
    }
}