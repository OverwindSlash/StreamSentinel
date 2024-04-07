using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.OnvifCamera
{
    internal class CameraInfo
    {
        public string DeviceId { get; set; }
        public string Ipv4Address { get; set; }
        public bool CanPTZ { get; set; }

        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Altitude { get; set; }
        public float HomePanToEast { get; set; }
        public float HomeTiltToHorizon { get; set; }
        public float MinPanDegree { get; set; }
        public float MaxPanDegree { get; set; }
        public float MinTiltDegree { get; set; }
        public float MaxTiltDegree { get; set; }
        public float MinZoomLevel { get; set; }
        public float MaxZoomLevel { get; set; }
        public float FocalLength { get; set; } = 4.8f;
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public Matrix3x3 CameraRotationMatrix { get; set; }


        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // 默认取主码流
        public string ProfileToken { get; set; } = string.Empty;
        public int VideoWidth { get; set; } = 1920;
        public int VideoHeight { get; set; } = 1080;

        public float Fy { get; set; } = 2500f;

        public float CCDWidth { get; set; } = 5.4f;
        public float CCDHeight { get; set; } = 4.0f;

        public string StreamUri { get; set; }
        public string ServerStreamUri {  get; set; }
    }
}
