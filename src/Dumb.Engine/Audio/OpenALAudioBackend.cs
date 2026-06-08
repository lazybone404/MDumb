using Silk.NET.OpenAL;

namespace Dumb.Engine.Audio;

public sealed unsafe class OpenALAudioBackend : IAudioBackend, IDisposable
{
#if BROWSER
    private readonly Dumb.Emscripten.OpenALContext _contextApi = new();
    private readonly Dumb.Emscripten.OpenAL _audioApi = new();
#else
    private readonly ALContext _contextApi = ALContext.GetApi(soft: true);
    private readonly AL _audioApi = AL.GetApi(soft: true);
#endif

    private Device* _device;
    private Context* _context;
    private bool _disposed;

    public OpenALAudioBackend(string? deviceName = null)
    {
        _device = _contextApi.OpenDevice(deviceName);
        if (_device is null)
            throw new InvalidOperationException("Failed to open OpenAL device.");

        _context = _contextApi.CreateContext(_device, null);
        if (_context is null)
            throw new InvalidOperationException("Failed to create OpenAL context.");

        _contextApi.MakeContextCurrent(_context);
    }

    public AudioBuffer CreateBuffer()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
#if BROWSER
        return new AudioBuffer(_audioApi.GenBuffer());
#else
        uint id = 0;
        _audioApi.GenBuffers(1, &id);
        return new AudioBuffer(id);
#endif
    }

    public void DeleteBuffer(AudioBuffer buffer)
    {
        if (buffer.Id == 0)
            return;

#if BROWSER
        _audioApi.DeleteBuffer(buffer.Id);
#else
        var id = buffer.Id;
        _audioApi.DeleteBuffers(1, &id);
#endif
    }

    public void BufferData(AudioBuffer buffer, AudioSampleFormat format, void* data, int size, int sampleRate)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var bufferFormat = format switch
        {
            AudioSampleFormat.Mono8 => BufferFormat.Mono8,
            AudioSampleFormat.Mono16 => BufferFormat.Mono16,
            AudioSampleFormat.Stereo8 => BufferFormat.Stereo8,
            AudioSampleFormat.Stereo16 => BufferFormat.Stereo16,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
        _audioApi.BufferData(buffer.Id, bufferFormat, data, size, sampleRate);
    }

    public AudioSource CreateSource()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
#if BROWSER
        return new AudioSource(_audioApi.GenSource());
#else
        uint id = 0;
        _audioApi.GenSources(1, &id);
        return new AudioSource(id);
#endif
    }

    public void DeleteSource(AudioSource source)
    {
        if (source.Id == 0)
            return;

#if BROWSER
        _audioApi.DeleteSource(source.Id);
#else
        var id = source.Id;
        _audioApi.DeleteSources(1, &id);
#endif
    }

    public void Play(AudioSource source) => _audioApi.SourcePlay(source.Id);

    public void Pause(AudioSource source) => _audioApi.SourcePause(source.Id);

    public void Stop(AudioSource source) => _audioApi.SourceStop(source.Id);

    public void SetBuffer(AudioSource source, AudioBuffer buffer) =>
        _audioApi.SetSourceProperty(source.Id, SourceInteger.Buffer, buffer.Id);

    public void SetLooping(AudioSource source, bool looping) =>
        _audioApi.SetSourceProperty(source.Id, SourceBoolean.Looping, looping);

    public void SetGain(AudioSource source, float gain) =>
        _audioApi.SetSourceProperty(source.Id, SourceFloat.Gain, gain);

    public void SetPitch(AudioSource source, float pitch) =>
        _audioApi.SetSourceProperty(source.Id, SourceFloat.Pitch, pitch);

    public AudioSourceState GetState(AudioSource source)
    {
        _audioApi.GetSourceProperty(source.Id, GetSourceInteger.SourceState, out var state);
        return state switch
        {
            0x1011 => AudioSourceState.Initial,
            0x1012 => AudioSourceState.Playing,
            0x1013 => AudioSourceState.Paused,
            0x1014 => AudioSourceState.Stopped,
            _ => AudioSourceState.Unknown
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_context is not null)
        {
            _contextApi.MakeContextCurrent(null);
            _contextApi.DestroyContext(_context);
            _context = null;
        }

        if (_device is not null)
        {
            _contextApi.CloseDevice(_device);
            _device = null;
        }
    }
}
