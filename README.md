# Dumb 引擎大规模重构文档

> **⚠️ 这是一次激进、接近破坏性的重构。几乎所有公共 API 都已变更，原代码几乎无法兼容。**

## 破坏性总览

| 变更 | 破坏程度 |
|------|----------|
| 8 个 `static class` 全部转为实例类 | 🔴 100% 不兼容 |
| `Browser/`、`Native/` 目录删除，`#if BROWSER` 统一 | 🔴 目录结构彻底改变 |
| `IMaterial` 接口 8 成员 → 3 成员 | 🔴 100% 不兼容 |
| 新增 `GpuResourceManager<T>` 基类 | 🔴 架构层变更 |
| 提取 `SurfaceManager` + `WindowExtensions` DI | 🟡 可选兼容 |
| 新增 107 个测试 | 🟢 纯增量 |

## 成果一览

| 指标 | 重构前 | 重构后 |
|------|--------|--------|
| static 类 | 8 个 | 0 个 |
| Browser/Native 目录 | 2 个 (6 文件) | 0 个 (已删除) |
| 平台特定文件 | 6 个 | 3 个 (统一 `#if BROWSER`) |
| IMaterial 静态成员 | 6 个/材质 | 2 个/材质 |
| 测试项目 | 1 个 (14 测试) | 2 个 (107 测试) |
| 硬编码依赖 (`new GlfwWindowBackend()`) | 有 | 无 (DI 可选注入) |

---

## 阶段〇：Static 类 → 实例类

### 问题

8 个资源管理类 (`Buffers`, `Textures`, `Shaders`, `Samplers`, `Pipelines`, `Commands`, `Mesh`, `Materials`) 全部是 `static class`，每个方法都要求传入 `GraphicsContext ctx`：

```csharp
// 重构前
public static class Buffers
{
    public static Entity Create(GraphicsContext ctx, ulong size, BufferUsage usage) { ... }
}
// 调用: Buffers.Create(ctx, 256, BufferUsage.Uniform);
```

后果：无法 mock、无法替换实现、调用啰嗦。

### 方案

全部转为实例类，通过 `GraphicsContext` 属性暴露：

```csharp
// 重构后
public class BufferManager(GraphicsContext ctx) : GpuResourceManager<BufferData>(ctx, ctx._buffers)
{
    public Entity Create(ulong size, BufferUsage usage) { ... }
}
// 调用: ctx.Buffers.Create(256, BufferUsage.Uniform);
```

### 变更清单

| 文件 | 变更类型 |
|------|----------|
| `Resources/Buffers.cs` | `static class` → `class BufferManager` |
| `Resources/Textures.cs` | `static class` → `class TextureManager` |
| `Resources/Shaders.cs` | `static class` → `class ShaderManager` |
| `Resources/Samplers.cs` | `static class` → `class SamplerManager` |
| `Resources/Pipelines.cs` | `static class` → `class PipelineManager` |
| `Resources/Mesh.cs` | `static class` → `class MeshManager` |
| `Resources/Materials.cs` | `static class` → `class MaterialManager` |
| `Core/GraphicsContext.cs` | 新增 8 个实例属性 + 构造函数初始化 |

---

## 阶段一：统一 GPU 资源引用计数管理

### 问题

7 个资源 Manager 中 `.Release()` / `.Retain()` / `.CreateResource()` 模式的代码完全相同（约 90% 重复）。每个类都手写相同的 `Interlocked.Decrement` + `entity.Destroy()` 逻辑。

### 方案

**1.1 新增 `IGpuResource` 标记接口**（[ResourceData.cs](src/Dumb.Graphics/Resources/ResourceData.cs)）

```csharp
public interface IGpuResource { }
```

10 个资源数据 struct 实现此接口：`BufferData`, `TextureData`, `TextureViewData`, `SamplerData`, `ShaderData`, `BindGroupLayoutData`, `BindGroupData`, `PipelineLayoutData`, `RenderPipelineData`, `ComputePipelineData`。

**1.2 新增抽象基类**（[GpuResourceManager.cs](src/Dumb.Graphics/Resources/GpuResourceManager.cs)）

