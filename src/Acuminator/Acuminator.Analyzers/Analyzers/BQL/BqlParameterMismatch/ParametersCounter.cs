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
		/// The BQL parameters counting logic
		/// </summary>
		protected class ParametersCounter
		{		
            private readonly PXContext pxContext;               
            
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

            public ParametersCounter(PXContext aPxContext)
            {
                pxContext = aPxContext;              
            }

			public void CountParametersInTypeSymbol(ITypeSymbol typeSymbol, CancellationToken cancellationToken = default)
			{
				if (typeSymbol == null || cancellationToken.IsCancellationRequested)
					return;

				PXCodeType? codeType = typeSymbol.GetCodeTypeFromGenericName();

				if (codeType == PXCodeType.BqlParameter)
				{
					if (!UpdateParametersCount(typeSymbol) && !cancellationToken.IsCancellationRequested)
					{
						UpdateParametersCount(typeSymbol.OriginalDefinition);
					}
				}
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
