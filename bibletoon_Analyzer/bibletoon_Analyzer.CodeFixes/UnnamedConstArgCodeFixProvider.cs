using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editing;

namespace bibletoon_Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnamedConstArgCodeFixProvider)), Shared]
    public class UnnamedConstArgCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UnnamedConstArgumentAnalyzer.Id); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ArgumentSyntax>().First();
            var argumentName = diagnostic.Properties["argumentName"];
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add name column",
                    createChangedSolution: c => AddNameColonAsync(context.Document, declaration, argumentName, c),
                    equivalenceKey: "Add name column"),
                diagnostic);
        }

        private async Task<Solution> AddNameColonAsync(Document document, ArgumentSyntax argumentSyntax, string argumentName, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct);

            return document.WithSyntaxRoot(
                root.ReplaceNode(
                    argumentSyntax,
                    argumentSyntax.WithNameColon(SyntaxFactory.NameColon(argumentName))
                )).Project.Solution;
        }
    }
}