```csharp
public abstract class GpuResourceManager<TData> where TData : struct, IGpuResource
{
    protected Entity CreateResource(TData data) { ... }
    public void Retain(Entity entity) { ... }
    public virtual void Release(Entity entity) { ... }

    // 子类只需实现这三个抽象方法
    protected abstract ref int GetRefCountRef(ref TData data);
    protected abstract nint GetNativePtr(ref TData data);
    protected abstract void ReleaseNative(nint nativePtr);
}
```

**关键设计决策：ref-return 抽象方法**

C# 泛型无法通过 `where T : struct, IGpuResource` 直接访问 struct 字段。因此引入 `GetRefCountRef` / `GetNativePtr` 两个 ref-return 抽象方法，子类用一行代码暴露字段引用：

```csharp
// BufferManager 中的实现
protected override ref int GetRefCountRef(ref BufferData data) => ref data.RefCount;
protected override nint GetNativePtr(ref BufferData data) => data.NativePtr;
protected override void ReleaseNative(nint nativePtr) => Ctx.Device.ReleaseBuffer(nativePtr);
```

**1.3 复合资源的特殊处理**

`MeshManager` 和 `MaterialManager` **不继承** `GpuResourceManager<T>`。它们的 Release 需要级联释放子资源（顶点缓冲→`Buffers.Release`，索引缓冲→`Buffers.Release`，Pipeline→`Pipelines.Release` 等），保持原有的独立实现。

`PipelineManager` **不继承**基类——它管理 6 种不同数据类型的 EntityHost，一个泛型基类无法覆盖。

### 变更清单

| 文件 | 变更 |
|------|------|
| `Resources/GpuResourceManager.cs` | **新增** 抽象基类 |
| `Resources/ResourceData.cs` | 添加 `IGpuResource` 接口，10 个 struct 实现之 |
| `Resources/Buffers.cs` | 继承 `GpuResourceManager<BufferData>`，删除重复 Release/Retain |
| `Resources/Textures.cs` | 继承 `GpuResourceManager<TextureData>` |
| `Resources/Shaders.cs` | 继承 `GpuResourceManager<ShaderData>` |
| `Resources/Samplers.cs` | 继承 `GpuResourceManager<SamplerData>` |
| `Resources/Mesh.cs` | 保持独立（级联释放） |
| `Resources/Materials.cs` | 保持独立（级联释放） |
| `Resources/Pipelines.cs` | 保持独立（多数据类型） |

---

## 阶段二：消除 Browser/Native 后端重复

### 问题

`src/Dumb.Graphics/Browser/` 和 `src/Dumb.Graphics/Native/` 各包含 3 个后端文件：

```
Browser/
├── BrowserDevice.cs      (~150 行)
├── BrowserCommand.cs     (~120 行)
└── BrowserSwapChain.cs   (~100 行)

Native/
├── NativeDevice.cs       (~150 行)
├── NativeCommand.cs      (~120 行)
└── NativeSwapChain.cs    (~110 行)
```

两套代码 **95%+ 的方法签名和逻辑完全相同**，仅存在以下差异：

1. 持有的 API 对象类型不同：`WGPUBrowser` (Browser) vs `WebGPU` (Native)
2. 回调包装方式不同：Browser 直接 cast delegate，Native 需 `new PfnXxxCallback(...)` 包装
3. `InstanceProcessEvents`：Browser 转发调用，Native 为 no-op
4. SwapChain API 完全不同：Browser 用 Dawn SwapChain API，Native 用标准 Surface API

### 方案

**删除 6 个平台特定文件**，用 `#if BROWSER` 预处理指令将差异合并到 3 个统一文件中：

```
Core/
├── DeviceBackend.cs      ← 替代 BrowserDevice + NativeDevice
├── CommandBackend.cs     ← 替代 BrowserCommand + NativeCommand
└── SwapChainBackend.cs   ← 替代 BrowserSwapChain + NativeSwapChain
```

#### DeviceBackend 差异处理（3/38 方法不同）

