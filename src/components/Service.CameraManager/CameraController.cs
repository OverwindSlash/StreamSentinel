using Service.CameraManager.OnvifCamera;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager
{
    internal class CameraController
    {
        private List<CameraInfo> cameras; // 摄像机列表
        private ICameraDataSource cameraDataSource;

        private readonly Dictionary<string, ICameraService> cameraApiServices= new Dictionary<string, ICameraService>();
        private Dictionary<string,MoveStatus> moveStatus = new Dictionary<string,MoveStatus>();

        #region detect and track


        private const float MinAngleToMove = 1.0f;
        #endregion


        public CameraController(string baseUrl)
        {
            var cameraApiService = new OnvifCameraService(baseUrl);
            this.cameraDataSource = new DatabaseCameraDataSource(cameraApiService);
            cameras = cameraDataSource.LoadCameras();

            foreach (var camera in cameras)
            {
                cameraApiServices.Add(camera.DeviceId, new OnvifCameraService(baseUrl));
            }

            int coreCount = Environment.ProcessorCount;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(coreCount, coreCount);
        }

        // 根据船只坐标找到最近的摄像机
        private CameraInfo FindNearestCamera(GeoLocation shipLocation)
        {
            CameraInfo nearestCamera = null;
            double minDistance = double.MaxValue;

            foreach (var camera in cameras)
            {
                double distance = CalculateDistance(shipLocation, new GeoLocation { Latitude = camera.Latitude, Longitude = camera.Longitude });
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCamera = camera;
                }
            }

            return nearestCamera;
        }

        // 计算两点间距离
        private double CalculateDistance(GeoLocation location1, GeoLocation location2)
        {
            // 使用合适的距离计算公式，比如Haversine公式
            // 这里给出一个简单的计算方式
            return Math.Sqrt(Math.Pow(location1.Latitude - location2.Latitude, 2) + Math.Pow(location1.Longitude - location2.Longitude, 2));
        }

        public void PrintCameraDetails()
        {
            Console.WriteLine("Camera Details:");
            foreach (var camera in cameras)
            {
                Console.WriteLine($"DeviceId: {camera.DeviceId}");
                Console.WriteLine($"Latitude: {camera.Latitude}");
                Console.WriteLine($"Longitude: {camera.Longitude}");
                Console.WriteLine($"Altitude: {camera.Altitude}");
                Console.WriteLine($"MinPan: {camera.MinPanDegree}");
                Console.WriteLine($"MaxPan: {camera.MaxPanDegree}");
                Console.WriteLine($"MinTilt: {camera.MinTiltDegree}");
                Console.WriteLine($"MaxTilt: {camera.MaxTiltDegree}");
                Console.WriteLine($"MinZoomLevel: {camera.MinZoomLevel}");
                Console.WriteLine($"MaxZoomLevel: {camera.MaxZoomLevel}");
                Console.WriteLine($"FocalLength: {camera.FocalLength}");
                Console.WriteLine($"HomePanToNorth: {camera.HomePanToEast}");
                Console.WriteLine($"HomeTiltToHorizon: {camera.HomeTiltToHorizon}");
                //Console.WriteLine($"AngleToXAxis: {camera.AngleToXAxis}");
                //Console.WriteLine($"AngleToYAxis: {camera.AngleToYAxis}");
                //Console.WriteLine($"AngleToZAxis: {camera.AngleToZAxis}");
                Console.WriteLine($"VideoWidth: {camera.VideoWidth}");
                Console.WriteLine($"VideoHeight: {camera.VideoHeight}");
                Console.WriteLine("-------------------------------------");

            }
        }


        #region calculate by pixel

        // 计算物体在相机图像中的位置
        private PointF CalculateObjectPositionInImage(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 将物体坐标从世界坐标系转换到相机坐标系
            var objectCoordinatesInCameraCoordinates = camera.CameraRotationMatrix * objectCoordinates;

            // 计算物体在相机图像中的位置
            var objectPositionInImage = new PointF
            {
                X = (float)((objectCoordinatesInCameraCoordinates.X / objectCoordinatesInCameraCoordinates.Z) * camera.FocalLength),
                Y = (float)((objectCoordinatesInCameraCoordinates.Y / objectCoordinatesInCameraCoordinates.Z) * camera.FocalLength)
            };

            return objectPositionInImage;
        }

        // 计算相机需要水平转动的角度
        private float CalculateHorizontalPanAngleByImage(RectangleF objectPositionInImage, CameraInfo camera)
        {
            // 计算相机需要水平转动的角度
            var horizontalPanAngle = MathF.Atan2(objectPositionInImage.X, camera.FocalLength) * 180 / MathF.PI - camera.HomePanToEast;

            return horizontalPanAngle;
        }


        // 计算相机需要垂直转动的角度
        private float CalculateVerticalTiltAngleByImage(RectangleF objectPositionInImage, CameraInfo camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = MathF.Atan2(objectPositionInImage.Y, camera.FocalLength) * 180 / MathF.PI - camera.HomeTiltToHorizon;

            return verticalTiltAngle;
        }
        #endregion
        #region core

        private Vector3 CalculateObjectPositionToCamera(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 将物体坐标从世界坐标系转换到相机坐标系
            var objectCoordinatesInCameraCoordinates = camera.CameraRotationMatrix * objectCoordinates;
            return objectCoordinatesInCameraCoordinates;
        }
        private float CalculateHorizontalPanAngle(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 计算相机需要水平转动的角度
            // 由于自定义的相机坐标系，Y朝正南，需要反向
            var horizontalPanAngle = MathF.Atan2(-objectCoordinates.Y, objectCoordinates.X) * 180 / MathF.PI - camera.HomePanToEast;

            //if (horizontalPanAngle < 0)
            //{
            //    horizontalPanAngle = 360 + horizontalPanAngle;
            //}
            return horizontalPanAngle;
        }
        private float CalculateVerticalTiltAngle(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = MathF.Atan2(-objectCoordinates.Z, -objectCoordinates.Y) * 180 / MathF.PI - camera.HomeTiltToHorizon;

            verticalTiltAngle = Math.Clamp(verticalTiltAngle, camera.MinTiltDegree, camera.MaxTiltDegree);
            return verticalTiltAngle;
        }

        private float CalculateZoomLevel(Vector3 objectPositionToCamera, CameraInfo cameraInfo)
        {
            //var F = cameraInfo.VideoWidth * objectPositionToCamera.X / cameraInfo.Fy / objectPositionToCamera.Z;
            var F = cameraInfo.CCDHeight * objectPositionToCamera.Y / cameraInfo.FocalLength / objectPositionToCamera.Z;
            F = Math.Clamp(MathF.Abs(F), 1f, cameraInfo.MaxZoomLevel);
            return F;
        }

        #endregion

        #region Move Controll
        private bool PrepareToMove(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            var status = cameraApiServices[deviceId].GetCurrentStatus(deviceId);

            if (status != null && status.Error == "NO error")
            {
                if (moveStatus.ContainsKey(deviceId))
                {
                    moveStatus[deviceId].CameraStatus = status;
                }
                else
                {
                    moveStatus.TryAdd(deviceId, new MoveStatus(status));
                }
                //Console.WriteLine($"Current Stutus: [Zoom: {moveStatus[deviceId].CameraStatus.ZoomPosition}]");
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool MoveAbsolute(string deviceId, float panInDegree, float tiltInDegree, float zoomLevel)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            if (!PrepareToMove(deviceId))
            {
                return false;
            }

            var status = cameraApiServices[deviceId].MoveToAbsolutePositionInDegree(deviceId, panInDegree, tiltInDegree, zoomLevel);
            if (status != null && status.Error == "NO error")
            {
                moveStatus[deviceId].CameraStatus = status;
            }
            return true;
        }

        public bool MoveRelative(string deviceId, float panInDegree, float tiltInDegree, float zoomLevel)
        {
            if (panInDegree == 0f && tiltInDegree ==0f && zoomLevel ==0)
            {
                return true;
            }
            var zoomPosition = moveStatus[deviceId].CameraStatus.ZoomPosition;
            if (MathF.Abs(panInDegree) < MinAngleToMove/ zoomPosition & MathF.Abs(tiltInDegree) < MinAngleToMove/ zoomPosition & zoomLevel <= 0.0f && zoomPosition <= 1.1f)
            {
                return false;
            }
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            if (!PrepareToMove(deviceId))
            {
                return false;
            }


            Console.WriteLine($"To Move relatively: {panInDegree}, {tiltInDegree}, {zoomLevel}");
            var status = cameraApiServices[deviceId].MoveToRelativePositionInDegree(deviceId, panInDegree, tiltInDegree, zoomLevel);
            if (status != null && status.Error == "NO error")
            {
                moveStatus[deviceId].CameraStatus = status;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectCoordinates">通过经纬计算出来的直角坐标，高度就是安装时设置的参数Altitude</param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public bool PointToTarget(Vector3 objectCoordinates, string deviceId)
        {
           CameraInfo cameraInfo = cameras.FirstOrDefault(p=>p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                return false;
            }

            // calculate pan/tilt/zoom
            // 计算物体在相机图像中的位置
            var objectPositionToCamera = CalculateObjectPositionToCamera(objectCoordinates, cameraInfo);

            // 计算相机需要水平转动的角度
            var horizontalPanAngle = CalculateHorizontalPanAngle(objectPositionToCamera, cameraInfo);

            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = CalculateVerticalTiltAngle(objectPositionToCamera, cameraInfo);

            // 计算相机需要变焦的倍数
            var zoomLevel = CalculateZoomLevel(objectPositionToCamera,cameraInfo);

            var result = MoveAbsolute(deviceId, horizontalPanAngle, verticalTiltAngle, zoomLevel);

            return result;
        }

        public bool PointToTargetByGeo(GeoLocation location, string deviceId ="")
        {
            /* 1 find the nearest camera
             * 2 point camera at target
             */
            CameraInfo camera = null;
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                camera = FindNearestCamera(location);
            }
            else
            {
                camera = cameras.FirstOrDefault((p)=>p.DeviceId == deviceId);
            }
            if (camera ==null)
            {
                return false;
            }
            var target = GetRelativeCartesianCoordinates(camera.Latitude, camera.Longitude, camera.Altitude, location.Latitude, location.Longitude, location.Altitude);

            var result = PointToTarget(target, camera.DeviceId);
            return true;
        }
        #endregion


        #region helper
        public static Vector3 ConvertLatLngToCartesian(double latitude, double longitude, double altitude)
        {
            // Convert latitude and longitude to radians
            double latRad = latitude * Math.PI / 180;
            double lonRad = longitude * Math.PI / 180;

            // Calculate the Earth's radius at the given latitude
            double radius = 6378137.0; // Earth's mean radius in meters
            double flattening = 1 / 298.257223563; // Earth's flattening
            double e2 = flattening * (2 - flattening);
            double a = radius * (1 - e2);
            double b = radius * Math.Sqrt(1 - e2);
            double c = Math.Sqrt(a * a - b * b);

            // Calculate the x, y, and z coordinates
            double x = (a * Math.Cos(latRad) * Math.Cos(lonRad) + altitude) ; // Convert to meters
            double y = (a * Math.Cos(latRad) * Math.Sin(lonRad)) ; // Convert to meters
            double z = ((b * b / a) * Math.Sin(latRad) + altitude) ; // Convert to meters

            // Return the Cartesian coordinates
            return new Vector3((float)x, (float)y, (float)z);
        }

        private static Vector3 ConvertLatLngToEcef(double latitude, double longitude, double altitude)
        {
            double a = 6378137.0; // WGS-84 semi-major axis
            double e2 = 6.69437999014e-3; // WGS-84 first eccentricity squared
            var lat = DegreeToRadian(latitude);
            var lon = DegreeToRadian(longitude);
            var alt = altitude;

            var N = a / Math.Sqrt(1 - e2 * Math.Sin(lat) * Math.Sin(lat));

            var x = (N + alt) * Math.Cos(lat) * Math.Cos(lon);
            var y = (N + alt) * Math.Cos(lat) * Math.Sin(lon);
            var z = (N * (1 - e2) + alt) * Math.Sin(lat);

            return new Vector3 { X = (float)x, Y = (float)y, Z = (float)z };
        }

        private static double DegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }
        public static Vector3 GetRelativeCartesianCoordinates(double latitude1, double longitude1, double altitude1, double latitude2, double longitude2, double altitude2)
        {
            // Convert the two sets of latitude and longitude to Cartesian coordinates
            //Vector3 cartesianCoordinates1 = ConvertLatLngToCartesian(latitude1, longitude1, altitude1);
            //Vector3 cartesianCoordinates2 = ConvertLatLngToCartesian(latitude2, longitude2, altitude2);

            double[] loc = CoordinateConverter.GetRelativePosition(longitude1, latitude1, longitude2, latitude2);

            // Calculate the relative Cartesian coordinates
            Vector3 relativeCartesianCoordinates = new Vector3((float)loc[0], (float)loc[1], (float)(altitude2 - altitude1));

            // Return the relative Cartesian coordinates
            return relativeCartesianCoordinates;
        }
        #endregion


        #region tracking

        public void TrackingByImage(string deviceId, RectangleF detection)
        {
            Vector3 delta = AdjustCameraPose(deviceId, detection);
            MoveRelative(deviceId, delta.X, delta.Y, delta.Z);
        }
        public Vector3 AdjustCameraPose(string deviceId, RectangleF detection)
        {
            var cameraInfo = cameras.FirstOrDefault(camera => camera.DeviceId == deviceId);
            if (cameraInfo ==null)
            {
                return new Vector3(0f,0f,0f);
            }
            var status = moveStatus[deviceId];

            return AdjustCameraPoseInternal(detection, cameraInfo, status);
        }
        private Vector3 AdjustCameraPoseInternal(RectangleF detection, CameraInfo cameraInfo, MoveStatus status)
        {
            // 计算水平和垂直偏移量
            float offsetX = (float)(detection.X - cameraInfo.VideoWidth/2) / cameraInfo.VideoWidth * cameraInfo.CCDWidth /2;
            float offsetY = (float)(detection.Y - cameraInfo.VideoHeight/2) / cameraInfo.VideoHeight * cameraInfo.CCDHeight/2;

            // 计算水平旋转角度
            var HorizontalRotationAngle = MathF.Atan2(offsetX, cameraInfo.FocalLength * status.CameraStatus.ZoomPosition) * 180 / MathF.PI;

            // 计算垂直旋转角度
            var VerticalRotationAngle = MathF.Atan2(offsetY, cameraInfo.FocalLength * status.CameraStatus.ZoomPosition) * 180 / MathF.PI;

            // TODO: zoom
            double frac = detection.Width / cameraInfo.VideoWidth;
            float zoom = 0;
            // TODO: bbox 触碰边缘时减小zoom level
            //if (detection.Bottom/(double)cameraInfo.VideoHeight > 0.95 || detection.Top/(double)cameraInfo.VideoHeight<0.05 )
            //{
            //    zool -= 1;
            //}
            if ( frac > 0.2)
            {
                zoom += -0.5f;
            }else if (frac <0.1)
            {
                zoom += 0.5f;
            }
            return new Vector3(HorizontalRotationAngle, VerticalRotationAngle, zoom);
        }
        #endregion

        #region For Image

        internal bool PointToTargetForAnotherPtzDevice(Vector3 movement, string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            if (!PrepareToMove(deviceId))
            {
                return false;
            }
            var move = CalculateMovementForAnotherDevice(movement, deviceId);
            

            var result = MoveAbsolute(deviceId, move.X, move.Y, 1);

            return result;
        }

        private Vector3 CalculateMovementForAnotherDevice(Vector3 movement, string deviceId)
        {
            CameraInfo cameraInfo = cameras.FirstOrDefault(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                return new Vector3();
            }
            // 有一点需要注意：cameraInfo 中原本配置的Home字段是相对大地的，此时可设置为相对固定相机，从而简化计算？需要测试
            var toPan= cameraInfo.HomePanToEast + movement.X;
            // TODO: 由于高度差异造成的tilt微调，暂时无法计算，可能需要估一个，比如观测距离100米远的目标时的一个角度差。
            var toTilt = -cameraInfo.HomeTiltToHorizon + movement.Y;
            var toZoom = movement.Z;

            return new Vector3(toPan, toTilt, toZoom);
        }

        internal bool MoveRelativeByImage(RectangleF bBox, string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            if (!PrepareToMove(deviceId))
            {
                return false;
            }
            var move = CalculateMovementByImage(bBox, deviceId);
            var result = MoveRelative(deviceId, move.X, move.Y, move.Z);

            return result;
        }

        private Vector3 CalculateMovementByImage(RectangleF bBox, string deviceId)
        {

            CameraInfo cameraInfo = cameras.FirstOrDefault(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                return new Vector3();
            }
            var status = moveStatus[deviceId];

            // calculate pan/tilt/zoom
            

            // 计算水平和垂直偏移量
            float offsetX = (float)(bBox.X - cameraInfo.VideoWidth / 2) / cameraInfo.VideoWidth * cameraInfo.CCDWidth / 2;
            float offsetY = (float)(bBox.Y - cameraInfo.VideoHeight / 2) / cameraInfo.VideoHeight * cameraInfo.CCDHeight / 2;

            // 计算水平旋转角度
            var HorizontalRotationAngle = MathF.Atan2(offsetX, cameraInfo.FocalLength * status.CameraStatus.ZoomPosition) * 180 / MathF.PI;

            // 计算垂直旋转角度
            var VerticalRotationAngle = MathF.Atan2(offsetY, cameraInfo.FocalLength * status.CameraStatus.ZoomPosition) * 180 / MathF.PI;


            // 计算相机需要变焦的倍数
            // TODO: how to find a proper zoomlevel
            double frac = bBox.Width / cameraInfo.VideoWidth;
            float zoom = 0;
            if (frac > 0.8)
            {
                zoom += -0.5f;
            }
            else if (frac < 0.6)
            {
                zoom += 0.5f;
            }
            return new Vector3(HorizontalRotationAngle, VerticalRotationAngle, zoom);
        }

        public Vector3 CalculateMovementFixedDevice(RectangleF bBox, string deviceId)
        {
            CameraInfo cameraInfo = cameras.FirstOrDefault(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                return new Vector3();
            }

            // calculate pan/tilt/zoom

            // 计算水平和垂直偏移量
            float offsetX = (float)(bBox.X - cameraInfo.VideoWidth / 2) / cameraInfo.VideoWidth * cameraInfo.CCDWidth / 2;
            float offsetY = (float)(bBox.Y - cameraInfo.VideoHeight / 2) / cameraInfo.VideoHeight * cameraInfo.CCDHeight / 2;

            // 计算水平旋转角度
            var HorizontalRotationAngle = MathF.Atan2(offsetX, cameraInfo.FocalLength) * 180 / MathF.PI;

            // 计算垂直旋转角度
            var VerticalRotationAngle = MathF.Atan2(offsetY, cameraInfo.FocalLength) * 180 / MathF.PI;


            // 计算相机需要变焦的倍数
            // TODO: how to find a proper zoomlevel
            double frac = bBox.Width / cameraInfo.VideoWidth;
            float zoom = 0;
            if (frac > 0.8)
            {
                zoom += -0.5f;
            }
            else if (frac < 0.6)
            {
                zoom += 0.5f;
            }
            return new Vector3(HorizontalRotationAngle, VerticalRotationAngle, zoom);
        }

        #endregion
    }
}
