using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacEventsGroupingNodeViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public FieldEventNodeViewModel(DacEventsGroupingNodeViewModel dacViewModel, GraphNodeSymbolItem memberInfo, bool isExpanded = false) :
								base(dacViewModel?.GraphMemberCategoryVM, memberInfo, isExpanded)
		{
			DacViewModel = dacViewModel;
			Name = GetUINameForFieldEvent();
		}	

		private string GetUINameForFieldEvent()
		{
			if (!(MemberInfo is GraphEventInfo eventInfo))
				return MemberSymbol.Name;

			switch (eventInfo.SignatureType)
			{	
				case EventHandlerSignatureType.Default:
					int startPos = DacViewModel.DacName.Length + 1;
					int lastUnderscoreIndex = MemberSymbol.Name.LastIndexOf('_');
					return lastUnderscoreIndex > 0
						? MemberSymbol.Name.Substring(startPos)
										   .Replace('_', ' ')
						: MemberSymbol.Name;

				case EventHandlerSignatureType.Generic:
					return GetUINameForGenericFieldEvent(eventInfo);

				case EventHandlerSignatureType.None:
				default:
					return MemberSymbol.Name;
			}
		}

		private string GetUINameForGenericFieldEvent(GraphEventInfo genericFieldEventInfo)
		{
			if (genericFieldEventInfo.Symbol.Name != GraphEventInfo.GenericEventName)
				return genericFieldEventInfo.Symbol.Name;

			if (genericFieldEventInfo.Symbol.Parameters.IsDefaultOrEmpty)
				return genericFieldEventInfo.Symbol.Name;

			if (!(genericFieldEventInfo.Symbol.Parameters[0]?.Type is INamedTypeSymbol firstParameter) ||
				 firstParameter.TypeArguments.Length < 2)
			{
				return genericFieldEventInfo.Symbol.Name;
			}

			ITypeSymbol dacField = firstParameter.TypeArguments[1];
			return dacField.IsDacField()
				? $"{dacField.Name.ToPascalCase()} {genericFieldEventInfo.EventType.ToString()}"
				: genericFieldEventInfo.Symbol.Name;
		}
	}
}