```csharp
public sealed unsafe class DeviceBackend : IDeviceBackend
{
#if BROWSER
    private readonly Dumb.Emscripten.WGPUBrowser _wgpu;
    public DeviceBackend(Dumb.Emscripten.WGPUBrowser wgpu) => _wgpu = wgpu;
#else
    private readonly WebGPU _wgpu;
    public DeviceBackend(WebGPU wgpu) => _wgpu = wgpu;
#endif

    // 大多数方法完全相同，直接转发：
    public nint CreateInstance(InstanceDescriptor* descriptor) =>
        (nint)_wgpu.CreateInstance(descriptor);

    // 仅 3 个方法有差异，使用 #if 隔离：
    public void InstanceProcessEvents(nint instance)
    {
#if BROWSER
        _wgpu.InstanceProcessEvents((Instance*)instance);
#endif
        // Native: no-op
    }
    // ...
}
```

#### CommandBackend（100% 相同，仅字段类型不同）

方法体完全一致，只需 `#if` 处理字段类型和构造函数。

#### SwapChainBackend（API 完全不同，每个方法体用 `#if`）

Browser (Dawn API) 和 Native (标准 Surface API) 的 SwapChain 流程完全不同：

| 操作 | Browser (Dawn) | Native (标准) |
|------|---------------|---------------|
| 配置 | `DeviceCreateSwapChain()` | `SurfaceConfigure()` |
| 获取帧 | `SwapChainGetCurrentTextureView()` | `SurfaceGetCurrentTexture()` + `TextureCreateView()` |
| 呈现 | 浏览器驱动（no-op） | `SurfacePresent()` |
| 销毁 | `SwapChainRelease()` | `TextureRelease()` + `SurfaceUnconfigure()` |

每个方法体用 `#if BROWSER` 分别实现完整逻辑。

### 变更清单

| 操作 | 文件 |
|------|------|
| **删除** | `Browser/BrowserDevice.cs`, `Browser/BrowserCommand.cs`, `Browser/BrowserSwapChain.cs` |
| **删除** | `Native/NativeDevice.cs`, `Native/NativeCommand.cs`, `Native/NativeSwapChain.cs` |
| **删除** | `Browser/`, `Native/` 目录 |
| **新增** | `Core/DeviceBackend.cs`, `Core/CommandBackend.cs`, `Core/SwapChainBackend.cs` |
| **修改** | `Dumb.Graphics.csproj` — 移除 `<Compile Remove="Browser\**\*.cs" />` 等条件排除 |
| **修改** | `Core/GraphicsContext.cs` — 构造函数简化为直接 `new DeviceBackend(_wgpu)` |

---

## 阶段三：GraphicsContext 职责分离 + 依赖注入

### 问题

- `GraphicsContext` 承担 12 个 EntityHost 管理 + Surface 完整生命周期 + 8 个 Manager 初始化 + 设备/适配器异步创建，约 283 行
- `WindowExtensions.CreateWindow` 硬编码 `new GlfwWindowBackend()` 和 `new GlfwInputBackend()`，无法测试

### 方案

**3.1 提取 SurfaceManager**（[SurfaceManager.cs](src/Dumb.Graphics/Core/SurfaceManager.cs)）

将 Surface 完整生命周期从 `GraphicsContext` 中提取出来：

```csharp
public unsafe class SurfaceManager
{
    public GraphicsSurface CreateFromNative(nint handle);
    public TextureFormat GetPreferredFormat(GraphicsSurface surface);
    public void Configure(in GraphicsSurface surface, ...);
    public SurfaceFrame BeginFrame(in GraphicsSurface surface);
    public void Present(nint surface);
    public void Destroy(ref GraphicsSurface surface);
}
```

`GraphicsContext` 的 Surface 方法变为薄委托：

```csharp
// GraphicsContext 中保留兼容 API
public GraphicsSurface CreateSurfaceFromNative(nint handle)
    => Surfaces.CreateFromNative(handle);

public TextureFormat GetSurfacePreferredFormat(GraphicsSurface surface)
    => Surfaces.GetPreferredFormat(surface);
```

**3.2 Window/Input 依赖注入**（[WindowExtensions.cs](src/Dumb.Engine/Window/WindowExtensions.cs)）

