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

			public bool IsCountingValid
			{
				get;
				private set;
			}

            public ParametersCounter(PXContext aPxContext)
            {
                pxContext = aPxContext;
				IsCountingValid = true;
            }

			/// <summary>
			/// Count parameters in type symbol. Return <c>false</c> if the diagnostic should ne stopped
			/// </summary>
			/// <param name="typeSymbol">The type symbol.</param>
			/// <param name="cancellationToken">(Optional) The cancellation token.</param>
			/// <returns/>
			public bool CountParametersInTypeSymbol(ITypeSymbol typeSymbol, CancellationToken cancellationToken = default)
			{
				if (!IsCountingValid || typeSymbol == null || IsCancelled(cancellationToken))
					return false;
				
				PXCodeType? codeType = typeSymbol.GetCodeTypeFromGenericName();

				switch (codeType)
				{
					case PXCodeType.BqlCommand:
						IsCountingValid = !typeSymbol.IsCustomBqlCommand(pxContext); //diagnostic for types inherited from standard views disabled. TODO: make analysis for them
						return IsCountingValid;
					case PXCodeType.BqlParameter:

						if (!UpdateParametersCount(typeSymbol) && !cancellationToken.IsCancellationRequested)
						{
							UpdateParametersCount(typeSymbol.OriginalDefinition);
						}

						return true;			
				}

				return true;
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

			private bool IsCancelled(CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					IsCountingValid = false;
				}

				return cancellationToken.IsCancellationRequested;
			}
		}
    }
}
