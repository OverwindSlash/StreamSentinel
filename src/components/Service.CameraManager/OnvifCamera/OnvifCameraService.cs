using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.OnvifCamera
{
    internal class OnvifCameraService : ICameraService
    {
        private readonly RestApiClient restApiClient;
        private List<CameraInfo> cameraInfos;

        public OnvifCameraService(string baseUri)
        {
            restApiClient = new RestApiClient(baseUri);
            cameraInfos = new List<CameraInfo>();
        }

        public List<CameraInfo> GetAllDevices()
        {
            cameraInfos.Clear();
            // 从数据库加载摄像机信息
            // get all devices
            // find username and password
            // find mainstream profile token
            string devicesJson = string.Empty;
            try
            {
                devicesJson = restApiClient.GetAsync("Device/GetAll").Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Network connection failed.");
                return cameraInfos;
            }
            var root = JsonConvert.DeserializeObject<RootResult<ResultData<DeviceItem>>>(devicesJson);
            if (root == null)
            {
                return cameraInfos;
            }
            foreach (var device in root?.Result?.Items)
            {
                var cameraInfo = new CameraInfo();
                cameraInfo.DeviceId = device.DeviceId;
                cameraInfo.Ipv4Address = device.Ipv4Address;

                var profile = device.Profiles.FirstOrDefault();
                if (profile != null)
                {
                    cameraInfo.HomePanToEast = profile.PtzParams.HomePanToNorth;
                    cameraInfo.HomeTiltToHorizon = profile.PtzParams.HomeTiltToHorizon;
                    cameraInfo.MinPanDegree = profile.PtzParams.MinPanDegree;
                    cameraInfo.MaxPanDegree = profile.PtzParams.MaxPanDegree;
                    cameraInfo.MinTiltDegree = profile.PtzParams.MinTiltDegree;
                    cameraInfo.MaxTiltDegree = profile.PtzParams.MaxTiltDegree;
                    cameraInfo.MinZoomLevel = profile.PtzParams.MinZoomLevel <= 0 ? 1 : profile.PtzParams.MinZoomLevel;
                    cameraInfo.MaxZoomLevel = profile.PtzParams.MaxZoomLevel <= 0 ? 33 : profile.PtzParams.MaxZoomLevel;
                    cameraInfo.FocalLength = profile.PtzParams.FocalLength <= 0 ? 4.8f : profile.PtzParams.FocalLength;
                    cameraInfo.CCDWidth = profile.PtzParams.SensorWidth <= 0 ? 7.2f : profile.PtzParams.SensorWidth;
                    cameraInfo.CCDHeight = profile.PtzParams.SensorHeight <= 0 ? 5.3f : profile.PtzParams.SensorHeight;

                    cameraInfo.ProfileToken = profile.Token;
                    cameraInfo.VideoHeight = profile.VideoHeight;
                    cameraInfo.VideoWidth = profile.VideoWidth;

                    cameraInfo.StreamUri = profile.StreamUri;
                    cameraInfo.ServerStreamUri = profile.StreamUri;
                }

                try
                {
                    var installationJson = restApiClient.GetAsync("Device/GetInstallationParams?deviceId=" + device.DeviceId).Result;
                    var installation = JsonConvert.DeserializeObject<RootResult<InstallationData>>(installationJson)?.Result;
                    if (installation == null)
                    {
                        continue;
                    }
                    cameraInfo.Longitude = installation.Longitude;
                    cameraInfo.Latitude = installation.Latitude;
                    cameraInfo.Altitude = installation.Altitude;
                    cameraInfo.Roll = installation.Roll;
                    cameraInfo.Pitch = installation.Pitch;
                    cameraInfo.Yaw = installation.Yaw;
                    cameraInfo.HomePanToEast = installation.HomePanToEast;
                    cameraInfo.HomeTiltToHorizon = installation.HomeTiltToHorizon;

                    //var parameters = new Dictionary<string, string>()
                    //{
                    //    {"deviceId", device.DeviceId },
                    //    {"profileToken", cameraInfo.ProfileToken }
                    //};

                    //var uri = restApiClient.BuildUri("Device/GetVideoSources", parameters);
                    //var videoSourceJson = restApiClient.GetAsync(uri).Result;
                    //var videoSource = JsonConvert.DeserializeObject<RootResult<VideoSource>>(videoSourceJson);
                    //if (videoSource != null)
                    //{
                    //    cameraInfo.UserName = videoSource.Result.Username;
                    //    cameraInfo.Password = videoSource.Result.Password;
                    //    cameraInfo.ServerStreamUri = videoSource.Result.ServerStreamUri;

                    //}

                }
                catch (Exception ex)
                {
                    Trace.TraceError($"ERROR DeviceId: {device.DeviceId}; Error Message: {ex.Message}");
                    continue;
                }

                cameraInfos.Add(cameraInfo);
            }

            return cameraInfos;
        }

        public CameraStatus GetCurrentStatus(string deviceId)
        {
            // TODO: delete
            //deviceId = "Cam-3345a6";
            var cameraStatus = new CameraStatus();
            // host username pw profile
            if (cameraInfos.Count == 0)
            {
                cameraInfos = GetAllDevices();
            }

            var cameraInfo = cameraInfos.Find(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                cameraStatus.Error = $"Can not find the Device: {deviceId}";
                return cameraStatus;
            }
            var parameters = new Dictionary<string, string>()
            {
                {"deviceId", deviceId },
                {"profileToken", cameraInfo.ProfileToken }
            };

            var uri = restApiClient.BuildUri("PTZ/GetStatusInDegree", parameters);

            try
            {
                var jsonString = restApiClient.GetAsync(uri).Result;
                // 将JSON字符串反序列化为ApiResponse对象
                var cameraStatusResponse = JsonConvert.DeserializeObject<CameraStatusResponse>(jsonString);
                cameraStatus = cameraStatusResponse?.Result;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to GetStatusInDegree");
                //throw;
                cameraStatus.Error = $"Failed to GetStatusInDegree. [DeviceId: {deviceId}]";
                return cameraStatus;
            }

            return cameraStatus;
        }

        public CameraStatus MoveToAbsolutePositionInDegree(string deviceId, float panInDegree, float tiltInDegree, float zoomLevel
            , float panSpeed = 1, float tiltSpeed = 1, float zoomSpeed = 1)
        {
            var cameraStatus = new CameraStatus();

            if (cameraInfos.Count == 0)
            {
                cameraInfos = GetAllDevices();
            }

            var cameraInfo = cameraInfos.Find(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                cameraStatus.Error = $"Can not find the Device: {deviceId}";
                return cameraStatus;
            }
            var parameters = new Dictionary<string, string>()
            {
                {"deviceId", deviceId },
                {"profileToken", cameraInfo.ProfileToken },
                {"panInDegree", panInDegree.ToString()},
                {"tiltInDegree", tiltInDegree.ToString()},
                {"zoomInLevel",zoomLevel.ToString()},
                {"panSpeed",panSpeed.ToString()},
                {"tiltSpeed",tiltSpeed.ToString()},
                {"zoomSpeed",zoomSpeed.ToString()}
            };

            var uri = restApiClient.BuildUri("PTZ/AbsoluteMoveWithDegree", parameters);

            try
            {
                var jsonString = restApiClient.PostAsync(uri, string.Empty).Result;
                // 将JSON字符串反序列化为ApiResponse对象
                cameraStatus = JsonConvert.DeserializeObject<CameraStatus>(jsonString);

            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to MoveToAbsolutePositionInDegree");
                //throw;
                cameraStatus.Error = $"Failed to MoveAbsolute. [DeviceId: {deviceId}]";
            }

            return cameraStatus;

        }

        public CameraStatus MoveToRelativePositionInDegree(string deviceId, float panInDegree, float tiltInDegree, float zoomLevel
            , float panSpeed = 1, float tiltSpeed = 1, float zoomSpeed = 1)
        {
            var cameraStatus = new CameraStatus();
            // TODO: delete
            //deviceId = "Cam-3345a6";

            if (cameraInfos.Count == 0)
            {
                cameraInfos = GetAllDevices();
            }

            var cameraInfo = cameraInfos.Find(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                cameraStatus.Error = $"Can not find the Device: {deviceId}";
                return cameraStatus;
            }
            var parameters = new Dictionary<string, string>()
            {
                {"deviceId", deviceId },
                {"profileToken", cameraInfo.ProfileToken },
                {"panInDegree", panInDegree.ToString()},
                {"tiltInDegree", tiltInDegree.ToString()},
                {"zoomInLevel",zoomLevel.ToString()},
                {"panSpeed",panSpeed.ToString()},
                {"tiltSpeed",tiltSpeed.ToString()},
                {"zoomSpeed",zoomSpeed.ToString()}
            };

            var uri = restApiClient.BuildUri("PTZ/RelativeMoveWithDegree", parameters);

            try
            {
                var jsonString = restApiClient.PostAsync(uri, string.Empty).Result;
                // 将JSON字符串反序列化为ApiResponse对象
                cameraStatus = JsonConvert.DeserializeObject<CameraStatus>(jsonString);
                Console.WriteLine($"Moved: [DeviceId: {deviceId}, Pan: {panInDegree}, Tilt: {tiltInDegree}, Zoom: {zoomLevel}]");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to MoveRelative [DeviceId: {deviceId}, Pan: {panInDegree}, Tilt: {tiltInDegree}, Zoom: {zoomLevel}]");
                //throw;
                cameraStatus.Error = $"Failed to MoveRelative. [DeviceId: {deviceId}, Pan: {panInDegree}, Tilt: {tiltInDegree}, Zoom: {zoomLevel}]";
            }

            return cameraStatus;
        }
    }
}
