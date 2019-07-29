using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.DacUiAttributes
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacUiAttributesAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1094_DacShouldHaveUiAttribute);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeDacUiAttributes(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeDacUiAttributes(SyntaxNodeAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(context.Node is ClassDeclarationSyntax classDeclaration))
			{
				return;
			}

			var classTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
			if (classTypeSymbol == null || !classTypeSymbol.IsDAC(pxContext))
			{
				return;
			}

			var dacAttributes = classTypeSymbol.GetAttributes();
			var pxCacheNameAttribute = pxContext.AttributeTypes.PXCacheNameAttribute;
			var pxHiddenAttribute = pxContext.AttributeTypes.PXHiddenAttribute;
			var hasPXCacheNameAttribute = false;
			var hasPXHiddenAttribute = false;

			foreach (var attribute in dacAttributes)
			{
				if (attribute.AttributeClass == null)
				{
					continue;
				}

				if (attribute.AttributeClass.InheritsFromOrEquals(pxCacheNameAttribute))
				{
					hasPXCacheNameAttribute = true;
				}

				if (attribute.AttributeClass.InheritsFromOrEquals(pxHiddenAttribute))
				{
					hasPXHiddenAttribute = true;
				}

				if (hasPXCacheNameAttribute || hasPXHiddenAttribute)
				{
					return;
				}
			}

			var diagnostic = Diagnostic.Create(
				Descriptors.PX1094_DacShouldHaveUiAttribute,
				classDeclaration.Identifier.GetLocation());

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}
