namespace Dumb.Engine.Audio;

public sealed unsafe class AudioSystem(IAudioBackend backend)
{
    private readonly IAudioBackend _backend = backend;

    public AudioBuffer CreateBuffer() => _backend.CreateBuffer();

    public void DeleteBuffer(AudioBuffer buffer) => _backend.DeleteBuffer(buffer);

    public void BufferData(AudioBuffer buffer, AudioSampleFormat format, void* data, int size, int sampleRate) =>
        _backend.BufferData(buffer, format, data, size, sampleRate);

    public AudioSource CreateSource() => _backend.CreateSource();

    public void DeleteSource(AudioSource source) => _backend.DeleteSource(source);

    public void Play(AudioSource source) => _backend.Play(source);

    public void Pause(AudioSource source) => _backend.Pause(source);

    public void Stop(AudioSource source) => _backend.Stop(source);

    public void SetBuffer(AudioSource source, AudioBuffer buffer) => _backend.SetBuffer(source, buffer);

    public void SetLooping(AudioSource source, bool looping) => _backend.SetLooping(source, looping);

    public void SetGain(AudioSource source, float gain) => _backend.SetGain(source, gain);

    public void SetPitch(AudioSource source, float pitch) => _backend.SetPitch(source, pitch);

    public AudioSourceState GetState(AudioSource source) => _backend.GetState(source);
}
