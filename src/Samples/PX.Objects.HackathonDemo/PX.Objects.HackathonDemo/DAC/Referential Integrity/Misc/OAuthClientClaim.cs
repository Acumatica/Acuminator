using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using System;

namespace PX.Owin.IdentityServerIntegration.DAC
{
	[Serializable]
	[PXCacheName("OAuthClientClaim")]
	public class OAuthClientClaim : IBqlTable
	{
		#region ClientID
		public abstract class clientID : PX.Data.BQL.BqlGuid.Field<clientID> { }

		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(OAuthClient.clientID))]
		[PXParent(typeof(Select<OAuthClient, Where<OAuthClient.clientID, Equal<Current<OAuthClientClaim.clientID>>>>))]
		public virtual Guid? ClientID { get; set; }
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion

		#region ClaimName
		public abstract class claimName : PX.Data.BQL.BqlString.Field<claimName> { }

		[PXDBString(50, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Claim Name", Enabled = false)]
		public virtual string ClaimName { get; set; }
		#endregion

		#region Scope
		public abstract class scope : PX.Data.BQL.BqlString.Field<scope> { }

		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Scope", Enabled = false)]
		public virtual string Scope { get; set; }
		#endregion

		#region Plugin
		public abstract class plugin : PX.Data.BQL.BqlString.Field<plugin> { }

		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Plugin", Enabled = false)]
		public virtual string Plugin { get; set; }
		#endregion
	}
}
