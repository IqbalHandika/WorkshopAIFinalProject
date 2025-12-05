using System.Collections.Generic;

/// Priority Queue sederhana dengan float priority.
/// Elemen dengan priority TERKECIL keluar terlebih dahulu.
public class PriorityQueue<T>
{
    private readonly List<(T item, float prio)> _elements = new List<(T, float)>();

    public int Count => _elements.Count;

    public void Enqueue(T item, float priority)
    {
        _elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < _elements.Count; i++)
        {
            if (_elements[i].prio < _elements[bestIndex].prio)
                bestIndex = i;
        }
        T bestItem = _elements[bestIndex].item;
        _elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
