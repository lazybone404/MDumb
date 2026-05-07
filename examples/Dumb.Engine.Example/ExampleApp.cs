using Dumb.Engine.Audio;
using Dumb.Engine.Input;
using Dumb.Engine.Window;

namespace Dumb.Engine.Example;

public sealed unsafe class ExampleApp : IDisposable
{
    private const int SampleRate = 44_100;
    private const float DroneFrequency = 220f;
    private const float PingFrequency = 880f;

    private readonly GlfwWindow _window;
    private readonly InputSystem _input;
    private readonly AudioSystem _audio;
    private readonly OpenALAudioBackend _audioBackend;
    private readonly InputAction _jump;
    private readonly InputAction _fire;
    private readonly InputAction _horizontal;
    private readonly InputAction _vertical;
    private readonly InputAction _scroll;
    private readonly AudioSource _droneSource;
    private readonly AudioBuffer _droneBuffer;
    private readonly AudioSource _pingSource;
    private readonly AudioBuffer _pingBuffer;
#if BROWSER
    private readonly BrowserDemoRenderer _renderer;
#endif
    private int _frame;
    private float _gain = 0.16f;
    private float _pitch = 1f;
    private float _hitPulse;
    private bool _muted;
    private bool _disposed;

    public ExampleApp(
        int width = 640,
        int height = 360,
        string title = "Dumb.Engine input/audio example",
        string canvasSelector = "#canvas")
    {
        _window = new GlfwWindow(width, height, title, visible: false);
        _input = new InputSystem(new GlfwInputBackend(_window));

        _audioBackend = new OpenALAudioBackend();
        _audio = new AudioSystem(_audioBackend);

        _jump = _input.Action("Jump")
            .AddBinding(InputBinding.Key(KeyCode.Space))
            .AddBinding(InputBinding.GamepadButton(GamepadButton.South));

        _fire = _input.Action("Fire")
            .AddBinding(InputBinding.MouseButton(MouseButton.Left))
            .AddBinding(InputBinding.GamepadButton(GamepadButton.RightShoulder));

        _horizontal = _input.Action("Horizontal")
            .AddBinding(InputBinding.Key(KeyCode.A, scale: -1))
            .AddBinding(InputBinding.Key(KeyCode.D, scale: 1))
            .AddBinding(InputBinding.GamepadAxis(axis: 0));

        _vertical = _input.Action("Vertical")
            .AddBinding(InputBinding.Key(KeyCode.S, scale: -1))
            .AddBinding(InputBinding.Key(KeyCode.W, scale: 1))
            .AddBinding(InputBinding.GamepadAxis(axis: 1, scale: -1));

        _scroll = _input.Action("MouseWheel")
            .AddBinding(InputBinding.MouseScroll());

        _droneSource = _audio.CreateSource();
        _droneBuffer = _audio.CreateBuffer();
        _pingSource = _audio.CreateSource();
        _pingBuffer = _audio.CreateBuffer();

        UploadTone(_audio, _droneBuffer, SampleRate, DroneFrequency, durationSeconds: 1.0f, amplitude: 0.22f);
        UploadTone(_audio, _pingBuffer, SampleRate, PingFrequency, durationSeconds: 0.08f, amplitude: 0.45f);

        _audio.SetBuffer(_droneSource, _droneBuffer);
        _audio.SetGain(_droneSource, _gain);
        _audio.SetLooping(_droneSource, true);
        _audio.Play(_droneSource);

        _audio.SetBuffer(_pingSource, _pingBuffer);
        _audio.SetGain(_pingSource, 0.45f);
        _audio.SetLooping(_pingSource, false);

#if BROWSER
        _renderer = new BrowserDemoRenderer(canvasSelector, width, height);
#endif

        Console.WriteLine("Dumb.Engine input/audio example initialized.");
        Console.WriteLine("Input: Space/South=pulse, mouse left/RB=ping, A/D=audio pitch, W/S=visual axis, wheel=gain, M=mute, R=reset.");
    }

