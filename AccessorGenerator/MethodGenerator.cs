using ExpressionDelegates.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.SourceGeneration;
using ISourceGenerator = Uno.SourceGeneration.ISourceGenerator;

namespace ExpressionDelegates
{
    public class MethodGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debugger.Launch();

            var registrationLines = new List<string>();

            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(tree);

                var invocationExpressions = SyntaxHelper.ExtractExpressionLambdas(tree, model)
                    .SelectMany(n => n.DescendantNodes())
                    .OfType<InvocationExpressionSyntax>();

                foreach (var invocation in invocationExpressions)
                {
                    if (!(model.GetSymbolInfo(invocation).Symbol is IMethodSymbol symbol))
                        continue;

                    symbol = symbol.ReducedFrom ?? symbol;

                    if (symbol.DeclaredAccessibility < Accessibility.Internal)
                        continue;

                    var targetFullType = symbol.ContainingType.ToDisplayString(SymbolFormat.FullName);
                    var targetPath = symbol.ToDisplayString(SymbolFormat.FullName);

                    var genericArgs = symbol.IsGenericMethod
                        ? '<' + string.Join(", ", symbol.TypeArguments.Select(a => a.ToDisplayString(SymbolFormat.FullName))) + '>'
                        : string.Empty;

                    var methodName = symbol.Name + genericArgs;

                    var parameters = string.Join(", ", symbol.Parameters
                        .Select((p, i) => $"({p.Type.ToDisplayString(SymbolFormat.FullName)})a[{i}]"));

                    var invoker = symbol switch
                    {
                        { IsStatic: true, ReturnsVoid: true } => $"(o, a) => {{ {targetFullType}.{methodName}({parameters}); return null; }}",
                        { IsStatic: true } => $"(o, a) => {targetFullType}.{methodName}({parameters})",
                        { ReturnsVoid: true } => $"(o, a) => {{ (({targetFullType})o).{methodName}({parameters}); return null; }}",
                        _ => $"(o, a) => (({targetFullType})o).{methodName}({parameters})"
                    };

                    registrationLines.Add(
                        $@"{nameof(Methods.Add)}(""{targetPath}"", {invoker});");
                }
            }

            registrationLines.Sort();

            var sourceBuilder = new StringBuilder(
$@"using static {typeof(Methods).Namespace}.{nameof(Methods)};

namespace {nameof(ExpressionDelegates)}.MethodInitialization
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
            context.AddSource("MethodRegistrator", SourceText.From(registratorCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}