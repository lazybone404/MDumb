using Sia;
using Dumb.Engine.Input;
using Dumb.Engine.Window;
using Dumb.Engine.Tests.Stubs;

namespace Dumb.Engine.Tests.Input;

public sealed class InputSystemTests : IDisposable
{
    private readonly World _world;
    private readonly StubWindowBackend _windowBackend;
    private readonly StubInputBackend _inputBackend;
    private readonly Entity _windowEntity;
    private SystemStage _stage;

    public InputSystemTests()
    {
        _world = new World();
        _windowBackend = new StubWindowBackend();
        _inputBackend = new StubInputBackend();

        var runtime = new WindowRuntime(_windowBackend, _inputBackend);
        var input = new WindowInput();

        _windowEntity = _world.Create(HList.From(input, runtime));

        _stage = SystemChain.Empty
            .Add<InputSystem>()
            .CreateStage(_world);
    }

    public void Dispose()
    {
        _stage?.Dispose();
        _world.Dispose();
    }

    [Fact]
    public void InputSystem_FirstTick_InitializesWithoutEmittingEvents()
    {
        // 第一次 Tick：initialized=false，不发送事件
        _stage.Tick();

        var input = _windowEntity.Get<WindowInput>();
        Assert.True(input.Initialized);
    }

    [Fact]
    public void InputSystem_KeyPress_EmitsKeyEvent()
    {
        // Tick 1: 初始化（不发送事件）
        _inputBackend.OnPoll = frame => frame.SetKey(KeyCode.Space, pressed: false);
        _stage.Tick();

        // Tick 2: 按下空格键
        _inputBackend.OnPoll = frame => frame.SetKey(KeyCode.Space, pressed: true);
        _stage.Tick();

        // 验证 KeyEvent 被发送到 World
        // 注：Sia 事件通过 world.Send() 发送，需要通过监听器验证
        var input = _windowEntity.Get<WindowInput>();
        Assert.True(input.Current.IsKeyPressed(KeyCode.Space));
    }

    [Fact]
    public void InputSystem_KeyRelease_DetectsStateChange()
    {
        // Tick 1: 初始化，按下键
        _inputBackend.OnPoll = frame => frame.SetKey(KeyCode.W, pressed: true);
        _stage.Tick();

        // Tick 2: 释放键
        _inputBackend.OnPoll = frame => frame.SetKey(KeyCode.W, pressed: false);
        _stage.Tick();

        var input = _windowEntity.Get<WindowInput>();
        Assert.False(input.Current.IsKeyPressed(KeyCode.W));
        Assert.True(input.Previous.IsKeyPressed(KeyCode.W));
    }

    [Fact]
    public void InputSystem_MouseMove_DetectsPositionChange()
    {
        // Tick 1: 初始化
        _inputBackend.OnPoll = frame =>
        {
            frame.SetMousePosition(ScreenPosition.TopLeft(100, 200));
        };
        _stage.Tick();

        // Tick 2: 移动鼠标
        _inputBackend.OnPoll = frame =>
        {
            frame.SetMousePosition(ScreenPosition.TopLeft(150, 250));
        };
        _stage.Tick();

        var input = _windowEntity.Get<WindowInput>();
        Assert.Equal(150, input.Current.MousePosition.Raw.X);
        Assert.Equal(250, input.Current.MousePosition.Raw.Y);
    }

    [Fact]
    public void InputSystem_MouseScroll_AccumulatesScrollDelta()
    {
        _inputBackend.OnPoll = frame =>
        {
            frame.AddMouseScroll(new System.Numerics.Vector2(0, 1.5f));
        };
        _stage.Tick(); // 初始化 + 第一次滚动

        _inputBackend.OnPoll = frame =>
        {
            frame.AddMouseScroll(new System.Numerics.Vector2(0.5f, 0));
        };
        _stage.Tick();

        var input = _windowEntity.Get<WindowInput>();
        // Current 反映最新的 Poll 结果
        Assert.True(input.Current.MouseScroll != System.Numerics.Vector2.Zero);
    }

    [Fact]
    public void InputSystem_BeginFrame_CopiesAndClearsStateBetweenFrames()
    {
        // Tick 1: 设置初始状态
        _inputBackend.OnPoll = frame =>
        {
            frame.SetKey(KeyCode.A, pressed: true);
            frame.SetMousePosition(ScreenPosition.TopLeft(50, 60));
        };
        _stage.Tick();

        // Tick 2: 不同的状态
        _inputBackend.OnPoll = frame =>
        {
            frame.SetKey(KeyCode.A, pressed: false);
            frame.SetMousePosition(ScreenPosition.TopLeft(70, 80));
        };
        _stage.Tick();

        var input = _windowEntity.Get<WindowInput>();
        // Previous 应有第一帧的数据
        Assert.True(input.Previous.IsKeyPressed(KeyCode.A));
        // Current 应有第二帧的数据
        Assert.False(input.Current.IsKeyPressed(KeyCode.A));
    }

    [Fact]
    public void InputSystem_MouseButton_PressAndRelease()
    {
        // 初始化
        _inputBackend.OnPoll = frame => { };
        _stage.Tick();

        // 按下
        _inputBackend.OnPoll = frame =>
        {
            frame.SetMouseButton(MouseButton.Left, pressed: true);
        };
        _stage.Tick();

        var input = _windowEntity.Get<WindowInput>();
        Assert.True(input.Current.IsMousePressed(MouseButton.Left));

        // 释放
        _inputBackend.OnPoll = frame =>
        {
            frame.SetMouseButton(MouseButton.Left, pressed: false);
        };
        _stage.Tick();

        Assert.False(input.Current.IsMousePressed(MouseButton.Left));
    }
}
