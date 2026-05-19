namespace Dumb.Engine.Mesh;

public sealed class MeshMergeException : InvalidOperationException
{
    internal MeshMergeException(string message) : base(message) { }
}

public sealed class MeshWindingException : InvalidOperationException
{
    internal MeshWindingException(string message) : base(message) { }
}

public sealed class GenerateTangentsException : InvalidOperationException
{
    internal GenerateTangentsException(string message) : base(message) { }
}

public sealed class MeshAttributeException : InvalidOperationException
{
    internal MeshAttributeException(string message) : base(message) { }
}
