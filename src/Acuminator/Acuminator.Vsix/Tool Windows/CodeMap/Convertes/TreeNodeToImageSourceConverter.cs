using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the string url of the image source.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(string))]
	public class TreeNodeToImageSourceConverter : IValueConverter
	{
		private const string BaseURI = @"pack://application:,,,/Acuminator;component/Resources/CodeMap";
		private const string IconFileExtension = "ico";

		private const string GraphIcon = "Graph";
		private const string ViewIcon = "View";
		private const string ViewDelegateIcon = "ViewDelegate";
		private const string ActionIcon = "Action";
		private const string ActionHandlerIcon = "ActionHandler";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TreeNodeViewModel treeNodeViewModel))
				return null;

			string iconFileName = GetIconFileName(treeNodeViewModel);

			if (iconFileName == null)
				return null;

			return $"{BaseURI}/{iconFileName}.{IconFileExtension}";
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		private string GetIconFileName(TreeNodeViewModel treeNodeViewModel)
		{
			switch (treeNodeViewModel)
			{
				case GraphNodeViewModel graphNode:
					return GraphIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.View:
					return ViewIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.ViewDelegate:
					return ViewDelegateIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.Action:
					return ActionIcon;
				case GraphMemberNodeViewModel graphMember when graphMember.MemberType == GraphMemberType.ActionHandler:
					return ActionHandlerIcon;
				default:
					return null;
			}
		}
	}
}
