using System.Numerics;

namespace Dumb.Engine.Input;

/// <summary>
/// Normalized cursor position (0-1) with explicit origin tag.
/// </summary>
/// <remarks>
/// There are two coordinate conventions in graphics and they cannot be unified:
/// <br/>- <b>TopLeft</b> (Y-down) — all windowing systems (GLFW, Win32, Cocoa, HTML).
/// <br/>- <b>BottomLeft</b> (Y-up) — all GPU pipelines (OpenGL, Vulkan, WebGPU).
/// <br/>Store the raw value with its origin. Convert at the use site via
/// <see cref="Uv"/> (render) or <see cref="Screen"/> (UI) so the direction is
/// always explicit.
/// </remarks>
public readonly record struct ScreenPosition(Vector2 Raw, ScreenOrigin Origin)
{
    public static ScreenPosition TopLeft(Vector2 v) => new(v, ScreenOrigin.TopLeft);
    public static ScreenPosition TopLeft(float x, float y) => new(new(x, y), ScreenOrigin.TopLeft);
    public static ScreenPosition BottomLeft(Vector2 v) => new(v, ScreenOrigin.BottomLeft);
    public static ScreenPosition BottomLeft(float x, float y) => new(new(x, y), ScreenOrigin.BottomLeft);

    /// <summary>
    /// Normalized render-space UV. Y-up, 0 = bottom. Pass this directly to shaders;
    /// the value matches the GPU's NDC / framebuffer convention.
    /// </summary>
    public readonly Vector2 Uv => Origin == ScreenOrigin.TopLeft
        ? new Vector2(Raw.X, 1 - Raw.Y)
        : Raw;

    /// <summary>
    /// Normalized window-space coordinate. Y-down, 0 = top. Use for UI layout,
    /// hit-testing, and any interaction with windowing-system APIs (GLFW, Win32, etc.).
    /// </summary>
    public readonly Vector2 Screen => Origin == ScreenOrigin.BottomLeft
        ? new Vector2(Raw.X, 1 - Raw.Y)
        : Raw;

    /// <summary>Pixel coords in rendering space (Y-up, bottom-left origin).</summary>
    public readonly Vector2 ToRender(int w, int h)
    {
        var uv = Uv;
        return new Vector2(uv.X * w, uv.Y * h);
    }
    public readonly Vector2 ToRender(Vector2 size) => ToRender((int)size.X, (int)size.Y);

    /// <summary>Pixel coords in window space (Y-down, top-left origin).</summary>
    public readonly Vector2 ToWindow(int w, int h)
    {
        var s = Screen;
        return new Vector2(s.X * w, s.Y * h);
    }
    public readonly Vector2 ToWindow(Vector2 size) => ToWindow((int)size.X, (int)size.Y);

    public void Deconstruct(out Vector2 raw, out ScreenOrigin origin)
    {
        raw = Raw;
        origin = Origin;
    }

    public override string ToString() => $"{Raw} ({Origin})";
}

/// <summary>
/// Origin convention for screen-space coordinates. TopLeft = Y-down (windowing
/// systems), BottomLeft = Y-up (GPU pipelines).
/// </summary>
public enum ScreenOrigin
{
    /// <summary>Y-down: (0,0) at top-left, Y increases downward.</summary>
    TopLeft,
    /// <summary>Y-up: (0,0) at bottom-left, Y increases upward.</summary>
    BottomLeft
}
