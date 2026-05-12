using System.Numerics;
using Dumb.Engine.Window;
using Sia;

namespace Dumb.Engine.Input;

public sealed class InputSystem : SystemBase
{
    private static readonly KeyCode[] KeyCodes = Enum.GetValues<KeyCode>();
    private static readonly MouseButton[] MouseButtons = Enum.GetValues<MouseButton>();
    private static readonly GamepadButton[] GamepadButtons = Enum.GetValues<GamepadButton>();

    private readonly GlfwInputBackend _backend;
    private readonly Dictionary<string, InputAction> _actions = [];
    private readonly Dictionary<int, Gamepad> _gamepads = [];
    private bool _initialized;

    public InputSystem(WindowHost host) : base(Matchers.Any)
    {
        if (host == null) throw new ArgumentNullException(nameof(host));
        _backend = new GlfwInputBackend(host);
        Keyboard = new Keyboard(this);
        Mouse = new Mouse(this);
    }

    internal InputFrame Current { get; } = new();
    internal InputFrame Previous { get; } = new();

    public Keyboard Keyboard { get; }
    public Mouse Mouse { get; }

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

    public override void Execute(World world, IEntityQuery query)
    {
        Previous.CopyFrom(Current);
        Current.Clear();
        _backend.Update(Current);

        if (_initialized)
            EmitEvents(world);
        else
            _initialized = true;
    }

    private void EmitEvents(World world)
    {
        var entity = _backend.WindowEntity;
        if (!entity.IsValid)
            return;

        // Keys
        foreach (var key in KeyCodes)
        {
            if (key == KeyCode.Unknown) continue;
            var prev = Previous.IsKeyPressed(key);
            var curr = Current.IsKeyPressed(key);
            if (prev != curr)
                world.Send(entity, new KeyEvent(key, curr));
        }

        // Mouse buttons
        foreach (var btn in MouseButtons)
        {
            var prev = Previous.IsMousePressed(btn);
            var curr = Current.IsMousePressed(btn);
            if (prev != curr)
                world.Send(entity, new MouseButtonEvent(btn, curr));
        }

        // Mouse move
        if (Current.MousePosition != Previous.MousePosition)
            world.Send(entity, new MouseMoveEvent(Current.MousePosition, Current.MousePosition - Previous.MousePosition));

        // Mouse scroll
        if (Current.MouseScroll != Vector2.Zero)
            world.Send(entity, new MouseScrollEvent(Current.MouseScroll));

        // Gamepads
        for (var gp = 0; gp < 16; gp++)
        {
            foreach (var btn in GamepadButtons)
            {
                var prev = Previous.IsGamepadButtonPressed(gp, btn);
                var curr = Current.IsGamepadButtonPressed(gp, btn);
                if (prev != curr)
                    world.Send(entity, new GamepadButtonEvent(gp, btn, curr));
            }

            for (var axis = 0; axis < 8; axis++)
            {
                var val = Current.ReadGamepadAxis(gp, axis);
                var pval = Previous.ReadGamepadAxis(gp, axis);
                if (Math.Abs(val - pval) > 0.0001f)
                    world.Send(entity, new GamepadAxisEvent(gp, axis, val));
            }
        }
    }
}
