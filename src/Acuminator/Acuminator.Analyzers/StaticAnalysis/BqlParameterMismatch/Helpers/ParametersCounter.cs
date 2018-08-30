using System.Collections.Generic;
using System.Threading;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		/// <summary>
		/// The BQL parameters counting logic
		/// </summary>
		protected class ParametersCounter
		{
			private readonly PXContext pxContext;
			private readonly Dictionary<ITypeSymbol, int> customPredicatesWithWeights;

			private const int DefaultWeight = 1;
			private const int AreDistinctWeight = 2;
			private const int AreSameWeight = 2;

			private int currentParameterWeight = DefaultWeight;

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
				customPredicatesWithWeights = new Dictionary<ITypeSymbol, int>
				{
					{ pxContext.BQL.AreDistinct, AreDistinctWeight },
					{ pxContext.BQL.AreSame, AreSameWeight }
				};
			}

			/// <summary>
			/// Count parameters in type symbol. Return <c>false</c> if the diagnostic should be stopped
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
						currentParameterWeight = DefaultWeight;
						IsCountingValid = !typeSymbol.IsCustomBqlCommand(pxContext); //diagnostic for types inherited from standard views disabled. TODO: make analysis for them
						return IsCountingValid;
					case PXCodeType.BqlOperator when typeSymbol.InheritsFrom(pxContext.BQL.CustomPredicate):  //Custom predicate
						{
							IsCountingValid = ProcessCustomPredicate(typeSymbol);
							return IsCountingValid;
						}
					case PXCodeType.BqlOperator:
						currentParameterWeight = DefaultWeight;
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

			private bool ProcessCustomPredicate(ITypeSymbol typeSymbol)
			{
				if (!customPredicatesWithWeights.TryGetValue(typeSymbol, out int weight) &&
					(typeSymbol.OriginalDefinition == null || !customPredicatesWithWeights.TryGetValue(typeSymbol.OriginalDefinition, out weight)))
				{
					return false;    //Non-default custom predicate
				}
		
				currentParameterWeight = weight;
				return true;
			}

			private bool UpdateParametersCount(ITypeSymbol typeSymbol)
			{
				if (typeSymbol.InheritsFromOrEquals(pxContext.BQL.Required) || typeSymbol.InheritsFromOrEquals(pxContext.BQL.Argument))
				{
					RequiredParametersCount += currentParameterWeight;
					return true;
				}
				else if (typeSymbol.InheritsFromOrEquals(pxContext.BQL.Optional) || typeSymbol.InheritsFromOrEquals(pxContext.BQL.Optional2))
				{
					OptionalParametersCount += currentParameterWeight;
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