```csharp
// 重构前：硬编码具体类型
public static Entity CreateWindow(this World world, WindowDescriptor descriptor)
{
    var window = new GlfwWindowBackend(descriptor);  // 硬编码!
    var input = new GlfwInputBackend(window);         // 硬编码!
    // ...
}

// 重构后：可选注入
public static Entity CreateWindow(
    this World world,
    WindowDescriptor descriptor,
    IWindowBackend? backend = null,
    IInputBackend? inputBackend = null)
{
    var window = backend ?? new GlfwWindowBackend(descriptor);   // 默认值
    var input = inputBackend ?? new Input.GlfwInputBackend(window); // 默认值
    // ...
}
```

这使得测试可以用 `StubWindowBackend` / `StubInputBackend` 注入，无需实际 GLFW 窗口。

### 变更清单

| 文件 | 变更 |
|------|------|
| `Core/SurfaceManager.cs` | **新增**，提取 Surface 生命周期 |
| `Core/GraphicsContext.cs` | Surface 方法改为委托，构造函数初始化 `SurfaceManager` |
| `Window/WindowExtensions.cs` | 添加可选 `IWindowBackend?` 和 `IInputBackend?` 参数 |
| `Input/IInputBackend.cs` | 原有接口（Phase 0 已存在），现用于 DI |
| `Window/IWindowBackend.cs` | 原有接口（Phase 0 已存在），现用于 DI |

---

## 阶段四：IMaterial 接口简化

### 问题

`IMaterial` 接口散落 6 个 static abstract 成员，实现一个材质需要写大量样板代码：

```csharp
// 重构前
public interface IMaterial
{
    static abstract string Name { get; }
    static abstract MeshDescriptor VertexDescriptor { get; }
    static abstract BindingLayout[][] BindGroupLayouts { get; }
    static abstract TextureFormat[] ColorFormats { get; }
    static abstract BlendState? Blend { get; }
    static abstract DepthStencilState? DepthStencil { get; }

    Entity GetShader(GraphicsContext ctx);
    Entity?[] CreateBindGroups(GraphicsContext ctx, Entity pipelineLayout);
}
```

### 方案

引入 `MaterialConfig` 记录类型，将 5 个配置属性合并为 1 个：

```csharp
// 重构后
public interface IMaterial
{
    static abstract string Name { get; }
    static abstract MaterialConfig Config { get; }  // ← 替代上方 5 个属性

    Entity GetShader(GraphicsContext ctx);
    Entity?[] CreateBindGroups(GraphicsContext ctx, Entity pipelineLayout);
}

public sealed record MaterialConfig
{
    public required MeshDescriptor VertexDescriptor { get; init; }
    public required BindingLayout[][] BindGroupLayouts { get; init; }
    public TextureFormat[] ColorFormats { get; init; } = [TextureFormat.Rgba8Unorm];
    public BlendState? Blend { get; init; }
    public DepthStencilState? DepthStencil { get; init; }
}
```

材质实现从 8 个成员缩减为 3 个：

```csharp
// PBRMaterial 示例
public static MaterialConfig Config => new()
{
    VertexDescriptor = new MeshDescriptor([...]),
    BindGroupLayouts = [[...], [...]],
    ColorFormats = [TextureFormat.Rgba8Unorm, TextureFormat.Rgba16float, TextureFormat.Rgba8Unorm],
    DepthStencil = new DepthStencilState { ... }
};
```

`Materials.Create<T>()` 和 `DeferredLightingMaterial.CreatePipeline()` 从读取 5 个属性改为读取 1 个 `T.Config`。

### 变更清单

| 文件 | 变更 |
|------|------|
| `Material/MaterialConfig.cs` | **新增** record |
| `Material/IMaterial.cs` | 8 static abstract 成员 → 3 个 (Name + Config + 2 实例方法) |
| `Material/PBRMaterial.cs` | 迁移到 `Config` 属性 |
| `Material/UnlitMaterial.cs` | 迁移到 `Config` 属性 |
| `Material/DeferredLightingMaterial.cs` | 迁移到 `Config` 属性 |
| `Resources/Materials.cs` | `Create<T>()` 读取 `T.Config` |
| `examples/.../ExampleApp.cs` (×2) | `PBRMaterial.VertexDescriptor` → `PBRMaterial.Config.VertexDescriptor` 等 |

