using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dumb.Emscripten.WGPUGenerator;

[Generator]
public sealed class WgpuSourceGenerator : IIncrementalGenerator
{
    private static readonly object NativeDependencyLock = new();
    private static int s_nativeDependenciesLoaded;

    private const string EmbeddedHeaderResourceName = "Dumb.Emscripten.WGPUGenerator.webgpu.h";
    private const string EmbeddedSilkHeaderResourceName = "Dumb.Emscripten.WGPUGenerator.webgpu-silk.h";
    private const int RTLD_NOW = 2;

    private static readonly DiagnosticDescriptor MissingHeader = new(
        id: "SWGPU001",
        title: "Embedded WebGPU header missing",
        messageFormat: "The embedded WebGPU header resource '{0}' was not found in the generator assembly",
        category: "Dumb.Emscripten.WGPUGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GenerationFailed = new(
        id: "SWGPU003",
        title: "WebGPU binding generation failed",
        messageFormat: "{0}",
        category: "Dumb.Emscripten.WGPUGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static string NativeExtension =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";

    private static Assembly? ResolveClangSharpInterop(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        if (assemblyName.Name != "ClangSharp.Interop")
            return null;
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Dumb.Emscripten.WGPUGenerator.ClangSharp.Interop.dll");
        if (stream == null)
            return null;
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        return Assembly.Load(bytes);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        AppDomain.CurrentDomain.AssemblyResolve += ResolveClangSharpInterop;

        context.RegisterSourceOutput(
            context.CompilationProvider,
            static (context, compilation) =>
            {
                var headerSource = TryGetEmbeddedHeader(EmbeddedHeaderResourceName);
                if (string.IsNullOrWhiteSpace(headerSource))
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingHeader, Location.None, EmbeddedHeaderResourceName));
                    return;
                }

                var silkHeaderSource = TryGetEmbeddedHeader(EmbeddedSilkHeaderResourceName);
                if (string.IsNullOrWhiteSpace(silkHeaderSource))
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingHeader, Location.None, EmbeddedSilkHeaderResourceName));
                    return;
                }

                try
                {
                    EnsureNativeDependencyPath();
                    var header = ClangWgpuHeaderParser.Parse(headerSource!);
                    var silkHeader = ClangWgpuHeaderParser.Parse(silkHeaderSource!);
                    var comparison = WgpuHeaderComparer.Compare(header, silkHeader);
                    var options = new WgpuGenerationOptions();

                    foreach (var @enum in header.Enums)
                        context.AddSource($"{@enum.Name}.g.cs", SourceText.From(WgpuCodeRenderer.RenderEnumFile(@enum, options.Namespace), System.Text.Encoding.UTF8));

                    foreach (var @struct in header.Structs)
                        context.AddSource($"{@struct.Name}.g.cs", SourceText.From(WgpuCodeRenderer.RenderStructFile(@struct, options.Namespace), System.Text.Encoding.UTF8));

                    foreach (var pointerType in WgpuNames.Pointers)
                        context.AddSource($"{pointerType}.g.cs", SourceText.From(WgpuCodeRenderer.RenderOpaquePointerFile(pointerType, options.Namespace), System.Text.Encoding.UTF8));

                    foreach (var callback in header.Callbacks)
                        context.AddSource($"{callback.Name}.g.cs", SourceText.From(WgpuCodeRenderer.RenderCallbackFile(callback, options.Namespace), System.Text.Encoding.UTF8));

                    var availableSilkApi = GetAvailableSilkApi(compilation, header, comparison);
                    context.AddSource("WgpuCast.g.cs", SourceText.From(
                        WgpuCodeRenderer.RenderCastFile(comparison, options.Namespace, availableSilkApi.Types, availableSilkApi.EnumValues),
                        System.Text.Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(GenerationFailed, Location.None, ex.ToString()));
                }
            });
    }

    private static string? TryGetEmbeddedHeader(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);
        if (stream is null)
            return null;
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static AvailableSilkApi GetAvailableSilkApi(Compilation compilation, WgpuHeader header, WgpuHeaderComparison comparison)
    {
        var names = WgpuNames.Pointers
            .Concat(header.Enums.Select(static @enum => @enum.Name))
            .Concat(comparison.Structs.Select(static @struct => @struct.Source.Name))
            .Select(WgpuNames.SilkTypeName);

        var types = new HashSet<string>(StringComparer.Ordinal);
        var enumValues = new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in names)
        {
            var type = compilation.GetTypeByMetadataName("Silk.NET.WebGPU." + name);
            if (type is null)
                continue;

            types.Add(name);

            foreach (var member in type.GetMembers().Where(static member => member.Kind == SymbolKind.Field))
                enumValues.Add(name + "." + member.Name);
        }

        return new AvailableSilkApi(types, enumValues);
    }

    private readonly struct AvailableSilkApi(ISet<string> types, ISet<string> enumValues)
    {
        public ISet<string> Types { get; } = types;

        public ISet<string> EnumValues { get; } = enumValues;
    }

    private readonly struct NativePlatform(string rid, string extension)
    {
        public string Rid { get; } = rid;
        public string Extension { get; } = extension;

        public string Suffix => Rid + Extension;
    }

    private static NativePlatform GetNativePlatform()
    {
        var arch = RuntimeInformation.ProcessArchitecture;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && arch == Architecture.X64)
            return new NativePlatform("win-x64", ".dll");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && arch == Architecture.X64)
            return new NativePlatform("linux-x64", ".so");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && arch == Architecture.X64)
            return new NativePlatform("osx-x64", ".dylib");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && arch == Architecture.Arm64)
            return new NativePlatform("osx-arm64", ".dylib");

        throw new PlatformNotSupportedException(
            $"Unsupported generator host platform. " +
            $"Supported platforms: win-x64, linux-x64, osx-x64, osx-arm64. " +
            $"Current platform: OS={RuntimeInformation.OSDescription}, Architecture={arch}.");
    }

    private static void EnsureNativeDependencyPath()
    {
        if (s_nativeDependenciesLoaded != 0)
            return;

        lock (NativeDependencyLock)
        {
            if (s_nativeDependenciesLoaded != 0)
                return;

            var platform = GetNativePlatform();
            var directory = ExtractNativeDependencies(platform.Suffix, platform.Extension);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Windows.SetDllDirectory(directory))
                {
                    throw new InvalidOperationException(
                        $"Failed to set DLL directory '{directory}'. Win32Error={Marshal.GetLastWin32Error()}");
                }
            }

            LoadNativeDependency(Path.Combine(directory, "libclang" + platform.Extension));
            LoadNativeDependency(Path.Combine(directory, "libClangSharp" + platform.Extension));

            s_nativeDependenciesLoaded = 1;
        }
    }

    private static string ExtractNativeDependencies(string suffix, string ext)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "Dumb.Emscripten.WGPUGenerator",
            Process.GetCurrentProcess().Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(directory);

        ExtractNativeDependency($"Dumb.Emscripten.WGPUGenerator.Native.libclang.{suffix}", Path.Combine(directory, $"libclang{ext}"));
        ExtractNativeDependency($"Dumb.Emscripten.WGPUGenerator.Native.libClangSharp.{suffix}", Path.Combine(directory, $"libClangSharp{ext}"));

        return directory;
    }

    private static void ExtractNativeDependency(string resourceName, string destinationPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var resource = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded native dependency '{resourceName}' was not found.");

        if (File.Exists(destinationPath) && new FileInfo(destinationPath).Length == resource.Length)
            return;

        using var file = File.Create(destinationPath);
        resource.CopyTo(file);
    }

    private static void LoadNativeDependency(string path)
    {
        var handle =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Windows.LoadLibrary(path) :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? MacOS.dlopen(path, RTLD_NOW) :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Linux.Load(path, RTLD_NOW) :
            throw new PlatformNotSupportedException(RuntimeInformation.OSDescription);

        if (handle == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to load native dependency '{path}'.");
    }

    private static class Windows
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);
    }

    private static class MacOS
    {
        [DllImport("libSystem.dylib", EntryPoint = "dlopen")]
        public static extern IntPtr dlopen(string path, int flags);
    }

    private static class Linux
    {
        public static IntPtr Load(string path, int flags)
        {
            try
            {
                return dlopen_libdl_so_2(path, flags);
            }
            catch (DllNotFoundException)
            {
                return dlopen_libdl(path, flags);
            }
        }

        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_libdl_so_2(string path, int flags);

        [DllImport("libdl", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_libdl(string path, int flags);
    }
}
