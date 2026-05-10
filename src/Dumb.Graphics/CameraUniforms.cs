using System.Numerics;
using System.Runtime.InteropServices;

namespace Dumb.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct CameraUniforms
{
    public Matrix4x4 ViewProjection;
    public Matrix4x4 View;
    public Matrix4x4 Projection;
    public Vector3 CameraPosition;
    private float _pad;

    public static CameraUniforms From(in Engine.Cameras.Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix;
        return new CameraUniforms
        {
            ViewProjection = view * proj,
            View = view,
            Projection = proj,
            CameraPosition = camera.Position
        };
    }
}
