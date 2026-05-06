using Shit.Emscripten;
using Shit.Emscripten.Demo;

const string canvasSelector = "#canvas";
const int width = 640;
const int height = 360;

Console.WriteLine("Shit.Emscripten interactive demo starting.");

Emscripten.ConsoleLog("=== Shit.Emscripten Interactive Demo ===");
Emscripten.SetCanvasElementSize(canvasSelector, width, height);

if (Emscripten.TryGetElementCSSSize(canvasSelector, out var cssW, out var cssH))
    Emscripten.ConsoleLog($"Canvas CSS size: {cssW}x{cssH}");

using var wgpu = new WGPUBrowser();
Emscripten.ConsoleLog($"WGPU_WHOLE_SIZE = {WGPUBrowser.WGPU_WHOLE_SIZE}");

var app = new DemoApp(wgpu, canvasSelector, width, height);
app.Run();
