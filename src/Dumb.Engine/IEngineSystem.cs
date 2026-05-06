namespace Shit.Engine;

public interface IEngineSystem
{
    string Name { get; }

    void Tick(TimeSpan deltaTime);
}
