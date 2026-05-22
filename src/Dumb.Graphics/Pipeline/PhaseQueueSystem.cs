using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sia;
using Silk.NET.WebGPU;

namespace Dumb.Graphics.Pipeline;

public sealed class PhaseQueueSystem : ExtractSystemBase
{
    private readonly GraphicsContext _ctx;
    private readonly CameraSyncSystem _cameraSync;
    private readonly TransformSyncSystem _transformSync;
    private readonly LightSyncSystem _lightSync;

    private readonly Dictionary<int, Engine.Mesh.MeshData> _meshRegistry = [];
    private readonly Dictionary<int, Entity> _gpuMeshMap = [];
    private readonly Dictionary<int, Entity> _gpuMaterialMap = [];

    private readonly HashSet<int> _seenIds = [];
    private readonly List<int> _removedIds = [];

    private Entity? _frameBindGroup;
    private int _frameBindGroupKey;
    private Entity? _frameBindGroupLayout;

    public BinnedRenderPhase OpaquePhase { get; } = new();
    public SortedRenderPhase TransparentPhase { get; } = new();

    public Entity? FrameBindGroupLayout
    {
        get => _frameBindGroupLayout;
        set => _frameBindGroupLayout = value;
    }

    public PhaseQueueSystem(
        GraphicsContext ctx,
        CameraSyncSystem cameraSync,
        TransformSyncSystem transformSync,
        LightSyncSystem lightSync)
        : base(Matchers.Any, extractMatcher: Matchers.Of<Engine.Mesh.VisibleEntity>())
    {
        _ctx = ctx;
        _cameraSync = cameraSync;
        _transformSync = transformSync;
        _lightSync = lightSync;
    }

    public void RegisterMesh(Entity engineEntity, Engine.Mesh.MeshData meshData)
    {
        _meshRegistry[engineEntity.Id.Value] = meshData;
    }

    public void RegisterMaterial(Entity engineEntity, Entity gpuMaterialEntity)
    {
        _gpuMaterialMap[engineEntity.Id.Value] = gpuMaterialEntity;
    }

    public void UnregisterEntity(Entity engineEntity)
    {
        var id = engineEntity.Id.Value;
        _meshRegistry.Remove(id);
        if (_gpuMeshMap.Remove(id, out var gpuMesh))
            Mesh.Release(_ctx, gpuMesh);
        if (_gpuMaterialMap.Remove(id, out var gpuMat))
            Material.Release(_ctx, gpuMat);
    }

    public override void Execute(World world, IEntityQuery query, IEntityQuery extract)
    {
        OpaquePhase.Clear();
        TransparentPhase.Clear();
        _seenIds.Clear();

        extract.ForSlice((Entity entity, ref Engine.Mesh.VisibleEntity visible) =>
        {
            _seenIds.Add(entity.Id.Value);

            if (!_meshRegistry.TryGetValue(entity.Id.Value, out var meshData))
                return;

            if (!TryGetOrCreateGpuMesh(entity.Id.Value, meshData, out var gpuMesh))
                return;

            if (!_gpuMaterialMap.TryGetValue(entity.Id.Value, out var gpuMaterial))
                return;

            if (!_transformSync.TryGetOffset(entity, out var modelOffset))
                return;

            ref var matData = ref gpuMaterial.Get<MaterialResourceData>();

            var frameBg = GetOrCreateFrameBindGroup();
            if (frameBg is null) return;

            var bindGroups = new Entity?[matData.BindGroups.Length];
            bindGroups[0] = frameBg;
            for (var i = 1; i < matData.BindGroups.Length; i++)
                bindGroups[i] = matData.BindGroups[i];

            var binKey = ((ulong)matData.Pipeline.Id.Value << 32)
                       | ((ulong)gpuMaterial.Id.Value & 0xFFFFFFFF);

            var item = new PhaseItem(
                DrawEntity: gpuMaterial,
                Pipeline: matData.Pipeline,
                PipelineLayout: matData.PipelineLayout,
                BindGroups: bindGroups,
                Mesh: gpuMesh,
                SubMeshIndex: 0,
                ModelOffset: modelOffset
            );

            OpaquePhase.Add(item, binKey);
        });

        CleanupRemovedEntities();
    }

    private Entity? GetOrCreateFrameBindGroup()
    {
        if (_frameBindGroupLayout is not { } frameBgl)
            return null;

        var cameraBuffer = _cameraSync.FirstBuffer;
        if (cameraBuffer is not { } camBuf)
            return null;

        var bindGroupKey = HashCode.Combine(
            camBuf.Id.Value,
            _transformSync.ModelBuffer.Id.Value);

        if (_frameBindGroup is not null && _frameBindGroupKey == bindGroupKey)
            return _frameBindGroup;

        var modelBuffer = _transformSync.ModelBuffer;

        Binding[] bindings =
        [
            Binding.Uniform<CameraUniforms>(0, camBuf),
            Binding.Buffer(2, modelBuffer, (nuint)Unsafe.SizeOf<Matrix4x4>()),
        ];

        if (_frameBindGroup is { } oldBg)
            Pipelines.ReleaseBindGroup(_ctx, oldBg);

        _frameBindGroup = Pipelines.BindGroup(_ctx, frameBgl, bindings);
        _frameBindGroupKey = bindGroupKey;

        return _frameBindGroup;
    }

    private bool TryGetOrCreateGpuMesh(int entityId, Engine.Mesh.MeshData data, out Entity gpuMesh)
    {
        if (_gpuMeshMap.TryGetValue(entityId, out gpuMesh!))
            return true;

        try
        {
            gpuMesh = Mesh.Create(_ctx, data);
            _gpuMeshMap[entityId] = gpuMesh;
            return true;
        }
        catch
        {
            gpuMesh = null!;
            return false;
        }
    }

    private void CleanupRemovedEntities()
    {
        _removedIds.Clear();

        foreach (var id in _gpuMeshMap.Keys)
            if (!_seenIds.Contains(id)) _removedIds.Add(id);

        foreach (var id in _gpuMaterialMap.Keys)
            if (!_seenIds.Contains(id) && !_removedIds.Contains(id)) _removedIds.Add(id);

        foreach (var id in _removedIds)
        {
            if (_gpuMeshMap.Remove(id, out var gpuMesh))
                Mesh.Release(_ctx, gpuMesh);
            if (_gpuMaterialMap.Remove(id, out var gpuMat))
                Material.Release(_ctx, gpuMat);
            _meshRegistry.Remove(id);
        }
    }
}
