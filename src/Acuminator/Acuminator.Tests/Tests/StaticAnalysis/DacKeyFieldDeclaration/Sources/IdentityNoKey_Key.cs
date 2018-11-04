using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;


namespace PX.Objects.HackathonDemo
{
	public class CFPMItemReq : IBqlTable
	{
		#region ItemReqID
		public abstract class itemReqID : PX.Data.IBqlField
		{
		}
		[PXDBIdentity]
		[PXUIField(DisplayName = "Item Req ID", Enabled = false, Visibility = PXUIVisibility.Invisible)]
		public virtual int? ItemReqID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField
		{
		}
		[Project(DisplayName = "Project ID", Enabled = false, DirtyRead = true, IsKey = true)]
		public virtual int? ProjectID { get; set; }
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.IBqlField
		{
		}
		[ProjectTask(DirtyRead = true)]
		public virtual int? TaskID { get; set; }
		#endregion
   
	}
	
	[PXDBInt()]
	[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.Visible)]
	public class ProjectTaskAttribute : AcctSubAttribute
	{
		public bool DirtyRead { get; set; }
		public bool IsKey { get; set; }
	}
	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
	public class ProjectAttribute : AcctSubAttribute
	{
		public string DisplayName { get; set; }
		public bool Enabled { get; set; }
		public bool DirtyRead { get; set; }
		public bool IsKey { get; set; }
	}
	public class AcctSubAttribute : PXAggregateAttribute
	{
	}
}
