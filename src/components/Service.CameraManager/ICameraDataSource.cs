using Service.CameraManager.OnvifCamera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager
{
    internal interface ICameraDataSource
    {
        List<CameraInfo> LoadCameras();
    }
}
