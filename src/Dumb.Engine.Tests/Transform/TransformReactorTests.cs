using System.Numerics;
using Dumb.Engine.Transform;
using Sia;

namespace Dumb.Engine.Tests.Transform;

public sealed class TransformReactorTests : IDisposable
{
    private const float Epsilon = 1e-4f;

    private readonly World _world;

    public TransformReactorTests()
    {
        _world = new World();

        _world.AcquireAddon<TransformReactor>();

        _world.AcquireHost<
            HList<LocalTransform, HList<GlobalTransform, EmptyHList>>,
            ArrayEntityHost<HList<LocalTransform, HList<GlobalTransform, EmptyHList>>>>();
    }

    public void Dispose() => _world.Dispose();

    private Entity CreateEntity(
        Vector3 position,
        Quaternion? rotation = null,
        Vector3? scale = null,
        Entity? parent = null) =>
        _world.Create(HList.From(
            new LocalTransform
            {
                Position = position,
                Rotation = rotation ?? Quaternion.Identity,
                Scale = scale ?? Vector3.One,
                Parent = parent
            },
            new GlobalTransform(Affine3D.Identity)));

    private static Quaternion QuaternionFromAxisAngle(Vector3 axis, float radians) =>
        Quaternion.CreateFromAxisAngle(axis, radians);

    private static void Near(float expected, float actual, float eps = Epsilon) =>
        Assert.True(
            MathF.Abs(expected - actual) <= eps,
            $"Expected {expected}, actual {actual}, diff {MathF.Abs(expected - actual)}");

    private static void Near(Vector3 expected, Vector3 actual, float eps = Epsilon)
    {
        Near(expected.X, actual.X, eps);
        Near(expected.Y, actual.Y, eps);
        Near(expected.Z, actual.Z, eps);
    }

    private static Vector3 GlobalPosition(Entity entity) => entity.Get<GlobalTransform>().Value.TransformPoint(Vector3.Zero);

    private static Affine3D LocalMatrix(Entity entity)
    {
        ref var lt = ref entity.Get<LocalTransform>();
        return Affine3D.FromTRS(lt.Position, lt.Rotation, lt.Scale);
    }

    private static Affine3D ExpectedGlobal(params Entity[] chainFromLeafToRoot)
    {
        Assert.NotEmpty(chainFromLeafToRoot);

        var result = LocalMatrix(chainFromLeafToRoot[0]);

        for (var i = 1; i < chainFromLeafToRoot.Length; i++)
            result = result * LocalMatrix(chainFromLeafToRoot[i]);

        return result;
    }

    private static void AssertGlobalMatchesExpected(Entity entity, params Entity[] chainFromLeafToRoot)
    {
        var expected = ExpectedGlobal(chainFromLeafToRoot);
        var actual = entity.Get<GlobalTransform>().Value;

        var samplePoints = new[]
        {
            Vector3.Zero,
            new Vector3(1, 2, 3),
            new Vector3(-4, 5, -6),
            new Vector3(0.25f, -0.5f, 0.75f)
        };

        foreach (var p in samplePoints)
            Near(expected.TransformPoint(p), actual.TransformPoint(p));
    }

    private void SetPosition(Entity entity, Vector3 value) => _world.Execute(entity, new LocalTransform.SetPosition(value));

    private void SetRotation(Entity entity, Quaternion value) => _world.Execute(entity, new LocalTransform.SetRotation(value));

    private void SetScale(Entity entity, Vector3 value) => _world.Execute(entity, new LocalTransform.SetScale(value));

    private void SetParent(Entity entity, Entity? value) => _world.Execute(entity, new LocalTransform.SetParent(value));

    [Fact]
    public void TransformReactor_CreateHierarchy_ComputesInitialGlobalForAllLevels()
    {
        var root = CreateEntity(
            new Vector3(10, -5, 3),
            rotation: QuaternionFromAxisAngle(Vector3.UnitY, 0.5f),
            scale: new Vector3(2, 2, 2));

        var child = CreateEntity(
            new Vector3(3, 4, 5),
            rotation: QuaternionFromAxisAngle(Vector3.UnitX, -0.25f),
            scale: new Vector3(1, 2, 1),
            parent: root);

        var leaf = CreateEntity(
            new Vector3(-2, 1, 6),
            rotation: QuaternionFromAxisAngle(Vector3.UnitZ, 0.75f),
            scale: new Vector3(0.5f, 1.5f, 2),
            parent: child);

        AssertGlobalMatchesExpected(root, root);
        AssertGlobalMatchesExpected(child, child, root);
        AssertGlobalMatchesExpected(leaf, leaf, child, root);

        Assert.True(root.IsValid);
        Assert.True(child.IsValid);
        Assert.True(leaf.IsValid);
    }

