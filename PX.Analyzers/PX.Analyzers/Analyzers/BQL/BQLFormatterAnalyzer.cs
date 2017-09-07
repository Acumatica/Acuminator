using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PX.Analyzers.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class BQLFormatterAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PXF1001_PXBadBqlDiagnostic);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeNode(c, pxContext), SyntaxKind.GenericName);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			GenericNameSyntax genericNode = syntaxContext.Node as GenericNameSyntax;

			if (!CheckGenericNodeParentKind(genericNode))
				return;

			ITypeSymbol typeSymbol = syntaxContext.SemanticModel.GetSymbolInfo(genericNode).Symbol as ITypeSymbol;

			if (typeSymbol == null)
				return;

			if (typeSymbol.InheritsFrom(pxContext.BQL.PXSelectBase))
			{
				DiagnosticDescriptor descriptor = Descriptors.PXF1001_PXBadBqlDiagnostic;
				syntaxContext.ReportDiagnostic(Diagnostic.Create(descriptor, genericNode.GetLocation()));
			}
		}

		private static bool CheckGenericNodeParentKind(GenericNameSyntax genericNode)
		{
			if (genericNode?.Parent == null)
				return false;

			SyntaxKind parentKind = genericNode.Parent.Kind();

			if (parentKind == SyntaxKind.VariableDeclaration)
				return true;

			if (parentKind == SyntaxKind.SimpleMemberAccessExpression)
			{
				SyntaxKind? grandPaKind = genericNode.Parent.Parent?.Kind();

				if (grandPaKind == SyntaxKind.InvocationExpression)
					return true;
			}


			return false;
		}

		private static bool CheckBQLStatement(GenericNameSyntax genericNode)
		{
			return true;
		}
	}
}
