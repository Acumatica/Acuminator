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
            private readonly PXContext pxContext;
            private readonly SemanticModel semanticModel;
            private readonly CancellationToken cancellationToken;
            
            public int RequiredParametersCount
            {
                get;
                private set;
            }

            public int OptionalParametersCount
            {
                get;
                private set;
            }

            public ParametersCounterSymbolWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
            {
                syntaxContext = aSyntaxContext;
                pxContext = aPxContext;
                semanticModel = syntaxContext.SemanticModel;
                cancellationToken = syntaxContext.CancellationToken;
            }

			public override void VisitNamedType(INamedTypeSymbol typeSymbol)
			{
				if (typeSymbol == null || cancellationToken.IsCancellationRequested)
					return;

				if (typeSymbol.IsUnboundGenericType)
				{
					typeSymbol = typeSymbol.OriginalDefinition;
				}

				PXCodeType? codeType = typeSymbol.GetCodeTypeFromGenericName();

				if (codeType == PXCodeType.BqlParameter)
				{
					if (!UpdateParametersCount(typeSymbol))
					{
						UpdateParametersCount(typeSymbol.OriginalDefinition);
					}
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

			private bool UpdateParametersCount(ITypeSymbol typeSymbol)
			{
				if (typeSymbol.InheritsFromOrEquals(pxContext.BQL.Required) || typeSymbol.InheritsFromOrEquals(pxContext.BQL.Argument))
				{
					RequiredParametersCount++;
					return true;
				}
				else if (typeSymbol.InheritsFromOrEquals(pxContext.BQL.Optional) || typeSymbol.InheritsFromOrEquals(pxContext.BQL.Optional2))
				{
					OptionalParametersCount++;
					return true;
				}

				return false;
			}
		}
    }
}
