#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Localization
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
                Descriptors.PX1053_ConcatenationPriorLocalization
            );

		public LocalizationPXExceptionAnalyzer() : base()
		{
		}

		/// <summary>
		/// Constructor for tests.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		public LocalizationPXExceptionAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionCtorInvocation(syntaxContext, pxContext), SyntaxKind.ObjectCreationExpression);
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzePXExceptionCtorInitializer(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzePXExceptionCtorInitializer(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            syntaxContext.CancellationToken.ThrowIfCancellationRequested();

            if (!(syntaxContext.Node is ClassDeclarationSyntax classDeclaration))
                return;

            INamedTypeSymbol type = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration);
            bool isLocalizableException = type != null && IsLocalizableException(syntaxContext, pxContext, type);

            if (!isLocalizableException)
                return;

            IEnumerable<ConstructorInitializerSyntax> baseCtors = classDeclaration.DescendantNodes()
                                                                  .OfType<ConstructorInitializerSyntax>();
            foreach (ConstructorInitializerSyntax c in baseCtors)
            {
                if (!(syntaxContext.SemanticModel.GetSymbolInfo(c, syntaxContext.CancellationToken).Symbol is IMethodSymbol methodSymbol))
                    continue;

                ImmutableArray<IParameterSymbol> pars = methodSymbol.Parameters;
                ExpressionSyntax? messageExpression = GetMessageExpression(syntaxContext, pars, c.ArgumentList);

                if (messageExpression == null)
                    continue;

                LocalizationMessageHelper messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, false);
                messageHelper.ValidateMessage();
            }
        }

        private ExpressionSyntax? GetMessageExpression(SyntaxNodeAnalysisContext syntaxContext, ImmutableArray<IParameterSymbol> pars, ArgumentListSyntax? args)
        {
            syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (args == null)
				return null;

            ArgumentSyntax? messageArg = null;

            foreach (ArgumentSyntax argument in args.Arguments)
            {
                IParameterSymbol? parameter = argument.DetermineParameter(pars, allowParams: false);

                if (_messageArgNames.Contains(parameter?.Name, StringComparer.Ordinal))
                {
                    messageArg = argument;
                    break;
                }
            }

            if (messageArg?.Expression == null)
                return null;

            return messageArg.Expression;
        }

        private void AnalyzePXExceptionCtorInvocation(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            syntaxContext.CancellationToken.ThrowIfCancellationRequested();

            if (!(syntaxContext.Node is ObjectCreationExpressionSyntax ctorNode) || ctorNode.ArgumentList == null)
                return;

            ITypeSymbol type = syntaxContext.SemanticModel.GetTypeInfo(ctorNode, syntaxContext.CancellationToken).Type;
            bool isLocalizableException = type != null && IsLocalizableException(syntaxContext, pxContext, type);

            if (!isLocalizableException)
                return;

            if (!(syntaxContext.SemanticModel.GetSymbolInfo(ctorNode, syntaxContext.CancellationToken).Symbol is IMethodSymbol ctorSymbol))
                return;

            ImmutableArray<IParameterSymbol> pars = ctorSymbol.Parameters;
            ExpressionSyntax? messageExpression = GetMessageExpression(syntaxContext, pars, ctorNode.ArgumentList);

            if (messageExpression == null)
                return;

            LocalizationMessageHelper messageHelper = new LocalizationMessageHelper(syntaxContext, pxContext, messageExpression, false);
            messageHelper.ValidateMessage();
        }

        private bool IsLocalizableException(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, ITypeSymbol type)
        {
            syntaxContext.CancellationToken.ThrowIfCancellationRequested();

            return type.InheritsFromOrEquals(pxContext.Exceptions.PXException) 
                   && !type.InheritsFromOrEquals(pxContext.Exceptions.PXBaseRedirectException);
        }
    }
}
