﻿#nullable enable
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A common non-generic graph event info DTO base class.
	/// </summary>
	public abstract class GraphEventInfoBase : NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		public EventHandlerSignatureType SignatureType { get; }

		public EventType EventType { get; }

		public string DacName { get; }

		public override string Name => GetEventGroupingKey();

		protected GraphEventInfoBase(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType) :
								base(node, symbol, declarationOrder)
		{
			SignatureType = signatureType;
			EventType = eventType;
			DacName = GetDacName();
		}

		/// <summary>
		/// Gets the event grouping key. The key should contain enough data to distinguish events with different types or different DACs/DAC fields.
		/// However, it should group together events with same event type, DAC and (if present) DAC field but with different signature types.
		/// </summary>
		internal abstract string GetEventGroupingKey();

		private string GetDacName() => SignatureType switch
		{
			EventHandlerSignatureType.Default => Symbol.Name.IndexOf('_') is int underscoreIndex && underscoreIndex > 0
													? Symbol.Name[..underscoreIndex]
													: string.Empty,				

			EventHandlerSignatureType.Generic => GetDacNameFromGenericEvent(),
			EventHandlerSignatureType.None    => string.Empty,
			_                                 => string.Empty
		};	

		private string GetDacNameFromGenericEvent()
		{
			if (Symbol.Parameters.IsDefaultOrEmpty ||
				Symbol.Parameters[0]?.Type is not INamedTypeSymbol firstParameter ||
				firstParameter.TypeArguments.IsDefaultOrEmpty)
			{
				return string.Empty;
			}

			ITypeSymbol dacOrDacField = firstParameter.TypeArguments[0];

			if (dacOrDacField.IsDAC())
				return dacOrDacField.Name;
			else if (dacOrDacField.IsDacBqlField())
				return dacOrDacField.ContainingType?.Name ?? string.Empty;
			else
				return string.Empty;
		}
	}
}
