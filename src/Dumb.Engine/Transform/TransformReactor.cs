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

        Listen((Entity entity, in LocalTransform.SetPosition cmd) => PropagateGlobal(entity));
        Listen((Entity entity, in LocalTransform.SetRotation cmd) => PropagateGlobal(entity));
        Listen((Entity entity, in LocalTransform.SetScale cmd) => PropagateGlobal(entity));

        Listen((Entity entity, in LocalTransform.SetParent cmd) => {
            ref var lt = ref entity.Get<LocalTransform>();

            var parent = lt.Parent;
            var prevParent = lt._prevParent;

            if (parent is { Host: not null })
            {
                try
                {
                    AssertNoCycle(entity, parent);
                }
                catch
                {
                    lt._parent = prevParent;
                    lt._prevParent = prevParent;
                    throw;
                }
            }

            if (prevParent == parent)
            {
                ComputeGlobal(entity);
                PropagateGlobal(entity);
                return;
            }

            if (prevParent is { Host: not null })
                RemoveChild(prevParent, entity);

            if (parent is { Host: not null })
                AddChild(parent, entity);

            lt._prevParent = parent;

            ComputeGlobal(entity);
            PropagateGlobal(entity);
        });
    }

    protected override void OnEntityAdded(Entity entity)
    {
        ref var lt = ref entity.Get<LocalTransform>();

        if (!entity.Contains<GlobalTransform>())
            entity.Add(new GlobalTransform(Affine3D.Identity));

        var parent = lt.Parent;
        if (parent is { Host: not null })
        {
            AddChild(parent, entity);
            var localMatrix = Affine3D.FromTRS(lt.Position, lt.Rotation, lt.Scale);
            ref var parentGt = ref parent.Get<GlobalTransform>();
            entity.Get<GlobalTransform>().Value = localMatrix * parentGt.Value;
        }
        else
        {
            entity.Get<GlobalTransform>().Value = Affine3D.FromTRS(lt.Position, lt.Rotation, lt.Scale);
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
            // Copy — child.Destroy() triggers RemoveChild which modifies the set
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

    /// <summary>
    /// Iterative BFS propagation. Each node's global = local_TRS * parent_global.
    /// </summary>
    private void PropagateGlobal(Entity root)
    {
        _propagateQueue.Enqueue(root);

        while (_propagateQueue.TryDequeue(out var entity))
        {
            ComputeGlobal(entity);
            if (_children.TryGetValue(entity, out var children))
            {
                foreach (var child in children)
                    _propagateQueue.Enqueue(child);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComputeGlobal(Entity entity)
    {
        ref var lt = ref entity.Get<LocalTransform>();
        ref var gt = ref entity.Get<GlobalTransform>();

        var localMatrix = Affine3D.FromTRS(lt.Position, lt.Rotation, lt.Scale);

        var parent = lt.Parent;
        if (parent is { Host: not null })
        {
            ref var parentGt = ref parent.Get<GlobalTransform>();
            gt.Value = localMatrix * parentGt.Value;
        }
        else
        {
            gt.Value = localMatrix;
        }
    }

    private static void AssertNoCycle(Entity entity, Entity targetParent)
    {
        var current = targetParent;
        while (current is { Host: not null })
        {
            if (current == entity)
                throw new InvalidOperationException("Setting this parent would create a transform cycle.");
            current = current.Get<LocalTransform>().Parent;
        }
    }
}
