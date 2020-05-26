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
			private readonly PXContext _pxContext;
			private readonly Dictionary<ITypeSymbol, int> _customPredicatesWithWeights;

			private const int DefaultWeight = 1;
			private const int AreDistinctWeight = 2;
			private const int AreSameWeight = 2;

			private int _currentParameterWeight = DefaultWeight;

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

			public ParametersCounter(PXContext pxContext)
			{
				_pxContext = pxContext;
				IsCountingValid = true;
				_customPredicatesWithWeights = new Dictionary<ITypeSymbol, int>
				{
					{ _pxContext.BQL.AreDistinct, AreDistinctWeight },
					{ _pxContext.BQL.AreSame, AreSameWeight }
				};
			}

			/// <summary>
			/// Count parameters in type symbol retrieved for <see cref="Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax"/> syntax node. 
			/// Return <c>false</c> if the diagnostic should be stopped
			/// </summary>
			/// <param name="typeSymbol">The type symbol.</param>
			/// <param name="cancellationToken">(Optional) The cancellation token.</param>
			/// <returns/>
			public bool CountParametersInTypeSymbolForGenericNode(ITypeSymbol typeSymbol, CancellationToken cancellationToken = default)
			{
				if (!IsCountingValid || typeSymbol == null || IsCancelled(cancellationToken))
					return false;
				
				PXCodeType? codeType = typeSymbol.GetCodeTypeFromGenericName();
				return CountParametersInTypeSymbolBasedOnCodeType(typeSymbol, codeType, cancellationToken);
			}

			/// <summary>
			/// Count parameters in type symbol retrieved for <see cref="Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax"/> syntax node. 
			/// Return <c>false</c> if the diagnostic should be stopped
			/// </summary>
			/// <param name="typeSymbol">The type symbol.</param>
			/// <param name="cancellationToken">(Optional) The cancellation token.</param>
			/// <returns/>
			public bool CountParametersInTypeSymbolForIdentifierNode(ITypeSymbol typeSymbol, CancellationToken cancellationToken = default)
			{
				if (!IsCountingValid || typeSymbol == null || IsCancelled(cancellationToken))
					return false;

				PXCodeType? codeType = typeSymbol.GetColoringTypeFromIdentifier();
				return CountParametersInTypeSymbolBasedOnCodeType(typeSymbol, codeType, cancellationToken);
			}

			private bool CountParametersInTypeSymbolBasedOnCodeType(ITypeSymbol typeSymbol, PXCodeType? codeType, CancellationToken cancellationToken)
			{
				switch (codeType)
				{
					case PXCodeType.BqlCommand:
						_currentParameterWeight = DefaultWeight;
						IsCountingValid = !typeSymbol.IsCustomBqlCommand(_pxContext); //diagnostic for types inherited from standard views disabled. TODO: make analysis for them
						return IsCountingValid;

					case PXCodeType.BqlOperator when typeSymbol.InheritsFrom(_pxContext.BQL.CustomPredicate):  //Custom predicate
						{
							IsCountingValid = ProcessCustomPredicate(typeSymbol);
							return IsCountingValid;
						}
					case PXCodeType.BqlOperator:
						_currentParameterWeight = DefaultWeight;
						return IsCountingValid;

					case PXCodeType.BqlParameter:
						if (!UpdateParametersCount(typeSymbol) && !cancellationToken.IsCancellationRequested)
						{
							UpdateParametersCount(typeSymbol.OriginalDefinition);
						}

						return true;

					default:
						return true;
				}
			}

			private bool ProcessCustomPredicate(ITypeSymbol typeSymbol)
			{
				if (!_customPredicatesWithWeights.TryGetValue(typeSymbol, out int weight) &&
					(typeSymbol.OriginalDefinition == null || !_customPredicatesWithWeights.TryGetValue(typeSymbol.OriginalDefinition, out weight)))
				{
					return false;    //Non-default custom predicate
				}
		
				_currentParameterWeight = weight;
				return true;
			}

			private bool UpdateParametersCount(ITypeSymbol typeSymbol)
			{
				if (typeSymbol.InheritsFromOrEquals(_pxContext.BQL.Required) || typeSymbol.InheritsFromOrEquals(_pxContext.BQL.Argument))
				{
					RequiredParametersCount += _currentParameterWeight;
					return true;
				}
				else if (typeSymbol.InheritsFromOrEquals(_pxContext.BQL.Optional) || typeSymbol.InheritsFromOrEquals(_pxContext.BQL.Optional2))
				{
					OptionalParametersCount += _currentParameterWeight;
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
