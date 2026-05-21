using System.Numerics;
using System.Runtime.InteropServices;

namespace Dumb.Graphics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct CameraUniforms
{
    public readonly Matrix4x4 ViewProjection;
    public readonly Matrix4x4 View;
    public readonly Matrix4x4 Projection;
    public readonly Vector3 CameraPosition;
    private readonly float _pad0;
    public readonly Matrix4x4 ViewInverse;
    public readonly Matrix4x4 ProjectionInverse;

    public CameraUniforms(Matrix4x4 viewProjection, Matrix4x4 view, Matrix4x4 projection,
        Vector3 cameraPosition, Matrix4x4 viewInverse, Matrix4x4 projectionInverse)
    {
        ViewProjection = viewProjection;
        View = view;
        Projection = projection;
        CameraPosition = cameraPosition;
        _pad0 = 0;
        ViewInverse = viewInverse;
        ProjectionInverse = projectionInverse;
    }

    public static CameraUniforms From(in Engine.Cameras.Camera camera)
    {
        var view = camera.ViewMatrix;
        var proj = camera.ProjectionMatrix;
        Matrix4x4.Invert(view, out var viewInv);
        Matrix4x4.Invert(proj, out var projInv);
        return new CameraUniforms(view * proj, view, proj, camera.Position, viewInv, projInv);
    }
}
