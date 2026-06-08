namespace Dumb.Emscripten.WGPUGenerator;

public static class WgpuHeaderComparer
{
    public static WgpuHeaderComparison Compare(WgpuHeader source, WgpuHeader silk) =>
        new(CompareEnums(source, silk), CompareStructs(source, silk));

    private static WgpuEnumComparison[] CompareEnums(WgpuHeader source, WgpuHeader silk)
    {
        var silkEnums = silk.Enums.ToDictionary(static @enum => @enum.Name, StringComparer.Ordinal);

        return source.Enums
            .Where(@enum => silkEnums.ContainsKey(@enum.Name))
            .Select(@enum => CompareEnum(@enum, silkEnums[@enum.Name]))
            .ToArray();
    }

    private static WgpuEnumComparison CompareEnum(WgpuEnum source, WgpuEnum silk)
    {
        var silkValues = silk.Values.ToDictionary(static value => value.Name, StringComparer.Ordinal);
        var maps = source.Values
            .Where(value => silkValues.ContainsKey(value.Name))
            .Select(value => new WgpuEnumValueMap(value, silkValues[value.Name]))
            .ToArray();

        return new WgpuEnumComparison(source, silk, maps);
    }

    private static WgpuStructComparison[] CompareStructs(WgpuHeader source, WgpuHeader silk)
    {
        var silkStructs = silk.Structs.ToDictionary(static @struct => @struct.Name, StringComparer.Ordinal);
        var convertibleTypes = new HashSet<string>(
            source.Enums.Select(static @enum => @enum.Name)
                .Concat(source.Structs.Select(static @struct => @struct.Name)),
            StringComparer.Ordinal);

        return source.Structs
            .Where(@struct => silkStructs.ContainsKey(@struct.Name))
            .Select(@struct => CompareStruct(@struct, silkStructs[@struct.Name], convertibleTypes))
            .ToArray();
    }

    private static WgpuStructComparison CompareStruct(WgpuStruct source, WgpuStruct silk, HashSet<string> convertibleTypes)
    {
        var silkFields = silk.Fields.ToDictionary(static field => field.Name, StringComparer.Ordinal);
        var maps = source.Fields
            .Where(field => silkFields.ContainsKey(field.Name) && CanConvert(field.Type, silkFields[field.Name].Type, convertibleTypes))
            .Select(field => new WgpuFieldMap(field, silkFields[field.Name]))
            .ToArray();

        return new WgpuStructComparison(source, silk, maps);
    }

    private static bool CanConvert(string sourceType, string silkType, HashSet<string> convertibleTypes)
    {
        if (sourceType == silkType)
            return true;

        var sourceCsType = WgpuTypeTranslator.Translate(sourceType);
        var silkCsType = WgpuTypeTranslator.TranslateSilk(silkType);

        if (sourceCsType == silkCsType)
            return true;

        if (IsPointer(sourceCsType) && IsPointer(silkCsType))
            return true;

        return convertibleTypes.Contains(UnwrapPointer(sourceCsType)) &&
            WgpuTypeTranslator.TranslateSilk(sourceType) == silkCsType;
    }

    private static bool IsPointer(string type) =>
        type.EndsWith("*", StringComparison.Ordinal);

    private static string UnwrapPointer(string type) =>
        IsPointer(type) ? type.Substring(0, type.Length - 1) : type;
}
