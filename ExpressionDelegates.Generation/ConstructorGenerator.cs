using ExpressionDelegates.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.SourceGeneration;
using ISourceGenerator = Uno.SourceGeneration.ISourceGenerator;

namespace ExpressionDelegates.Generation
{
    public class ConstructorGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debugger.Launch();

            var registrationLines = new List<string>();

            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(tree);

                var constructionExpression = SyntaxHelper.ExtractExpressionLambdas(tree, model)
                    .SelectMany(n => n.DescendantNodes())
                    .OfType<ObjectCreationExpressionSyntax>();

                foreach (var construction in constructionExpression)
                {
                    if (!(model.GetSymbolInfo(construction).Symbol is IMethodSymbol symbol))
                        continue;

                    if (symbol.DeclaredAccessibility < Accessibility.Internal)
                        continue;

                    var targetFullType = symbol.ContainingType.ToDisplayString(SymbolFormat.FullName);
                    var targetPath = symbol.ToDisplayString(SymbolFormat.FullName);

                    var genericArgs = symbol.IsGenericMethod
                        ? '<' + string.Join(", ", symbol.TypeArguments.Select(a => a.ToDisplayString(SymbolFormat.FullName))) + '>'
                        : string.Empty;

                    var parameters = string.Join(", ", symbol.Parameters
                        .Select((p, i) => $"({p.Type.ToDisplayString(SymbolFormat.FullName)})a[{i}]"));

                    var invoker = $"a => new {targetFullType}{genericArgs}({parameters})";

                    registrationLines.Add(
                        $@"{nameof(Constructors.Add)}(""{targetPath}"", {invoker});");
                }
            }

            registrationLines.Sort();

            var sourceBuilder = new StringBuilder(
$@"using static {typeof(Constructors).Namespace}.{nameof(Constructors)};

namespace {nameof(ExpressionDelegates)}.ConstructorInitialization
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
            context.AddSource("ConstructorRegistrator", SourceText.From(registratorCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
