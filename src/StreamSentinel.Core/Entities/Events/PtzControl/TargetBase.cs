using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Entities.Events.PtzControl
{
    public class TargetBase : ITarget
    {
        public RectangleF BBox { get; set; }
        public DirectionEnum Direction { get; set; }
        public CommandSourceEnum CommandSource { get; set; }
    }
}
