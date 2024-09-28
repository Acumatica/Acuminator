#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacNodeViewModelBase : TreeNodeViewModel
	{
		public override Icon NodeIcon => IsDac
			? Icon.Dac
			: Icon.DacExtension;

		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public abstract DacOrDacExtInfoBase DacOrDacExtInfo { get; }

		public bool IsDac => DacOrDacExtInfo is DacInfo;

		public override string Name
		{
			get => DacOrDacExtInfo.Name;
			protected set { }
		}

		protected DacNodeViewModelBase(TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded) : 
								  base(tree, parent, isExpanded)
		{
		}

		protected TextViewModel CreateDacTypeInfo()
		{
			Color color = Color.FromRgb(38, 155, 199);

			string dacType = IsDac
				? VSIXResource.CodeMap_ExtraInfo_IsDac
				: VSIXResource.CodeMap_ExtraInfo_IsDacExtension;

			return new TextViewModel(this, dacType, darkThemeForeground: color, lightThemeForeground: color);
		}

		public override Task NavigateToItemAsync()
		{
			if (TryNavigateToItemWithVisualStudioWorkspace(DacOrDacExtInfo.Symbol))
				return Task.CompletedTask;

			// Fallback to the old VS navigation
			var references = DacOrDacExtInfo.Symbol.DeclaringSyntaxReferences;

			if (references.IsDefaultOrEmpty)
			{
				Location? location = GetLocationForNavigation(DacOrDacExtInfo.Node);

				return location?.NavigateToAsync() ?? Task.CompletedTask;
			}
			else
				return DacOrDacExtInfo.Symbol.NavigateToAsync();
		}

		protected Location? GetLocationForNavigation(ClassDeclarationSyntax? dacNode)
		{
			if (dacNode != null)
				return dacNode.Identifier.GetLocation().NullIfLocationKindIsNone() ?? dacNode.GetLocation();

			if (Parent is not DacBaseTypesCategoryNodeViewModel baseTypesCategoryNodeViewModel ||
				baseTypesCategoryNodeViewModel.Parent is not DacNodeViewModel dacNodeViewModel ||
				dacNodeViewModel.DacModel.Node?.BaseList?.Types.Count is null or 0)
			{
				return null;
			}

			var baseType = dacNodeViewModel.DacModel.Node.BaseList.Types[0];
			return baseType.GetLocation().NullIfLocationKindIsNone();
		}
	}
}