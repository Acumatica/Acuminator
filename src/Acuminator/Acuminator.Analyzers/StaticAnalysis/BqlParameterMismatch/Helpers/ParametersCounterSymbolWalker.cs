using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		/// <summary>
		/// The BQL parameters counting symbol  walker.
		/// </summary>
		protected class ParametersCounterSymbolWalker : SymbolVisitor
		{
			private readonly bool _isAcumatica2018R2;
			private readonly INamedTypeSymbol _iViewConfig2018R2;

			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private readonly CancellationToken _cancellationToken;

			public ParametersCounter ParametersCounter { get; }

			public ParametersCounterSymbolWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
			{
				_syntaxContext = syntaxContext;
				_cancellationToken = _syntaxContext.CancellationToken;

				_isAcumatica2018R2 = pxContext.IsAcumatica2018R2;

				if (_isAcumatica2018R2)
				{
					_iViewConfig2018R2 = pxContext.IViewConfig2018R2;
				}

				ParametersCounter = new ParametersCounter(pxContext);
			}

			public bool CountParametersInTypeSymbol(ITypeSymbol typeSymbol)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				Visit(typeSymbol);
				return ParametersCounter.IsCountingValid && !_cancellationToken.IsCancellationRequested;
			}

			public override void VisitNamedType(INamedTypeSymbol typeSymbol)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (typeSymbol == null)
					return;

				if (typeSymbol.IsUnboundGenericType)
				{
					typeSymbol = typeSymbol.OriginalDefinition;
				}

				if (!ParametersCounter.CountParametersInTypeSymbolForGenericNode(typeSymbol, _cancellationToken))
					return;

				if (_isAcumatica2018R2 && typeSymbol.ContainingType != null && ImplementsIViewConfig(typeSymbol))
				{
					_cancellationToken.ThrowIfCancellationRequested();
					Visit(typeSymbol.ContainingType);
				}

				var typeArguments = typeSymbol.TypeArguments;

				if (!typeArguments.IsDefaultOrEmpty)
				{
					foreach (ITypeSymbol typeArg in typeArguments)
					{
						_cancellationToken.ThrowIfCancellationRequested();
						Visit(typeArg);
					}
				}

				_cancellationToken.ThrowIfCancellationRequested();
				base.VisitNamedType(typeSymbol);
			}

			public override void VisitTypeParameter(ITypeParameterSymbol typeParameterSymbol)
			{
				if (typeParameterSymbol == null)
					return;

				foreach (ITypeSymbol constraintType in typeParameterSymbol.ConstraintTypes)
				{
					_cancellationToken.ThrowIfCancellationRequested();
					Visit(constraintType);
				}

				_cancellationToken.ThrowIfCancellationRequested();
				base.VisitTypeParameter(typeParameterSymbol);
			}

			private bool ImplementsIViewConfig(ITypeSymbol type)
			{
				if (type == null || _iViewConfig2018R2 == null)
					return false;

				return type.AllInterfaces.Any(interfaceType => _iViewConfig2018R2.Equals(interfaceType) || 
															   _iViewConfig2018R2.Equals(interfaceType?.OriginalDefinition));
			}
		}
	}
}
