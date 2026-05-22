namespace Dumb.Graphics.Pipeline;

public sealed class BinnedRenderPhase
{
    private readonly SortedDictionary<ulong, List<PhaseItem>> _bins = [];

    public void Add(in PhaseItem item, ulong binKey)
    {
        if (!_bins.TryGetValue(binKey, out var list))
        {
            list = [];
            _bins[binKey] = list;
        }
        list.Add(item);
    }

    public void Clear() => _bins.Clear();

    public int Count
    {
        get
        {
            var c = 0;
            foreach (var (_, items) in _bins)
                c += items.Count;
            return c;
        }
    }

    public IEnumerable<List<PhaseItem>> Bins => _bins.Values;
}

public sealed class SortedRenderPhase
{
    private readonly List<PhaseItem> _items = [];
    private bool _dirty;

    public void Add(in PhaseItem item)
    {
        _items.Add(item);
        _dirty = true;
    }

    public void Clear()
    {
        _items.Clear();
        _dirty = false;
    }

    public void Sort()
    {
        if (!_dirty) return;
        _items.Sort(static (a, b) => a.SortKey.CompareTo(b.SortKey));
        _dirty = false;
    }

    public int Count => _items.Count;

    public ReadOnlySpan<PhaseItem> Items
    {
        get
        {
            Sort();
            return System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_items);
        }
    }
}
