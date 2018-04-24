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
        /// The BQL parameters counting syntax walker.
        /// </summary>
        protected class ParametersCounterSyntaxWalker : CSharpSyntaxWalker
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

            public ParametersCounterSyntaxWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
            {
                syntaxContext = aSyntaxContext;
                pxContext = aPxContext;
                semanticModel = syntaxContext.SemanticModel;
                cancellationToken = syntaxContext.CancellationToken;
            }

            public override void VisitGenericName(GenericNameSyntax genericNode)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(genericNode);

                if (cancellationToken.IsCancellationRequested || !(symbolInfo.Symbol is ITypeSymbol typeSymbol))
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }

                if (genericNode.IsUnboundGenericName)
                    typeSymbol = typeSymbol.OriginalDefinition;

                PXCodeType? codeType = typeSymbol.GetCodeTypeFromGenericName();

                if (codeType == PXCodeType.BqlParameter)
                {
					if (!UpdateParametersCount(typeSymbol))
					{
						UpdateParametersCount(typeSymbol.OriginalDefinition);
					}
				}

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitGenericName(genericNode);             
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
