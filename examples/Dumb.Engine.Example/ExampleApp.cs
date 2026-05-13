using Dumb.Engine.Audio;
using Dumb.Engine.Input;
using Dumb.Engine.Window;
using Sia;

namespace Dumb.Engine.Example;

public sealed unsafe class ExampleApp : IDisposable
{
    private const int SampleRate = 44_100;
    private const float DroneFrequency = 220f;
    private const float PingFrequency = 880f;

    private readonly World _world;
    private readonly Entity _window;
    private readonly SystemStage _stage;
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
    private DemoRenderer? _renderer;
    private int _frame;
    private float _gain = 0.16f;
    private float _pitch = 1f;
    private float _hitPulse;
    private bool _muted;
    private bool _disposed;

    public ExampleApp(
        DemoRenderer? renderer = null,
        int width = 640,
        int height = 360,
        string title = "Dumb.Engine input/audio example")
    {
        _world = new World();
        _window = _world.CreateWindow(new WindowDescriptor
        {
            Width = width,
            Height = height,
            Title = title,
            Visible = true
        });

        _renderer = renderer;
        _stage = SystemChain.Empty
            .Add<WindowSystem>()
            .Add<InputSystem>()
            .CreateStage(_world);

        ref var input = ref _window.Get<WindowInput>();

        _audioBackend = new OpenALAudioBackend();
        _audio = new AudioSystem(_audioBackend);

        _jump = input.Action("Jump")
            .AddBinding(InputBinding.Key(KeyCode.Space))
            .AddBinding(InputBinding.GamepadButton(GamepadButton.South));

        _fire = input.Action("Fire")
            .AddBinding(InputBinding.MouseButton(MouseButton.Left))
            .AddBinding(InputBinding.GamepadButton(GamepadButton.RightShoulder));

        _horizontal = input.Action("Horizontal")
            .AddBinding(InputBinding.Key(KeyCode.A, scale: -1))
            .AddBinding(InputBinding.Key(KeyCode.D, scale: 1))
            .AddBinding(InputBinding.GamepadAxis(axis: 0));

        _vertical = input.Action("Vertical")
            .AddBinding(InputBinding.Key(KeyCode.S, scale: -1))
            .AddBinding(InputBinding.Key(KeyCode.W, scale: 1))
            .AddBinding(InputBinding.GamepadAxis(axis: 1, scale: -1));

        _scroll = input.Action("MouseWheel")
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

        Console.WriteLine("Dumb.Engine input/audio example initialized.");
        Console.WriteLine("Input: Space/South=pulse, mouse left/RB=ping, A/D=audio pitch, W/S=visual axis, wheel=gain, M=mute, R=reset.");
    }

    public Entity Window => _window;

    public void SetRenderer(DemoRenderer renderer)
    {
        _renderer = renderer;
    }

    public bool ShouldClose => _window.Get<WindowState>().ShouldClose;

    public bool EscapeWasPressed => _window.Get<WindowInput>().Keyboard[KeyCode.Escape].WasPressedThisFrame;

    public void TickFrame()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _frame++;
        _stage.Tick();
        ref var window = ref _window.Get<WindowState>();
        ref var input = ref _window.Get<WindowInput>();

        var jumped = _jump.WasPressedThisFrame;
        var fired = _fire.WasPressedThisFrame;
        var reset = input.Keyboard[KeyCode.R].WasPressedThisFrame;
        var muteToggled = input.Keyboard[KeyCode.M].WasPressedThisFrame;

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

        var horizontal = Math.Clamp(_horizontal.Value, -1f, 1f);
        var vertical = Math.Clamp(_vertical.Value, -1f, 1f);
        var wheel = _scroll.Vector2.Y;
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

        _renderer?.Render(
            window.Width,
            window.Height,
            input.Mouse.Position.Value,
            horizontal,
            vertical,
            _gain,
            _pitch,
            _hitPulse,
            _muted);

        if (_frame % 120 == 0)
        {
            var mouse = input.Mouse.Position.Value;
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
        _renderer?.Dispose();
        _stage.Dispose();
        _world.Dispose();
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
