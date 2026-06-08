namespace Dumb.Graphics.Deferred.Example;

public static class Program
{
    public static async Task Main()
    {
        using var app = new ExampleApp();
        await app.RunAsync();
    }
}
