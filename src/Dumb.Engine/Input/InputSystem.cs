namespace Dumb.Engine.Input;

public sealed class InputSystem
{
    private readonly IInputBackend _backend;
    private readonly Dictionary<string, InputAction> _actions = [];
    private readonly Dictionary<int, Gamepad> _gamepads = [];

    public InputSystem(IInputBackend backend)
    {
        _backend = backend;
        Keyboard = new Keyboard(this);
        Mouse = new Mouse(this);
    }

    internal InputFrame Current { get; } = new();

    internal InputFrame Previous { get; } = new();

    public Keyboard Keyboard { get; }

    public Mouse Mouse { get; }

    public void Update()
    {
        Previous.CopyFrom(Current);
        Current.Clear();
        _backend.Update(Current);
    }

    public Gamepad Gamepad(int index = 0)
    {
        if (!_gamepads.TryGetValue(index, out var gamepad))
        {
            gamepad = new Gamepad(this, index);
            _gamepads.Add(index, gamepad);
        }

        return gamepad;
    }

    public InputAction Action(string name)
    {
        if (!_actions.TryGetValue(name, out var action))
        {
            action = new InputAction(this, name);
            _actions.Add(name, action);
        }

        return action;
    }
}
