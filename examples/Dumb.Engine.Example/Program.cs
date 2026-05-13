#if BROWSER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dumb.Engine.Window;
using EmscriptenApi = Dumb.Emscripten.Emscripten;
#else
using Dumb.Engine.Window;
#endif

namespace Dumb.Engine.Example;

public static unsafe class Program
{
#if BROWSER
    private const string CanvasSelector = "#canvas";
    private const int CanvasWidth = 640;
    private const int CanvasHeight = 360;
    private static ExampleApp _app = null!;
#endif

    public static void Main()
    {
#if BROWSER
        EmscriptenApi.SetCanvasElementSize(CanvasSelector, CanvasWidth, CanvasHeight);
        if (EmscriptenApi.TryGetElementCSSSize(CanvasSelector, out var cssWidth, out var cssHeight))
            EmscriptenApi.ConsoleLog($"[EngineExample] Canvas CSS size: {cssWidth}x{cssHeight}");

        _app = new ExampleApp(
            new DemoRenderer(CanvasSelector, CanvasWidth, CanvasHeight),
            CanvasWidth,
            CanvasHeight,
            "Dumb.Engine browser input/audio example");

        EmscriptenApi.RequestAnimationFrameLoop(&Tick, null);
#else
        using var app = new ExampleApp(
            title: "Dumb.Engine native input/audio example");

        // Create desktop renderer after window is ready
        var window = app.Window;
        var runtime = window.Get<WindowRuntime>();
        using var renderer = new DemoRenderer(runtime, 640, 360);
        app.SetRenderer(renderer);

        while (!app.ShouldClose)
        {
            app.TickFrame();
            if (app.EscapeWasPressed)
                break;

            Thread.Sleep(1);
        }
#endif
    }

#if BROWSER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Tick(double timestamp, void* userData)
    {
        _app.TickFrame();
        if (_app.ShouldClose || _app.EscapeWasPressed)
        {
            _app.Dispose();
            return 0;
        }

        return 1;
    }
#endif
}
