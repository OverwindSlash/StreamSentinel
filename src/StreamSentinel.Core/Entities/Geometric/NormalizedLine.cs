using System;

namespace StreamSentinel.Entities.Geometric
{
    public class NormalizedLine : ImageBasedGeometric
    {
        private NormalizedPoint _start;
        private NormalizedPoint _stop;

        public NormalizedPoint Start
        {
            get
            {
                CheckImageSizeInitialized();
                return _start;
            }
            set => _start = value;
        }

        public NormalizedPoint Stop
        {
            get
            {
                CheckImageSizeInitialized();
                return _stop;
            }
            set => _stop = value;
        }

        // For line generation by json deserialization
        // 1. Use constructor to create a new NormalizedLine object.
        // 2. Use Properties set method to set _start and _stop.
        // 3. Manual call SetImageSize to specify real image width and height,
        // then call the SetImageSize functions of _start and _stop object respectively.
        public NormalizedLine()
        {
            _start = new NormalizedPoint();
            _stop = new NormalizedPoint();
        }
        
        public override void SetImageSize(int width, int height)
        {
            base.SetImageSize(width, height);
            
            _start.SetImageSize(width, height);
            _stop.SetImageSize(width, height);
        }

        // For line generation by hand
        public NormalizedLine(NormalizedPoint start, NormalizedPoint stop)
        {
            if ((start == null) || (stop == null))
            {
                throw new ArgumentException("points of line contain null value.");
            }

            if ((start.ImageWidth != stop.ImageWidth) || (start.ImageHeight != stop.ImageHeight))
            {
                throw new ArgumentException("start and stop are not in same scale.");
            }
            
            base.SetImageSize(start.ImageWidth, start.ImageHeight);
            
            Start = start;
            Stop = stop;
        }

        public bool IsCrossedLine(NormalizedPoint p1, NormalizedPoint p2)
        {
            NormalizedPoint cmp = new NormalizedPoint(ImageWidth, ImageHeight,
                _start.OriginalX - p1.OriginalX, _start.OriginalY - p1.OriginalY);
            NormalizedPoint r = new NormalizedPoint(ImageWidth, ImageHeight,
                p2.OriginalX - p1.OriginalX, p2.OriginalY - p1.OriginalY);
            NormalizedPoint s = new NormalizedPoint(ImageWidth, ImageHeight,
                _stop.OriginalX - _start.OriginalX, _stop.OriginalY - _start.OriginalY);

            double cmpXr = cmp.OriginalX * r.OriginalY - cmp.OriginalY * r.OriginalX;
            double cmpXs = cmp.OriginalX * s.OriginalY - cmp.OriginalY * s.OriginalX;
            double rxs = r.OriginalX * s.OriginalY - r.OriginalY * s.OriginalX;

            if (cmpXr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap
                return ((_start.OriginalX - p1.OriginalX < 0f) != (_start.OriginalX - p2.OriginalX < 0f))
                       || ((_start.OriginalY - p1.OriginalY < 0f) != (_start.OriginalY - p2.OriginalY < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            double rxsr = 1f / rxs;
            double t = cmpXs * rxsr;
            double u = cmpXr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }
    }
}
