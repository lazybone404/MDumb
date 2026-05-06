namespace Dumb.Emscripten;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.OpenAL;

public static unsafe partial class OpenALNative
{
    private const string LibraryName = "__Internal_al";

    #region Context

    [LibraryImport(LibraryName, EntryPoint = "alcOpenDevice")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial Device* OpenDevice(byte* name);

    [LibraryImport(LibraryName, EntryPoint = "alcCloseDevice")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void CloseDevice(Device* device);

    [LibraryImport(LibraryName, EntryPoint = "alcCreateContext")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial Context* CreateContext(Device* device, int* attrlist);

    [LibraryImport(LibraryName, EntryPoint = "alcDestroyContext")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DestroyContext(Context* context);

    [LibraryImport(LibraryName, EntryPoint = "alcMakeContextCurrent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MakeContextCurrent(Context* context);

    #endregion

    #region Buffers

    [LibraryImport(LibraryName, EntryPoint = "alBufferData")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void BufferData(uint buffer, BufferFormat format, void* data, int size, int freq);

    [LibraryImport(LibraryName, EntryPoint = "alDeleteBuffers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DeleteBuffers(int n, uint* buffers);

    [LibraryImport(LibraryName, EntryPoint = "alGenBuffers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GenBuffers(int n, uint* buffers);

    #endregion

    #region Sources

    [LibraryImport(LibraryName, EntryPoint = "alGenSources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GenSources(int n, uint* sources);

    [LibraryImport(LibraryName, EntryPoint = "alDeleteSources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DeleteSources(int n, uint* sources);

    [LibraryImport(LibraryName, EntryPoint = "alSourcePlay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourcePlay(uint source);

    [LibraryImport(LibraryName, EntryPoint = "alSourcePause")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourcePause(uint source);

    [LibraryImport(LibraryName, EntryPoint = "alSourceStop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourceStop(uint source);

    [LibraryImport(LibraryName, EntryPoint = "alSourceRewind")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourceRewind(uint source);

    [LibraryImport(LibraryName, EntryPoint = "alGetSourcei")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSourcei(uint source, GetSourceInteger param, int* value);

    [LibraryImport(LibraryName, EntryPoint = "alGetSourcei")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSourcei(uint source, SourceBoolean param, [MarshalAs(UnmanagedType.I4)] ref bool value);

    [LibraryImport(LibraryName, EntryPoint = "alGetSource3i")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSource3i(uint source, GetSourceInteger param, int* value1, int* value2, int* value3);

    [LibraryImport(LibraryName, EntryPoint = "alGetSourceiv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSourceiv(uint source, GetSourceInteger param, int* values);

    [LibraryImport(LibraryName, EntryPoint = "alGetSourcef")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSourcef(uint source, SourceFloat param, float* value);

    [LibraryImport(LibraryName, EntryPoint = "alGetSource3f")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSource3f(uint source, SourceVector3 param, float* value1, float* value2, float* value3);

    [LibraryImport(LibraryName, EntryPoint = "alGetSourcefv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetSourcefv(uint source, SourceVector3 param, float* values);

    [LibraryImport(LibraryName, EntryPoint = "alSourcei")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Sourcei(uint source, SourceInteger param, int value);

    [LibraryImport(LibraryName, EntryPoint = "alSourcei")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Sourcei(uint source, SourceBoolean param, [MarshalAs(UnmanagedType.I4)] bool value);

    [LibraryImport(LibraryName, EntryPoint = "alSource3i")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Source3i(uint source, SourceInteger param, int value1, int value2, int value3);

    [LibraryImport(LibraryName, EntryPoint = "alSourceiv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Sourceiv(uint source, SourceInteger param, int* value);

    [LibraryImport(LibraryName, EntryPoint = "alSourcef")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Sourcef(uint source, SourceFloat param, float value);

    [LibraryImport(LibraryName, EntryPoint = "alSource3f")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Source3f(uint source, SourceVector3 param, float value1, float value2, float value3);

    [LibraryImport(LibraryName, EntryPoint = "alSourcefv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Sourcefv(uint source, SourceVector3 param, float* value);

    [LibraryImport(LibraryName, EntryPoint = "alSourcePlayv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourcePlayv(int n, uint* sources);

    [LibraryImport(LibraryName, EntryPoint = "alSourceStopv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourceStopv(int n, uint* sources);

    [LibraryImport(LibraryName, EntryPoint = "alSourcePausev")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourcePausev(int n, uint* sources);

    [LibraryImport(LibraryName, EntryPoint = "alSourceRewindv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourceRewindv(int n, uint* sources);

    [LibraryImport(LibraryName, EntryPoint = "alSourceQueueBuffers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourceQueueBuffers(uint source, int nb, uint* buffers);

    [LibraryImport(LibraryName, EntryPoint = "alSourceUnqueueBuffers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SourceUnqueueBuffers(uint source, int nb, uint* buffers);

    #endregion
}
