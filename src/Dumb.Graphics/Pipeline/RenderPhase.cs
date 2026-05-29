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
