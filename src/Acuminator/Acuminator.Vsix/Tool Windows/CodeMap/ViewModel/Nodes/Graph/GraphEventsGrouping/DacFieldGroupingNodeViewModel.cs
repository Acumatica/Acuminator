using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacFieldGroupingNodeViewModel : TreeNodeViewModel
	{
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
														   GraphEventNodeConstructor eventVMCreator,
														   bool isExpanded = false)
		{
			if (eventVMCreator == null || dacFieldEvents.IsNullOrEmpty() || dacFieldName.IsNullOrWhiteSpace())
			{
				return null;
			}

			var dacFieldVM = new DacFieldGroupingNodeViewModel(dacVM, dacFieldName, isExpanded);
			var dacFieldEventVMs = dacFieldVM.GetDacFieldNodeChildren(graphEventsForDAC, eventVMCreator, areChildrenExpanded);
			dacVM.Children.AddRange(dacMembers);
			return dacVM;
		}

		protected virtual IEnumerable<GraphMemberNodeViewModel> GetDacFieldNodeChildren(IEnumerable<GraphEventInfo> dacFieldEvents,
																						GraphEventNodeConstructor eventVMCreator, 
																						bool isExpanded)
		{
			return dacFieldEvents.Select(eventInfo => eventVMCreator(this, eventInfo, isExpanded))
								 .Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
								 .OrderBy(graphMemberVM => graphMemberVM.Name);
		}
	}
}
