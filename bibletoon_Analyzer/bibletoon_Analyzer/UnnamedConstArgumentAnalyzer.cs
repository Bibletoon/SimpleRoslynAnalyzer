using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations;

namespace bibletoon_Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnnamedConstArgumentAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "BA001";
        private static string Title => "Unnamed constant argument";
        private static string Description => "Constant argument should be named";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(Id, Title, Description, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.Argument);
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            var argumentOperation = (IArgumentOperation)context.Operation;
            var syntax = (ArgumentSyntax)argumentOperation.Syntax;

            if (argumentOperation.Value.ConstantValue.HasValue && syntax.NameColon is null)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string>();
                properties.Add("argumentName", argumentOperation.Parameter.Name);
                var diagnostic = Diagnostic.Create(Rule, syntax.GetLocation(), properties: properties.ToImmutable());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}