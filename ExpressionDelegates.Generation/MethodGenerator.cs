﻿using Microsoft.CodeAnalysis;
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
    public class MethodGenerator : ISourceGenerator
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

                    if (symbol.Parameters.Any(p => p.RefKind == RefKind.Ref))
                        continue;

                    var targetFullType = symbol.ContainingType.ToDisplayString(SymbolFormat.FullName);
                    var targetPath = symbol.ToDisplayString(SymbolFormat.FullName);
                    var methodName = symbol.ToDisplayString(SymbolFormat.FullName
                        .WithMemberOptions(SymbolDisplayMemberOptions.None));

                    var parameters = string.Join(", ", symbol.Parameters
                        .Select((p, i) => $"({p.Type.ToDisplayString(SymbolFormat.FullName)})a[{i}]"));

                    var invoker = symbol.IsStatic
                        ? $"(o, a) => {targetFullType}.{methodName}({parameters})"
                        : $"(o, a) => (({targetFullType})o).{methodName}({parameters})";

                    registrationLines.Add(
                        $@"{nameof(Methods.Add)}(""{targetPath}"", {invoker});");
                }
            }

            registrationLines.Sort();

            var sourceBuilder = new StringBuilder(
$@"using static {typeof(Methods).Namespace}.{nameof(Methods)};

namespace {nameof(ExpressionDelegates)}.MethodRegistration
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
            context.AddSource("MethodRegistration", SourceText.From(registratorCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}