    [Fact]
    public void TransformReactor_RootMutation_PropagatesThroughFullSubtree()
    {
        var root = CreateEntity(
            new Vector3(1, 2, 3),
            rotation: QuaternionFromAxisAngle(Vector3.UnitY, 0.2f),
            scale: Vector3.One);

        var childA = CreateEntity(new Vector3(5, 0, 0), parent: root);
        var childB = CreateEntity(new Vector3(0, 6, 0), parent: root);
        var leafA = CreateEntity(new Vector3(0, 0, 7), parent: childA);
        var leafB = CreateEntity(new Vector3(1, 1, 1), parent: childB);

        SetPosition(root, new Vector3(100, -50, 25));
        SetRotation(root, QuaternionFromAxisAngle(new Vector3(1, 1, 0), 0.9f));
        SetScale(root, new Vector3(2, 3, 4));

        AssertGlobalMatchesExpected(root, root);
        AssertGlobalMatchesExpected(childA, childA, root);
        AssertGlobalMatchesExpected(childB, childB, root);
        AssertGlobalMatchesExpected(leafA, leafA, childA, root);
        AssertGlobalMatchesExpected(leafB, leafB, childB, root);
    }

    [Fact]
    public void TransformReactor_ChildMutation_UpdatesOnlyItsOwnSubtree()
    {
        var root = CreateEntity(new Vector3(10, 0, 0));
        var childA = CreateEntity(new Vector3(1, 0, 0), parent: root);
        var childB = CreateEntity(new Vector3(2, 0, 0), parent: root);
        var leafA = CreateEntity(new Vector3(3, 0, 0), parent: childA);
        var leafB = CreateEntity(new Vector3(4, 0, 0), parent: childB);

        var rootBefore = GlobalPosition(root);
        var childBBefore = GlobalPosition(childB);
        var leafBBefore = GlobalPosition(leafB);

        SetPosition(childA, new Vector3(20, 30, 40));
        SetRotation(childA, QuaternionFromAxisAngle(Vector3.UnitZ, 1.2f));
        SetScale(childA, new Vector3(2, 2, 2));

        Near(rootBefore, GlobalPosition(root));
        Near(childBBefore, GlobalPosition(childB));
        Near(leafBBefore, GlobalPosition(leafB));

        AssertGlobalMatchesExpected(childA, childA, root);
        AssertGlobalMatchesExpected(leafA, leafA, childA, root);
        AssertGlobalMatchesExpected(childB, childB, root);
        AssertGlobalMatchesExpected(leafB, leafB, childB, root);
    }

    [Fact]
    public void TransformReactor_Reparenting_UpdatesOldParentNewParentAndDescendants()
    {
        var parentA = CreateEntity(
            new Vector3(100, 0, 0),
            rotation: QuaternionFromAxisAngle(Vector3.UnitY, 0.25f),
            scale: new Vector3(2, 2, 2));

        var parentB = CreateEntity(
            new Vector3(-50, 10, 20),
            rotation: QuaternionFromAxisAngle(Vector3.UnitZ, -0.7f),
            scale: new Vector3(1, 3, 2));

        var child = CreateEntity(
            new Vector3(5, 6, 7),
            rotation: QuaternionFromAxisAngle(Vector3.UnitX, 0.4f),
            scale: new Vector3(1, 2, 1),
            parent: parentA);

        var leaf = CreateEntity(
            new Vector3(1, 2, 3),
            parent: child);

        AssertGlobalMatchesExpected(child, child, parentA);
        AssertGlobalMatchesExpected(leaf, leaf, child, parentA);

        SetParent(child, parentB);

        AssertGlobalMatchesExpected(parentA, parentA);
        AssertGlobalMatchesExpected(parentB, parentB);
        AssertGlobalMatchesExpected(child, child, parentB);
        AssertGlobalMatchesExpected(leaf, leaf, child, parentB);

        SetPosition(parentA, new Vector3(999, 999, 999));

        AssertGlobalMatchesExpected(child, child, parentB);
        AssertGlobalMatchesExpected(leaf, leaf, child, parentB);

        SetPosition(parentB, new Vector3(10, 20, 30));

        AssertGlobalMatchesExpected(child, child, parentB);
        AssertGlobalMatchesExpected(leaf, leaf, child, parentB);

        SetParent(child, null);

        AssertGlobalMatchesExpected(child, child);
        AssertGlobalMatchesExpected(leaf, leaf, child);
    }

