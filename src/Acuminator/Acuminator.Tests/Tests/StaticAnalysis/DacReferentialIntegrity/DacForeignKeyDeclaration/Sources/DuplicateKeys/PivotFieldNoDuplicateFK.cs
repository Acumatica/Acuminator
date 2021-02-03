using System;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.DacForeignKeyDeclaration.Sources.DuplicateKeys
{
	[PXCacheName("Pivot Field")]
	public class PivotField : IBqlTable
	{
		public class PK : PrimaryKeyOf<PivotField>.By<screenID, pivotTableID, pivotFieldID>
		{
			public static PivotField Find(PXGraph graph, string screenID, int? pivotTableID, int? pivotFieldID)
				=> FindBy(graph, screenID, pivotTableID, pivotFieldID);
		}

		public static class FK
		{
			public class SiteMapFK : SiteMap.UK.ForeignKeyOf<PivotField>.By<screenID> { }

			public class PortalMapFK : PortalMap.UK.ForeignKeyOf<PivotField>.By<screenID> { }
		}

		#region PivotTableID
		public abstract class pivotTableID : PX.Data.BQL.BqlInt.Field<pivotTableID> { }

		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField not applicable to NetTools
		[PXDBInt(IsKey = true)]
		public virtual int? PivotTableID { get; set; }
		#endregion

		#region PivotFieldID
		public abstract class pivotFieldID : PX.Data.BQL.BqlInt.Field<pivotFieldID> { }

		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField not applicable to NetTools
		[PXDBIdentity(IsKey = true)]
		public virtual int? PivotFieldID { get; set; }
		#endregion

		#region ScreenID
		public abstract class screenID : PX.Data.BQL.BqlString.Field<screenID> { }

		[PXDBString(8, IsFixed = true, InputMask = "CC.CC.CC.CC")]
		public virtual string ScreenID { get; set; }
		#endregion
	}

	/// <summary>
	/// A DAC used to provide PK and UK to compile PivotField sources with foreign keys which refer to SiteMap UK
	/// </summary>
	[PXHidden]
	public class SiteMap : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SiteMap>.By<nodeID>
		{
			public static SiteMap Find(PXGraph graph, Guid? nodeID) => FindBy(graph, nodeID);
		}

		public class UK : PrimaryKeyOf<SiteMap>.By<screenID>
		{
			public static SiteMap Find(PXGraph graph, string screenID) => FindBy(graph, screenID);
		}
		#endregion

		#region NodeID
		public abstract class nodeID : PX.Data.BQL.BqlGuid.Field<nodeID> { }

		[PXDBGuid(IsKey = true)]
		[PXDefault]
		public virtual Guid? NodeID
		{
			get;
			set;
		}
		#endregion

		#region ScreenID
		public abstract class screenID : PX.Data.BQL.BqlString.Field<screenID> { }

		[PXDBString(8, InputMask = ">CC.CC.CC.CC")]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Screen ID")]
		[PXDefault]
		[PXReferentialIntegrityCheck]
		public virtual String ScreenID
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// A DAC used to provide PK and UK to compile PivotField sources with foreign keys which refer to PortalMap UK
	/// </summary>
	[PXHidden]
	public class PortalMap : SiteMap
	{
		public new class PK : PrimaryKeyOf<PortalMap>.By<nodeID>
		{
			public static PortalMap Find(PXGraph graph, Guid? nodeID) => FindBy(graph, nodeID);
		}

		public new class UK : PrimaryKeyOf<PortalMap>.By<screenID>
		{
			public static PortalMap Find(PXGraph graph, string screenID) => FindBy(graph, screenID);
		}

		/// <exclude/>
		public new abstract class nodeID : PX.Data.BQL.BqlGuid.Field<nodeID> { }

		/// <exclude/>
		public new abstract class screenID : PX.Data.BQL.BqlString.Field<screenID> { }
	}
}
