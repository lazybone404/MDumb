using ClangSharp.Interop;

namespace Dumb.Emscripten.WGPUGenerator;

public static class ClangWgpuHeaderParser
{
    public static WgpuHeader Parse(string source) =>
        WithTranslationUnit(source, translationUnit =>
        {
            ThrowOnParseErrors(translationUnit);
            return CreateHeader(translationUnit.Cursor);
        });

    private static WgpuHeader CreateHeader(CXCursor root)
    {
        var children = root.Children().ToArray();

        return new WgpuHeader(
            children.Where(IsEnumDeclaration).Select(CreateEnum).ToArray(),
            children.Where(IsStructDeclaration).Select(CreateStruct).ToArray(),
            children.Select(CreateCallback).Where(static callback => callback is not null).Select(static callback => callback!).ToArray());
    }

    private static WgpuEnum CreateEnum(CXCursor cursor) =>
        new(
            cursor.Spelling.CString,
            cursor.Children()
                .Where(static child => child.Kind == CXCursorKind.CXCursor_EnumConstantDecl)
                .Select(child => new WgpuEnumValue(
                    WgpuNameTransforms.NormalizeEnumValueName(child.Spelling.CString, cursor.Spelling.CString + "_"),
                    child.EnumConstantDeclValue.ToString()))
                .ToArray());

    private static WgpuStruct CreateStruct(CXCursor cursor) =>
        new(
            WgpuNameTransforms.NormalizeStructName(cursor.Spelling.CString),
            cursor.Children()
                .Where(static child => child.Kind == CXCursorKind.CXCursor_FieldDecl)
                .Select(static child => new WgpuField(
                    WgpuNameTransforms.ToPascalCase(child.Spelling.CString),
                    WgpuTypeTranslator.NormalizeCType(child.Type.Spelling.CString)))
                .ToArray());

    private static WgpuCallback? CreateCallback(CXCursor cursor) =>
        IsCallbackTypedef(cursor)
            ? CreateCallback(cursor, GetFunctionType(cursor.TypedefDeclUnderlyingType))
            : null;

    private static WgpuCallback? CreateCallback(CXCursor cursor, CXType functionType) =>
        IsFunctionType(functionType)
            ? new WgpuCallback(
                cursor.Spelling.CString,
                WgpuTypeTranslator.NormalizeCType(functionType.ResultType.Spelling.CString),
                CreateParameters(functionType, cursor.Children().ToArray()).ToArray())
            : null;

    private static IEnumerable<WgpuParameter> CreateParameters(CXType functionType, CXCursor[] children) =>
        Enumerable.Range(0, functionType.NumArgTypes)
            .Select(index => CreateParameter(index, functionType.GetArgType((uint)index), children));

    private static WgpuParameter CreateParameter(int index, CXType type, CXCursor[] children) =>
        new(GetParameterName(index, children), WgpuTypeTranslator.NormalizeCType(type.Spelling.CString));

    private static string GetParameterName(int index, CXCursor[] children) =>
        children
            .Where(static child => child.Kind == CXCursorKind.CXCursor_ParmDecl)
            .Select(static child => child.Spelling.CString)
            .ElementAtOrDefault(index) ?? $"arg{index}";

    private static CXType GetFunctionType(CXType underlyingType) =>
        underlyingType.kind == CXTypeKind.CXType_Pointer
            ? underlyingType.PointeeType
            : underlyingType;

    private static bool IsEnumDeclaration(CXCursor cursor) =>
        cursor.Kind == CXCursorKind.CXCursor_EnumDecl &&
        cursor.NumEnumerators > 0 &&
        IsWgpuName(cursor.Spelling.CString);

    private static bool IsStructDeclaration(CXCursor cursor) =>
        cursor.Kind == CXCursorKind.CXCursor_StructDecl &&
        cursor.NumFields > 0 &&
        IsWgpuName(cursor.Spelling.CString);

    private static bool IsCallbackTypedef(CXCursor cursor) =>
        cursor.Kind == CXCursorKind.CXCursor_TypedefDecl && WgpuNames.CallbackSet.Contains(cursor.Spelling.CString);

    private static bool IsWgpuName(string name) =>
        name.StartsWith("WGPU", StringComparison.Ordinal);

    private static bool IsFunctionType(CXType type) =>
        type.kind is CXTypeKind.CXType_FunctionProto or CXTypeKind.CXType_FunctionNoProto;

    private static TResult WithTranslationUnit<TResult>(string source, Func<CXTranslationUnit, TResult> useTranslationUnit)
    {
        var index = CXIndex.Create(excludeDeclarationsFromPch: false, displayDiagnostics: false);
        var unsavedFile = CXUnsavedFile.Create(WgpuNames.HEADER_FILE_NAME, source);

        try
        {
            var translationUnit = CXTranslationUnit.Parse(
                index,
                WgpuNames.HEADER_FILE_NAME,
                CreateParseArguments(),
                new[] { unsavedFile },
                CXTranslationUnit_Flags.CXTranslationUnit_None);

            try
            {
                return useTranslationUnit(translationUnit);
            }
            finally
            {
                translationUnit.Dispose();
            }
        }
        finally
        {
            unsavedFile.Dispose();
            index.Dispose();
        }
    }

    private static string[] CreateParseArguments() =>
    [
        "-x",
        "c",
        "-std=c11",
        "-DWGPU_SHARED_LIBRARY",
        "-D_WIN32",
        "-DWGPU_SKIP_PROCS",
    ];

    private static void ThrowOnParseErrors(CXTranslationUnit translationUnit)
    {
        var errors = translationUnit.DiagnosticSet
            .Where(static diagnostic => diagnostic.Severity >= CXDiagnosticSeverity.CXDiagnostic_Error)
            .Select(static diagnostic => diagnostic.Format(CXDiagnostic.DefaultDisplayOptions).CString)
            .ToArray();

        if (errors.Length != 0)
        {
            throw new InvalidOperationException(
                $"Failed to parse {WgpuNames.HEADER_FILE_NAME}:{WgpuNames.NEW_LINE}{string.Join(WgpuNames.NEW_LINE, errors)}");
        }
    }
}
