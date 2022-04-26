using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Owin.IdentityServerIntegration.DAC
{
	[Serializable]
	[PXCacheName("OAuthClient")]
	public class OAuthClient : IBqlTable
	{
		#region ClientID
		public abstract class clientID : PX.Data.BQL.BqlGuid.Field<clientID> { }

		[PXDBGuid(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Client ID")]
		[PXSelector(typeof(Search<clientID>), typeof(fullClientID), typeof(clientName), typeof(flow), typeof(enabled), DescriptionField = typeof(fullClientID))]
		public virtual Guid? ClientID { get; set; }
		#endregion

		#region FullClientID
		public abstract class fullClientID : PX.Data.BQL.BqlString.Field<fullClientID> { }

		[PXString]
		[PXUIField(DisplayName = "Client ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string FullClientID { get; set; }
		#endregion

		#region ClientName
		public abstract class clientName : PX.Data.BQL.BqlString.Field<clientName> { }

		[PXDBString(100, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Client Name")]
		public virtual string ClientName { get; set; }
		#endregion

		#region Enabled
		public abstract class enabled : PX.Data.BQL.BqlBool.Field<enabled> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Enabled { get; set; }
		#endregion

		#region ClientUri
		public abstract class clientUri : PX.Data.BQL.BqlString.Field<clientUri> { }

		[PXDBString(-1, IsUnicode = true)]
		public virtual string ClientUri { get; set; }
		#endregion

		#region LogoUri
		public abstract class logoUri : PX.Data.BQL.BqlString.Field<logoUri> { }

		[PXDBString(-1, IsUnicode = true)]
		public virtual string LogoUri { get; set; }
		#endregion

		#region Flow
		public abstract class flow : PX.Data.BQL.BqlString.Field<flow> { }

		[PXDBString(50, IsUnicode = false)]
		[PXStringList(nameof(OAuthClientFlow.AuthorizationCode) + ";Authorization Code"
			+ "," + nameof(OAuthClientFlow.Implicit) + ";Implicit"
			+ "," + nameof(OAuthClientFlow.ResourceOwner) + ";Resource Owner Password Credentials"
			+ "," + nameof(OAuthClientFlow.Hybrid) + ";Hybrid"
			)]
		[PXDefault]
		[PXUIField(DisplayName = "Flow")]
		public virtual string Flow { get; set; }
		#endregion

		#region Plugin
		public abstract class plugin : PX.Data.BQL.BqlString.Field<plugin> { }

		[PXDBString(128, IsUnicode = true)]
		[PXOAuthClientPlugin]
		[PXUIField(DisplayName = "Plug-In")]
		public virtual string Plugin { get; set; }
		#endregion

		public class PXOAuthClientPluginAttribute : PXCustomSelectorAttribute
		{
			public PXOAuthClientPluginAttribute() : base(typeof(OpenIdPlugin.name))
			{
				DescriptionField = typeof(OpenIdPlugin.description);
			}

			public IEnumerable GetRecords() => new object[] { };
		}
	}

	internal enum OAuthClientFlow
	{
		//we've inherited these values from IdentityServer3
		//https://github.com/IdentityServer/IdentityServer3/blob/master/source/Core/Models/Enums.cs#L38
		AuthorizationCode = 0,
		Implicit = 1,
		ClientCredentials = 3, //TODO: we should probably get rid of this one, it is used exclusively in one test
		ResourceOwner = 4,
		Hybrid = 5,
	}

	[Serializable]
	[PXHidden]
	public class OpenIdPlugin : IBqlTable
	{
		#region Name

		public abstract class name : BqlString.Field<name> { }
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public string Name { get; set; }

		#endregion

		#region Description

		public abstract class description : BqlString.Field<description> { }
		[PXString]
		[PXUIField(DisplayName = "Description", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public string Description { get; set; }

		#endregion
	}
}
