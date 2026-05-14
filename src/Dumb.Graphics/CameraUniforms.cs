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
    private float _pad0;
    public Matrix4x4 ViewInverse;
    public Matrix4x4 ProjectionInverse;

    public static CameraUniforms From(in Engine.Cameras.Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix;
        Matrix4x4.Invert(view, out var viewInv);
        Matrix4x4.Invert(proj, out var projInv);
        return new CameraUniforms
        {
            ViewProjection = view * proj,
            View = view,
            Projection = proj,
            CameraPosition = camera.Position,
            ViewInverse = viewInv,
            ProjectionInverse = projInv
        };
    }
}
