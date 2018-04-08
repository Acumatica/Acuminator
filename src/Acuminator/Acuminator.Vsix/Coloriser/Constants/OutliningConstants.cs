using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acuminator.Vsix.Coloriser
{
	public static class OutliningConstants
	{		
        public enum OutliningRegionType { BQL, Attribute }

        public const string DefaultCollapsedBQLRegionText = "<...>";
        public const int HintTooltipMaxLength = 2390;

        public const int MaxAttributeOutliningLevel = 1;

        public static string SuffixForTooLongTooltips => Environment.NewLine + "...";
    }
}
