using System.Numerics;

namespace Dumb.Engine.Input;

public sealed class InputFrame
{
    public const int MAX_GAMEPADS = 16;
    public const int MAX_GAMEPAD_AXES = 8;

    private const int KeyOffset = 1;
    private const int KeyCapacity = 512;

    private static readonly int MouseButtonCount = Enum.GetValues<MouseButton>().Length;
    private static readonly int GamepadButtonCount = Enum.GetValues<GamepadButton>().Length;

    private readonly bool[] _keys = new bool[KeyCapacity];
    private readonly bool[] _mouseButtons = new bool[MouseButtonCount];
    private readonly float[,] _gamepadAxes = new float[MAX_GAMEPADS, MAX_GAMEPAD_AXES];
    private readonly bool[,] _gamepadButtons = new bool[MAX_GAMEPADS, GamepadButtonCount];

    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseScroll { get; private set; }

    public bool IsKeyPressed(KeyCode key)
    {
        var index = KeyIndex(key);
        return index >= 0 && _keys[index];
    }

    public bool IsMousePressed(MouseButton button)
    {
        var index = (int)button;
        return index >= 0 && index < _mouseButtons.Length && _mouseButtons[index];
    }

    public float ReadGamepadAxis(int gamepad, int axis) =>
        IsValidGamepad(gamepad) && axis is >= 0 and < MAX_GAMEPAD_AXES
            ? _gamepadAxes[gamepad, axis]
            : 0f;

    public bool IsGamepadButtonPressed(int gamepad, GamepadButton button) =>
        IsValidGamepad(gamepad) && IsValidGamepadButton(button)
            && _gamepadButtons[gamepad, (int)button];

    public void Clear()
    {
        Array.Clear(_keys);
        Array.Clear(_mouseButtons);
        Array.Clear(_gamepadAxes);
        Array.Clear(_gamepadButtons);
        MouseScroll = Vector2.Zero;
    }

    public void CopyFrom(InputFrame source)
    {
        Clear();
        MousePosition = source.MousePosition;
        MouseScroll = source.MouseScroll;

        Array.Copy(source._keys, _keys, _keys.Length);
        Array.Copy(source._mouseButtons, _mouseButtons, _mouseButtons.Length);
        Array.Copy(source._gamepadAxes, _gamepadAxes, _gamepadAxes.Length);
        Array.Copy(source._gamepadButtons, _gamepadButtons, _gamepadButtons.Length);
    }

    public void SetKey(KeyCode key, bool pressed)
    {
        var index = KeyIndex(key);
        if (index >= 0)
            _keys[index] = pressed;
    }

    public void SetMouseButton(MouseButton button, bool pressed)
    {
        var index = (int)button;
        if (index >= 0 && index < _mouseButtons.Length)
            _mouseButtons[index] = pressed;
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
        if (IsValidGamepad(gamepad) && axis is >= 0 and < MAX_GAMEPAD_AXES)
            _gamepadAxes[gamepad, axis] = value;
    }

    public void SetGamepadButton(int gamepad, GamepadButton button, bool pressed)
    {
        if (IsValidGamepad(gamepad) && IsValidGamepadButton(button))
            _gamepadButtons[gamepad, (int)button] = pressed;
    }

    private static int KeyIndex(KeyCode key)
    {
        var index = (int)key + KeyOffset;
        return index is >= 0 and < KeyCapacity ? index : -1;
    }

    private static bool IsValidGamepad(int gamepad)
        => gamepad is >= 0 and < MAX_GAMEPADS;

    private static bool IsValidGamepadButton(GamepadButton button)
        => (int)button >= 0 && (int)button < GamepadButtonCount;
}
