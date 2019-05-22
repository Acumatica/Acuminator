using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public static class Constants
	{
		public static class Types
		{

			#region FieldAttributeSymbols

			/// <summary>
			/// The PXDBPackedIntegerArrayAttribute type full name in Acumatica 2018R2. Doesn't exist in previous versions.
			/// </summary>
			static readonly string PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2 = "PX.Data.PXDBPackedIntegerArrayAttribute";

			static readonly string PeriodIDAttribute = "PX.Objects.GL.PeriodIDAttribute";
			static readonly string AcctSubAttribute = "PX.Objects.GL.AcctSubAttribute";
			static readonly string PXDBCalcedAttribute = "PX.Data.PXDBCalcedAttribute";
			static readonly string PXDBScalarAttribute = "PX.Data.PXDBScalarAttribute";
			static readonly string PXDBUserPasswordAttribute = "PX.Data.PXDBUserPasswordAttribute";
			static readonly string PXDBAttributeAttribute = "PX.Data.PXDBAttributeAttribute";
			static readonly string PXDBDataLengthAttribute = "PX.Data.PXDBDataLengthAttribute";
			static readonly string PXDBBinaryAttribute = "PX.Data.PXDBBinaryAttribute";
			static readonly string PXDBLongIdentityAttribute = "PX.Data.PXDBLongIdentityAttribute";
			static readonly string PXDBIdentityAttribute = "PX.Data.PXDBIdentityAttribute";
			static readonly string PXDBTimestampAttribute = "PX.Data.PXDBTimestampAttribute";
			static readonly string PXDBBoolAttribute = "PX.Data.PXDBBoolAttribute";
			static readonly string PXDBGuidAttribute = "PX.Data.PXDBGuidAttribute";
			static readonly string PXDBDateAttribute = "PX.Data.PXDBDateAttribute";
			static readonly string PXDBFloatAttribute = "PX.Data.PXDBFloatAttribute";
			static readonly string PXDBDoubleAttribute = "PX.Data.PXDBDoubleAttribute";


			static readonly string PXLongAttribute = "PX.Data.PXLongAttribute";
			static readonly string PXIntAttribute = "PX.Data.PXIntAttribute";
			static readonly string PXShortAttribute = "PX.Data.PXShortAttribute";
			static readonly string PXStringAttribute = "PX.Data.PXStringAttribute";
			static readonly string PXByteAttribute = "PX.Data.PXByteAttribute";
			static readonly string PXDecimalAttribute = "PX.Data.PXDecimalAttribute";
			static readonly string PXDoubleAttribute = "PX.Data.PXDoubleAttribute";
			static readonly string PXFloatAttribute = "PX.Data.PXFloatAttribute";
			static readonly string PXDateAttribute = "PX.Data.PXDateAttribute";
			static readonly string PXGuidAttribute = "PX.Data.PXGuidAttribute";
			static readonly string PXBoolAttribute = "PX.Data.PXBoolAttribute";
			static readonly string PXDBFieldAttribute = "PX.Data.PXDBFieldAttribute";
			static readonly string PXDBLongAttribute = "PX.Data.PXDBLongAttribute";
			static readonly string PXDBIntAttribute = "PX.Data.PXDBIntAttribute";
			static readonly string PXDBShortAttribute = "PX.Data.PXDBShortAttribute";
			static readonly string PXDBStringAttribute = "PX.Data.PXDBStringAttribute";
			static readonly string PXDBByteAttribute = "PX.Data.PXDBByteAttribute";
			static readonly string PXDBDecimalAttribute = "PX.Data.PXDBDecimalAttribute";

			#endregion

			static readonly string PXGraphExtension = "PX.Data.PXGraphExtension";
			static readonly string PXCacheExtension = "PX.Data.PXCacheExtension";
			static readonly string PXMappedCacheExtension = "PX.Data.PXMappedCacheExtension";
			static readonly string PXLongOperation = "PX.Data.PXLongOperation";
			static readonly string PXActionCollection = "PX.Data.PXActionCollection";
			static readonly string PXAdapter = "PX.Data.PXAdapter";
			static readonly string IBqlTable = "PX.Data.IBqlTable";
			static readonly string IBqlField = "PX.Data.IBqlField";
			static readonly string Constant = "PX.Data.Constant`1";
			static readonly string IPXResultset = "PX.Data.IPXResultset";
			static readonly string PXResult = "PX.Data.PXResult";
			static readonly string PXFieldState = "PX.Data.PXFieldState";
			static readonly string PXAttributeFamilyAttribute = "PX.Data.PXAttributeFamilyAttribute";
			static readonly string IPXLocalizableList = "PX.Data.IPXLocalizableList";
			static readonly string PXConnectionScope = "PX.Data.PXConnectionScope";

			static readonly string PXGraphExtensionInitialize = "Initialize";
			static readonly string PXLongOperationStartOperation = "StartOperation";

		}
	}
}