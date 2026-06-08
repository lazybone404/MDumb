using System.Runtime.CompilerServices;
using Sia;
using Sia.Reactors;

namespace Dumb.Engine.Transform;

public class TransformReactor : ReactorBase<TypeUnion<LocalTransform>>
{
    private readonly Dictionary<Entity, HashSet<Entity>> _children = [];
    private readonly Stack<HashSet<Entity>> _childrenPool = new();
    private readonly Queue<Entity> _propagateQueue = [];

    public override void OnInitialize(World world)
    {
        base.OnInitialize(world);

        Listen((Entity entity, in LocalTransform.SetPosition cmd) => {
            entity.Get<LocalTransform>()._dirty = true;
            PropagateGlobal(entity);
        });
        Listen((Entity entity, in LocalTransform.SetRotation cmd) => {
            entity.Get<LocalTransform>()._dirty = true;
            PropagateGlobal(entity);
        });
        Listen((Entity entity, in LocalTransform.SetScale cmd) => {
            entity.Get<LocalTransform>()._dirty = true;
            PropagateGlobal(entity);
        });

        Listen((Entity entity, in LocalTransform.SetParent cmd) => {
            ref var lt = ref entity.Get<LocalTransform>();

            var parent = lt.Parent;
            var prevParent = lt._prevParent;

            if (parent is { Host: not null } && WouldCreateCycle(entity, parent))
            {
                lt._parent = prevParent;
                lt._prevParent = prevParent;
                throw new InvalidOperationException("Setting this parent would create a transform cycle.");
            }

            if (prevParent == parent)
            {
                lt._dirty = true;
                PropagateGlobal(entity);
                return;
            }

            if (prevParent is { Host: not null })
                RemoveChild(prevParent, entity);

            if (parent is { Host: not null })
                AddChild(parent, entity);

            lt._prevParent = parent;
            lt._dirty = true;
            PropagateGlobal(entity);
        });
    }

    protected override void OnEntityAdded(Entity entity)
    {
        ref var lt = ref entity.Get<LocalTransform>();

        if (!entity.Contains<GlobalTransform>())
            entity.Add(new GlobalTransform());

        var localMatrix = Affine3D.FromTRS(lt.Position, lt.Rotation, lt.Scale);
        ref var gt = ref entity.Get<GlobalTransform>();
        gt.LocalMatrix = localMatrix;

        var parent = lt.Parent;
        if (parent is { Host: not null })
        {
            AddChild(parent, entity);
            ref var parentGt = ref parent.Get<GlobalTransform>();
            gt.Value = localMatrix * parentGt.Value;
        }
        else
        {
            gt.Value = localMatrix;
        }
    }

    protected override void OnEntityRemoved(Entity entity)
    {
        ref var lt = ref entity.Get<LocalTransform>();
        var parent = lt.Parent;
        if (parent is { Host: not null })
            RemoveChild(parent, entity);

        if (_children.TryGetValue(entity, out var children))
        {
            var snapshot = children.ToArray();
            children.Clear();
            _childrenPool.Push(children);
            _children.Remove(entity);

            foreach (var child in snapshot)
                child.Destroy();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddChild(Entity parent, Entity child)
    {
        if (!_children.TryGetValue(parent, out var children))
        {
            children = _childrenPool.TryPop(out var pooled) ? pooled : [];
            _children[parent] = children;
        }
        children.Add(child);
    }

    private void RemoveChild(Entity parent, Entity child)
    {
        if (!_children.TryGetValue(parent, out var children))
            return;

        children.Remove(child);
        if (children.Count == 0)
        {
            _childrenPool.Push(children);
            _children.Remove(parent);
        }
    }

    private void PropagateGlobal(Entity root)
    {
        _propagateQueue.Enqueue(root);

        while (_propagateQueue.TryDequeue(out var entity))
        {
            var changed = ComputeGlobal(entity);
            if (_children.TryGetValue(entity, out var children))
            {
                foreach (var child in children)
                {
                    ref var childLt = ref child.Get<LocalTransform>();
                    if (childLt._dirty || changed)
                        _propagateQueue.Enqueue(child);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ComputeGlobal(Entity entity)
    {
        ref var lt = ref entity.Get<LocalTransform>();
        ref var gt = ref entity.Get<GlobalTransform>();

        Affine3D localMatrix;
        if (lt._dirty)
        {
            localMatrix = Affine3D.FromTRS(lt.Position, lt.Rotation, lt.Scale);
            gt.LocalMatrix = localMatrix;
            lt._dirty = false;
        }
        else
        {
            localMatrix = gt.LocalMatrix;
        }

        Affine3D newGlobal;
        var parent = lt.Parent;
        if (parent is { Host: not null })
        {
            ref var parentGt = ref parent.Get<GlobalTransform>();
            newGlobal = localMatrix * parentGt.Value;
        }
        else
        {
            newGlobal = localMatrix;
        }

        if (newGlobal == gt.Value)
            return false;

        gt.Value = newGlobal;
        return true;
    }

    private static bool WouldCreateCycle(Entity entity, Entity targetParent)
    {
        var current = targetParent;
        while (current is { Host: not null })
        {
            if (current == entity)
                return true;
            current = current.Get<LocalTransform>().Parent;
        }
        return false;
    }
}
