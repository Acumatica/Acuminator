using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Base class for graph event information.
	/// </summary>
	public abstract class GraphEventInfoBase<TDerived> : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	where TDerived : GraphEventInfoBase<TDerived>
	{
		public TDerived Base { get; }

		public EventHandlerSignatureType SignatureType { get; }

		public EventType EventType { get; }

		public string DacName { get; }

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType) :
								base(node, symbol, declarationOrder)
		{
			SignatureType = signatureType;
			EventType = eventType;
			DacName = GetDacName();
		}

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType, TDerived baseInfo)
							  : this(node, symbol, declarationOrder, signatureType, eventType)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));

			Base = baseInfo;
		}

		public bool IsDacFieldEvent()
		{
			switch (EventType)
			{
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.FieldUpdated:
					return true;
				default:
					return false;
			}
		}

		public bool IsDacRowEvent()
		{
			switch (EventType)
			{
				case EventType.RowSelecting:
				case EventType.RowSelected:
				case EventType.RowInserting:
				case EventType.RowInserted:
				case EventType.RowUpdating:
				case EventType.RowUpdated:
				case EventType.RowDeleting:
				case EventType.RowDeleted:
				case EventType.RowPersisting:
				case EventType.RowPersisted:
					return true;
				default:
					return false;
			}
		}

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
