using System.Runtime.InteropServices;

namespace Detector.YoloV4Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BboxContainer
    {
        public const int MaxObjects = 1000;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxObjects)]
        internal BboxT[] candidates;
    }
}
