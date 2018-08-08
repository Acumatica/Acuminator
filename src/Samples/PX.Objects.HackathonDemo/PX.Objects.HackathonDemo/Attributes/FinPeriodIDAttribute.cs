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
		public FinPeriodIDAttribute()
			: this(null)
		{
			throw new NotImplementedException();
		}

		public FinPeriodIDAttribute(Type SourceType)
			: this(SourceType, null, null) 
		{
			throw new NotImplementedException();
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
		{
			throw new NotImplementedException();
		}
	}
}