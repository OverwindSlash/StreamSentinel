using OpenCvSharp;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Entities.AnalysisEngine;
using System.Collections.Concurrent;
using System.Diagnostics;
using StreamSentinel.Entities.Events;

namespace Snapshot.InMemory
{
    public class InMemorySnapshotService : IAnalysisHandler, IDisposable
    {
        // frameId -> Scene
        private readonly ConcurrentDictionary<long, Mat> _scenesOfFrame;

        // object snapshot list by confidence -> (confidence, objectMat)
        private readonly ConcurrentDictionary<string, SortedList<float, Mat>> _snapshotsOfObject;

        private int _maxObjectSnapshots = 10;

        public InMemorySnapshotService()
        {
            _scenesOfFrame = new ConcurrentDictionary<long, Mat>();
            _snapshotsOfObject = new ConcurrentDictionary<string, SortedList<float, Mat>>();
        }

        public AnalysisResult Analyze(Frame frame)
        {
            AddSceneByFrameId(frame.FrameId, frame.Scene);
            AddSnapshotOfObjectById(frame);

            return new AnalysisResult(true);
        }

        private void AddSceneByFrameId(long frameId, Mat sceneImage)
        {
            if (!_scenesOfFrame.ContainsKey(frameId))
            {
                _scenesOfFrame.TryAdd(frameId, sceneImage);
            }
        }

        private void AddSnapshotOfObjectById(Frame frame)
        {
            foreach (var obj in frame.DetectedObjects)
            {
                Mat objSnapshot = frame.Scene.SubMat(new Rect(obj.X, obj.Y, obj.Width, obj.Height)).Clone();
                //AddSnapshotOfObjectById(obj.Id, obj.Confidence, objSnapshot);
                AddSnapshotOfObjectById(obj.Id, obj.Width, objSnapshot);
            }
        }

        private void AddSnapshotOfObjectById(string id, float confidence, Mat snapshot)
        {
            if (!_snapshotsOfObject.ContainsKey(id))
            {
                _snapshotsOfObject.TryAdd(id, new SortedList<float, Mat>());
            }

            SortedList<float, Mat> snapshotsById = _snapshotsOfObject[id];

            if (!snapshotsById.ContainsKey(confidence))
            {
                snapshotsById.Add(confidence, snapshot);
            }
            else
            {
                snapshotsById[confidence] = snapshot;
            }

            if (snapshotsById.Count > _maxObjectSnapshots)
            {
                for (int i = 0; i < snapshotsById.Count - _maxObjectSnapshots; i++)
                {
                    // remove tail (lowest confidence)
                    snapshotsById.RemoveAt(i);
                }
            }
        }

        public int GetCachedSceneCount()
        {
            return _scenesOfFrame.Count;
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

        public SortedList<float, Mat> GetObjectSnapshotsByObjectId(string id)
        {
            if (!_snapshotsOfObject.ContainsKey(id))
            {
                return new SortedList<float, Mat>();
            }

            return _snapshotsOfObject[id];
        }

        #region Observer Handlers
        void IObserver<ObjectExpiredEvent>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<FrameExpiredEvent>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(FrameExpiredEvent value)
        {
            Task.Run(() =>
            {
                ReleaseSceneByFrameId(value.FrameId);
                //Trace.WriteLine($"Frame:{value.FrameId} snapshot cleaned.");
            });
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
            throw new NotImplementedException();
        }

        void IObserver<ObjectExpiredEvent>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ObjectExpiredEvent value)
        {
            Task.Run(() =>
            {
                ReleaseSnapshotsByObjectId(value.Id);
                Trace.WriteLine($"Object:{value.Id} snapshot cleaned.");
            });
        }

        private void ReleaseSnapshotsByObjectId(string id)
        {
            if (!_snapshotsOfObject.ContainsKey(id))
            {
                return;
            }

            SortedList<float, Mat> snapshots = _snapshotsOfObject[id];

            var highestConfidence = snapshots.Keys.Max();

            string filename = id.Replace(':', '_');
            Mat highestSnapshot = snapshots[highestConfidence];

            if (highestSnapshot.Width > 200 /*&& highestSnapshot.Height > 200*/)
            {
                highestSnapshot.SaveImage($"Snapshots/{filename}.jpg");
            }
            

            foreach (Mat snapshot in snapshots.Values)
            {
                snapshot.Dispose();
            }

            _snapshotsOfObject.TryRemove(id, out var removedSnapshots);
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
