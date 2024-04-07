using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager
{
    internal class Point3F
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
