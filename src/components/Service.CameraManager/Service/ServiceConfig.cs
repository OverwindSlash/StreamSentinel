using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.Service
{
    internal class ServiceConfig
    {
        public string PtzUri { get; set; }
        public string FixedDeviceId { get; set;}
        public string PtzDeviceId { get; set; }
        /// <summary>
        /// XYZ 表示相对坐标
        /// </summary>
        public Vector3 RelativePosition { get; set; }

        public float FakeDistance { get; set; }
    }
}
