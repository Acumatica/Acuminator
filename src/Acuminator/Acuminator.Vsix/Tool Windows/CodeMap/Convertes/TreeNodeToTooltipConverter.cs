using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Microsoft.CodeAnalysis;
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
				case ViewNodeViewModel viewNode:
					return viewNode.ViewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
				case ActionNodeViewModel actionNode:
					return actionNode.ActionInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
				case CacheAttachedNodeViewModel cacheAttachedNode 
				when cacheAttachedNode.Children.Count > 0:
					var attributeStrings = cacheAttachedNode.Children
															.OfType<AttributeNodeViewModel>()
															.Select(attribute => attribute.Tooltip);															
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
