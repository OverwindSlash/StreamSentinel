using OpenCvSharp;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using System.Collections.Concurrent;

namespace Snapshot.InMemory
{
    public class InMemorySnapshot : ISnapshot, IObserver<ObjectExpiredEvent>, IObserver<FrameExpiredEvent>
    {
        // frameId -> Scene
        private readonly ConcurrentDictionary<long, Mat> _scenesOfFrame;

        // object snapshot list by factor -> (factor, objectMat)
        private readonly ConcurrentDictionary<string, SortedList<float, Mat>> _snapshotsByScore;

        private string _snapshotsDir = "Snapshots";
        private int _maxObjectSnapshots = 10;
        private int _minSnapshotWidth = 40;
        private int _maxSnapshotHeight = 40;

        public string Name => nameof(InMemorySnapshot);

        public InMemorySnapshot(Dictionary<string, string> preferences)
        {
            _scenesOfFrame = new ConcurrentDictionary<long, Mat>();
            _snapshotsByScore = new ConcurrentDictionary<string, SortedList<float, Mat>>();

            _snapshotsDir = preferences["SnapshotsDir"];
            _maxObjectSnapshots = int.Parse(preferences["MaxSnapshots"]);
            _minSnapshotWidth = int.Parse(preferences["MinSnapshotWidth"]);
            _maxSnapshotHeight = int.Parse(preferences["MinSnapshotHeight"]);

            var currentDirectory = Directory.GetCurrentDirectory();
            var combine = Path.Combine(currentDirectory, _snapshotsDir);
            var exists = Directory.Exists(combine);

            if (!exists)
            {
                Directory.CreateDirectory(_snapshotsDir);
            }
        }

        public void TakeSnapshot(Frame frame)
        {
            AddSceneByFrameId(frame.FrameId, frame);
            AddSnapshotOfObjectById(frame);
        }

        public void AddSceneByFrameId(long frameId, Frame frame)
        {
            if (!_scenesOfFrame.ContainsKey(frameId))
            {
                _scenesOfFrame.TryAdd(frameId, frame.Scene);
            }
        }

        public Mat GetSceneByFrameId(long frameId)
        {
            if (_scenesOfFrame.ContainsKey(frameId))
            {
                _scenesOfFrame.TryGetValue(frameId, out var scene);
                return scene;
            }

            return new Mat();
        }

        public int GetCachedSceneCount()
        {
            return _scenesOfFrame.Count;
        }

        public void AddSnapshotOfObjectById(Frame frame)
        {
            foreach (var obj in frame.DetectedObjects)
            {
                AddSnapshotOfObjectById(obj.Id, CalculateFactor(obj), frame, obj.Bbox);
            }
        }

        private float CalculateFactor(DetectedObject obj)
        {
            // Area as order factor.
            // return obj.Width * obj.Height;
            return obj.Width;
        }

        public void AddSnapshotOfObjectById(string objId, float score, Frame frame, BoundingBox bboxs)
        {
            if (!_snapshotsByScore.ContainsKey(objId))
            {
                _snapshotsByScore.TryAdd(objId, new SortedList<float, Mat>());
            }

            Mat snapshot = frame.Scene.SubMat(new Rect(bboxs.X, bboxs.Y, bboxs.Width, bboxs.Height)).Clone();

            SortedList<float, Mat> snapshotsById = _snapshotsByScore[objId];
            if (!snapshotsById.ContainsKey(score))
            {
                snapshotsById.Add(score, snapshot);
            }
            else
            {
                snapshotsById[score] = snapshot;
            }

            if (snapshotsById.Count > _maxObjectSnapshots)
            {
                for (int i = 0; i < snapshotsById.Count - _maxObjectSnapshots; i++)
                {
                    // remove tail (lowest score)
                    snapshotsById.RemoveAt(i);
                }
            }
        }

        public SortedList<float, Mat> GetObjectSnapshotsByObjectId(string id)
        {
            if (!_snapshotsByScore.ContainsKey(id))
            {
                return new SortedList<float, Mat>();
            }

            return _snapshotsByScore[id];
        }

        public int GetCachedSnapshotCount()
        {
            return _snapshotsByScore.Count;
        }

        #region Observer Handlers
        void IObserver<ObjectExpiredEvent>.OnCompleted()
        {
            // Do nothing
        }

        void IObserver<FrameExpiredEvent>.OnError(Exception error)
        {
            // Do nothing
        }

        public void OnNext(FrameExpiredEvent value)
        {
            Task.Run(() =>
            {
                ReleaseSceneByFrameId(value.FrameId);
            }).Wait();
        }

        private void ReleaseSceneByFrameId(long frameId)
        {
            if (_scenesOfFrame.ContainsKey(frameId))
            {
                _scenesOfFrame[frameId].Dispose();

                _scenesOfFrame.TryRemove(frameId, out var mat);
            }
        }

        void IObserver<FrameExpiredEvent>.OnCompleted()
        {
            // Do nothing
        }

        void IObserver<ObjectExpiredEvent>.OnError(Exception error)
        {
            // Do nothing
        }

        public void OnNext(ObjectExpiredEvent value)
        {
            Task.Run(() =>
            {
                ReleaseSnapshotsByObjectId(value.Id, false);
                ReleaseSnapshotsByObjectId($"cb_{value.Id}");
            }).Wait();
        }

        private void ReleaseSnapshotsByObjectId(string id, bool saveBeforeRelease = true)
        {
            if (!_snapshotsByScore.ContainsKey(id))
            {
                return;
            }

            SortedList<float, Mat> snapshots = _snapshotsByScore[id];

            if (saveBeforeRelease)
            {
                var highestScore = snapshots.Keys.Max();
                Mat highestSnapshot = snapshots[highestScore];

                SaveBestSnapshot(id, highestSnapshot);
            }

            foreach (Mat snapshot in snapshots.Values)
            {
                snapshot.Dispose();
            }

            _snapshotsByScore.TryRemove(id, out var removedSnapshots);
        }

        private void SaveBestSnapshot(string id, Mat highestSnapshot)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string filename = id.Replace(':', '_');
            if (highestSnapshot.Width > _minSnapshotWidth && highestSnapshot.Height > _maxSnapshotHeight)
            {
                highestSnapshot.SaveImage($"{_snapshotsDir}/{filename}_{timestamp}.jpg");
            }
        }
        #endregion

        public void Dispose()
        {
            foreach (Mat scene in _scenesOfFrame.Values)
            {
                scene.Dispose();
            }
        }
    }
}
