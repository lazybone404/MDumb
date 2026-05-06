namespace Dumb.Emscripten;

using System.Numerics;
using System.Text;
using Silk.NET.OpenAL;

public class OpenALContext
{
    public unsafe Device* OpenDevice(string? name = null)
    {
        if (string.IsNullOrEmpty(name))
            return OpenALNative.OpenDevice(null);

        var utf8Name = Encoding.UTF8.GetBytes(name + '\0');
        fixed (byte* namePtr = utf8Name)
            return OpenALNative.OpenDevice(namePtr);
    }

    public unsafe void CloseDevice(Device* device)
    {
        OpenALNative.CloseDevice(device);
    }

    public unsafe Context* CreateContext(Device* device, int* attrlist)
    {
        return OpenALNative.CreateContext(device, attrlist);
    }

    public unsafe void DestroyContext(Context* context)
    {
        OpenALNative.DestroyContext(context);
    }

    public unsafe void MakeContextCurrent(Context* context)
    {
        OpenALNative.MakeContextCurrent(context);
    }
}

public class OpenAL
{
    public unsafe void BufferData(uint buffer, BufferFormat format, void* data, int size, int freq)
    {
        OpenALNative.BufferData(buffer, format, data, size, freq);
    }

    public unsafe uint GenBuffer()
    {
        uint result = 0;
        OpenALNative.GenBuffers(1, &result);
        return result;
    }

    public unsafe void GenBuffers(int n, uint* buffers)
    {
        OpenALNative.GenBuffers(n, buffers);
    }

    public unsafe void DeleteBuffer(uint buffer)
    {
        OpenALNative.DeleteBuffers(1, &buffer);
    }

    public unsafe void DeleteBuffers(int n, uint* buffers)
    {
        OpenALNative.DeleteBuffers(n, buffers);
    }

    public unsafe uint GenSource()
    {
        uint result = 0;
        OpenALNative.GenSources(1, &result);
        return result;
    }

    public unsafe void GenSources(int n, uint* sources)
    {
        OpenALNative.GenSources(n, sources);
    }

    public unsafe void DeleteSource(uint source)
    {
        OpenALNative.DeleteSources(1, &source);
    }

    public unsafe void DeleteSources(int n, uint* sources)
    {
        OpenALNative.DeleteSources(n, sources);
    }

    public void SourcePlay(uint source)
    {
        OpenALNative.SourcePlay(source);
    }

    public void SourcePause(uint source)
    {
        OpenALNative.SourcePause(source);
    }

    public void SourceStop(uint source)
    {
        OpenALNative.SourceStop(source);
    }

    public void SourceRewind(uint source)
    {
        OpenALNative.SourceRewind(source);
    }

    public unsafe void SourcePlay(int n, uint* sources)
    {
        OpenALNative.SourcePlayv(n, sources);
    }

    public unsafe void SourcePause(int n, uint* sources)
    {
        OpenALNative.SourcePausev(n, sources);
    }

    public unsafe void SourceStop(int n, uint* sources)
    {
        OpenALNative.SourceStopv(n, sources);
    }

    public unsafe void SourceRewind(int n, uint* sources)
    {
        OpenALNative.SourceRewindv(n, sources);
    }

    public unsafe void SourceQueueBuffers(uint source, int nb, uint* buffers)
    {
        OpenALNative.SourceQueueBuffers(source, nb, buffers);
    }

    public unsafe void SourceUnqueueBuffers(uint source, int nb, uint* buffers)
    {
        OpenALNative.SourceUnqueueBuffers(source, nb, buffers);
    }

    public unsafe void GetSourceProperty(uint source, GetSourceInteger param, out int value)
    {
        fixed (int* ptr = &value)
            OpenALNative.GetSourcei(source, param, ptr);
    }

    public void GetSourceProperty(uint source, SourceBoolean param, out bool value)
    {
        value = false;
        OpenALNative.GetSourcei(source, param, ref value);
    }

    public unsafe void GetSourceProperty(uint source, SourceFloat param, out float value)
    {
        fixed (float* ptr = &value)
            OpenALNative.GetSourcef(source, param, ptr);
    }

    public unsafe void GetSourceProperty(uint source, SourceVector3 param, out Vector3 value)
    {
        fixed (float* x = &value.X, y = &value.Y, z = &value.Z)
            OpenALNative.GetSource3f(source, param, x, y, z);
    }

    public void SetSourceProperty(uint source, SourceFloat param, float value)
    {
        OpenALNative.Sourcef(source, param, value);
    }

    public void SetSourceProperty(uint source, SourceVector3 param, Vector3 value)
    {
        OpenALNative.Source3f(source, param, value.X, value.Y, value.Z);
    }

    public void SetSourceProperty(uint source, SourceBoolean param, bool value)
    {
        OpenALNative.Sourcei(source, param, value);
    }

    public void SetSourceProperty(uint source, SourceInteger param, int value)
    {
        OpenALNative.Sourcei(source, param, value);
    }

    public void SetSourceProperty(uint source, SourceInteger param, uint value)
    {
        OpenALNative.Sourcei(source, param, checked((int)value));
    }
}
