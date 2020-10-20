using AccessorGenerator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GeneratorExecutionContext = Uno.SourceGeneration.GeneratorExecutionContext;
using GeneratorInitializationContext = Uno.SourceGeneration.GeneratorInitializationContext;
using ISourceGenerator = Uno.SourceGeneration.ISourceGenerator;

namespace AccessorGenerator
{
    //Fix generics
    public class Generator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debugger.Launch();

            var registrationLines = new List<string>();

            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(tree);

                var memberExpressions = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<LambdaExpressionSyntax>()
                    .Where(l => model.GetTypeInfo(l).ConvertedType?.Name == "Expression")
                    .SelectMany(n => n.DescendantNodes())
                    .OfType<MemberAccessExpressionSyntax>()
                    .ToList();

                foreach (var memberAccess in memberExpressions)
                {
                    var symbol = model.GetSymbolInfo(memberAccess).Symbol;
                    var memberType = model.GetTypeInfo(memberAccess).Type;

                    var isReadOnly = false;
                    var isWriteOnly = false;

                    if (symbol is IPropertySymbol propertySymbol)
                    {
                        isReadOnly = propertySymbol.SetMethod == null;
                        isWriteOnly = propertySymbol.GetMethod == null;
                    }
                    else if (symbol is IFieldSymbol fieldSymbol)
                    {
                        if (fieldSymbol.IsConst || fieldSymbol.IsReadOnly)
                        {
                            isReadOnly = true;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    if (memberType == null)
                        continue;

                    var targetFullType = symbol.ContainingType.ToDisplayString();
                    var memberFullType = $"{memberType.ContainingNamespace.ToDisplayString()}.{memberType.Name}";
                    var targetPath = $"{targetFullType}.{symbol.Name}";

                    var getter = isWriteOnly ? "null" : $"o => (({targetFullType})o).{symbol.Name}";
                    var setter = isReadOnly ? "null" : $"(t, m) => (({targetFullType})t).{symbol.Name} = ({memberFullType})m";

                    registrationLines.Add($"{nameof(ExpressionAccessors)}.{nameof(ExpressionAccessors.Add)}(\"{targetPath}\", {getter}, {setter});");
                    //TODO test for assignment avaliability
                    //TODO check inheritance
                    //TODO check for avaliability (not private)
                }
            }

            registrationLines.Sort();

            var sourceBuilder = new StringBuilder($@"
namespace {typeof(ExpressionAccessors).Namespace}
{{
    public static class ModuleInitializer
    {{
        public static void Initialize()
        {{
");
            foreach (var line in registrationLines.Distinct())
            {
                sourceBuilder.AppendLine(line);
            }

            sourceBuilder.Append(@"
        }
    }
}");

            var registratorCode = sourceBuilder.ToString();
            context.AddSource("Accessor.Generated.cs", SourceText.From(registratorCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}