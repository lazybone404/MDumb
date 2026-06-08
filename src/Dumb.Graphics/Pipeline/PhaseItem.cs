using Sia;

namespace Dumb.Graphics.Pipeline;

public readonly record struct PhaseItem(
    Entity DrawEntity,
    Entity Pipeline,
    Entity PipelineLayout,
    Entity?[] BindGroups,
    Entity Mesh,
    uint SubMeshIndex,
    float SortKey = 0,
    uint ModelOffset = 0
);
