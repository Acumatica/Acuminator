using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn
{
	public static class Constants
	{
		public static class Types
		{

			#region FieldAttributeSymbols

			/// <summary>
			/// The PXDBPackedIntegerArrayAttribute type full name in Acumatica 2018R2. Doesn't exist in previous versions.
			/// </summary>
			public static readonly string PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2 = "PX.Data.PXDBPackedIntegerArrayAttribute";

			public static readonly string PeriodIDAttribute = "PX.Objects.GL.PeriodIDAttribute";
			public static readonly string AcctSubAttribute = "PX.Objects.GL.AcctSubAttribute";
			public static readonly string PXDBCalcedAttribute = "PX.Data.PXDBCalcedAttribute";
			public static readonly string PXDBScalarAttribute = "PX.Data.PXDBScalarAttribute";
			public static readonly string PXDBUserPasswordAttribute = "PX.Data.PXDBUserPasswordAttribute";
			public static readonly string PXDBAttributeAttribute = "PX.Data.PXDBAttributeAttribute";
			public static readonly string PXDBDataLengthAttribute = "PX.Data.PXDBDataLengthAttribute";
			public static readonly string PXDBBinaryAttribute = "PX.Data.PXDBBinaryAttribute";
			public static readonly string PXDBLongIdentityAttribute = "PX.Data.PXDBLongIdentityAttribute";
			public static readonly string PXDBIdentityAttribute = "PX.Data.PXDBIdentityAttribute";
			public static readonly string PXDBTimestampAttribute = "PX.Data.PXDBTimestampAttribute";
			public static readonly string PXDBBoolAttribute = "PX.Data.PXDBBoolAttribute";
			public static readonly string PXDBGuidAttribute = "PX.Data.PXDBGuidAttribute";
			public static readonly string PXDBDateAttribute = "PX.Data.PXDBDateAttribute";
			public static readonly string PXDBFloatAttribute = "PX.Data.PXDBFloatAttribute";
			public static readonly string PXDBDoubleAttribute = "PX.Data.PXDBDoubleAttribute";


			public static readonly string PXLongAttribute = "PX.Data.PXLongAttribute";
			public static readonly string PXIntAttribute = "PX.Data.PXIntAttribute";
			public static readonly string PXShortAttribute = "PX.Data.PXShortAttribute";
			public static readonly string PXStringAttribute = "PX.Data.PXStringAttribute";
			public static readonly string PXByteAttribute = "PX.Data.PXByteAttribute";
			public static readonly string PXDecimalAttribute = "PX.Data.PXDecimalAttribute";
			public static readonly string PXDoubleAttribute = "PX.Data.PXDoubleAttribute";
			public static readonly string PXFloatAttribute = "PX.Data.PXFloatAttribute";
			public static readonly string PXDateAttribute = "PX.Data.PXDateAttribute";
			public static readonly string PXGuidAttribute = "PX.Data.PXGuidAttribute";
			public static readonly string PXBoolAttribute = "PX.Data.PXBoolAttribute";
			public static readonly string PXDBFieldAttribute = "PX.Data.PXDBFieldAttribute";
			public static readonly string PXDBLongAttribute = "PX.Data.PXDBLongAttribute";
			public static readonly string PXDBIntAttribute = "PX.Data.PXDBIntAttribute";
			public static readonly string PXDBShortAttribute = "PX.Data.PXDBShortAttribute";
			public static readonly string PXDBStringAttribute = "PX.Data.PXDBStringAttribute";
			public static readonly string PXDBByteAttribute = "PX.Data.PXDBByteAttribute";
			public static readonly string PXDBDecimalAttribute = "PX.Data.PXDBDecimalAttribute";

			#endregion

			public static readonly string PXGraphExtension = "PX.Data.PXGraphExtension";
			public static readonly string PXCacheExtension = "PX.Data.PXCacheExtension";
			public static readonly string PXMappedCacheExtension = "PX.Data.PXMappedCacheExtension";
			public static readonly string PXLongOperation = "PX.Data.PXLongOperation";
			public static readonly string PXActionCollection = "PX.Data.PXActionCollection";
			public static readonly string PXAdapter = "PX.Data.PXAdapter";
			public static readonly string IBqlTable = "PX.Data.IBqlTable";
			public static readonly string IBqlField = "PX.Data.IBqlField";
			public static readonly string Constant = "PX.Data.Constant`1";
			public static readonly string IPXResultset = "PX.Data.IPXResultset";
			public static readonly string PXResult = "PX.Data.PXResult";
			public static readonly string PXFieldState = "PX.Data.PXFieldState";
			public static readonly string PXAttributeFamilyAttribute = "PX.Data.PXAttributeFamilyAttribute";
			public static readonly string IPXLocalizableList = "PX.Data.IPXLocalizableList";
			public static readonly string PXConnectionScope = "PX.Data.PXConnectionScope";

			public static readonly string PXGraphExtensionInitialize = "Initialize";
			public static readonly string PXLongOperationStartOperation = "StartOperation";

		}
	}
}