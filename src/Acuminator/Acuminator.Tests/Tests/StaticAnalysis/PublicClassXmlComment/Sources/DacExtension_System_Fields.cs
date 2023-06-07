using System;
using PX.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace PX.Objects
{
	/// <summary>
	/// A base DAC. Description is present to avoid diagnostic alert.
	/// </summary>
	public class BaseDAC : IBqlTable
	{ }

	/// <summary>
	/// A DAC extension with system fields. Description is present to avoid diagnostic alert.
	/// </summary>
	public sealed class DacExtensionWithSystemFields : PXCacheExtension<BaseDAC>
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXUIField(DisplayName = "Selected", Visible = false)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime] 
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion

		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }

		[PXDBGroupMask]
		public virtual Byte[] GroupMask
		{
			get;
			set;
		}
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		[CRAttributesField(typeof(Contract.templateID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region DeletedDatabaseRecord
		public abstract class deletedDatabaseRecord : PX.Data.BQL.BqlBool.Field<deletedDatabaseRecord> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? DeletedDatabaseRecord { get; set; }
		#endregion
	}
}
