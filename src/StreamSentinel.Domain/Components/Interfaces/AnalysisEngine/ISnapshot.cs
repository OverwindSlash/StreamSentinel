using OpenCvSharp;
using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Components.Interfaces.AnalysisEngine
{
    public interface ISnapshot : IDisposable
    {
        void ProcessSnapshots(Frame frame);

        void AddSceneByFrameId(long frameId, Frame frame);
        public Mat GetSceneByFrameId(long frameId);
        public int GetCachedSceneCount();

        void AddSnapshotOfObjectById(Frame frame);
        void AddSnapshotOfObjectById(string objId, float score, Frame frame, BoundingBox bboxs);
        public SortedList<float, Mat> GetObjectSnapshotsByObjectId(string objId);
        public int GetCachedSnapshotCount();

        Mat TakeSnapshot(Frame frame, BoundingBox bboxs);
    }
}
