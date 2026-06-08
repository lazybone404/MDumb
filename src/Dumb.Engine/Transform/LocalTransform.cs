using System.Numerics;
using System.Runtime.CompilerServices;
using Sia;

namespace Dumb.Engine.Transform;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public partial record struct LocalTransform(
    [Sia] Vector3 Position,
    [Sia] Quaternion Rotation,
    [Sia] Vector3 Scale)
{
    public LocalTransform()
        : this(Vector3.Zero, Quaternion.Identity, Vector3.One)
    {
    }

    public LocalTransform(
        Vector3 position,
        Entity? parent)
        : this(position, Quaternion.Identity, Vector3.One)
    {
        Parent = parent;
    }

    public LocalTransform(
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Entity? parent)
        : this(position, rotation, scale)
    {
        Parent = parent;
    }

    [Sia]
    public Entity? Parent
    {
        readonly get => _parent;
        set
        {
            _prevParent = _parent;
            _parent = value;
        }
    }

    public Entity? _parent;
    public Entity? _prevParent;
    public bool _dirty;
}
