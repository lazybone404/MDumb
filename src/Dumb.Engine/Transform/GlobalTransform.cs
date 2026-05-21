namespace Dumb.Engine.Transform;

public struct GlobalTransform
{
    public Affine3D Value;
    public Affine3D LocalMatrix;

    public GlobalTransform()
    {
        Value = Affine3D.Identity;
        LocalMatrix = Affine3D.Identity;
    }
}
