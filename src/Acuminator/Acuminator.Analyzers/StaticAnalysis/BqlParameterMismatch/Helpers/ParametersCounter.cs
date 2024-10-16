﻿
using System.Collections.Generic;
using System.Linq;
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
			private readonly INamedTypeSymbol _bqlParameterType;
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
				_bqlParameterType = _pxContext.BQL.BqlParameter;

				_customPredicatesWithWeights = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default)
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
			public bool CountParametersInTypeSymbolForGenericNode(ITypeSymbol typeSymbol, CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (!IsCountingValid || typeSymbol == null)
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
			public bool CountParametersInTypeSymbolForIdentifierNode(ITypeSymbol typeSymbol, CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (!IsCountingValid || typeSymbol == null)
					return false;

				PXCodeType? codeType = typeSymbol.GetCodeTypeFromIdentifier();
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

						if (!cancellationToken.IsCancellationRequested)
							UpdateParametersCount(typeSymbol);
						
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
				if (UpdateParametersCountForNonFbqlParameter(typeSymbol))
					return true;

				if (_bqlParameterType == null)		//In case of Acumatica version without FBQL 
					return false;

				var fbqlStyleParameter = 
					typeSymbol.GetBaseTypesAndThis()
							  .FirstOrDefault(t => t.Equals(_bqlParameterType, SymbolEqualityComparer.Default) || 
												   t.OriginalDefinition.Equals(_bqlParameterType, SymbolEqualityComparer.Default)) as INamedTypeSymbol;

				if (fbqlStyleParameter == null || fbqlStyleParameter.TypeArguments.IsDefaultOrEmpty)
					return false;

				var wrappedParameterType = fbqlStyleParameter.TypeArguments[0];
				return UpdateParametersCountForNonFbqlParameter(wrappedParameterType);
			}

			/// <summary>
			/// Updates the parameters count for <paramref name="typeSymbol"/> if it is old non-FBQL parameter.
			/// </summary>
			/// <param name="typeSymbol">The type symbol representing parameter.</param>
			/// <returns/>
			private bool UpdateParametersCountForNonFbqlParameter(ITypeSymbol typeSymbol)
			{
				typeSymbol = typeSymbol.OriginalDefinition ?? typeSymbol;

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
		}
	}
}
