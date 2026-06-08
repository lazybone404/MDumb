namespace Dumb.Engine.Audio;

public enum AudioSampleFormat
{
    Mono8,
    Mono16,
    Stereo8,
    Stereo16
}

public enum AudioSourceState
{
    Initial,
    Playing,
    Paused,
    Stopped,
    Unknown
}

public readonly record struct AudioBuffer(uint Id);

public readonly record struct AudioSource(uint Id);
