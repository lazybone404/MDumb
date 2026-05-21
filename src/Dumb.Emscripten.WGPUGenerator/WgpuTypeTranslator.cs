namespace Dumb.Emscripten.WGPUGenerator;

public static class WgpuTypeTranslator
{
    public static string Translate(string type) =>
        NormalizeTypeName(type switch
        {
            "void" => "void",
            "void*" => "void*",
            "int" => "int",
            "int32_t" => "int",
            "uint8_t" => "byte",
            "uint16_t" => "ushort",
            "uint32_t" => "uint",
            "uint64_t" => "ulong",
            "size_t" => "nuint",
            "float" => "float",
            "double" => "double",
            "WGPUBool" => "uint",
            "WGPUOptionalBool" => "uint",
            "WGPUStringView" => "byte*",
            "char" => "byte",
            "char*" => "byte*",
            "uint32_t*" => "uint*",
            _ when type.Contains("(*)") => "nint",
            _ when WgpuNames.PointerSet.Contains(type) => $"{type}*",
            _ => type,
        });

    public static string TranslateSilk(string type) =>
        ToSilkTypeName(Translate(type));

    public static string NormalizeCType(string type) =>
        type.Replace("struct ", string.Empty)
            .Replace(" const", string.Empty)
            .Replace("const ", string.Empty)
            .Replace(" *", "*")
            .Trim();

    private static string NormalizeTypeName(string name) =>
        name.Replace(" ", string.Empty)
            .Replace("const", string.Empty)
            .Replace("Flags", string.Empty);

    private static string ToSilkTypeName(string name)
    {
        if (name.StartsWith("WGPU", StringComparison.Ordinal))
            return WgpuNames.SilkTypeName(name);

        if (name.StartsWith("WGPU", StringComparison.Ordinal) && name.EndsWith("*", StringComparison.Ordinal))
            return WgpuNames.SilkTypeName(name.Substring(0, name.Length - 1)) + "*";

        if (name.EndsWith("*", StringComparison.Ordinal))
        {
            var elementName = name.Substring(0, name.Length - 1);
            return elementName.StartsWith("WGPU", StringComparison.Ordinal)
                ? WgpuNames.SilkTypeName(elementName) + "*"
                : name;
        }

        return name;
    }
}
