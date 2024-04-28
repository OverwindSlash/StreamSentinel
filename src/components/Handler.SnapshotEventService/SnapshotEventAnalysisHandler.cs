using System.Collections.Concurrent;
using System;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using OpenCvSharp;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using StreamSentinel.Eventbus;
using StreamSentinel.DataStructures;

namespace Handler.SnapshotEventService
{
    public class SnapshotEventAnalysisHandler : IAnalysisHandler, IDisposable
    {
        // frameId -> Scene
        private readonly ConcurrentDictionary<long, Mat> _scenesOfFrame;

        // object snapshot list by factor -> (factor, objectMat)
        private readonly ConcurrentDictionary<string, SortedList<float, Mat>> _snapshotsByConfidence;

        private string _snapshotsDir = "Snapshots";
        private int _maxObjectSnapshots = 10;
        private int _minSnapshotWidth = 40;
        private int _maxSnapshotHeight = 40;
        private string _senderPipeline;
        private string _targetPipeline;
        private string _snapType;
        public string Name => nameof(SnapshotEventAnalysisHandler);

        private IDomainEventPublisher _domainEventPublisher;

        private const float ImageQualityThreshold = 100.0f;
        private readonly ConcurrentBoundedQueue<int> _doneSnapshotList = new ConcurrentBoundedQueue<int>(10);

        public SnapshotEventAnalysisHandler(Dictionary<string, string> preferences)
        {
            _scenesOfFrame = new ConcurrentDictionary<long, Mat>();
            _snapshotsByConfidence = new ConcurrentDictionary<string, SortedList<float, Mat>>();

            _snapshotsDir = preferences["SnapshotsDir"];
            _maxObjectSnapshots = int.Parse(preferences["MaxSnapshots"]);
            _minSnapshotWidth = int.Parse(preferences["MinSnapshotWidth"]);
            _maxSnapshotHeight = int.Parse(preferences["MinSnapshotHeight"]);
            _senderPipeline = preferences["SenderPipeline"];
            _targetPipeline = preferences["TargetPipeline"];
            _snapType = preferences["SnapType"];
            var currentDirectory = Directory.GetCurrentDirectory();
            var combine = Path.Combine(currentDirectory, _snapshotsDir);
            var exists = Directory.Exists(combine);

            if (!exists)
            {
                Directory.CreateDirectory(_snapshotsDir);
            }
        }

        public void SetDomainEventPublisher(IDomainEventPublisher domainEventPublisher)
        {
            _domainEventPublisher = domainEventPublisher;
        }

        public AnalysisResult Analyze(Frame frame)
        {
            var targets = frame.DetectedObjects.Where(p => p.Label.Contains(_snapType));
            if (targets.Count() <= 0)
            {
                return new AnalysisResult(true);
            }

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
                if (!obj.Label.Contains(_snapType))
                {
                    continue;
                }
                if (_doneSnapshotList.Contains(obj.TrackId))
                {
                    continue;
                }
                Mat objSnapshot = frame.Scene.SubMat(new Rect(obj.X, obj.Y, obj.Width, obj.Height)).Clone();
                var f = CalculateFactor(obj);
                AddSnapshotOfObjectById(obj.Id, f, objSnapshot);

                if ( f > ImageQualityThreshold)
                {
                    SendNotification(obj.Id, obj.TrackId);
                    _doneSnapshotList.Enqueue(obj.TrackId);
                }
            }
        }

        private float CalculateFactor(DetectedObject obj)
        {
            // Area as order factor.
            // return obj.Width * obj.Height;
            return obj.Width;
        }

        private void AddSnapshotOfObjectById(string id, float confidence, Mat snapshot)
        {
            if (!_snapshotsByConfidence.ContainsKey(id))
            {
                _snapshotsByConfidence.TryAdd(id, new SortedList<float, Mat>());
            }

            SortedList<float, Mat> snapshotsById = _snapshotsByConfidence[id];
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
            if (!_snapshotsByConfidence.ContainsKey(id))
            {
                return new SortedList<float, Mat>();
            }

            return _snapshotsByConfidence[id];
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
                ReleaseSnapshotsByObjectId(value.Id);
            });
        }

        private void ReleaseSnapshotsByObjectId(string id)
        {
            if (!_snapshotsByConfidence.ContainsKey(id))
            {
                return;
            }

            SortedList<float, Mat> snapshots = _snapshotsByConfidence[id];

            var highestConfidence = snapshots.Keys.Max();
            Mat highestSnapshot = snapshots[highestConfidence];
            SaveBestSnapshot(id, highestSnapshot);

            foreach (Mat snapshot in snapshots.Values)
            {
                snapshot.Dispose();
            }

            _snapshotsByConfidence.TryRemove(id, out var removedSnapshots);
            //if (id.Contains(_snapType))
            //{
            //    SendNotification(id);
            //}
        }

        private void SaveBestSnapshot(string id, Mat highestSnapshot)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddhhmmss");
            string filename = id.Replace(':', '_');
            if (highestSnapshot.Width > _minSnapshotWidth && highestSnapshot.Height > _maxSnapshotHeight)
            {
                highestSnapshot.SaveImage($"{_snapshotsDir}/{_senderPipeline}_{timestamp}_{filename}.jpg");
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

        private void SendNotification(string msg, int trackId)
        {
            var eventArgs =
                new PipelineSnapshotEventArgs($"SnapshotEvent: {msg}", this.Name, _senderPipeline, _targetPipeline);
            eventArgs.TrackId = trackId;
            EventBus.Instance.PublishNotification(eventArgs);
        }
    }
}
