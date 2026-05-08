namespace Dumb.Graphics.Example;

public static class Program
{
    public static async Task Main()
    {
        using var app = new GraphicsExampleApp();
        await app.RunAsync();
    }
}
