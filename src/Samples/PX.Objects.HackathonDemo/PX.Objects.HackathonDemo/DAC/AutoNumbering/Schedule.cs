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
	[PXCacheName("Schedule")]
	public partial class Schedule : IBqlTable
	{
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search<Schedule.scheduleID, Where<Schedule.module, Equal<Current<Schedule.module>>>>))]
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
