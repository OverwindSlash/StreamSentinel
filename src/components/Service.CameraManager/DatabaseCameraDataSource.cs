using Service.CameraManager.OnvifCamera;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager
{
    internal class DatabaseCameraDataSource : ICameraDataSource
    {
        private readonly ICameraService cameraApiService;

        public DatabaseCameraDataSource(ICameraService _cameraApiService)
        {
            cameraApiService= _cameraApiService;
        }

        public string BaseUri { get; set; }

        public List<CameraInfo> LoadCameras()
        {
            var cameraInfos = cameraApiService.GetAllDevices();
            cameraInfos.ForEach(c => { c.CameraRotationMatrix = Matrix3x3.CalculateCameraRotationMatrix(c.Pitch, c.Yaw, c.Roll); });

            return cameraInfos;
        }

    }
}
