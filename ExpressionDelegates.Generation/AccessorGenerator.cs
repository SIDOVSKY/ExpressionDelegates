using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Uno.SourceGeneration;
using ISourceGenerator = Uno.SourceGeneration.ISourceGenerator;

namespace ExpressionDelegates.Generation
{
    public class AccessorGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            var registrationLines = new List<string>();

            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(tree);

                var memberExpressions = SyntaxHelper.ExtractExpressionLambdas(tree, model)
                    .SelectMany(n => n.DescendantNodes())
                    .OfType<MemberAccessExpressionSyntax>();

                foreach (var memberAccess in memberExpressions)
                {
                    var symbol = model.GetSymbolInfo(memberAccess).Symbol;
                    if (symbol == null)
                        continue;

                    if (symbol.DeclaredAccessibility < Accessibility.Internal)
                        continue;

                    ITypeSymbol memberType;
                    switch (symbol)
                    {
                        case IPropertySymbol propertySymbol when !propertySymbol.IsWriteOnly:
                            memberType = propertySymbol.Type;
                            break;

                        case IFieldSymbol fieldSymbol:
                            memberType = fieldSymbol.Type;
                            break;

                        default: continue;
                    }

                    var targetFullType = symbol.ContainingType.ToDisplayString(SymbolFormat.FullName);
                    var memberFullType = memberType.ToDisplayString(SymbolFormat.FullName);
                    var targetPath = symbol.ToDisplayString(SymbolFormat.FullName);

                    var getter = symbol.IsStatic
                        ? $"o => {targetFullType}.{symbol.Name}"
                        : $"o => (({targetFullType})o).{symbol.Name}";

                    var setter = symbol switch
                    {
                        IPropertySymbol { IsReadOnly: true } => "null",
                        IFieldSymbol { IsConst: true } => "null",
                        IFieldSymbol { IsReadOnly: true } => "null",
                        { IsStatic: true } => $"(t, m) => {targetFullType}.{symbol.Name} = ({memberFullType})m",
                        _ => $"(t, m) => (({targetFullType})t).{symbol.Name} = ({memberFullType})m"
                    };

                    registrationLines.Add(
                        $@"{nameof(Accessors.Add)}(""{targetPath}"", {getter}, {setter});");
                }
            }

            registrationLines.Sort();

            var sourceBuilder = new StringBuilder(
$@"using static {typeof(Accessors).Namespace}.{nameof(Accessors)};

namespace {nameof(ExpressionDelegates)}.AccessorRegistration
{{
    public static class ModuleInitializer
    {{
        public static void Initialize()
        {{");

            foreach (var line in registrationLines.Distinct())
            {
                sourceBuilder.AppendLine();
                sourceBuilder.Append(' ', 12).Append(line);
            }

            sourceBuilder.Append(@"
        }
    }
}");

            var registratorCode = sourceBuilder.ToString();
            context.AddSource("AccessorRegistration", SourceText.From(registratorCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}