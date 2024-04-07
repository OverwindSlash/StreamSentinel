using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.OnvifCamera
{
    internal class CameraStatus
    {
        public float PanPosition { get; set; }
        public float TiltPosition { get; set; }
        public float ZoomPosition { get; set; }
        public string PanTiltStatus { get; set; }
        public string ZoomStatus { get; set; }
        public string UtcDateTime { get; set; }
        public string Error { get; set; }=string.Empty;
    }

    internal class CameraStatusResponse
    {
        public CameraStatus Result { get; set; }
        public string TargetUrl { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool UnAuthorizedRequest { get; set; }
        public bool __abp { get; set; }
    }
}
