using System.Threading;
using Sia;

namespace Dumb.Graphics;

/// <summary>
/// GPU 资源管理器抽象基类 — 统一 Release/Retain/CreateResource 模式。
/// 适用于 NativePtr + RefCount 模式的简单 GPU 资源（Buffer, Texture, Shader, Sampler 等）。
/// 复合资源（Mesh, Material）不使用此基类。
/// </summary>
/// <typeparam name="TData">实现了 IGpuResource 的资源 struct，必须包含 public nint NativePtr 和 public int RefCount 字段</typeparam>
public abstract class GpuResourceManager<TData>
    where TData : struct, IGpuResource
{
    protected readonly GraphicsContext Ctx;
    private readonly IEntityHost<HList<TData, EmptyHList>> _host;

    protected GpuResourceManager(GraphicsContext ctx, IEntityHost<HList<TData, EmptyHList>> host)
    {
        Ctx = ctx;
        _host = host;
    }

    /// <summary>
    /// 在 _host 中创建实体并绑定资源数据。
    /// </summary>
    protected Entity CreateResource(TData data)
    {
        return _host.Create(HList.From(data));
    }

    /// <summary>
    /// 增加引用计数。
    /// </summary>
    public void Retain(Entity entity)
    {
        ref var data = ref entity.Get<TData>();
        Interlocked.Increment(ref GetRefCountRef(ref data));
    }

    /// <summary>
    /// 减少引用计数，归零时释放原生资源并销毁实体。
    /// </summary>
    public virtual void Release(Entity entity)
    {
        ref var data = ref entity.Get<TData>();
        if (Interlocked.Decrement(ref GetRefCountRef(ref data)) == 0)
        {
            ReleaseNative(GetNativePtr(ref data));
            entity.Destroy();
        }
    }

    /// <summary>
    /// 获取 RefCount 字段的引用，供 Interlocked 操作使用。
    /// </summary>
    protected abstract ref int GetRefCountRef(ref TData data);

    /// <summary>
    /// 获取 NativePtr 字段的值。
    /// </summary>
    protected abstract nint GetNativePtr(ref TData data);

    /// <summary>
    /// 释放原生 GPU 资源。由子类实现，调用对应的 Device.ReleaseXxx 方法。
    /// </summary>
    protected abstract void ReleaseNative(nint nativePtr);
}
