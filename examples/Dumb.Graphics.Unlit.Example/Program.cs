namespace Dumb.Unlit.Example;

public static class Program
{
    public static async Task Main()
    {
        using var app = new ExampleApp();
        await app.RunAsync();
    }
}
