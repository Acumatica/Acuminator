using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	/// <summary>
	/// Attribute describes FinPeriod Field.
	/// This attribute contains static Util functions.
	/// </summary>
	public class FinPeriodIDAttribute : Attribute
	{
		#region Ctor
		public FinPeriodIDAttribute()
			: this(null)
		{
		}
		public FinPeriodIDAttribute(Type SourceType)
			: this(SourceType, null, null) 
		{
		}

		public FinPeriodIDAttribute(Type sourceType = null,
			Type branchSourceType = null,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			bool checkFinPeriodExistenceForDate = true,
			bool useMasterOrganizationIDByDefault = false)
	,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				checkFinPeriodExistenceForDate: checkFinPeriodExistenceForDate,
				filterByOrganizationID: true,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault)
		{
		}
	}
}
