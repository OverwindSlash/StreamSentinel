using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamSentinel.DataStructures;

public class ConcurrentBoundedQueue<T> : IEnumerable<T>, IConcurrentBoundedQueue<T>
    {
        private const int DefaultMaxCapacity = 100;

        private readonly ConcurrentQueue<T> _queue = new();
        private readonly object _lockObject = new();
        private readonly int _maxCapacity;
        private int _maxOccupied;

        public ConcurrentBoundedQueue(int maxCapacity = DefaultMaxCapacity)
        {
            if (maxCapacity <= 0)
                throw new ArgumentException("Maximum capacity must be greater than zero.");

            _maxCapacity = maxCapacity;
        }

        public int MaxCapacity => _maxCapacity;

        public int MaxOccupied => _maxOccupied;

        public int Count => _queue.Count;

        public void Enqueue(T item)
        {
            if (item == null)
            {
                return;
            }

            _queue.Enqueue(item);

            lock (_lockObject)
            {
                if (_queue.Count > _maxOccupied)
                {
                    _maxOccupied = _queue.Count;
                }

                while (_queue.Count > _maxCapacity && _queue.TryDequeue(out var head))
                {
                    if (head is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        public T Dequeue()
        {
            while (true)
            {
                lock (_lockObject)
                {
                    if (_queue.TryDequeue(out var item))
                        return item;
                }
                Task.Delay(10).Wait(); // Wait for 10ms
            }
        }

        public async Task<T> DequeueAsync()
        {
            // return await Task.Run(() =>
            // {
            //     bool isOk = false;
            //     while (!isOk)
            //     {
            //         isOk = _queue.TryDequeue(out var item);
            //         return item;
            //     }
            //
            //     return default(T);
            // });

            return await Task.Run(() =>
            {
                while (true)
                {
                    lock (_lockObject)
                    {
                        if (_queue.TryDequeue(out var item))
                            return item;
                    }
                    Task.Delay(10).Wait(); // Wait for 10ms
                }
            });
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _queue.Clear();
            }
        }

        #region Enumerator
            public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }