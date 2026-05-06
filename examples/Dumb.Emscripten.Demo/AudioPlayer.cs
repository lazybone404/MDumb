using System.Runtime.InteropServices;
using Shit.Emscripten;
using Silk.NET.OpenAL;

namespace Shit.Emscripten.Demo;

public unsafe class AudioPlayer : IDisposable
{
    private readonly OpenALContext _alc = new();
    private readonly OpenAL _al = new();

    private Device* _device;
    private Context* _context;
    private uint _source;
    private uint _buffer;
    private int _sampleRate;
    private int _bufferSamples;
    private float _baseFrequency = 440f;
    private int _waveform;
    private bool _disposed;

    public void Init()
    {
        Emscripten.ConsoleLog("[OpenAL] Opening default device...");
        _device = _alc.OpenDevice("");
        if (_device == null)
        {
            Emscripten.ConsoleLog("[OpenAL] ERROR: Failed to open device!");
            return;
        }
        Emscripten.ConsoleLog($"[OpenAL] Device opened: 0x{(nint)_device:X}");

        _context = _alc.CreateContext(_device, null);
        if (_context == null)
        {
            Emscripten.ConsoleLog("[OpenAL] ERROR: Failed to create context!");
            return;
        }
        _alc.MakeContextCurrent(_context);
        Emscripten.ConsoleLog("[OpenAL] Context created and made current.");

        _sampleRate = 44100;
        _bufferSamples = _sampleRate / 4; // 0.25s loop

        _source = _al.GenSource();
        _buffer = _al.GenBuffer();
        Emscripten.ConsoleLog($"[OpenAL] Source={_source}, Buffer={_buffer}");

        GenerateBuffer(440f, 0); // sine wave
        _al.SetSourceProperty(_source, SourceBoolean.Looping, true);
        _al.SetSourceProperty(_source, SourceFloat.Gain, 0.3f);

        _al.SourcePlay(_source);
        Emscripten.ConsoleLog("[OpenAL] Playback started (440Hz sine, looping).");
    }

    private void GenerateBuffer(float frequency, int waveformType)
    {
        short[] samples = new short[_bufferSamples];
        double phase = 0;
        double phaseInc = (double)frequency / _sampleRate * 2.0 * Math.PI;

        for (int i = 0; i < _bufferSamples; i++)
        {
            double value = waveformType switch
            {
                0 => Math.Sin(phase),                                    // sine
                1 => Math.Sin(phase) >= 0 ? 1.0 : -0.8,                  // square
                2 => 2.0 * ((phase / (2.0 * Math.PI)) % 1.0) - 1.0,     // saw
                3 => 2.0 * Math.Abs(2.0 * ((phase / (2.0 * Math.PI)) % 1.0) - 1.0) - 1.0, // triangle
                _ => Math.Sin(phase)
            };
            samples[i] = (short)(value * 0.8 * short.MaxValue);
            phase += phaseInc;
        }

        fixed (short* ptr = samples)
        {
            _al.BufferData(_buffer, BufferFormat.Mono16, ptr, _bufferSamples * sizeof(short), _sampleRate);
        }
        _baseFrequency = frequency;
    }

    public void SetFrequency(float hz)
    {
        if (_source == 0) return;
        float pitch = Math.Clamp(hz / _baseFrequency, 0.25f, 4.0f);
        _al.SetSourceProperty(_source, SourceFloat.Pitch, pitch);
    }

    public void SetVolume(float v)
    {
        if (_source == 0) return;
        _al.SetSourceProperty(_source, SourceFloat.Gain, Math.Clamp(v, 0f, 1f));
    }

    public void SetWaveform(int type)
    {
        if (_source == 0 || type == _waveform) return;
        _waveform = Math.Clamp(type, 0, 3);
        _al.SourceStop(_source);
        GenerateBuffer(_baseFrequency, _waveform);
        _al.SourcePlay(_source);

        string name = _waveform switch { 0 => "sine", 1 => "square", 2 => "saw", 3 => "triangle", _ => "?" };
        Emscripten.ConsoleLog($"[OpenAL] Waveform switched to {name}");
    }

    public bool IsPlaying
    {
        get
        {
            if (_source == 0) return false;
            _al.GetSourceProperty(_source, GetSourceInteger.SourceState, out int state);
            return state == 0x1012; // AL_PLAYING
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_source != 0)
        {
            _al.SourceStop(_source);
            _al.DeleteSource(_source);
        }
        if (_buffer != 0)
            _al.DeleteBuffer(_buffer);
        if (_context != null)
        {
            _alc.MakeContextCurrent(null);
            _alc.DestroyContext(_context);
        }
        if (_device != null)
            _alc.CloseDevice(_device);

        Emscripten.ConsoleLog("[OpenAL] Shutdown complete.");
    }
}
