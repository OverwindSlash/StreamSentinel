using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Entities.AnalysisEngine
{
    public interface IPrediction
    {
        public DetectionObjectType DetectionObjectType { get; }
        public Rectangle CurrentBoundingBox { get; }
        public float Confidence { get; }

        public int TrackId { get; set; }
    }
}
