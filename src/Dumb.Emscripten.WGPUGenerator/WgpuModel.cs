namespace Dumb.Emscripten.WGPUGenerator;

internal sealed class WgpuHeader(WgpuEnum[] enums, WgpuStruct[] structs, WgpuCallback[] callbacks)
{
    public static readonly WgpuHeader Empty = new([], [], []);

    public WgpuEnum[] Enums { get; } = enums;

    public WgpuStruct[] Structs { get; } = structs;

    public WgpuCallback[] Callbacks { get; } = callbacks;
}

internal sealed class WgpuEnum(string name, WgpuEnumValue[] values)
{
    public string Name { get; } = name;

    public WgpuEnumValue[] Values { get; } = values;
}

internal sealed class WgpuEnumValue(string name, string value)
{
    public string Name { get; } = name;

    public string Value { get; } = value;
}

internal sealed class WgpuStruct(string name, WgpuField[] fields)
{
    public string Name { get; } = name;

    public WgpuField[] Fields { get; } = fields;
}

internal sealed class WgpuField(string name, string type)
{
    public string Name { get; } = name;

    public string Type { get; } = type;
}

internal sealed class WgpuCallback(string name, string returnType, WgpuParameter[] parameters)
{
    public string Name { get; } = name;

    public string ReturnType { get; } = returnType;

    public WgpuParameter[] Parameters { get; } = parameters;
}

internal sealed class WgpuParameter(string name, string type)
{
    public string Name { get; } = name;

    public string Type { get; } = type;
}

internal sealed class WgpuHeaderComparison(WgpuEnumComparison[] enums, WgpuStructComparison[] structs)
{
    public WgpuEnumComparison[] Enums { get; } = enums;

    public WgpuStructComparison[] Structs { get; } = structs;
}

internal sealed class WgpuEnumComparison(WgpuEnum source, WgpuEnum silk, WgpuEnumValueMap[] valueMaps)
{
    public WgpuEnum Source { get; } = source;

    public WgpuEnum Silk { get; } = silk;

    public WgpuEnumValueMap[] ValueMaps { get; } = valueMaps;

    public bool HasValueDifferences => ValueMaps.Any(static map => map.Source.Value != map.Silk.Value);
}

internal sealed class WgpuEnumValueMap(WgpuEnumValue source, WgpuEnumValue silk)
{
    public WgpuEnumValue Source { get; } = source;

    public WgpuEnumValue Silk { get; } = silk;
}

internal sealed class WgpuStructComparison(WgpuStruct source, WgpuStruct silk, WgpuFieldMap[] fieldMaps)
{
    public WgpuStruct Source { get; } = source;

    public WgpuStruct Silk { get; } = silk;

    public WgpuFieldMap[] FieldMaps { get; } = fieldMaps;

    public bool IsFieldCompatible =>
        Source.Fields.Length == Silk.Fields.Length &&
        FieldMaps.Length == Source.Fields.Length;

    public bool IsLayoutCompatible =>
        IsFieldCompatible &&
        FieldMaps.All(static map =>
            WgpuTypeTranslator.Translate(map.Source.Type) ==
            WgpuTypeTranslator.Translate(map.Silk.Type));
}

internal sealed class WgpuFieldMap(WgpuField source, WgpuField silk)
{
    public WgpuField Source { get; } = source;

    public WgpuField Silk { get; } = silk;
}
