using Sia;
using Dumb.Engine.Window;
using Dumb.Engine.Tests.Stubs;

namespace Dumb.Engine.Tests.Window;

public sealed class WindowSystemTests : IDisposable
{
    private readonly World _world;
    private readonly StubWindowBackend _windowBackend;
    private readonly StubInputBackend _inputBackend;
    private readonly Entity _windowEntity;
    private SystemStage _stage;

    public WindowSystemTests()
    {
        _world = new World();
        _windowBackend = new StubWindowBackend();
        _inputBackend = new StubInputBackend();

        var runtime = new WindowRuntime(_windowBackend, _inputBackend);
        var state = new WindowState(800, 600, 800, 600);

        _windowEntity = _world.Create(HList.From(state, runtime));

        _stage = SystemChain.Empty
            .Add<WindowSystem>()
            .CreateStage(_world);
    }

    public void Dispose()
    {
        _stage?.Dispose();
        _world.Dispose();
    }

    [Fact]
    public void WindowSystem_FirstTick_InitializesWithoutEmittingEvents()
    {
        _stage.Tick();

        Assert.True(_windowEntity.Get<WindowRuntime>().Initialized);
    }

    [Fact]
    public void WindowSystem_Resize_UpdatesWindowState()
    {
        var resizeCalled = false;
        _windowBackend.OnPump = sink =>
        {
            if (!resizeCalled)
            {
                sink.Resize(1920, 1080, 1920, 1080);
                resizeCalled = true;
            }
        };

        // Tick 1: 初始化
        _stage.Tick();

        // Tick 2: 触发 resize（此时 Initialized=true，会发送事件）
        _windowBackend.OnPump = sink =>
        {
            sink.Resize(1920, 1080, 1920, 1080);
        };
        _stage.Tick();

        ref var state = ref _windowEntity.Get<WindowState>();
        Assert.Equal(1920, state.Width);
        Assert.Equal(1080, state.Height);
        Assert.Equal(1920, state.FramebufferWidth);
        Assert.Equal(1080, state.FramebufferHeight);
    }

    [Fact]
    public void WindowSystem_ShouldClose_SetsFlag()
    {
        var closeRequested = false;
        _windowBackend.OnPump = sink =>
        {
            if (!closeRequested)
            {
                sink.CloseRequested();
                closeRequested = true;
            }
        };

        // Tick 1: 初始化，关闭请求被消费但不设置 ShouldClose
        _stage.Tick();

        // Tick 2: 再次请求关闭
        _windowBackend.OnPump = sink =>
        {
            sink.CloseRequested();
        };
        _stage.Tick();

        ref var state = ref _windowEntity.Get<WindowState>();
        Assert.True(state.ShouldClose);
    }

    [Fact]
    public void WindowSystem_MultipleResizes_TracksLatestState()
    {
        // 初始化
        _stage.Tick();

        _windowBackend.OnPump = sink =>
        {
            sink.Resize(1024, 768, 2048, 1536);
        };
        _stage.Tick();

        _windowBackend.OnPump = sink =>
        {
            sink.Resize(3840, 2160, 3840, 2160);
        };
        _stage.Tick();

        ref var state = ref _windowEntity.Get<WindowState>();
        Assert.Equal(3840, state.Width);
        Assert.Equal(2160, state.Height);
        Assert.Equal(3840, state.FramebufferWidth);
        Assert.Equal(2160, state.FramebufferHeight);
    }

    [Fact]
    public void WindowSystem_ShouldClose_OnceSet_StaysTrue()
    {
        // 初始化
        _stage.Tick();

        // 请求关闭
        _windowBackend.OnPump = sink => sink.CloseRequested();
        _stage.Tick();

        ref var state = ref _windowEntity.Get<WindowState>();
        Assert.True(state.ShouldClose);

        // 再次 tick — ShouldClose 保持 true
        _windowBackend.OnPump = sink => { /* 无事件 */ };
        _stage.Tick();

        Assert.True(state.ShouldClose);
    }

    [Fact]
    public void WindowSystem_NoEvents_StateUnchanged()
    {
        // 初始化
        _stage.Tick();

        _windowBackend.OnPump = sink => { /* 无事件 */ };
        _stage.Tick();

        ref var state = ref _windowEntity.Get<WindowState>();
        // 默认状态不变（初始化时设置的值）
        Assert.Equal(800, state.Width);
        Assert.Equal(600, state.Height);
    }

    [Fact]
    public void WindowRuntime_ProvidesBackendProperties()
    {
        var runtime = _windowEntity.Get<WindowRuntime>();

        Assert.Same(_windowBackend, runtime.Window);
        Assert.Same(_inputBackend, runtime.Input);
        Assert.Equal((nint)0, runtime.NativeHandle);
    }
}
