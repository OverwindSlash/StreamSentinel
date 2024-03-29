using OpenCvSharp;
using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Components.Interfaces.ObjectTracker
{
    public interface IObjectTracker
    {
        void Track(Mat scene, List<DetectedObject> detectedObjects);
    }
}
