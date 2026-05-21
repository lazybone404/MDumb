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

public sealed class Keyboard(WindowInput input)
{
    public ButtonControl this[KeyCode key] => Key(key);

    public ButtonControl Key(KeyCode key)
    {
        var current = input.Current.IsKeyPressed(key);
        var previous = input.Previous.IsKeyPressed(key);
        return new ButtonControl(current, current && !previous, !current && previous);
    }
}

public sealed class Mouse(WindowInput input)
{
    public ScreenPosition Position => input.Current.MousePosition;
    public ScreenPosition PreviousPosition => input.Previous.MousePosition;

    /// <summary>Normalized delta in render space (Y-up, 0-1). Moving up = positive Y.</summary>
    public Vector2 NormalizedDelta =>
        input.Current.MousePosition.Uv - input.Previous.MousePosition.Uv;

    public Vector2 Scroll => input.Current.MouseScroll;

    public ButtonControl LeftButton => Button(MouseButton.Left);

    public ButtonControl RightButton => Button(MouseButton.Right);

    public ButtonControl MiddleButton => Button(MouseButton.Middle);

    public ButtonControl Button(MouseButton button)
    {
        var current = input.Current.IsMousePressed(button);
        var previous = input.Previous.IsMousePressed(button);
        return new ButtonControl(current, current && !previous, !current && previous);
    }
}

public sealed class Gamepad(WindowInput input, int index)
{
    public int Index { get; } = index;

    public AxisControl Axis(int axis) =>
        new(input.Current.ReadGamepadAxis(Index, axis), input.Previous.ReadGamepadAxis(Index, axis));

    public ButtonControl Button(GamepadButton button)
    {
        var current = input.Current.IsGamepadButtonPressed(Index, button);
        var previous = input.Previous.IsGamepadButtonPressed(Index, button);
        return new ButtonControl(current, current && !previous, !current && previous);
    }
}
