namespace Detector.YoloV4Native
{
    public class CudaEnvironment
    {
        // Nvida CUDA Toolkit
        public bool CudaExists { get; set; }

        // Nvida cuDNN for CUDA
        public bool CudnnExists { get; set; }

        // Graphic device id
        public int GraphicDeviceId { get; set; }

        // Graphic device name
        public string GraphicDeviceName { get; set; }
    }
}
