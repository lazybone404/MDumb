using System.Numerics;

namespace Dumb.Engine.Input;

public readonly record struct ButtonControl(bool IsPressed, bool WasPressedThisFrame, bool WasReleasedThisFrame);

public readonly record struct AxisControl(float Value, float PreviousValue)
{
    public float Delta => Value - PreviousValue;
}

public readonly record struct Vector2Control(Vector2 Value, Vector2 PreviousValue)
{
    public Vector2 Delta => Value - PreviousValue;
}

public sealed class Keyboard
{
    private readonly WindowInput _input;

    internal Keyboard(WindowInput input)
    {
        _input = input;
    }

    public ButtonControl this[KeyCode key] => Key(key);

    public ButtonControl Key(KeyCode key)
    {
        var current = _input.Current.IsKeyPressed(key);
        var previous = _input.Previous.IsKeyPressed(key);
        return new ButtonControl(current, current && !previous, !current && previous);
    }
}

public sealed class Mouse
{
    private readonly WindowInput _input;

    internal Mouse(WindowInput input)
    {
        _input = input;
    }

    public Vector2Control Position => new(_input.Current.MousePosition, _input.Previous.MousePosition);

    public Vector2 Scroll => _input.Current.MouseScroll;

    public ButtonControl LeftButton => Button(MouseButton.Left);

    public ButtonControl RightButton => Button(MouseButton.Right);

    public ButtonControl MiddleButton => Button(MouseButton.Middle);

    public ButtonControl Button(MouseButton button)
    {
        var current = _input.Current.IsMousePressed(button);
        var previous = _input.Previous.IsMousePressed(button);
        return new ButtonControl(current, current && !previous, !current && previous);
    }
}

public sealed class Gamepad
{
    private readonly WindowInput _input;

    internal Gamepad(WindowInput input, int index)
    {
        _input = input;
        Index = index;
    }

    public int Index { get; }

    public AxisControl Axis(int axis) =>
        new(_input.Current.ReadGamepadAxis(Index, axis), _input.Previous.ReadGamepadAxis(Index, axis));

    public ButtonControl Button(GamepadButton button)
    {
        var current = _input.Current.IsGamepadButtonPressed(Index, button);
        var previous = _input.Previous.IsGamepadButtonPressed(Index, button);
        return new ButtonControl(current, current && !previous, !current && previous);
    }
}
