using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.HackathonDemo
{
	[PXHidden]
	public partial class NonStringAutoNumberingDac : IBqlTable
	{
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]        //Display error for AutoNumber non on string DAC property
		[PXDefault]
		public virtual string ScheduleID
		{
			get;
			set;
		}
		#endregion

		#region ScheduleName
		public abstract class scheduleName : PX.Data.BQL.BqlString.Field<scheduleName> { }

		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ScheduleName
		{
			get;
			set;
		}
		#endregion

		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }

		[PXDBString(2, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
	}
}
