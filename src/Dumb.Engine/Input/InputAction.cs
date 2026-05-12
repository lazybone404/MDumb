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
    float Scale = 1f)
{
    public static InputBinding Key(KeyCode key, float scale = 1f) =>
        new(InputBindingKind.Key, (int)key, Scale: scale);

    public static InputBinding MouseButton(MouseButton button, float scale = 1f) =>
        new(InputBindingKind.MouseButton, (int)button, Scale: scale);

    public static InputBinding GamepadButton(GamepadButton button, int gamepad = 0, float scale = 1f) =>
        new(InputBindingKind.GamepadButton, (int)button, gamepad, scale);

    public static InputBinding GamepadAxis(int axis, int gamepad = 0, float scale = 1f) =>
        new(InputBindingKind.GamepadAxis, axis, gamepad, scale);

    public static InputBinding MousePosition() =>
        new(InputBindingKind.MousePosition, 0);

    public static InputBinding MouseScroll() =>
        new(InputBindingKind.MouseScroll, 0);
}

public sealed class InputAction
{
    private readonly InputSystem _input;
    private readonly List<InputBinding> _bindings = [];

    internal InputAction(InputSystem input, string name)
    {
        _input = input;
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<InputBinding> Bindings => _bindings;

    public InputAction AddBinding(InputBinding binding)
    {
        _bindings.Add(binding);
        return this;
    }

    public bool IsPressed()
    {
        foreach (var binding in _bindings)
        {
            if (ReadButton(binding).IsPressed)
                return true;
        }

        return false;
    }

    public bool WasPressedThisFrame()
    {
        foreach (var binding in _bindings)
        {
            if (ReadButton(binding).WasPressedThisFrame)
                return true;
        }

        return false;
    }

    public bool WasReleasedThisFrame()
    {
        foreach (var binding in _bindings)
        {
            if (ReadButton(binding).WasReleasedThisFrame)
                return true;
        }

        return false;
    }

    public float ReadValue()
    {
        var value = 0f;
        foreach (var binding in _bindings)
            value += ReadFloat(binding);
        return value;
    }

    public Vector2 ReadVector2()
    {
        var value = Vector2.Zero;
        foreach (var binding in _bindings)
            value += ReadVector2(binding);
        return value;
    }

    private ButtonControl ReadButton(InputBinding binding) => binding.Kind switch
    {
        InputBindingKind.Key => _input.Keyboard[(KeyCode)binding.Code],
        InputBindingKind.MouseButton => _input.Mouse.Button((MouseButton)binding.Code),
        InputBindingKind.GamepadButton => _input.Gamepad(binding.Gamepad).Button((GamepadButton)binding.Code),
        InputBindingKind.GamepadAxis => AxisAsButton(binding),
        _ => default
    };

    private ButtonControl AxisAsButton(InputBinding binding)
    {
        var axis = _input.Gamepad(binding.Gamepad).Axis(binding.Code);
        var current = Math.Abs(axis.Value) > 0.5f;
        var previous = Math.Abs(axis.PreviousValue) > 0.5f;
        return new ButtonControl(current, current && !previous, !current && previous);
    }

    private float ReadFloat(InputBinding binding) => binding.Kind switch
    {
        InputBindingKind.Key => _input.Keyboard[(KeyCode)binding.Code].IsPressed ? binding.Scale : 0f,
        InputBindingKind.MouseButton => _input.Mouse.Button((MouseButton)binding.Code).IsPressed ? binding.Scale : 0f,
        InputBindingKind.GamepadButton => _input.Gamepad(binding.Gamepad).Button((GamepadButton)binding.Code).IsPressed ? binding.Scale : 0f,
        InputBindingKind.GamepadAxis => _input.Gamepad(binding.Gamepad).Axis(binding.Code).Value * binding.Scale,
        InputBindingKind.MousePosition => 0f,
        InputBindingKind.MouseScroll => 0f,
        _ => 0f
    };

    private Vector2 ReadVector2(InputBinding binding) => binding.Kind switch
    {
        InputBindingKind.MousePosition => _input.Mouse.Position.Value,
        InputBindingKind.MouseScroll => _input.Mouse.Scroll,
        _ => new Vector2(ReadFloat(binding), 0)
    };
}
