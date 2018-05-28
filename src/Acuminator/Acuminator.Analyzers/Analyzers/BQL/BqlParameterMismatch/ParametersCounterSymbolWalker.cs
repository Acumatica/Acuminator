using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using PX.Data;


namespace Acuminator.Analyzers
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		/// <summary>
		/// The BQL parameters counting symbol  walker.
		/// </summary>
		protected class ParametersCounterSymbolWalker : SymbolVisitor
		{
			private readonly SyntaxNodeAnalysisContext syntaxContext;
			private readonly CancellationToken cancellationToken;

			public ParametersCounter ParametersCounter { get; }

			public ParametersCounterSymbolWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
			{
				syntaxContext = aSyntaxContext;
				cancellationToken = syntaxContext.CancellationToken;
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
		}
	}
}
