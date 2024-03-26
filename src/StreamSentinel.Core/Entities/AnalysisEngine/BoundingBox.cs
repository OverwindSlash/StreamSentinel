using System.Text.Json.Serialization;

namespace StreamSentinel.Entities.AnalysisEngine;

public class BoundingBox
{
    public int LabelId { get; set; }
    public string Label { get; set; }
    public float Confidence { get; set; }

    public uint TrackingId { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
        

    [JsonIgnore]
    public int CenterX => (X + Width / 2);
    [JsonIgnore]
    public int CenterY => (Y + Height / 2);
    [JsonIgnore]
    public int BottomCenterX => (X + Width / 2);
    [JsonIgnore]
    public int BottomCenterY => (Y + Height);
    [JsonIgnore]
    public int BottomLeftX => X;
    [JsonIgnore]
    public int BottomLeftY => (Y + Height);
    [JsonIgnore]
    public int BottomRightX => (X + Width);
    [JsonIgnore]
    public int BottomRightY => BottomLeftY;
}