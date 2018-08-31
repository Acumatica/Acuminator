using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn;
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
			private readonly bool isAcumatica2018R2;
			private readonly INamedTypeSymbol iViewConfig2018R2;

			private readonly SyntaxNodeAnalysisContext syntaxContext;
			private readonly CancellationToken cancellationToken;

			public ParametersCounter ParametersCounter { get; }

			public ParametersCounterSymbolWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
			{
				syntaxContext = aSyntaxContext;
				cancellationToken = syntaxContext.CancellationToken;

				isAcumatica2018R2 = aPxContext.IsAcumatica2018R2;

				if (isAcumatica2018R2)
				{
					iViewConfig2018R2 = aPxContext.IViewConfig2018R2;
				}

				ParametersCounter = new ParametersCounter(aPxContext);
			}

			public bool CountParametersInTypeSymbol(ITypeSymbol typeSymbol)
			{
				if (cancellationToken.IsCancellationRequested)
					return false;

				Visit(typeSymbol);
				return ParametersCounter.IsCountingValid && !cancellationToken.IsCancellationRequested;
			}

			public override void VisitNamedType(INamedTypeSymbol typeSymbol)
			{
				if (typeSymbol == null || cancellationToken.IsCancellationRequested)
					return;

				if (typeSymbol.IsUnboundGenericType)
				{
					typeSymbol = typeSymbol.OriginalDefinition;
				}

				if (!ParametersCounter.CountParametersInTypeSymbol(typeSymbol, cancellationToken))
					return;

				if (isAcumatica2018R2 && !cancellationToken.IsCancellationRequested && typeSymbol.ContainingType != null &&
					ImplementsIViewConfig(typeSymbol))
				{
					Visit(typeSymbol.ContainingType);
				}

				var typeArguments = typeSymbol.TypeArguments;

				if (!typeArguments.IsDefaultOrEmpty)
				{
					foreach (ITypeSymbol typeArg in typeArguments)
					{
						if (cancellationToken.IsCancellationRequested)
							return;

						Visit(typeArg);
					}
				}

				if (!cancellationToken.IsCancellationRequested)
					base.VisitNamedType(typeSymbol);
			}

			public override void VisitTypeParameter(ITypeParameterSymbol typeParameterSymbol)
			{
				if (typeParameterSymbol == null || cancellationToken.IsCancellationRequested)
					return;

				foreach (ITypeSymbol constraintType in typeParameterSymbol.ConstraintTypes)
				{
					if (cancellationToken.IsCancellationRequested)
						return;

					Visit(constraintType);
				}

				if (!cancellationToken.IsCancellationRequested)
					base.VisitTypeParameter(typeParameterSymbol);
			}

			private bool ImplementsIViewConfig(ITypeSymbol type)
			{
				if (type == null || iViewConfig2018R2 == null)
					return false;

				return type.AllInterfaces.Any(interfaceType => iViewConfig2018R2.Equals(interfaceType) || 
															   iViewConfig2018R2.Equals(interfaceType?.OriginalDefinition));
			}
		}
	}
}
