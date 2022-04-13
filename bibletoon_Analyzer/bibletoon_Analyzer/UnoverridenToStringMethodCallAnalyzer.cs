using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace bibletoon_Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnoverridenToStringMethodCallAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "BA002";
        private static string Title => "Un-overriden method call";
        private static string Description => "Method ToString is not declared in class {0}";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(Id, Title, Description, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            var invocationOperation = (IInvocationOperation)context.Operation;

            if (invocationOperation.TargetMethod.ReceiverType.Name == "Object" && invocationOperation.Syntax.ChildNodes().First().ChildNodes().Last().ToString() == "ToString")
            {
                var model = context.Compilation.GetSemanticModel(invocationOperation.Syntax.SyntaxTree);
                var symbol = model.GetSymbolInfo(invocationOperation.Children.First().Syntax).Symbol;
                var diagnostic = Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation(), ((ILocalSymbol)symbol).Type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}