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
            private readonly CancellationToken cancellationToken;

			public ParametersCounter ParametersCounter { get; }


            public ParametersCounterSyntaxWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
            {
				syntaxContext = aSyntaxContext;
                cancellationToken = syntaxContext.CancellationToken;
				ParametersCounter = new ParametersCounter(aPxContext);
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

				ParametersCounter.CountParametersInTypeSymbol(typeSymbol, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitGenericName(genericNode);             
            }		
		}
    }
}
