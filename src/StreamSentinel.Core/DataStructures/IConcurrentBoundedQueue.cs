using System.Threading.Tasks;

namespace StreamSentinel.DataStructures;

public interface IConcurrentBoundedQueue<T>
{
    int Count { get; }
    int MaxCapacity { get; }
    int MaxOccupied { get; }

    void Enqueue(T item);
    T Dequeue();
    Task<T> DequeueAsync();
    void Clear();
}