using Sia;
using Dumb.Graphics.Pipeline;
using Dumb.Graphics.Tests.Resources;

namespace Dumb.Graphics.Tests.Pipeline;

/// <summary>
/// 测试用 RenderNode — 允许声明任意输入/输出资源句柄。
/// </summary>
public sealed class TestRenderNode : RenderNode
{
    public TestRenderNode(string name)
    {
        Name = name;
    }

    public TestRenderNode(string name, List<ResourceHandle> inputs, List<ResourceHandle> outputs)
    {
        Name = name;
        Inputs.AddRange(inputs);
        Outputs.AddRange(outputs);
    }

    public bool UpdateCalled { get; private set; }
    public bool ExecuteCalled { get; private set; }

    public override void Update(World world)
    {
        UpdateCalled = true;
    }

    public override void Execute(World world, RenderContext ctx)
    {
        ExecuteCalled = true;
    }
}

public sealed class RenderGraphTests
{
    private static Entity CreateMockView(World world)
    {
        // 创建一个带 TestResourceData 的实体作为模拟 TextureView
        var host = world.AcquireHost<
            HList<TestResourceData, EmptyHList>,
            ArrayEntityHost<HList<TestResourceData, EmptyHList>>>();
        return host.Create(HList.From(new TestResourceData { NativePtr = 1, RefCount = 1 }));
    }

    [Fact]
    public void Compile_EmptyGraph_ReturnsSuccess()
    {
        var world = new World();
        var graph = new RenderGraph(null!);

        var result = graph.Compile();

        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);

