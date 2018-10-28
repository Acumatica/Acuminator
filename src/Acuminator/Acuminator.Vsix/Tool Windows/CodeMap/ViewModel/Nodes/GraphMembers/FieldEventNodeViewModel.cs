using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class FieldEventNodeViewModel : GraphMemberNodeViewModel
	{
		public DacGroupingNodeViewModel DacViewModel { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public FieldEventNodeViewModel(DacGroupingNodeViewModel dacViewModel, ISymbol memberSymbol, bool isExpanded = false) :
								base(dacViewModel?.GraphMemberCategoryVM, memberSymbol, isExpanded)
		{
			DacViewModel = dacViewModel;

			int startPos = dacViewModel.DacName.Length + 1;
			int lastUnderscoreIndex = memberSymbol.Name.LastIndexOf('_');
			Name = lastUnderscoreIndex > 0
				? memberSymbol.Name.Substring(startPos)
				: memberSymbol.Name;
		}	
	}
}
