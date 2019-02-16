using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the cache attached event in graph.
	/// </summary>
	public class GraphEventInfo : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		public GraphEventInfo Base { get; }

		public EventHandlerSignatureType SignatureType { get; }

		public EventType EventType { get; }

		public string DacName { get; }

		public GraphEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType) :
						 base(node, symbol, declarationOrder)
		{
			SignatureType = signatureType;
			EventType = eventType;
			DacName = GetDacName();
		}

		public GraphEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
							  EventHandlerSignatureType signatureType, EventType eventType, GraphEventInfo baseInfo)
			: this(node, symbol, declarationOrder, signatureType, eventType)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));

			Base = baseInfo;
		}

		public static string GetDacFieldNameForDefaultFieldEvent(GraphEventInfo defaultFieldEventInfo)
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

			int length = lastUnderscoreIndex - firstUnderscoreIndex;
			return eventName.Substring(firstUnderscoreIndex, lastUnderscoreIndex);
		}

		public static string GetDacFieldNameForGenericFieldEvent(GraphEventInfo genericFieldEventInfo)
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
					if (Symbol.Parameters.IsDefaultOrEmpty ||
					   !(Symbol.Parameters[0]?.Type is INamedTypeSymbol firstParameter) ||
					   firstParameter.TypeArguments.IsDefaultOrEmpty)
					{
						return string.Empty;
					}

					return firstParameter.TypeArguments[0].IsDAC()
						? firstParameter.TypeArguments[0].Name
						: string.Empty;

				case EventHandlerSignatureType.None:
				default:
					return string.Empty;
			}
		}
	}
}
