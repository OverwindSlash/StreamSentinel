using Newtonsoft.Json;
using Service.CameraManager.OnvifCamera;
using StreamSentinel.Entities.Events.PtzControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.Service
{
    public class CameraManagementService
    {
        private bool isCameraMoving = false;
        private CameraController _cameraController;
        private ServiceConfig _serviceConfig;

        public CameraManagementService(string configFile)
        {
            try
            {
                _serviceConfig = JsonConvert.DeserializeObject<ServiceConfig>(File.ReadAllText(configFile));
                _cameraController = new CameraController(_serviceConfig.PtzUri);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"CameraManagementService Constructor Error: {ex.Message}");
                return;
            }            
        }

        public bool LookTo(ITarget target)
        {
            if (isCameraMoving)
            {
                return false;
            }
            bool result =false;
            isCameraMoving = true;
            // 依据不同源的命令，进入不同的状态
            switch (target.CommandSource)
            {
                case CommandSourceEnum.FixedCamera:
                    // 需要通过固定相机的参数来计算需要相对运动的角度
                    var movement = _cameraController.CalculateMovementFixedDevice(target.BBox, _serviceConfig.FixedDeviceId);
                    Trace.TraceInformation($"Movement: {movement.X}, {movement.Y}, {movement.Z}");
                    result = _cameraController.PointToTargetForAnotherPtzDevice(movement, _serviceConfig.RelativePosition, _serviceConfig.FakeDistance, _serviceConfig.PtzDeviceId);

                    break;
                case CommandSourceEnum.PtzCamera:
                    result = _cameraController.MoveRelativeByImage(target.BBox, _serviceConfig.PtzDeviceId);
                    break;
                default:
                    break;
            }
            isCameraMoving = false;

            return result;
        }

    }
}
