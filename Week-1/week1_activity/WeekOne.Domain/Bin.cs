namespace WeekOne.Domain;

public class Bin<T>
{
    private readonly List<T> _items = new();

    public int Count => _items.Count;

    public void Add(T item) => _items.Add(item);

    public bool Contains(T item) => _items.Contains(item);

    public T this[int index] => _items[index];

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public void Insert(int index, T item) => _items.Insert(index,item);
}