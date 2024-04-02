using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StreamSentinel.DataStructures
{
    public class ObservableSlideWindow : IObservable<ObjectExpiredEvent>, IObservable<FrameExpiredEvent>
    {
        private readonly ConcurrentBoundedQueue<Frame> _frames;

        private readonly Dictionary<string, HashSet<Frame>> _objectInWhichFrames;
        private readonly ConcurrentDictionary<string, bool> _objectsUnderTracking;

        private readonly List<IObserver<ObjectExpiredEvent>> _objExpiredObservers;
        private readonly List<IObserver<FrameExpiredEvent>> _frmExpiredObservers;

        public ObservableSlideWindow(int windowSize = 100)
        {
            _frames = new ConcurrentBoundedQueue<Frame>(windowSize, CleanupItem);

            _objectInWhichFrames = new Dictionary<string, HashSet<Frame>>();
            _objectsUnderTracking = new ConcurrentDictionary<string, bool>();

            _objExpiredObservers = new List<IObserver<ObjectExpiredEvent>>();
            _frmExpiredObservers = new List<IObserver<FrameExpiredEvent>>();
        }

        #region Observer related
        public IDisposable Subscribe(IObserver<ObjectExpiredEvent> observer)
        {
            if (!_objExpiredObservers.Contains(observer))
                _objExpiredObservers.Add(observer);

            return new Unsubscriber<ObjectExpiredEvent>(_objExpiredObservers, observer);
        }

        public IDisposable Subscribe(IObserver<FrameExpiredEvent> observer)
        {
            if (!_frmExpiredObservers.Contains(observer))
                _frmExpiredObservers.Add(observer);

            return new Unsubscriber<FrameExpiredEvent>(_frmExpiredObservers, observer);
        }

        private class Unsubscriber<T> : IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null)
                {
                    _observers.Remove(_observer);
                }
            }
        }

        private void NotifyObservers(ObjectExpiredEvent cleanUpEvent)
        {
            foreach (IObserver<ObjectExpiredEvent> observer in _objExpiredObservers)
            {
                observer.OnNext(cleanUpEvent);
            }
        }

        private void NotifyObservers(FrameExpiredEvent cleanUpEvent)
        {
            foreach (IObserver<FrameExpiredEvent> observer in _frmExpiredObservers)
            {
                observer.OnNext(cleanUpEvent);
            }
        }
        #endregion

        public void AddNewFrame(Frame newFrame)
        {
            if (newFrame == null)
            {
                return;
            }

            _frames.Enqueue(newFrame);
            AddNewFrameToObjectInWhichFrames(newFrame);
        }

        private void AddNewFrameToObjectInWhichFrames(Frame newFrame)
        {
            if (newFrame.DetectedObjects == null)
            {
                return;
            }

            foreach (DetectedObject detectedObject in newFrame.DetectedObjects)
            {
                if (!_objectInWhichFrames.ContainsKey(detectedObject.Id))
                {
                    // same object may be showed in multiple frame slot
                    _objectInWhichFrames.Add(detectedObject.Id, new HashSet<Frame>());
                }

                HashSet<Frame> framesContainObjId = _objectInWhichFrames[detectedObject.Id];
                framesContainObjId.Add(newFrame);

                AddNewTrackingObjId(detectedObject.Id);
            }
        }

        public List<Frame> GetFramesContainObjectId(string objId)
        {
            if (!_objectInWhichFrames.ContainsKey(objId))
            {
                return new List<Frame>();
            }

            return _objectInWhichFrames[objId].ToList();
        }

        private void CleanupItem(Frame expiredFrame)
        {
            if (expiredFrame == null)
            {
                return;
            }

            RemoveExpiredFrameInObjectFrameSet(expiredFrame);

            // Notify expired objects.
            foreach (var detectedObject in expiredFrame.DetectedObjects)
            {
                int existenceCount = GetExistenceCountByObjId(detectedObject.Id);

                // If the existenceCount = 0, it means that the object with the specified id has not
                // reappeared in this lifecycle, and the id can be deleted directly.
                if (existenceCount == 0)
                {
                    RemoveObjIdFromTracking(detectedObject.Id);

                    _objectInWhichFrames.Remove(detectedObject.Id);
                    NotifyObservers(new ObjectExpiredEvent(detectedObject));   // Notify object expired.
                }
            }

            // Notify expired frame.
            NotifyObservers(new FrameExpiredEvent(expiredFrame));

            expiredFrame.Dispose();
        }

        private void RemoveExpiredFrameInObjectFrameSet(Frame expiredFrame)
        {
            if (expiredFrame.DetectedObjects == null)
            {
                return;
            }

            foreach (DetectedObject detectedObject in expiredFrame.DetectedObjects)
            {
                if (!_objectInWhichFrames.ContainsKey(detectedObject.Id))
                {
                    continue;
                }

                HashSet<Frame> framesContainObjId = _objectInWhichFrames[detectedObject.Id];
                if (framesContainObjId != null)
                {
                    framesContainObjId.Remove(expiredFrame);
                }
            }
        }

        private int GetExistenceCountByObjId(string objId)
        {
            if (!_objectInWhichFrames.ContainsKey(objId))
            {
                return 0;
            }

            HashSet<Frame> framesContainObjId = _objectInWhichFrames[objId];
            if (framesContainObjId == null)
            {
                _objectInWhichFrames.Remove(objId);
                return 0;
            }

            return framesContainObjId.Count;
        }

        private void AddNewTrackingObjId(string objectId)
        {
            if (_objectsUnderTracking.ContainsKey(objectId))
            {
                return;
            }

            _objectsUnderTracking.TryAdd(objectId, true);
        }

        public bool IsObjIdUnderTracking(string objectId)
        {
            return _objectsUnderTracking.ContainsKey(objectId);
        }

        private void RemoveObjIdFromTracking(string objectId)
        {
            if (_objectsUnderTracking.ContainsKey(objectId))
            {
                _objectsUnderTracking.TryRemove(objectId, out var value);
            }
        }
    }
}
