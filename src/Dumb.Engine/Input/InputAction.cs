using System.Numerics;

namespace Dumb.Engine.Input;

public enum InputBindingKind
{
    Key,
    MouseButton,
    GamepadButton,
    GamepadAxis,
    MousePosition,
    MouseScroll
}

public readonly record struct InputBinding(
    InputBindingKind Kind,
    int Code,
    int Gamepad = 0,
    float Scale = 1f,
    float Threshold = 0.5f)
{
    public static InputBinding Key(KeyCode key, float scale = 1f) =>
        new(InputBindingKind.Key, (int)key, Scale: scale);

    public static InputBinding MouseButton(MouseButton button, float scale = 1f) =>
        new(InputBindingKind.MouseButton, (int)button, Scale: scale);

    public static InputBinding GamepadButton(GamepadButton button, int gamepad = 0, float scale = 1f) =>
        new(InputBindingKind.GamepadButton, (int)button, gamepad, scale);

    public static InputBinding GamepadAxis(int axis, int gamepad = 0, float scale = 1f, float threshold = 0.5f) =>
        new(InputBindingKind.GamepadAxis, axis, gamepad, scale, threshold);

    public static InputBinding MousePosition() =>
        new(InputBindingKind.MousePosition, 0);

    public static InputBinding MouseScroll() =>
        new(InputBindingKind.MouseScroll, 0);
}

public sealed class InputAction(string name)
{
    private readonly List<InputBinding> _bindings = [];

    public string Name { get; } = name;

    public IReadOnlyList<InputBinding> Bindings => _bindings;

    public bool IsPressed { get; private set; }
    public bool WasPressedThisFrame { get; private set; }
    public bool WasReleasedThisFrame { get; private set; }
    public float Value { get; private set; }
    public Vector2 Vector2 { get; private set; }

    public InputAction AddBinding(InputBinding binding)
    {
        _bindings.Add(binding);
        return this;
    }

    public void Update(InputFrame current, InputFrame previous)
    {
        var pressed = false;
        var wasPressed = false;
        var wasReleased = false;
        var value = 0f;
        var vector = Vector2.Zero;

        foreach (var binding in _bindings)
        {
            var button = ReadButton(binding, current, previous);
            pressed |= button.IsPressed;
            wasPressed |= button.WasPressedThisFrame;
            wasReleased |= button.WasReleasedThisFrame;

            value += ReadFloat(binding, current);
            vector += ReadVector2(binding, current);
        }

        IsPressed = pressed;
        WasPressedThisFrame = wasPressed;
        WasReleasedThisFrame = wasReleased;
        Value = value;
        Vector2 = vector;
    }

    private static ButtonControl ReadButton(InputBinding binding, InputFrame currentFrame, InputFrame previousFrame)
        => binding.Kind switch
    {
        InputBindingKind.Key => Button(
            currentFrame.IsKeyPressed((KeyCode)binding.Code),
            previousFrame.IsKeyPressed((KeyCode)binding.Code)),
        InputBindingKind.MouseButton => Button(
            currentFrame.IsMousePressed((MouseButton)binding.Code),
            previousFrame.IsMousePressed((MouseButton)binding.Code)),
        InputBindingKind.GamepadButton => Button(
            currentFrame.IsGamepadButtonPressed(binding.Gamepad, (GamepadButton)binding.Code),
            previousFrame.IsGamepadButtonPressed(binding.Gamepad, (GamepadButton)binding.Code)),
        InputBindingKind.GamepadAxis => AxisAsButton(binding, currentFrame, previousFrame),
        _ => default
    };

    private static ButtonControl AxisAsButton(InputBinding binding, InputFrame currentFrame, InputFrame previousFrame)
    {
        var currentValue = currentFrame.ReadGamepadAxis(binding.Gamepad, binding.Code);
        var previousValue = previousFrame.ReadGamepadAxis(binding.Gamepad, binding.Code);
        var current = AxisPressed(currentValue, binding);
        var previous = AxisPressed(previousValue, binding);
        return Button(current, previous);
    }

    private static bool AxisPressed(float value, InputBinding binding)
        => binding.Scale >= 0 ? value >= binding.Threshold : value <= -binding.Threshold;

    private static ButtonControl Button(bool current, bool previous)
        => new(current, current && !previous, !current && previous);

    private static float ReadFloat(InputBinding binding, InputFrame currentFrame) => binding.Kind switch
    {
        InputBindingKind.Key => currentFrame.IsKeyPressed((KeyCode)binding.Code) ? binding.Scale : 0f,
        InputBindingKind.MouseButton => currentFrame.IsMousePressed((MouseButton)binding.Code) ? binding.Scale : 0f,
        InputBindingKind.GamepadButton => currentFrame.IsGamepadButtonPressed(binding.Gamepad, (GamepadButton)binding.Code) ? binding.Scale : 0f,
        InputBindingKind.GamepadAxis => currentFrame.ReadGamepadAxis(binding.Gamepad, binding.Code) * binding.Scale,
        InputBindingKind.MousePosition => 0f,
        InputBindingKind.MouseScroll => currentFrame.MouseScroll.Length(),
        _ => 0f
    };

    private static Vector2 ReadVector2(InputBinding binding, InputFrame currentFrame) => binding.Kind switch
    {
        InputBindingKind.MousePosition => currentFrame.MousePosition.Uv,
        InputBindingKind.MouseScroll => currentFrame.MouseScroll,
        _ => new Vector2(ReadFloat(binding, currentFrame), 0)
    };
}

public sealed class InputActionMap
{
    private readonly Dictionary<string, InputAction> _actions = [];

    public IReadOnlyDictionary<string, InputAction> Actions => _actions;

    public InputAction Action(string name)
    {
        if (!_actions.TryGetValue(name, out var action))
        {
            action = new InputAction(name);
            _actions.Add(name, action);
        }
        return action;
    }

    public void Update(InputFrame current, InputFrame previous)
    {
        foreach (var action in _actions.Values)
            action.Update(current, previous);
    }
}
