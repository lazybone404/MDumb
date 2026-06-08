using Sia;

namespace Dumb.Graphics.Tests.Resources;

/// <summary>
/// GpuResourceManager 模式的测试替身 — 使用真实的 World + ECS EntityHost，
/// 实现相同的 RefCount/Retain/Release 模式，但 ReleaseNative 是空操作。
/// 这验证了 GpuResourceManager 基类定义的核心引用计数契约。
/// </summary>
public sealed class TestResourceManager : IDisposable
{
    private readonly World _world;
    private int _releaseNativeCallCount;

    public int ReleaseNativeCallCount => _releaseNativeCallCount;

    public IEntityHost<HList<TestResourceData, EmptyHList>> Host { get; }

    public TestResourceManager()
    {
        _world = new World();
        Host = _world.AcquireHost<
            HList<TestResourceData, EmptyHList>,
            ArrayEntityHost<HList<TestResourceData, EmptyHList>>>();
    }

    /// <summary>
    /// 对应 GpuResourceManager.CreateResource — 创建实体并绑定资源数据。
    /// </summary>
    public Entity CreateTestResource(TestResourceData data)
    {
        return Host.Create(HList.From(data));
    }

    /// <summary>
    /// 对应 GpuResourceManager.Retain — 增加引用计数。
    /// </summary>
    public void Retain(Entity entity)
    {
        ref var data = ref entity.Get<TestResourceData>();
        Interlocked.Increment(ref data.RefCount);
    }

    /// <summary>
    /// 对应 GpuResourceManager.Release — 减少引用计数，归零时释放并销毁。
    /// </summary>
    public void Release(Entity entity)
    {
        ref var data = ref entity.Get<TestResourceData>();
        if (Interlocked.Decrement(ref data.RefCount) == 0)
        {
            ReleaseNative(data.NativePtr);
            entity.Destroy();
        }
    }

    /// <summary>
    /// 对应 GpuResourceManager.ReleaseNative — 子类在此处调用 GPU API。
    /// </summary>
    private void ReleaseNative(nint nativePtr)
    {
        Interlocked.Increment(ref _releaseNativeCallCount);
    }

    public void Dispose() => _world.Dispose();
}

public struct TestResourceData : IGpuResource
{
    public nint NativePtr;
    public int RefCount;
}

public sealed class GpuResourceRefCountTests : IDisposable
{
    private readonly TestResourceManager _manager;

    public GpuResourceRefCountTests()
    {
        _manager = new TestResourceManager();
    }

    public void Dispose() => _manager.Dispose();

    [Fact]
    public void Create_InitializesRefCountToOne()
    {
        var data = new TestResourceData { NativePtr = 42, RefCount = 1 };
        var entity = _manager.CreateTestResource(data);

        ref var stored = ref entity.Get<TestResourceData>();

        Assert.Equal(1, stored.RefCount);
        Assert.Equal((nint)42, stored.NativePtr);
    }

    [Fact]
    public void Retain_IncrementsRefCount()
    {
        var data = new TestResourceData { NativePtr = 1, RefCount = 1 };
        var entity = _manager.CreateTestResource(data);

        _manager.Retain(entity);

        ref var stored = ref entity.Get<TestResourceData>();
        Assert.Equal(2, stored.RefCount);
    }

    [Fact]
    public void Release_DecrementsRefCount()
    {
        var data = new TestResourceData { NativePtr = 1, RefCount = 2 };
        var entity = _manager.CreateTestResource(data);

        _manager.Release(entity);

        ref var stored = ref entity.Get<TestResourceData>();
        Assert.Equal(1, stored.RefCount);
        Assert.Equal(0, _manager.ReleaseNativeCallCount);
    }

    [Fact]
    public void Release_WhenRefCountReachesZero_CallsReleaseNativeAndDestroysEntity()
    {
        var data = new TestResourceData { NativePtr = 1, RefCount = 1 };
        var entity = _manager.CreateTestResource(data);
        Assert.True(entity.IsValid);

        _manager.Release(entity);

        Assert.Equal(1, _manager.ReleaseNativeCallCount);
        Assert.False(entity.IsValid);
    }

    [Fact]
    public void RetainRelease_MultipleCycles_MaintainsCorrectRefCount()
    {
        var data = new TestResourceData { NativePtr = 1, RefCount = 1 };
        var entity = _manager.CreateTestResource(data);

        _manager.Retain(entity); // 2
        _manager.Retain(entity); // 3
        _manager.Release(entity); // 2
        _manager.Retain(entity); // 3
        _manager.Release(entity); // 2
        _manager.Release(entity); // 1

        ref var stored = ref entity.Get<TestResourceData>();
        Assert.Equal(1, stored.RefCount);
        Assert.True(entity.IsValid);
        Assert.Equal(0, _manager.ReleaseNativeCallCount);

        _manager.Release(entity); // 0 → destroy
        Assert.Equal(1, _manager.ReleaseNativeCallCount);
        Assert.False(entity.IsValid);
    }

    [Fact]
    public void Release_AfterDestroy_EntityIsInvalid()
    {
        var data = new TestResourceData { NativePtr = 1, RefCount = 1 };
        var entity = _manager.CreateTestResource(data);

        _manager.Release(entity);

        Assert.Equal(1, _manager.ReleaseNativeCallCount);
        Assert.False(entity.IsValid);
    }

    [Fact]
    public void Retain_AfterMultipleHolds_PreventsPrematureRelease()
    {
        var data = new TestResourceData { NativePtr = 100, RefCount = 1 };
        var entity = _manager.CreateTestResource(data);

        // 模拟 3 个持有者
        _manager.Retain(entity); // 2
        _manager.Retain(entity); // 3

        // 持有者 1 释放
        _manager.Release(entity); // 2
        Assert.True(entity.IsValid);

        // 持有者 2 释放
        _manager.Release(entity); // 1
        Assert.True(entity.IsValid);

        // 持有者 3 (原始) 释放
        _manager.Release(entity); // 0 → destroy
        Assert.False(entity.IsValid);
        Assert.Equal(1, _manager.ReleaseNativeCallCount);
    }
}
