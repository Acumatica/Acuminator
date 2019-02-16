using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;




namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldGroupingNodeViewModel : TreeNodeViewModel
	{
		public GraphEventCategoryNodeViewModel GraphEventsCategoryVM => DacVM.GraphEventsCategoryVM;

		public DacEventsGroupingNodeViewModel DacVM { get; }

		public string DacFieldName { get; }

		public override string Name
		{
			get => $"{DacFieldName}({Children.Count})";
			protected set { }
		}

		protected DacFieldGroupingNodeViewModel(DacEventsGroupingNodeViewModel dacVM, string dacFieldName, bool isExpanded) :
										   base(dacVM?.Tree, isExpanded)
		{
			dacFieldName.ThrowOnNullOrWhiteSpace(nameof(dacFieldName));

			DacVM = dacVM;
			DacFieldName = dacFieldName;
		}

		public static DacFieldGroupingNodeViewModel Create(DacEventsGroupingNodeViewModel dacVM, string dacFieldName, 
														   IEnumerable<GraphEventInfo> dacFieldEvents,
														   bool isExpanded = false)
		{
			if (dacFieldEvents.IsNullOrEmpty() || dacFieldName.IsNullOrWhiteSpace())
			{
				return null;
			}

			var dacFieldVM = new DacFieldGroupingNodeViewModel(dacVM, dacFieldName, isExpanded);
			var dacFieldEventVMs = dacFieldVM.GetDacFieldNodeChildren(graphEventsForDAC, eventVMCreator, areChildrenExpanded);
			dacVM.Children.AddRange(dacMembers);
			return dacVM;
		}

		public static string GetDacFieldNameForFieldEvent(GraphEventInfo eventInfo)
		{
			if (eventInfo == null)
				return string.Empty;

			switch (eventInfo.SignatureType)
			{
				case EventHandlerSignatureType.Default:
					
					return lastUnderscoreIndex > 0 && lastUnderscoreIndex < MemberSymbol.Name.Length - 1
						? MemberSymbol.Name.Substring(lastUnderscoreIndex + 1)
						: MemberSymbol.Name;
				case EventHandlerSignatureType.Generic:
					return GetDacFieldNameForGenericFieldEvent(eventInfo);
				case EventHandlerSignatureType.None:
				default:
					return MemberSymbol.Name;
			}
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacFieldNodeChildren(IEnumerable<GraphEventInfo> dacFieldEvents,
																						bool isExpanded)
		{
			return dacFieldEvents.Select(eventInfo => eventVMCreator(this, eventInfo, isExpanded))
								 .Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
								 .OrderBy(graphMemberVM => graphMemberVM.Name);
		}
	}
}