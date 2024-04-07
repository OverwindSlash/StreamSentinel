using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.Service
{
    public interface ITarget
    {
        public CommandSourceEnum CommandSource { get; set; }
        public RectangleF BBox{ get; set; }
        public DirectionEnum Direction { get; set; }
    }

    public enum DirectionEnum
    {
        Coming,
        Leaving
    }

    public enum CommandSourceEnum
    {
        FixedCamera,
        VariableCamera
    }
}
