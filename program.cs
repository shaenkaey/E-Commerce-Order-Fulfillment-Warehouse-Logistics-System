namespace OrderManagement;

// All storage is private. No built-in collection classes are used.
public sealed class CustomStack<T>
{
    private sealed class Node(T value, Node? next)
    { public T Value = value; public Node? Next = next; }
    private Node? top;
    public int Count { get; private set; }
    public void Push(T value) { top = new Node(value, top); Count++; }
    public T Peek() => top is null ? throw new InvalidOperationException("Stack is empty.") : top.Value;
    public T Pop()
    {
        T value = Peek(); top = top!.Next; Count--; return value;
    }
}

public sealed class CustomQueue<T>
{
    private sealed class Node(T value)
    { public T Value = value; public Node? Next; }
    private Node? head, tail;
    public int Count { get; private set; }
    public void Enqueue(T value)
    {
        var node = new Node(value);
        if (tail is null) head = node; else tail.Next = node;
        tail = node; Count++;
    }
    public T Dequeue()
    {
        if (head is null) throw new InvalidOperationException("Queue is empty.");
        T value = head.Value; head = head.Next;
        if (head is null) tail = null;
        Count--; return value;
    }
    public void Visit(Action<T> action)
    { for (var node = head; node is not null; node = node.Next) action(node.Value); }
}

// Ordered insertion: larger priority wins; ties keep arrival order.
public sealed class CustomPriorityQueue<T>
{
    private sealed class Node(T value, int priority)
    { public T Value = value; public int Priority = priority; public Node? Next; }
    private Node? head;
    public int Count { get; private set; }
    public void Enqueue(T value, int priority)
    {
        var node = new Node(value, priority);
        if (head is null || priority > head.Priority)
        { node.Next = head; head = node; }
        else
        {
            var current = head;
            while (current.Next is not null && current.Next.Priority >= priority)
                current = current.Next;
            node.Next = current.Next; current.Next = node;
        }
        Count++;
    }
    public T Dequeue()
    {
        if (head is null) throw new InvalidOperationException("Priority queue is empty.");
        T value = head.Value; head = head.Next; Count--; return value;
    }
    public void Visit(Action<T> action)
    { for (var node = head; node is not null; node = node.Next) action(node.Value); }
    public void RemoveWhere(Predicate<T> match)
    {
        while (head is not null && match(head.Value)) { head = head.Next; Count--; }
        for (var node = head; node?.Next is not null;)
        {
            if (match(node.Next.Value)) { node.Next = node.Next.Next; Count--; }
            else node = node.Next;
        }
    }
}

public sealed class CustomSet<T> where T : notnull
{
    private T[] items = new T[8];
    public int Count { get; private set; }
    public bool Contains(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        for (int i = 0; i < Count; i++) if (items[i].Equals(value)) return true;
        return false;
    }
    public bool Add(T value)
    {
        if (Contains(value)) return false;
        if (Count == items.Length)
        {
            var larger = new T[items.Length * 2];
            Array.Copy(items, larger, Count); items = larger;
        }
        items[Count++] = value; return true;
    }
    public CustomSet<T> Union(CustomSet<T> other)
    {
        var result = new CustomSet<T>();
        Visit(value => result.Add(value)); other.Visit(value => result.Add(value));
        return result;
    }
    public CustomSet<T> Intersection(CustomSet<T> other)
    {
        var result = new CustomSet<T>();
        Visit(value => { if (other.Contains(value)) result.Add(value); }); return result;
    }
    public CustomSet<T> Difference(CustomSet<T> other)
    {
        var result = new CustomSet<T>();
        Visit(value => { if (!other.Contains(value)) result.Add(value); }); return result;
    }
    public void Visit(Action<T> action)
    { for (int i = 0; i < Count; i++) action(items[i]); }
}

// Hash table with separate chaining: collisions share a linked bucket.
public sealed class CustomMap<TKey, TValue> where TKey : notnull
{
    private sealed class Entry(TKey key, TValue value, Entry? next)
    { public TKey Key = key; public TValue Value = value; public Entry? Next = next; }
    private readonly Entry?[] buckets;
    public int Count { get; private set; }
    public CustomMap(int capacity = 101)
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        buckets = new Entry?[capacity];
    }
    private int Index(TKey key)
    { ArgumentNullException.ThrowIfNull(key); return (int)((uint)key.GetHashCode() % (uint)buckets.Length); }
    public void Put(TKey key, TValue value)
    {
        int index = Index(key);
        for (var entry = buckets[index]; entry is not null; entry = entry.Next)
            if (entry.Key.Equals(key)) { entry.Value = value; return; }
        buckets[index] = new Entry(key, value, buckets[index]); Count++;
    }
    public bool TryGetValue(TKey key, out TValue value)
    {
        for (var entry = buckets[Index(key)]; entry is not null; entry = entry.Next)
            if (entry.Key.Equals(key)) { value = entry.Value; return true; }
        value = default!; return false;
    }
}
