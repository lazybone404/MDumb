using System.Numerics;

namespace Dumb.Engine.Input;

public sealed class InputFrame
{
    private readonly HashSet<KeyCode> _keys = [];
    private readonly HashSet<MouseButton> _mouseButtons = [];
    private readonly Dictionary<(int Gamepad, int Axis), float> _gamepadAxes = [];
    private readonly HashSet<(int Gamepad, GamepadButton Button)> _gamepadButtons = [];

    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseScroll { get; private set; }

    public bool IsKeyPressed(KeyCode key) => _keys.Contains(key);

    public bool IsMousePressed(MouseButton button) => _mouseButtons.Contains(button);

    public float ReadGamepadAxis(int gamepad, int axis) =>
        _gamepadAxes.TryGetValue((gamepad, axis), out var value) ? value : 0f;

    public bool IsGamepadButtonPressed(int gamepad, GamepadButton button) =>
        _gamepadButtons.Contains((gamepad, button));

    public void Clear()
    {
        _keys.Clear();
        _mouseButtons.Clear();
        _gamepadAxes.Clear();
        _gamepadButtons.Clear();
        MouseScroll = Vector2.Zero;
    }

    public void CopyFrom(InputFrame source)
    {
        Clear();
        MousePosition = source.MousePosition;
        MouseScroll = source.MouseScroll;

        foreach (var key in source._keys)
            _keys.Add(key);
        foreach (var button in source._mouseButtons)
            _mouseButtons.Add(button);
        foreach (var axis in source._gamepadAxes)
            _gamepadAxes.Add(axis.Key, axis.Value);
        foreach (var button in source._gamepadButtons)
            _gamepadButtons.Add(button);
    }

    public void SetKey(KeyCode key, bool pressed)
    {
        Set(_keys, key, pressed);
    }

    public void SetMouseButton(MouseButton button, bool pressed)
    {
        Set(_mouseButtons, button, pressed);
    }

    public void SetMousePosition(Vector2 position)
    {
        MousePosition = position;
    }

    public void AddMouseScroll(Vector2 delta)
    {
        MouseScroll += delta;
    }

    public void SetGamepadAxis(int gamepad, int axis, float value)
    {
        _gamepadAxes[(gamepad, axis)] = value;
    }

    public void SetGamepadButton(int gamepad, GamepadButton button, bool pressed)
    {
        Set(_gamepadButtons, (gamepad, button), pressed);
    }

    private static void Set<T>(HashSet<T> set, T value, bool present)
    {
        if (present)
            set.Add(value);
        else
            set.Remove(value);
    }
}
