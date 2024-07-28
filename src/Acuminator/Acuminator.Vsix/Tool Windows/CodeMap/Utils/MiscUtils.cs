#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.PXFieldAttributes;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal static class MiscUtils
	{
		public static string? GetDbBoundnessLabelText(this DbBoundnessType dbBoundnessType) => dbBoundnessType switch
		{
			DbBoundnessType.Unbound    => VSIXResource.CodeMap_DbBoundnessIndicator_Unbound,
			DbBoundnessType.DbBound    => VSIXResource.CodeMap_DbBoundnessIndicator_Bound,
			DbBoundnessType.PXDBCalced => VSIXResource.CodeMap_DbBoundnessIndicator_PXDBCalced,
			DbBoundnessType.PXDBScalar => VSIXResource.CodeMap_DbBoundnessIndicator_PXDBScalar,
			DbBoundnessType.Unknown    => VSIXResource.CodeMap_DbBoundnessIndicator_Unknown,
			DbBoundnessType.Error 	   => VSIXResource.CodeMap_DbBoundnessIndicator_Inconsistent,
			_ 						   => null
		};
	}
}