    [Fact]
    public void TransformReactor_CycleDetection_RejectsSelfAndAncestorCyclesWithoutCorruptingHierarchy()
    {
        var root = CreateEntity(new Vector3(1, 0, 0));
        var child = CreateEntity(new Vector3(2, 0, 0), parent: root);
        var leaf = CreateEntity(new Vector3(3, 0, 0), parent: child);

        Assert.Throws<InvalidOperationException>(() => SetParent(root, root));
        Assert.Throws<InvalidOperationException>(() => SetParent(root, child));
        Assert.Throws<InvalidOperationException>(() => SetParent(root, leaf));
        Assert.Throws<InvalidOperationException>(() => SetParent(child, leaf));

        AssertGlobalMatchesExpected(root, root);
        AssertGlobalMatchesExpected(child, child, root);
        AssertGlobalMatchesExpected(leaf, leaf, child, root);

        SetPosition(root, new Vector3(10, 0, 0));

        AssertGlobalMatchesExpected(root, root);
        AssertGlobalMatchesExpected(child, child, root);
        AssertGlobalMatchesExpected(leaf, leaf, child, root);
    }

    [Fact]
    public void TransformReactor_DestroyParent_DestroysEntireSubtreeButNotUnrelatedEntities()
    {
        var root = CreateEntity(Vector3.Zero);
        var childA = CreateEntity(new Vector3(1, 0, 0), parent: root);
        var childB = CreateEntity(new Vector3(2, 0, 0), parent: root);
        var leafA = CreateEntity(new Vector3(3, 0, 0), parent: childA);
        var leafB = CreateEntity(new Vector3(4, 0, 0), parent: childB);

        var unrelated = CreateEntity(new Vector3(100, 100, 100));

        root.Destroy();

        Assert.False(root.IsValid);
        Assert.False(childA.IsValid);
        Assert.False(childB.IsValid);
        Assert.False(leafA.IsValid);
        Assert.False(leafB.IsValid);

        Assert.True(unrelated.IsValid);
        Near(new Vector3(100, 100, 100), GlobalPosition(unrelated));
    }

    [Fact]
    public void TransformReactor_PointAndDirectionSemantics_ArePreservedThroughHierarchy()
    {
        var root = CreateEntity(
            new Vector3(10, 20, 30),
            rotation: QuaternionFromAxisAngle(Vector3.UnitY, 0.7f),
            scale: new Vector3(2, 3, 4));

        var child = CreateEntity(
            new Vector3(5, 6, 7),
            rotation: QuaternionFromAxisAngle(Vector3.UnitX, -0.4f),
            scale: new Vector3(1.5f, 0.5f, 2),
            parent: root);

        var leaf = CreateEntity(
            new Vector3(-1, -2, -3),
            rotation: QuaternionFromAxisAngle(Vector3.UnitZ, 0.9f),
            scale: new Vector3(0.75f, 1.25f, 1.5f),
            parent: child);

        var global = leaf.Get<GlobalTransform>().Value;
        var expected = ExpectedGlobal(leaf, child, root);

        var point = new Vector3(1, 2, 3);
        var direction = new Vector3(1, 2, 3);

        Near(expected.TransformPoint(point), global.TransformPoint(point));
        Near(expected.TransformDirection(direction), global.TransformDirection(direction));

        Assert.True(
            Vector3.Distance(
                global.TransformPoint(point),
                global.TransformDirection(direction)) > 1e-3f);
    }
}
