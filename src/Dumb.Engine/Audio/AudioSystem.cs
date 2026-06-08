namespace Dumb.Engine.Audio;

public sealed unsafe class AudioSystem(IAudioBackend backend)
{
    public AudioBuffer CreateBuffer() => backend.CreateBuffer();

    public void DeleteBuffer(AudioBuffer buffer) => backend.DeleteBuffer(buffer);

    public void BufferData(AudioBuffer buffer, AudioSampleFormat format, void* data, int size, int sampleRate) =>
        backend.BufferData(buffer, format, data, size, sampleRate);

    public AudioSource CreateSource() => backend.CreateSource();

    public void DeleteSource(AudioSource source) => backend.DeleteSource(source);

    public void Play(AudioSource source) => backend.Play(source);

    public void Pause(AudioSource source) => backend.Pause(source);

    public void Stop(AudioSource source) => backend.Stop(source);

    public void SetBuffer(AudioSource source, AudioBuffer buffer) => backend.SetBuffer(source, buffer);

    public void SetLooping(AudioSource source, bool looping) => backend.SetLooping(source, looping);

    public void SetGain(AudioSource source, float gain) => backend.SetGain(source, gain);

    public void SetPitch(AudioSource source, float pitch) => backend.SetPitch(source, pitch);

    public AudioSourceState GetState(AudioSource source) => backend.GetState(source);
}
