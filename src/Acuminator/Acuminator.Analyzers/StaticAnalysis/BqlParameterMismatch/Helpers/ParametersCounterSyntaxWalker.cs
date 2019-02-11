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
			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private readonly CancellationToken _cancellationToken;

			public ParametersCounter ParametersCounter { get; }

			public ParametersCounterSyntaxWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
			{
				_syntaxContext = aSyntaxContext;
				_cancellationToken = _syntaxContext.CancellationToken;
				ParametersCounter = new ParametersCounter(aPxContext);
			}

			public bool CountParametersInNode(SyntaxNode node)
			{
				if (_cancellationToken.IsCancellationRequested)
					return false;

				Visit(node);
				return ParametersCounter.IsCountingValid && !_cancellationToken.IsCancellationRequested;
			}

			public override void VisitGenericName(GenericNameSyntax genericNode)
			{
				if (_cancellationToken.IsCancellationRequested)
					return;

				SymbolInfo symbolInfo = _syntaxContext.SemanticModel.GetSymbolInfo(genericNode, _cancellationToken);

				if (_cancellationToken.IsCancellationRequested || !(symbolInfo.Symbol is ITypeSymbol typeSymbol))
				{
					if (!_cancellationToken.IsCancellationRequested)
						base.VisitGenericName(genericNode);

					return;
				}

				if (genericNode.IsUnboundGenericName)
					typeSymbol = typeSymbol.OriginalDefinition;

				if (!ParametersCounter.CountParametersInTypeSymbol(typeSymbol, _cancellationToken))
					return;

				if (!_cancellationToken.IsCancellationRequested)
					base.VisitGenericName(genericNode);
			}
		}
	}
}
