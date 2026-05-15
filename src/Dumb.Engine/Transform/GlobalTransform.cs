namespace Dumb.Engine.Transform;

public struct GlobalTransform
{
    public Affine3D Value;
    internal Affine3D _localMatrix;

    public GlobalTransform(Affine3D value)
    {
        Value = value;
        _localMatrix = Affine3D.Identity;
    }

    public GlobalTransform()
    {
        Value = Affine3D.Identity;
        _localMatrix = Affine3D.Identity;
    }
}
