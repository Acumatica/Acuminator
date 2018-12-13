using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the tooltip.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(string))]
	public class TreeNodeToTooltipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value)
			{
				case GraphMemberNodeViewModel graphMemberNode 
				when graphMemberNode.MemberInfo is DataViewInfo viewInfo:
					return viewInfo.Type.ToDisplayString();
				case GraphMemberNodeViewModel graphMemberNode
				when graphMemberNode.MemberInfo is ActionInfo actionInfo:
					return actionInfo.Type.ToDisplayString();
				case CacheAttachedNodeViewModel cacheAttachedNode:
					var attributeStrings = cacheAttachedNode.Children
															.OfType<AttributeNodeViewModel>()
															.Select(attribute => $"[{attribute.Attribute.ToString()}]");
					return string.Join(Environment.NewLine, attributeStrings);
				case AttributeNodeViewModel attributeNode:
					return attributeNode.Tooltip;
				default:
					return Binding.DoNothing;
			}
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
