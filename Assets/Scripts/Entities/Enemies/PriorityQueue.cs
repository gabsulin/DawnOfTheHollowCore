using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> heap = new();
    private Dictionary<T, int> itemToIndex = new();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        itemToIndex[item] = heap.Count - 1;
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        T result = heap[0].item;
        itemToIndex.Remove(result);

        int lastIndex = heap.Count - 1;
        if (lastIndex > 0)
        {
            heap[0] = heap[lastIndex];
            itemToIndex[heap[0].item] = 0;
        }
        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
            HeapifyDown(0);

        return result;
    }

    public bool Contains(T item)
    {
        return itemToIndex.ContainsKey(item);
    }

    public void UpdatePriority(T item, float newPriority)
    {
        if (!itemToIndex.TryGetValue(item, out int index))
            return;

        float oldPriority = heap[index].priority;
        heap[index] = (item, newPriority);

        if (newPriority < oldPriority)
            HeapifyUp(index);
        else
            HeapifyDown(index);
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (heap[index].priority >= heap[parentIndex].priority)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < heap.Count && heap[leftChild].priority < heap[smallest].priority)
                smallest = leftChild;

            if (rightChild < heap.Count && heap[rightChild].priority < heap[smallest].priority)
                smallest = rightChild;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;

        itemToIndex[heap[i].item] = i;
        itemToIndex[heap[j].item] = j;
    }
}