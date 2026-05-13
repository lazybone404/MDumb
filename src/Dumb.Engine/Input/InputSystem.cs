using System.Numerics;
using Dumb.Engine.Window;
using Sia;

namespace Dumb.Engine.Input;

public sealed class InputSystem : SystemBase
{
    private static readonly KeyCode[] KeyCodes = Enum.GetValues<KeyCode>();
    private static readonly MouseButton[] MouseButtons = Enum.GetValues<MouseButton>();
    private static readonly GamepadButton[] GamepadButtons = Enum.GetValues<GamepadButton>();

    public InputSystem() : base(Matchers.Of<WindowInput, WindowRuntime>()) {}

    public override void Execute(World world, IEntityQuery query)
    {
        foreach (var entity in query)
        {
            var input = entity.Get<WindowInput>();
            var runtime = entity.Get<WindowRuntime>();

            input.BeginFrame();
            runtime.Input.Poll(input.Current);
            input.Actions.Update(input.Current, input.Previous);
            
            if (input.Initialized)
                EmitEvents(world, entity, input);
            else
                input.Initialized = true;
        }
    }

    private static void EmitEvents(World world, Entity entity, WindowInput input)
    {
        var current = input.Current;
        var previous = input.Previous;

        foreach (var key in KeyCodes)
        {
            if (key == KeyCode.Unknown) continue;
            var prev = previous.IsKeyPressed(key);
            var curr = current.IsKeyPressed(key);
            if (prev != curr)
                world.Send(entity, new KeyEvent(key, curr));
        }

        foreach (var btn in MouseButtons)
        {
            var prev = previous.IsMousePressed(btn);
            var curr = current.IsMousePressed(btn);
            if (prev != curr)
                world.Send(entity, new MouseButtonEvent(btn, curr));
        }

        if (current.MousePosition != previous.MousePosition)
            world.Send(entity, new MouseMoveEvent(current.MousePosition, current.MousePosition - previous.MousePosition));

        if (current.MouseScroll != Vector2.Zero)
            world.Send(entity, new MouseScrollEvent(current.MouseScroll));

        for (var gp = 0; gp < InputFrame.MaxGamepads; gp++)
        {
            foreach (var btn in GamepadButtons)
            {
                var prev = previous.IsGamepadButtonPressed(gp, btn);
                var curr = current.IsGamepadButtonPressed(gp, btn);
                if (prev != curr)
                    world.Send(entity, new GamepadButtonEvent(gp, btn, curr));
            }

            for (var axis = 0; axis < InputFrame.MaxGamepadAxes; axis++)
            {
                var val = current.ReadGamepadAxis(gp, axis);
                var pval = previous.ReadGamepadAxis(gp, axis);
                if (Math.Abs(val - pval) > 0.0001f)
                    world.Send(entity, new GamepadAxisEvent(gp, axis, val));
            }
        }
    }
}

public sealed class WindowInput
{
    private readonly Dictionary<int, Gamepad> _gamepads = [];

    public InputFrame Current { get; } = new();
    public InputFrame Previous { get; } = new();
    public InputActionMap Actions { get; } = new();

    public Keyboard Keyboard { get; }
    public Mouse Mouse { get; }

    internal bool Initialized { get; set; }

    public WindowInput()
    {
        Keyboard = new Keyboard(this);
        Mouse = new Mouse(this);
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

    public InputAction Action(string name) => Actions.Action(name);

    internal void BeginFrame()
    {
        Previous.CopyFrom(Current);
        Current.Clear();
    }
}
