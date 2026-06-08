using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Dumb.Engine.Transform;
using Sia;

namespace Dumb.Engine.Benchmarks;

[MemoryDiagnoser]
public class TransformReactorBenchmarks
{
    private const int OperationsPerInvoke = 16;

    private World _world = null!;
    private Scene _scene;
    private int _mutationIndex;

    [Params(10_000, 50_000)]
    public int EntityCount { get; set; }

    private readonly record struct Scene(
        Entity Root,
        Entity LeftRoot,
        Entity RightRoot,
        Entity SubtreeRoot,
        Entity LastEntity,
        Entity LastSubtreeEntity);

    [GlobalSetup(Target = nameof(RootMutation_PropagateWholeTree))]
    public void GlobalSetupForRootMutation()
    {
        _world = CreateWorld();
        _scene = BuildScene(_world, EntityCount);
    }

    [GlobalCleanup(Target = nameof(RootMutation_PropagateWholeTree))]
    public void GlobalCleanupForRootMutation()
    {
        _world.Dispose();
    }

    [IterationSetup(Target = nameof(ReparentLargeSubtree))]
    public void IterationSetupForReparent()
    {
        _world = CreateWorld();
        _scene = BuildScene(_world, EntityCount);
    }

    [IterationCleanup(Target = nameof(ReparentLargeSubtree))]
    public void IterationCleanupForReparent()
    {
        _world.Dispose();
    }

    [Benchmark]
    public Vector3 BuildBalancedHierarchy()
    {
        using var world = CreateWorld();
        var scene = BuildScene(world, EntityCount);

        return scene.LastEntity
            .Get<GlobalTransform>()
            .Value
            .TransformPoint(new Vector3(1, 2, 3));
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public Vector3 RootMutation_PropagateWholeTree()
    {
        var result = Vector3.Zero;

        for (var i = 0; i < OperationsPerInvoke; i++)
        {
            _mutationIndex++;

            SetPosition(_world, _scene.Root, new Vector3(
                _mutationIndex,
                _mutationIndex * 2,
                _mutationIndex * 3));

            SetRotation(_world, _scene.Root, RotationFor(_mutationIndex));

            SetScale(_world, _scene.Root, new Vector3(
                1.0f + (_mutationIndex % 5) * 0.01f,
                1.1f + (_mutationIndex % 7) * 0.01f,
                1.2f + (_mutationIndex % 11) * 0.01f));

            result = _scene.LastEntity
                .Get<GlobalTransform>()
                .Value
                .TransformPoint(new Vector3(1, 2, 3));
        }

        return result;
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public Vector3 ReparentLargeSubtree()
    {
        var result = Vector3.Zero;

        for (var i = 0; i < OperationsPerInvoke; i++)
        {
            _mutationIndex++;

            var target = (_mutationIndex & 1) == 0
                ? _scene.RightRoot
                : _scene.LeftRoot;

            SetParent(_world, _scene.SubtreeRoot, target);

            result = _scene.LastSubtreeEntity
                .Get<GlobalTransform>()
                .Value
                .TransformPoint(new Vector3(1, 2, 3));
        }

        return result;
    }

    private static World CreateWorld()
    {
        var world = new World();

        world.AcquireAddon<TransformReactor>();

        world.AcquireHost<
            HList<LocalTransform, HList<GlobalTransform, EmptyHList>>,
            ArrayEntityHost<HList<LocalTransform, HList<GlobalTransform, EmptyHList>>>>();

        return world;
    }

    private static Scene BuildScene(World world, int entityCount)
    {
        var root = CreateEntity(world, Vector3.Zero);
        var leftRoot = CreateEntity(world, new Vector3(10, 0, 0), parent: root);
        var rightRoot = CreateEntity(world, new Vector3(-10, 0, 0), parent: root);
        var subtreeRoot = CreateEntity(world, new Vector3(1, 2, 3), parent: leftRoot);

        var half = entityCount / 2;

        Entity lastSubtreeEntity = subtreeRoot;
        Entity lastEntity = subtreeRoot;

        for (var i = 0; i < half; i++)
        {
            lastSubtreeEntity = CreateEntity(
                world,
                new Vector3(i % 17, i % 19, i % 23),
                rotation: RotationFor(i),
                scale: new Vector3(1 + i % 3, 1 + i % 5, 1 + i % 7),
                parent: subtreeRoot);

            lastEntity = lastSubtreeEntity;
        }

        for (var i = half; i < entityCount; i++)
        {
            lastEntity = CreateEntity(
                world,
                new Vector3(i % 29, i % 31, i % 37),
                rotation: RotationFor(i),
                scale: new Vector3(1 + i % 2, 1 + i % 3, 1 + i % 4),
                parent: rightRoot);
        }

        return new Scene(
            Root: root,
            LeftRoot: leftRoot,
            RightRoot: rightRoot,
            SubtreeRoot: subtreeRoot,
            LastEntity: lastEntity,
            LastSubtreeEntity: lastSubtreeEntity);
    }

    private static Entity CreateEntity(
        World world,
        Vector3 position,
        Quaternion? rotation = null,
        Vector3? scale = null,
        Entity? parent = null)
    {
        return world.Create(HList.From(
            new LocalTransform
            {
                Position = position,
                Rotation = rotation ?? Quaternion.Identity,
                Scale = scale ?? Vector3.One,
                Parent = parent
            },
            new GlobalTransform()));
    }

    private static void SetPosition(World world, Entity entity, Vector3 value)
    {
        world.Execute(entity, new LocalTransform.SetPosition(value));
    }

    private static void SetRotation(World world, Entity entity, Quaternion value)
    {
        world.Execute(entity, new LocalTransform.SetRotation(value));
    }

    private static void SetScale(World world, Entity entity, Vector3 value)
    {
        world.Execute(entity, new LocalTransform.SetScale(value));
    }

    private static void SetParent(World world, Entity entity, Entity? value)
    {
        world.Execute(entity, new LocalTransform.SetParent(value));
    }

    private static Quaternion RotationFor(int i)
    {
        return Quaternion.Normalize(
            Quaternion.CreateFromAxisAngle(
                Vector3.Normalize(new Vector3(1, 2, 3)),
                0.0001f * i));
    }
}
