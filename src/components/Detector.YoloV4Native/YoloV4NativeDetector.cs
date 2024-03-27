using Detector.YoloV4Native.Config;
using OpenCvSharp;
using StreamSentinel.Components.Interfaces.ObjectDetector;
using StreamSentinel.Entities.AnalysisEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace Detector.YoloV4Native
{
    public class YoloV4NativeDetector : IObjectDetector
    {
        private const string YoloLibraryWin = @"Library/win/darknet.dll";
        private const string YoloLibraryLinux = @"Library/linux/libdarknet.so";

        private const string CudaPath = @"CUDA_PATH";
        private const string CudnnFileWin = @"bin/cudnn64_8.dll";
        private const string CudnnFileLinux = @"lib64/libcudnn.so.8";

        // For Windows
        private const string YoloLibrary = YoloLibraryWin;
        private const string CudnnFile = CudnnFileWin;

        // For Linux
        // private const string YoloLibrary = YoloLibraryLinux;
        // private const string CudnnFile = CudnnFileLinux;

        public CudaEnvironment CudaEnv { get; private set; }

        private readonly Dictionary<int, string> _objectType = new Dictionary<int, string>();

        public YoloV4NativeDetector()
        {
            CudaEnv = new CudaEnvironment();
        }

        #region DllImport Gpu
        [DllImport(YoloLibrary, EntryPoint = "init", CharSet = CharSet.Ansi)]
        internal static extern int InitializeYolo(string configurationFilename, string weightsFilename, int gpu, int batch_size);

        [DllImport(YoloLibrary, EntryPoint = "detect_image", CharSet = CharSet.Ansi)]
        internal static extern int DetectImage(string filename, ref BboxContainer container);

        [DllImport(YoloLibrary, EntryPoint = "detect_mat_data")]
        internal static extern int DetectMatData(IntPtr pArray, int nSize, ref BboxContainer container, float thresh);

        [DllImport(YoloLibrary, EntryPoint = "detect_mat_ptr")]
        internal static extern int DetectMatPtr(IntPtr pArray, int nSize, ref BboxContainer container, float thresh);

        [DllImport(YoloLibrary, EntryPoint = "set_tracking_thresh")]
        internal static extern int SetTrackingThresh(bool change_history, int frames_story, int max_dist);

        [DllImport(YoloLibrary, EntryPoint = "get_device_count")]
        internal static extern int GetDeviceCount();

        [DllImport(YoloLibrary, EntryPoint = "get_device_name", CharSet = CharSet.Ansi)]
        internal static extern int GetDeviceName(int gpu, StringBuilder deviceName);

        [DllImport(YoloLibrary, EntryPoint = "dispose")]
        internal static extern int DisposeYolo();
        #endregion

        public void PrepareEnv(Dictionary<string, string>? envParam = null)
        {
            int gpuIndex = 0;

            if (envParam != null && envParam.TryGetValue("gpu_index", out var gpuIndexString))
            {
                int.TryParse(gpuIndexString, out gpuIndex);
            }

            CheckCudaEnvironment(gpuIndex);
        }

        private void CheckCudaEnvironment(int gpuIndex)
        {
            var envVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);

            if (!envVariables.Contains(CudaPath))
            {
                throw new NotSupportedException("CUDA_PATH not found.");
            }

            string? cudaPath = Environment.GetEnvironmentVariable(CudaPath);
            if (string.IsNullOrEmpty(cudaPath))
            {
                throw new NotSupportedException("CUDA_PATH value not exist.");
            }

            string cudnnFile = Path.Combine(cudaPath, CudnnFile);
            if (!File.Exists(cudnnFile))
            {
                throw new NotSupportedException("Cudnn file not exist.");
            }

            CudaEnv.CudaExists = true;
            CudaEnv.CudnnExists = true;

            GetVideoCardInfo(gpuIndex);
        }

        private void GetVideoCardInfo(int gpuIndex)
        {
            var deviceCount = GetDeviceCount();
            if (deviceCount == 0)
            {
                throw new NotSupportedException("No graphic device is available");
            }

            if (gpuIndex > (deviceCount - 1))
            {
                throw new IndexOutOfRangeException("Graphic device index is out of range");
            }

            CudaEnv.GraphicDeviceId = gpuIndex;

            var deviceName = new StringBuilder(256);
            GetDeviceName(gpuIndex, deviceName);
            CudaEnv.GraphicDeviceName = deviceName.ToString();
        }

        public void Init(Dictionary<string, string>? initParam = null)
        {
            if (initParam == null)
            {
                throw new ArgumentNullException(nameof(initParam));
            }

            if (!initParam.TryGetValue("config_path", out var configPath))
            {
                throw new ArgumentException("initParam does not contain config_path element.");
            }

            var config = ConfigurationLoader.Load(configPath);

            GetObjectNames(config.NamesFile);

            var result = InitializeYolo(config.ConfigFile, config.WeightsFile,
                CudaEnv.GraphicDeviceId, 1);

            if (result != 1)
            {
                throw new Exception("Yolo v4 native detector initialization failed.");
            }
        }

        private void GetObjectNames(string namesFilename)
        {
            var lines = File.ReadAllLines(namesFilename);
            for (var i = 0; i < lines.Length; i++)
            {
                _objectType.Add(i, lines[i]);
            }
        }

        public int GetClassNumber()
        {
            return _objectType.Count;
        }

        public List<BoundingBox> Detect(Mat image, float thresh)
        {
            var container = new BboxContainer();

            try
            {
                int count = DetectMatPtr(image.CvPtr, image.Width * image.Height, ref container, thresh);

                if (count == -1)
                {
                    throw new NotImplementedException("darknet dll compiled incorrectly");
                }
            }
            catch (Exception)
            {
                return new List<BoundingBox>();
            }

            IEnumerable<BoundingBox> result = Convert(container);

            return result.ToList();
        }

        private IEnumerable<BoundingBox> Convert(BboxContainer container)
        {
            var boundingBoxes = new List<BoundingBox>();

            foreach (var item in container.candidates.Where(o => o.h > 0 || o.w > 0))
            {
                if (!_objectType.TryGetValue((int)item.obj_id, out var objectType))
                {
                    objectType = "Unknown";
                }

                var boundingBox = new BoundingBox()
                {
                    X = (int)item.x,
                    Y = (int)item.y,
                    Height = (int)item.h,
                    Width = (int)item.w,

                    LabelId = (int)item.obj_id,
                    Label = objectType,
                    Confidence = item.prob,

                    TrackingId = item.track_id
                };

                boundingBoxes.Add(boundingBox);
            }

            return boundingBoxes;
        }

        public List<BoundingBox> Detect(byte[] imageData, float thresh = 0.7F)
        {
            var container = new BboxContainer();

            var gcHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            IntPtr pnt = gcHandle.AddrOfPinnedObject();

            try
            {
                int count = DetectMatData(pnt, imageData.Length, ref container, thresh);

                if (count == -1)
                {
                    throw new NotImplementedException("darknet library compiled incorrectly");
                }
            }
            catch (Exception)
            {
                return new List<BoundingBox>();
            }
            finally
            {
                // Free the unmanaged memory.
                gcHandle.Free();
            }

            IEnumerable<BoundingBox> result = Convert(container);

            return result.ToList();
        }

        public List<BoundingBox> Detect(string imageFile, float thresh)
        {
            Mat mat = new Mat(imageFile, ImreadModes.Color);

            return Detect(mat, thresh);
        }

        public void Close()
        {
            Dispose();
        }

        public void CleanupEnv()
        {
            CudaEnv.CudaExists = false;
            CudaEnv.CudnnExists = false;
            CudaEnv.GraphicDeviceId = 0;
            CudaEnv.GraphicDeviceName = string.Empty;
        }

        public int SetTrackingParam(bool change_history, int frames_story, int max_dist)
        {
            return SetTrackingThresh(change_history, frames_story, max_dist);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            DisposeYolo();
        }

        ~YoloV4NativeDetector()
        {
            ReleaseUnmanagedResources();
        }
    }
}
