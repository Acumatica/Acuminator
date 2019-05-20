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

		public static string GetDacFieldNameForFieldEvent(TDerived eventInfo)
		{
			if (eventInfo == null || !eventInfo.IsDacFieldEvent())
				return string.Empty;

			switch (eventInfo.SignatureType)
			{
				case EventHandlerSignatureType.Default:
					return GetDacFieldNameForDefaultFieldEvent(eventInfo);
				case EventHandlerSignatureType.Generic:
					return GetDacFieldNameForGenericFieldEvent(eventInfo);
				default:
					return string.Empty;
			}
		}


		private static string GetDacFieldNameForDefaultFieldEvent(TDerived defaultFieldEventInfo)
		{
			if (defaultFieldEventInfo?.SignatureType != EventHandlerSignatureType.Default || !defaultFieldEventInfo.IsDacFieldEvent())
			{
				return string.Empty;
			}

			string eventName = defaultFieldEventInfo.Symbol.Name;

			if (eventName.Length < 5 || eventName[0] == '_' || eventName[eventName.Length - 1] == '_')
			{
				return string.Empty;
			}

			int underscoreCount = 0;
			int firstUnderscoreIndex = -1, lastUnderscoreIndex = -1;

			for (int i = 0; i < eventName.Length; i++)
			{
				if (eventName[i] != '_')
					continue;

				if (underscoreCount == 0)
				{
					firstUnderscoreIndex = i;
				}
				else
				{
					lastUnderscoreIndex = i;
				}

				underscoreCount++;
			}

			if (underscoreCount != 2 || firstUnderscoreIndex < 0 || lastUnderscoreIndex < 0)
				return string.Empty;

			int length = lastUnderscoreIndex - firstUnderscoreIndex - 1;

			if (length == 0)
				return string.Empty;

			return eventName.Substring(firstUnderscoreIndex + 1, length);
		}

		private static string GetDacFieldNameForGenericFieldEvent(TDerived genericFieldEventInfo)
		{
			if (genericFieldEventInfo?.SignatureType != EventHandlerSignatureType.Generic || !genericFieldEventInfo.IsDacFieldEvent() ||
				genericFieldEventInfo.Symbol.Parameters.IsDefaultOrEmpty)
			{
				return string.Empty;
			}

			if (!(genericFieldEventInfo.Symbol.Parameters[0]?.Type is INamedTypeSymbol firstParameter) ||
				 firstParameter.TypeArguments.Length < 2)
			{
				return string.Empty;
			}

			ITypeSymbol dacField = firstParameter.TypeArguments[1];
			return dacField.IsDacField()
				? dacField.Name.ToPascalCase()
				: string.Empty;
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
