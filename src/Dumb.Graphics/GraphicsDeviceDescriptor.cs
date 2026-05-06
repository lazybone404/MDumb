namespace Shit.Graphics;

public sealed record GraphicsDeviceDescriptor(
    string Backend,
    string? AdapterName = null,
    bool EnableValidation = false);
