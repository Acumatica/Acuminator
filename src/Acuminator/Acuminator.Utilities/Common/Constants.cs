using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	internal static class Constants
	{
		internal static class Types
		{

			#region FieldAttributeSymbols

			/// <summary>
			/// The PXDBPackedIntegerArrayAttribute type full name in Acumatica 2018R2. Doesn't exist in previous versions.
			/// </summary>
			internal static readonly string PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2 = "PX.Data.PXDBPackedIntegerArrayAttribute";

			internal static readonly string PeriodIDAttribute = "PX.Objects.GL.PeriodIDAttribute";
			internal static readonly string AcctSubAttribute = "PX.Objects.GL.AcctSubAttribute";
			internal static readonly string PXDBCalcedAttribute = "PX.Data.PXDBCalcedAttribute";
			internal static readonly string PXDBScalarAttribute = "PX.Data.PXDBScalarAttribute";
			internal static readonly string PXDBUserPasswordAttribute = "PX.Data.PXDBUserPasswordAttribute";
			internal static readonly string PXDBAttributeAttribute = "PX.Data.PXDBAttributeAttribute";
			internal static readonly string PXDBDataLengthAttribute = "PX.Data.PXDBDataLengthAttribute";
			internal static readonly string PXDBBinaryAttribute = "PX.Data.PXDBBinaryAttribute";
			internal static readonly string PXDBLongIdentityAttribute = "PX.Data.PXDBLongIdentityAttribute";
			internal static readonly string PXDBIdentityAttribute = "PX.Data.PXDBIdentityAttribute";
			internal static readonly string PXDBTimestampAttribute = "PX.Data.PXDBTimestampAttribute";
			internal static readonly string PXDBBoolAttribute = "PX.Data.PXDBBoolAttribute";
			internal static readonly string PXDBGuidAttribute = "PX.Data.PXDBGuidAttribute";
			internal static readonly string PXDBDateAttribute = "PX.Data.PXDBDateAttribute";
			internal static readonly string PXDBFloatAttribute = "PX.Data.PXDBFloatAttribute";
			internal static readonly string PXDBDoubleAttribute = "PX.Data.PXDBDoubleAttribute";


			internal static readonly string PXLongAttribute = "PX.Data.PXLongAttribute";
			internal static readonly string PXIntAttribute = "PX.Data.PXIntAttribute";
			internal static readonly string PXShortAttribute = "PX.Data.PXShortAttribute";
			internal static readonly string PXStringAttribute = "PX.Data.PXStringAttribute";
			internal static readonly string PXByteAttribute = "PX.Data.PXByteAttribute";
			internal static readonly string PXDecimalAttribute = "PX.Data.PXDecimalAttribute";
			internal static readonly string PXDoubleAttribute = "PX.Data.PXDoubleAttribute";
			internal static readonly string PXFloatAttribute = "PX.Data.PXFloatAttribute";
			internal static readonly string PXDateAttribute = "PX.Data.PXDateAttribute";
			internal static readonly string PXGuidAttribute = "PX.Data.PXGuidAttribute";
			internal static readonly string PXBoolAttribute = "PX.Data.PXBoolAttribute";
			internal static readonly string PXDBFieldAttribute = "PX.Data.PXDBFieldAttribute";
			internal static readonly string PXDBLongAttribute = "PX.Data.PXDBLongAttribute";
			internal static readonly string PXDBIntAttribute = "PX.Data.PXDBIntAttribute";
			internal static readonly string PXDBShortAttribute = "PX.Data.PXDBShortAttribute";
			internal static readonly string PXDBStringAttribute = "PX.Data.PXDBStringAttribute";
			internal static readonly string PXDBByteAttribute = "PX.Data.PXDBByteAttribute";
			internal static readonly string PXDBDecimalAttribute = "PX.Data.PXDBDecimalAttribute";

			#endregion

			internal static readonly string PXGraphExtension = "PX.Data.PXGraphExtension";
			internal static readonly string PXCacheExtension = "PX.Data.PXCacheExtension";
			internal static readonly string PXMappedCacheExtension = "PX.Data.PXMappedCacheExtension";
			internal static readonly string PXLongOperation = "PX.Data.PXLongOperation";
			internal static readonly string PXActionCollection = "PX.Data.PXActionCollection";
			internal static readonly string PXAdapter = "PX.Data.PXAdapter";
			internal static readonly string IBqlTable = "PX.Data.IBqlTable";
			internal static readonly string IBqlField = "PX.Data.IBqlField";
			internal static readonly string Constant = "PX.Data.Constant`1";
			internal static readonly string IPXResultset = "PX.Data.IPXResultset";
			internal static readonly string PXResult = "PX.Data.PXResult";
			internal static readonly string PXFieldState = "PX.Data.PXFieldState";
			internal static readonly string PXAttributeFamilyAttribute = "PX.Data.PXAttributeFamilyAttribute";
			internal static readonly string IPXLocalizableList = "PX.Data.IPXLocalizableList";
			internal static readonly string PXConnectionScope = "PX.Data.PXConnectionScope";

			internal static readonly string PXGraphExtensionInitialize = "Initialize";
			internal static readonly string PXLongOperationStartOperation = "StartOperation";

		}
	}
}