    public bool ShouldClose => _window.ShouldClose;

    public bool EscapeWasPressed => _input.Keyboard[KeyCode.Escape].WasPressedThisFrame;

    public void TickFrame()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _frame++;
        _window.PollEvents();
        _input.Update();

        var jumped = _jump.WasPressedThisFrame();
        var fired = _fire.WasPressedThisFrame();
        var reset = _input.Keyboard[KeyCode.R].WasPressedThisFrame;
        var muteToggled = _input.Keyboard[KeyCode.M].WasPressedThisFrame;

        if (reset)
        {
            _gain = 0.16f;
            _pitch = 1f;
            _muted = false;
            _audio.Play(_droneSource);
            Console.WriteLine("Audio/input state reset.");
        }

        if (muteToggled)
        {
            _muted = !_muted;
            if (_muted)
                _audio.Pause(_droneSource);
            else
                _audio.Play(_droneSource);
            Console.WriteLine(_muted ? "Drone paused." : "Drone resumed.");
        }

        var horizontal = Math.Clamp(_horizontal.ReadValue(), -1f, 1f);
        var vertical = Math.Clamp(_vertical.ReadValue(), -1f, 1f);
        var wheel = _scroll.ReadVector2().Y;
        if (wheel != 0)
            _gain = Math.Clamp(_gain + wheel * 0.04f, 0f, 0.8f);

        _pitch = Math.Clamp(1f + horizontal * 0.65f + vertical * 0.2f, 0.35f, 2.2f);
        _audio.SetPitch(_droneSource, _pitch);
        _audio.SetGain(_droneSource, _muted ? 0f : _gain);

        if (jumped || fired)
        {
            _audio.Stop(_pingSource);
            _audio.SetPitch(_pingSource, fired ? 1.35f : 1f);
            _audio.Play(_pingSource);
            _hitPulse = 1f;
            Console.WriteLine($"{(fired ? "Fire" : "Jump")} pulse at frame {_frame}.");
        }

        _hitPulse = Math.Max(0f, _hitPulse - 0.08f);

#if BROWSER
        _renderer.Render(
            _window.Width,
            _window.Height,
            _input.Mouse.Position.Value,
            horizontal,
            vertical,
            _gain,
            _pitch,
            _hitPulse,
            _muted);
#endif

        if (_frame % 120 == 0)
        {
            var mouse = _input.Mouse.Position.Value;
            var state = _audio.GetState(_droneSource);
            Console.WriteLine(
                $"frame={_frame} mouse=({mouse.X:F0},{mouse.Y:F0}) axis=({horizontal:F2},{vertical:F2}) " +
                $"wheel={wheel:F1} gain={_gain:F2} pitch={_pitch:F2} audio={state}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _audio.Stop(_droneSource);
        _audio.Stop(_pingSource);
        _audio.DeleteSource(_droneSource);
        _audio.DeleteSource(_pingSource);
        _audio.DeleteBuffer(_droneBuffer);
        _audio.DeleteBuffer(_pingBuffer);
        _audioBackend.Dispose();
#if BROWSER
        _renderer.Dispose();
#endif
        _window.Dispose();
    }

    private static void UploadTone(
        AudioSystem audio,
        AudioBuffer buffer,
        int sampleRate,
        float frequency,
        float durationSeconds,
        float amplitude)
    {
        var sampleCount = Math.Max(1, (int)(sampleRate * durationSeconds));
        var samples = new short[sampleCount];
        var phaseStep = Math.Tau * frequency / sampleRate;

        for (var i = 0; i < samples.Length; i++)
        {
            var fade = Math.Min(1.0, Math.Min(i, samples.Length - 1 - i) / (sampleRate * 0.01));
            samples[i] = (short)(Math.Sin(i * phaseStep) * short.MaxValue * amplitude * fade);
        }

        fixed (short* ptr = samples)
        {
            audio.BufferData(
                buffer,
                AudioSampleFormat.Mono16,
                ptr,
                samples.Length * sizeof(short),
                sampleRate);
        }
    }
}
