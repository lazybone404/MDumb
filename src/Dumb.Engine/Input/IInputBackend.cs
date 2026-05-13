namespace Dumb.Engine.Input;

public interface IInputBackend : IDisposable
{
    void Poll(InputFrame frame);
}
