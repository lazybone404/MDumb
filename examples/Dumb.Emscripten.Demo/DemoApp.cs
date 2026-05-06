using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dumb.Emscripten;

namespace Dumb.Emscripten.Demo;

public class DemoApp
{
    private readonly WGPUBrowser _wgpu;
    private readonly string _canvasSelector;
    private readonly int _width, _height;

    private FractalRenderer _fractal = null!;
    private AudioPlayer _audio = null!;

    private InputState _input;
    private InputState _prevInput;
    private bool _dragging;
    private float _prevMouseX, _prevMouseY;
    private int _frameCount;

    private static DemoApp? _current;

    public DemoApp(WGPUBrowser wgpu, string canvasSelector, int width, int height)
    {
        _wgpu = wgpu;
        _canvasSelector = canvasSelector;
        _width = width;
        _height = height;
    }

    public unsafe void Run()
    {
        _current = this;
        Emscripten.ConsoleLog("=== Card Interactive Demo ===");

        GLFWInput.Init();
        _audio = new AudioPlayer();
        _audio.Init();
        _audio.SetVolume(0.15f); // low initial volume

        _fractal = new FractalRenderer();
        _fractal.Run(_wgpu, _canvasSelector, _width, _height);

        Emscripten.ConsoleLog("[Demo] Starting main loop...");
        Emscripten.RequestAnimationFrameLoop(&MainLoop, null);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static int MainLoop(double timestamp, void* userdata)
    {
        var self = _current!;
        self._frameCount++;

        // Poll GLFW input
        self._prevInput = self._input;
        GLFWInput.Poll(ref self._input);

        // Always call Render — it drives WGPU async init on main thread
        self._fractal.Render();

        // Only process input when fractal is ready
        if (self._fractal.IsReady)
        {
            self.ApplyInput();
        }

        // Log status every 120 frames
        if (self._frameCount % 120 == 0)
        {
            Emscripten.ConsoleLog(
                $"[Demo] Frame {self._frameCount} | " +
                $"center=({self._fractal.CenterX:F4},{self._fractal.CenterY:F4}) " +
                $"zoom={self._fractal.Zoom:F4} iters={self._fractal.MaxIterations}");
        }

        return 1; // keep looping
    }

    private void ApplyInput()
    {
        float mouseX = _input.MouseX;
        float mouseY = _input.MouseY;

        // Mouse drag to pan
        if (_input.MouseLeft)
        {
            if (_prevInput.MouseLeft)
            {
                float dx = (mouseX - _prevMouseX) * _fractal.Zoom * 0.004f;
                float dy = (mouseY - _prevMouseY) * _fractal.Zoom * 0.004f;
                _fractal.CenterX -= dx;
                _fractal.CenterY += dy;
            }
        }

        // Scroll to zoom
        if (_input.ScrollDelta != 0)
        {
            float zoomFactor = _input.ScrollDelta > 0 ? 0.85f : 1.15f;
            _fractal.Zoom = Math.Clamp(_fractal.Zoom * zoomFactor, 0.001f, 100f);
            float hz = 220f + (1f / Math.Max(_fractal.Zoom, 0.1f)) * 440f;
            _audio.SetFrequency(hz);
        }

        // WASD fine pan
        float panSpeed = _fractal.Zoom * 0.02f;
        if (_input.KeyA) _fractal.CenterX -= panSpeed;
        if (_input.KeyD) _fractal.CenterX += panSpeed;
        if (_input.KeyW) _fractal.CenterY += panSpeed;
        if (_input.KeyS) _fractal.CenterY -= panSpeed;

        // Number keys switch waveform + color palette
        if (PressedThisFrame(_input.Key1, _prevInput.Key1)) { SetColorPreset(0); _audio.SetWaveform(0); }
        if (PressedThisFrame(_input.Key2, _prevInput.Key2)) { SetColorPreset(1); _audio.SetWaveform(1); }
        if (PressedThisFrame(_input.Key3, _prevInput.Key3)) { SetColorPreset(2); _audio.SetWaveform(2); }
        if (PressedThisFrame(_input.Key4, _prevInput.Key4)) { SetColorPreset(3); _audio.SetWaveform(3); }

        // J/L cycle iterations
        if (PressedThisFrame(_input.KeyJ, _prevInput.KeyJ))
            _fractal.MaxIterations = Math.Max(10, _fractal.MaxIterations - 10);
        if (PressedThisFrame(_input.KeyL, _prevInput.KeyL))
            _fractal.MaxIterations = Math.Min(500, _fractal.MaxIterations + 10);

        // Mouse right click = more iterations
        if (PressedThisFrame(_input.MouseRight, _prevInput.MouseRight))
            _fractal.MaxIterations = Math.Min(500, _fractal.MaxIterations + 30);

        // Mouse left click (not drag) = reset
        if (!_input.MouseLeft && _prevInput.MouseLeft && !_dragging)
        {
            _fractal.CenterX = -0.5f;
            _fractal.CenterY = 0f;
            _fractal.Zoom = 1f;
            _fractal.MaxIterations = 80;
            _fractal.ColorShift = 0;
            _audio.SetFrequency(440f);
        }

        // Joystick
        if (Math.Abs(_input.JoystickAxisX) > 0.1f)
            _fractal.CenterX += _input.JoystickAxisX * _fractal.Zoom * 0.01f;
        if (Math.Abs(_input.JoystickAxisY) > 0.1f)
            _audio.SetVolume(Math.Clamp(_input.JoystickAxisY * 0.5f + 0.5f, 0f, 1f));
        if (_input.JoystickButton0)
        {
            _fractal.Zoom = 1f;
            _audio.SetFrequency(440f);
        }

        // Track drag state
        _dragging = _input.MouseLeft && _prevInput.MouseLeft;
        _prevMouseX = mouseX;
        _prevMouseY = mouseY;
    }

    private void SetColorPreset(int preset)
    {
        _fractal.ColorShift = preset * 0.25f;
        Emscripten.ConsoleLog($"[Demo] Color preset {preset}, shift={_fractal.ColorShift:F2}");
    }

    private static bool PressedThisFrame(bool current, bool previous) => current && !previous;
}
