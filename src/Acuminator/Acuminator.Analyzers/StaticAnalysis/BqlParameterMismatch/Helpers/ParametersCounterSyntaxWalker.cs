using System.Threading;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		/// <summary>
		/// The BQL parameters counting syntax walker.
		/// </summary>
		protected class ParametersCounterSyntaxWalker : CSharpSyntaxWalker
		{
			private readonly SyntaxNodeAnalysisContext syntaxContext;
			private readonly CancellationToken cancellationToken;

			public ParametersCounter ParametersCounter { get; }

			public ParametersCounterSyntaxWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
			{
				syntaxContext = aSyntaxContext;
				cancellationToken = syntaxContext.CancellationToken;
				ParametersCounter = new ParametersCounter(aPxContext);
			}

			public bool CountParametersInNode(SyntaxNode node)
			{
				if (cancellationToken.IsCancellationRequested)
					return false;

				Visit(node);
				return ParametersCounter.IsCountingValid && !cancellationToken.IsCancellationRequested;
			}

			public override void VisitGenericName(GenericNameSyntax genericNode)
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				SymbolInfo symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(genericNode, cancellationToken);

				if (cancellationToken.IsCancellationRequested || !(symbolInfo.Symbol is ITypeSymbol typeSymbol))
				{
					if (!cancellationToken.IsCancellationRequested)
						base.VisitGenericName(genericNode);

					return;
				}

				if (genericNode.IsUnboundGenericName)
					typeSymbol = typeSymbol.OriginalDefinition;

				if (!ParametersCounter.CountParametersInTypeSymbol(typeSymbol, cancellationToken))
					return;

				if (!cancellationToken.IsCancellationRequested)
					base.VisitGenericName(genericNode);
			}
		}
	}
}
