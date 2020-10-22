using Microsoft.CodeAnalysis;

namespace ExpressionDelegates.Generation
{
    public static class SymbolFormat
    {
        public static SymbolDisplayFormat FullName { get; } = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);
    }
}