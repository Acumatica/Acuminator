using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the <see cref="BitmapImage"/> icon.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(BitmapImage))]
	public class TreeNodeToImageSourceConverter : IValueConverter
	{
		private const string BitmapsCollectionURI = @"pack://application:,,,/Acuminator;component/Resources/CodeMap/Themes/BitmapImages.xaml";

		private const string GraphIcon = "Graph";
		private const string DacIcon = "DAC";
		private const string DacFieldIcon = "DacField";

		private const string ViewIcon = "View";
		private const string ViewDelegateIcon = "ViewDelegate";

		private const string ActionIcon = "Action";
		private const string ActionHandlerIcon = "ActionHandler";

		private const string PXOverrideIcon = "PXOverride";

		private const string EventIcon = "Event";
		private const string RowEventIcon = "RowEvent";
		private const string FieldEventIcon = "FieldEvent";
		private const string CacheAttachedIcon = "CacheAttached";

		private const string GroupNodeIcon = "GroupNode";

		private ResourceDictionary _resourceDictionary = new ResourceDictionary()
		{
			Source = new Uri(BitmapsCollectionURI)
		};


		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TreeNodeViewModel treeNodeViewModel))
				return null;

			string iconKey = GetIconResourceKeyForNode(treeNodeViewModel);

			if (iconKey == null)
				return null;

			if (!_resourceDictionary.TryGetValue(iconKey, out BitmapImage icon))
				return null;

			return icon;
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		private string GetIconResourceKeyForNode(TreeNodeViewModel treeNodeViewModel)
		{
			switch (treeNodeViewModel)
			{
				case GraphNodeViewModel graphNode:
					return GraphIcon;
				case CacheAttachedCategoryNodeViewModel cacheAttachedCategoryNode:
				case RowEventCategoryNodeViewModel rowEventCategoryNode:		
				case FieldEventCategoryNodeViewModel fieldEventCategoryNode:
					return EventIcon;
				case GraphMemberCategoryNodeViewModel graphMemberCategoryNode:
					return GroupNodeIcon;
				case DacGroupingNodeBaseViewModel dacGroupingNodeViewModel:
					return DacIcon;
				case DacFieldGroupingNodeBaseViewModel dacFieldGroupingNodeViewModel:
					return DacFieldIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.View:
					return ViewIcon;		
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.Action:
					return ActionIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.PXOverride:
					return PXOverrideIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.RowEvent:
					return RowEventIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.FieldEvent:
					return FieldEventIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.CacheAttached:
					return CacheAttachedIcon;
				case GraphMemberInfoNodeViewModel graphMemberInfo when graphMemberInfo.GraphMemberInfoType == GraphMemberInfoType.ViewDelegate:
					return ViewDelegateIcon;
				case GraphMemberInfoNodeViewModel graphMemberInfo when graphMemberInfo.GraphMemberInfoType == GraphMemberInfoType.ActionHandler:
					return ActionHandlerIcon;
				default:
					return null;
			}
		}
	}
}
