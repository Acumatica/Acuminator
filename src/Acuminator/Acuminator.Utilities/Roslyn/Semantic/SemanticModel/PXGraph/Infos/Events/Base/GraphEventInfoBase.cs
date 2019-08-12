using System.Diagnostics;
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

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
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

		private string GetDacName()
		{
			switch (SignatureType)
			{
				case EventHandlerSignatureType.Default:
					var underscoreIndex = Symbol.Name.IndexOf('_');
					return underscoreIndex > 0
						? Symbol.Name.Substring(0, underscoreIndex)
						: string.Empty;

				case EventHandlerSignatureType.Generic:
					return GetDacNameFromGenericEvent();

				case EventHandlerSignatureType.None:
				default:
					return string.Empty;
			}
		}

		private string GetDacNameFromGenericEvent()
		{
			if (Symbol.Parameters.IsDefaultOrEmpty ||
					   !(Symbol.Parameters[0]?.Type is INamedTypeSymbol firstParameter) ||
					   firstParameter.TypeArguments.IsDefaultOrEmpty)
			{
				return string.Empty;
			}

			return firstParameter.TypeArguments[0].IsDAC()
				? firstParameter.TypeArguments[0].Name
				: string.Empty;
		}
	}
}