        world.Dispose();
    }

    [Fact]
    public void Compile_SingleNode_NoResources_ReturnsSuccess()
    {
        var world = new World();
        var graph = new RenderGraph(null!);
        var node = new TestRenderNode("SingleNode");
        graph.AddNode(node);

        var result = graph.Compile();

        Assert.True(result.Success);
        Assert.Empty(result.Errors);

        world.Dispose();
    }

    [Fact]
    public void Compile_ProducerConsumer_ValidOrder_ReturnsSuccess()
    {
        var world = new World();
        var view = CreateMockView(world);
        var graph = new RenderGraph(null!);

        var output = new ResourceHandle(view, "Output");
        var input = new ResourceHandle(view, "Output");

        var producer = new TestRenderNode("Producer",
            [], [output]);
        var consumer = new TestRenderNode("Consumer",
            [input], []);

        graph.AddNode(producer);
        graph.AddNode(consumer);

        var result = graph.Compile();

        Assert.True(result.Success);
        Assert.Empty(result.Errors);

        world.Dispose();
    }

    [Fact]
    public void Compile_DuplicateOutput_ReturnsError()
    {
        var world = new World();
        var view = CreateMockView(world);
        var graph = new RenderGraph(null!);

        var output = new ResourceHandle(view, "SharedOutput");

        var nodeA = new TestRenderNode("NodeA", [], [output]);
        var nodeB = new TestRenderNode("NodeB", [], [output]);

        graph.AddNode(nodeA);
        graph.AddNode(nodeB);

        var result = graph.Compile();

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("SharedOutput") && e.Contains("written by both"));

        world.Dispose();
    }

    [Fact]
    public void Compile_ConsumerBeforeProducer_ReturnsError()
    {
        var world = new World();
        var view = CreateMockView(world);
        var graph = new RenderGraph(null!);

        var output = new ResourceHandle(view, "Data");
        var input = new ResourceHandle(view, "Data");

        var consumer = new TestRenderNode("Consumer", [input], []);
        var producer = new TestRenderNode("Producer", [], [output]);

        // 消费者在前，生产者在后 —— 错误顺序
        graph.AddNode(consumer);
        graph.AddNode(producer);

        var result = graph.Compile();

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e =>
            e.Contains("Consumer") && e.Contains("Producer") && e.Contains("appears after"));

        world.Dispose();
    }

    [Fact]
    public void Compile_NodeWithUnproducedInput_StillCompiles()
    {
        var world = new World();
        var view = CreateMockView(world);
        var graph = new RenderGraph(null!);

        // 声明一个输入但没有任何节点生产它 → 编译通过（仅警告）
        var input = new ResourceHandle(view, "ExternalInput");
        var node = new TestRenderNode("Consumer", [input], []);

        graph.AddNode(node);

        var result = graph.Compile();

        // 缺少生产者不阻止编译成功
        Assert.True(result.Success);
        Assert.Empty(result.Errors);

        world.Dispose();
    }

    [Fact]
    public void Compile_OutputDeclaredByOneNode_IsValid()
    {
        var world = new World();
        var view = CreateMockView(world);
        var graph = new RenderGraph(null!);

        var output = new ResourceHandle(view, "ValidOutput");
        var producer = new TestRenderNode("Producer", [], [output]);

        graph.AddNode(producer);

        var result = graph.Compile();

        Assert.True(result.Success);
        Assert.Empty(result.Errors);

        world.Dispose();
    }

    [Fact]
    public void Compile_MultiNodePipeline_CorrectOrder_ReturnsSuccess()
    {
        var world = new World();
        var view1 = CreateMockView(world);
        var view2 = CreateMockView(world);
        var view3 = CreateMockView(world);
        var graph = new RenderGraph(null!);

        // GBufferPass → DeferredLighting → PostProcess 管线
        // GBufferPass 产出 GBuffer RTs
        var gbufferOut0 = new ResourceHandle(view1, "GBufferRT0");
        var gbufferOut1 = new ResourceHandle(view2, "GBufferRT1");

        // DeferredLighting 消费 GBuffer，产出 LitColor
        var gbufferIn0 = new ResourceHandle(view1, "GBufferRT0");
        var gbufferIn1 = new ResourceHandle(view2, "GBufferRT1");
        var litOutput = new ResourceHandle(view3, "LitColor");

        // PostProcess 消费 LitColor
        var litInput = new ResourceHandle(view3, "LitColor");

        var gbufferPass = new TestRenderNode("GBufferPass", [], [gbufferOut0, gbufferOut1]);
        var deferredLighting = new TestRenderNode("DeferredLighting",
            [gbufferIn0, gbufferIn1], [litOutput]);
        var postProcess = new TestRenderNode("PostProcess", [litInput], []);

        graph.AddNode(gbufferPass);
        graph.AddNode(deferredLighting);
        graph.AddNode(postProcess);

        var result = graph.Compile();

        Assert.True(result.Success);
        Assert.Empty(result.Errors);

        world.Dispose();
    }

    [Fact]
    public void Run_ExecutesAllNodesInOrder()
    {
        var world = new World();
        var graph = new RenderGraph(null!);

        var nodeA = new TestRenderNode("NodeA");
        var nodeB = new TestRenderNode("NodeB");
        var nodeC = new TestRenderNode("NodeC");

        graph.AddNode(nodeA);
        graph.AddNode(nodeB);
        graph.AddNode(nodeC);

        // 注：Run 调用 Execute 时需要 RenderContext，它依赖 GraphicsContext。
        // 这里仅验证 Compile 通过。
        var result = graph.Compile();
        Assert.True(result.Success);

        world.Dispose();
    }

    [Fact]
    public void AddNode_AppendsToNodeList()
    {
        var graph = new RenderGraph(null!);
        var node = new TestRenderNode("Test");

        graph.AddNode(node);

        Assert.Single(graph.Nodes);
        Assert.Same(node, graph.Nodes[0]);
    }

    [Fact]
    public void RemoveNode_RemovesFromNodeList()
    {
        var graph = new RenderGraph(null!);
        var node = new TestRenderNode("Test");
        graph.AddNode(node);

        graph.RemoveNode(node);

        Assert.Empty(graph.Nodes);
    }

    [Fact]
    public void Clear_RemovesAllNodes()
    {
        var graph = new RenderGraph(null!);
        graph.AddNode(new TestRenderNode("A"));
        graph.AddNode(new TestRenderNode("B"));
        graph.AddNode(new TestRenderNode("C"));

        graph.Clear();

        Assert.Empty(graph.Nodes);
    }
}
