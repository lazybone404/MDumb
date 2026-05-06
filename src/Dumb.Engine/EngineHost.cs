namespace Shit.Engine;

public sealed class EngineHost
{
    private readonly List<IEngineSystem> _systems = [];

    public IReadOnlyList<IEngineSystem> Systems => _systems;

    public EngineHost AddSystem(IEngineSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
        return this;
    }

    public void Tick(TimeSpan deltaTime)
    {
        foreach (var system in _systems)
        {
            system.Tick(deltaTime);
        }
    }
}
