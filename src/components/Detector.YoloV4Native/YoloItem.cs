using System.Drawing;

namespace Detector.YoloV4Native
{
    public class YoloItem
    {
        public int TypeId { get; set; }
        public string Type { get; set; }
        public float Confidence { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public uint TrackingId { get; set; }

        public Point TopLeft => new Point(X, Y);
        public Point TopRight => new Point(X + Width, Y);
        public Point BottomLeft => new Point(X, Y + Height);
        public Point BottomRight => new Point(X + Width, Y + Height);
        public Point Center => new Point(X + Width / 2, Y + Height / 2);

        public List<Point> CornerPoints => new List<Point>() { TopLeft, TopRight, BottomLeft, BottomRight };
    }
}
