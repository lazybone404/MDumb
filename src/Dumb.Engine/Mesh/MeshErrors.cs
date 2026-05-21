namespace Dumb.Engine.Mesh;

public sealed class MeshMergeException(string message) : InvalidOperationException(message);
public sealed class MeshWindingException(string message) : InvalidOperationException(message);
public sealed class GenerateTangentsException(string message) : InvalidOperationException(message);
public sealed class MeshAttributeException(string message) : InvalidOperationException(message);
