using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationPXExceptionAnalyzer : PXDiagnosticAnalyzer
    {
        private readonly string[] _messageArgNames = new[] { "message", "format" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1050_HardcodedStringInLocalizationMethod,
                Descriptors.PX1051_NonLocalizableString,
                Descriptors.PX1053_ConcatinationPriorLocalization
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeThrowOfPXException(syntaxContext, pxContext), SyntaxKind.ThrowStatement);
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionCtorInitializer(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzePXExceptionCtorInitializer(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (!(syntaxContext.Node is ClassDeclarationSyntax classDeclaration) || syntaxContext.CancellationToken.IsCancellationRequested)
                return;

            INamedTypeSymbol type = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration);
            bool isPXException = type != null && type.InheritsFromOrEquals(pxContext.PXException);

            if (!isPXException)
                return;

            IEnumerable<ConstructorInitializerSyntax> baseCtors = classDeclaration.DescendantNodes()
                                                                  .OfType<ConstructorInitializerSyntax>();
            foreach (ConstructorInitializerSyntax c in baseCtors)
            {
                if (!(syntaxContext.SemanticModel.GetSymbolInfo(c, syntaxContext.CancellationToken).Symbol is IMethodSymbol methodSymbol))
                    continue;

                ImmutableArray<IParameterSymbol> pars = methodSymbol.Parameters;
                ExpressionSyntax messageExpression = GetMessageExpression(syntaxContext, pars, c.ArgumentList);

                if (messageExpression == null)
                    continue;

                LocalizationMessageHelper messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, false);
                messageHelper.ValidateMessage();
            }
        }

        private ExpressionSyntax GetMessageExpression(SyntaxNodeAnalysisContext syntaxContext, ImmutableArray<IParameterSymbol> pars, ArgumentListSyntax args)
        {
            if (syntaxContext.CancellationToken.IsCancellationRequested)
                return null;

            ArgumentSyntax messageArg = null;

            foreach (ArgumentSyntax a in args.Arguments)
            {
                IParameterSymbol p = a.DetermineParameter(pars, false, syntaxContext.CancellationToken);

                if (p != null && !string.IsNullOrEmpty(p.Name) && _messageArgNames.Contains(p.Name, StringComparer.Ordinal))
                {
                    messageArg = a;
                    break;
                }
            }

            if (messageArg == null || messageArg.Expression == null)
                return null;

            return messageArg.Expression;
        }

        private void AnalyzeThrowOfPXException(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (!(syntaxContext.Node is ThrowStatementSyntax throwNode) || !(throwNode.Expression is ObjectCreationExpressionSyntax ctorNode) ||
                ctorNode.ArgumentList == null || syntaxContext.CancellationToken.IsCancellationRequested)
                return;

            ITypeSymbol type = syntaxContext.SemanticModel.GetTypeInfo(ctorNode).Type;
            bool isPXException = type != null && type.InheritsFromOrEquals(pxContext.PXException);

            if (!isPXException)
                return;

            if (!(syntaxContext.SemanticModel.GetSymbolInfo(ctorNode).Symbol is IMethodSymbol ctorSymbol))
                return;

            ImmutableArray<IParameterSymbol> pars = ctorSymbol.Parameters;
            ExpressionSyntax messageExpression = GetMessageExpression(syntaxContext, pars, ctorNode.ArgumentList);

            if (messageExpression == null)
                return;

            LocalizationMessageHelper messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, false);
            messageHelper.ValidateMessage();
        }
    }
}
