using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace bibletoon_Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnoverridenToStringMethodCallCodeFixProvider)), Shared]
    public class UnoverridenToStringMethodCallCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UnoverridenToStringMethodCallAnalyzer.Id); }
        }
        
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var identifier = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IdentifierNameSyntax>().First();
            var symbol = ModelExtensions.GetSymbolInfo(context.Document.GetSemanticModelAsync().Result, identifier).Symbol;
            var isFromSource = ((ILocalSymbol)symbol).Type.DeclaringSyntaxReferences.Length > 0;
            if (!isFromSource)
                return;
            
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Override ToString",
                    createChangedSolution: c => OverrideToStringMethodAsync(context.Document, symbol, c),
                    equivalenceKey: "Override ToString"),
                diagnostic);
        }

        private async Task<Solution> OverrideToStringMethodAsync(Document contextDocument, ISymbol symbol, CancellationToken cancellationToken)
        {
            var classDeclaration = await ((ILocalSymbol)symbol).Type.DeclaringSyntaxReferences.First()
                                                               .GetSyntaxAsync(cancellationToken) as ClassDeclarationSyntax;

            var props = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"StringBuilder resultSb = new StringBuilder("""");");

            foreach (var prop in props)
            {
                sb.AppendLine($@"resultSb.AppendFormat(""{{0}}: {{1}}"", nameof({prop.Identifier.Value}), {prop.Identifier.Value});");
            }

            sb.AppendLine(@"return resultSb.ToString();");

            var generatedToString = CreateToStringDeclaration(sb.ToString());
            var newClass = classDeclaration.AddMembers(generatedToString);
            var document = contextDocument.Project.Solution.GetDocument(classDeclaration.SyntaxTree);
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            return document.WithSyntaxRoot(
                root.ReplaceNode(
                    classDeclaration,
                    newClass
                )
            ).Project.Solution;
        }

        private MethodDeclarationSyntax CreateToStringDeclaration(string body)
        {
            var syntax = SyntaxFactory.ParseStatement(body);

            var modifiers = new SyntaxToken[]
                { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword) };

            return SyntaxFactory.MethodDeclaration(attributeLists: SyntaxFactory.List<AttributeListSyntax>(),
                                                   modifiers: SyntaxFactory.TokenList(modifiers),
                                                   returnType: SyntaxFactory.ParseTypeName("string"),
                                                   explicitInterfaceSpecifier: null,
                                                   identifier: SyntaxFactory.Identifier("ToString"),
                                                   typeParameterList: null,
                                                   parameterList: SyntaxFactory.ParameterList(),
                                                   constraintClauses: SyntaxFactory
                                                       .List<TypeParameterConstraintClauseSyntax>(),
                                                   body: SyntaxFactory.Block(syntax),
                                                   null);
        }
    }
}