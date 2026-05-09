using System.Runtime.CompilerServices;

namespace Dumb.Engine.Transform;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct GlobalTransform(Affine3D value)
{
    public Affine3D Value = value;
}
