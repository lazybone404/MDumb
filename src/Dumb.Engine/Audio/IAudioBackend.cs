namespace Dumb.Engine.Audio;

public unsafe interface IAudioBackend
{
    AudioBuffer CreateBuffer();
    void DeleteBuffer(AudioBuffer buffer);
    void BufferData(AudioBuffer buffer, AudioSampleFormat format, void* data, int size, int sampleRate);

    AudioSource CreateSource();
    void DeleteSource(AudioSource source);
    void Play(AudioSource source);
    void Pause(AudioSource source);
    void Stop(AudioSource source);
    void SetBuffer(AudioSource source, AudioBuffer buffer);
    void SetLooping(AudioSource source, bool looping);
    void SetGain(AudioSource source, float gain);
    void SetPitch(AudioSource source, float pitch);
    AudioSourceState GetState(AudioSource source);
}
