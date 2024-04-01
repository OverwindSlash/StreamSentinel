using System;
using System.Collections.Generic;

namespace StreamSentinel.Entities.Geometric
{
    public class NormalizedPolygon : ImageBasedGeometric
    {
        private List<NormalizedPoint> _points;

        public List<NormalizedPoint> Points
        {
            get
            {
                CheckImageSizeInitialized();
                return _points;
            }
            set => _points = value;
        }

        // For polygon generation by json deserialization
        // 1. Use constructor to create a new NormalizedPolygon object.
        // 2. Use Properties set method to set _points.
        // 3. Manual call SetImageSize to specify real image width and height,
        // then call the SetImageSize functions of each NormalizedPoint object.
        public NormalizedPolygon()
        {
            Points = new List<NormalizedPoint>();
        }
        
        public override void SetImageSize(int width, int height)
        {
            base.SetImageSize(width, height);

            foreach (NormalizedPoint point in Points)
            {
                point.SetImageSize(width, height);
            }
        }

        // For polygon generation by hand
        public NormalizedPolygon(List<NormalizedPoint> points)
        {
            if ((points == null) || (points.Count == 0))
            {
                throw new ArgumentException("null or empty points list.");
            }

            int imageWidth = points[0].ImageWidth;
            int imageHeight = points[0].ImageHeight;
            foreach (NormalizedPoint point in points)
            {
                if ((point.ImageWidth != imageWidth) || (point.ImageHeight != imageHeight))
                {
                    throw new ArgumentException("point does not match scale of others.");
                }
            }
            
            base.SetImageSize(imageWidth, imageHeight);

            _points = points;
        }

        public void AddPoint(NormalizedPoint point)
        {
            // When first point to be add.
            if (!IsInitialized())
            {
                base.SetImageSize(point.ImageWidth, point.ImageHeight);
            }
            
            if ((point.ImageWidth != ImageWidth) || (point.ImageHeight != ImageHeight))
            {
                throw new ArgumentException("point does not match scale of others.");
            }
            
            _points.Add(point);
        }

        public void RemovePoint(NormalizedPoint point)
        {
            if (point == null)
            {
                return;
            }

            _points.Remove(point);
        }

        public bool IsPointInPolygon(NormalizedPoint p)
        {
            if (_points.Count == 0)
            {
                return false;
            }

            double minX = _points[0].OriginalX;
            double maxX = _points[0].OriginalX;
            double minY = _points[0].OriginalY;
            double maxY = _points[0].OriginalY;
            for (int i = 1; i < _points.Count; i++)
            {
                NormalizedPoint q = _points[i];
                minX = Math.Min(q.OriginalX, minX);
                maxX = Math.Max(q.OriginalX, maxX);
                minY = Math.Min(q.OriginalY, minY);
                maxY = Math.Max(q.OriginalY, maxY);
            }

            if (p.OriginalX < minX || p.OriginalX > maxX || p.OriginalY < minY || p.OriginalY > maxY)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = _points.Count - 1; i < _points.Count; j = i++)
            {
                if ((_points[i].OriginalY > p.OriginalY) != (_points[j].OriginalY > p.OriginalY) &&
                    p.OriginalX < (_points[j].OriginalX - _points[i].OriginalX) * (p.OriginalY - _points[i].OriginalY) 
                    / (_points[j].OriginalY - _points[i].OriginalY) + _points[i].OriginalX)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
