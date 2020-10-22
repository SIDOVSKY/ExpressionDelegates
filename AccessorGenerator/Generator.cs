using AccessorGenerator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.SourceGeneration;
using ISourceGenerator = Uno.SourceGeneration.ISourceGenerator;

namespace AccessorGenerator
{
    public class Generator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debugger.Launch();

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
                        case IPropertySymbol propertySymbol:
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

                    var getter = symbol switch
                    {
                        IPropertySymbol { IsWriteOnly: true } => "null",
                        { IsStatic: true } => $"o => {targetFullType}.{symbol.Name}",
                        _ => $"o => (({targetFullType})o).{symbol.Name}"
                    };

                    var setter = symbol switch
                    {
                        IPropertySymbol { IsReadOnly: true } => "null",
                        IFieldSymbol { IsConst: true } => "null",
                        IFieldSymbol { IsReadOnly: true } => "null",
                        { IsStatic: true } => $"(t, m) => {targetFullType}.{symbol.Name} = ({memberFullType})m",
                        _ => $"(t, m) => (({targetFullType})t).{symbol.Name} = ({memberFullType})m"
                    };

                    registrationLines.Add(
                        $@"{nameof(ExpressionAccessors.Add)}(""{targetPath}"", {getter}, {setter});");
                    //TODO test for read-, write- only
                }
            }

            registrationLines.Sort();

            var sourceBuilder = new StringBuilder(
$@"using static {typeof(ExpressionAccessors).Namespace}.{nameof(ExpressionAccessors)};

namespace {nameof(AccessorGenerator)}.AccessorInitialization
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
            context.AddSource("AccessorRegistrator", SourceText.From(registratorCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}