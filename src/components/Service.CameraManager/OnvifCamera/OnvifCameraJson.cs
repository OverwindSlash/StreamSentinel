using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.OnvifCamera
{
    // Define the Result class to match the JSON structure
    internal class RootResult<T>
    {
        public T Result { get; set; }
        public string TargetUrl { get; set; }
        public bool Success { get; set; }
        public object Error { get; set; }
        public bool UnAuthorizedRequest { get; set; }
    }

    internal class ResultData<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
    }

    internal class DeviceItem
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Ipv4Address { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public List<string> Types { get; set; }
        public List<string> Capabilities { get; set; }
        public List<OnivifProfile> Profiles { get; set; }
        public DeviceGroup Group { get; set; }
        public int Id { get; set; }
    }

    internal class OnivifProfile
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public string VideoSourceToken { get; set; }
        public string VideoEncoding { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public float FrameRate { get; set; }
        public string StreamUri { get; set; }
        public string AudioSourceToken { get; set; }
        public string AudioEncoding { get; set; }
        public int AudioBitrate { get; set; }
        public int AudioSampleRate { get; set; }
        public PtzParams PtzParams { get; set; }
        public string DeviceId { get; set; }
    }

    internal class PtzParams
    {
        public float HomePanToNorth { get; set; }
        public float HomeTiltToHorizon { get; set; }
        public float MinPanDegree { get; set; }
        public float MaxPanDegree { get; set; }
        public float MinTiltDegree { get; set; }
        public float MaxTiltDegree { get; set; }
        public float MinZoomLevel { get; set; }
        public float MaxZoomLevel { get; set; }
        public float FocalLength { get; set; }
        public float SensorWidth { get; set; }
        public float SensorHeight { get; set; }
        public float PanDegreeRange { get; set; }
        public float TiltDegreeRange { get; set; }
        public float ZoomLevelRange { get; set; }
        public float MinPanNormal { get; set; }
        public float MaxPanNormal { get; set; }
        public float MinTiltNormal { get; set; }
        public float MaxTiltNormal { get; set; }
        public float MinZoomNormal { get; set; }
        public float MaxZoomNormal { get; set; }
        public float PanNormalRange { get; set; }
        public float TiltNormalRange { get; set; }
        public float ZoomNormalRange { get; set; }
        public string PanDegreePerNormal { get; set; }
        public string TiltDegreePerNormal { get; set; }
        public string ZoomLevelPerNormal { get; set; }
        public int Id { get; set; }
    }

    internal class InstallationData
    {
        public float HomePanToEast {  get; set; }
        public float HomeTiltToHorizon { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Altitude { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public string DeviceId { get; set; }
    }

    internal class VideoSource
    {
        public string Token { get; set; }
        public string StreamUri { get; set; }
        public string ServerStreamUri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public float Framerate { get; set; }
    }
}