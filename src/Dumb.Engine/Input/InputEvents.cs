using System.Numerics;
using Sia;

namespace Dumb.Engine.Input;

public readonly record struct KeyEvent(KeyCode Key, bool Pressed) : IEvent;
public readonly record struct MouseMoveEvent(ScreenPosition Position, Vector2 NormalizedDelta) : IEvent;
public readonly record struct MouseButtonEvent(MouseButton Button, bool Pressed) : IEvent;
public readonly record struct MouseScrollEvent(Vector2 Delta) : IEvent;
public readonly record struct GamepadButtonEvent(int Gamepad, GamepadButton Button, bool Pressed) : IEvent;
public readonly record struct GamepadAxisEvent(int Gamepad, int Axis, float Value) : IEvent;