---

## 阶段五：补充测试覆盖

### 新建项目：[Dumb.Graphics.Tests](src/Dumb.Graphics.Tests/)

xUnit + NSubstitute，64 个测试，覆盖重构后的核心模块：

| 测试文件 | 数量 | 测试对象 |
|----------|------|----------|
| `GpuResourceRefCountTests.cs` | 7 | RefCount 生命周期：创建(=1)、Retain(+1)、Release(-1)、归零销毁、多持有者 |
| `MaterialConfigTests.cs` | 13 | PBR/Unlit/DeferredLighting 三种材质的 Config 正确性 |
| `RenderGraphTests.cs` | 13 | 图编译：空图、生产者-消费者管线、重复输出检测、顺序错误、节点增删清空 |
| `MeshManagerTests.cs` | 15 | `ToVertexBufferLayouts` 顶点布局转换、`MeshData.Validate` 校验、`FromVertices`/`CreateQuad` |
| `BindingLayoutTests.cs` | 18 | BindingLayout 工厂方法、Binding 值对象、VertexAttributeLayout 等值类型常量 |

### Dumb.Engine.Tests 扩展（从 14 → 43 测试）

| 测试文件 | 数量 | 测试对象 |
|----------|------|----------|
| `MeshBuilderTests.cs` | 16 | MeshBuilder 顶点/索引累积、Build()、Clear()、MeshDescriptor.TryFindAttribute、ComputeAabb |
| `InputSystemTests.cs` | 7 | Stub 驱动：键盘按下/释放、鼠标移动/滚轮/按钮、帧切换状态复制 |
| `WindowSystemTests.cs` | 7 | Stub 驱动：Resize 更新、ShouldClose、多事件累积 |

**关键设计决策**：使用手写 Stub（`StubInputBackend` / `StubWindowBackend`）而非 NSubstitute mock 对象，因为 ECS（Sia）组件存储对代理类型可能不兼容。

### 变更清单

| 文件 | 变更 |
|------|------|
| `src/Dumb.Graphics.Tests/` | **新增** 整个测试项目（csproj + 5 个测试文件） |
| `src/Dumb.Engine.Tests/Mesh/MeshBuilderTests.cs` | **新增** |
| `src/Dumb.Engine.Tests/Input/InputSystemTests.cs` | **新增** |
| `src/Dumb.Engine.Tests/Window/WindowSystemTests.cs` | **新增** |
| `src/Dumb.Engine.Tests/Stubs/TestBackends.cs` | **新增** 共享测试桩 |
| `Dumb.slnx` | 添加 `Dumb.Graphics.Tests` 项目引用 |

---

## 架构决策记录

### 1. 编译期安全 > 运行时灵活

- 用泛型 + ref-return 抽象方法（而非反射）解决 C# 泛型无法访问 struct 字段的限制
- 接口和抽象类在编译期约束实现正确性

### 2. 显式依赖 > 隐式全局

- 所有 Manager 通过 `GraphicsContext` 属性暴露，不依赖 `GraphicsContext.Current` 等静态状态
- `WindowExtensions.CreateWindow` 接受可选的后端注入参数

### 3. 删除代码 > 添加代码

- 6 个后端文件 → 3 个统一文件（净减少 3 个文件）
- 7 个 Manager 中重复的 Release/Retain → 1 个基类
- IMaterial 每材质 6 个 static 成员 → 2 个

### 4. .NET 10 特性利用

- `static abstract` 接口成员（IMaterial）
- `sealed record`（MaterialConfig）
- `params ReadOnlySpan<T>` / `stackalloc` / `ref return`
- `required` / `init` 属性
- 主构造函数（`class BufferManager(GraphicsContext ctx)`）

---

## 测试验证

```bash
# 运行全部测试
dotnet test src/Dumb.Engine.Tests/   # 43 tests ✅
dotnet test src/Dumb.Graphics.Tests/ # 64 tests ✅

# 构建
dotnet build Dumb.slnx               # 0 errors
```
