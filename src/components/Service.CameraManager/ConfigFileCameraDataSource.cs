using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.CameraManager.OnvifCamera;
using Newtonsoft.Json;

namespace Service.CameraManager
{
    internal class ConfigFileCameraDataSource : ICameraDataSource
    {
        private string configFile;

        public ConfigFileCameraDataSource(string configFile)
        {
            this.configFile = configFile;
            BaseUri = configFile;
        }

        public string BaseUri { get; set; }

        public List<CameraInfo> LoadCameras()
        {
            List<CameraInfo> cameras = new List<CameraInfo>();

            try
            {
                // 加载配置文件
                var config = JsonConvert.DeserializeObject<CameraConfig>(File.ReadAllText(configFile));

                // 获取摄像头列表
                cameras = config.Cameras;

                cameras.ForEach(c => { c.CameraRotationMatrix = Matrix3x3.CalculateCameraRotationMatrix(c.Pitch, c.Yaw, c.Roll); });

            }
            catch (Exception ex)
            {
                // 处理异常
            }

            return cameras;
        }
    }
}
