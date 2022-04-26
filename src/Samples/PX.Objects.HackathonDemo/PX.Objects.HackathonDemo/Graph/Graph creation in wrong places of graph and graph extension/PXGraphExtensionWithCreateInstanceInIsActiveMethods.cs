using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces.Sources
{
	public class SWKMapadocConnMaintExt1 : PXGraphExtension<SWKMapadocConnMaint>
	{
		public static bool IsActive() => PXGraph.CreateInstance<SWKMapadocConnMaint>().IsActiveFeature();

		public static bool IsActiveForGraph<TGraph>()
		 where TGraph : PXGraph =>
			 PXGraph.CreateInstance<SWKMapadocConnMaint>().GetType() == typeof(SWKMapadocConnMaint) &&
			 SWKMapadocConnMaint.CheckThatFeatureIsActive1();
	}

	public class SWKMapadocConnMaintExt2 : PXGraphExtension<SWKMapadocConnMaint>
	{
		public static bool IsActive()
		{
			return SWKMapadocConnMaint.CheckThatFeatureIsActive2();
		}

		public static bool IsActiveForGraph<TGraph>()
		where TGraph : PXGraph
		{
			bool isActive = PXGraph.CreateInstance<SWKMapadocConnMaint>().GetType() == typeof(SWKMapadocConnMaint) &&
							SWKMapadocConnMaint.IsActiveSwitch;
			return isActive;
		}
	}

	public class SWKMapadocConnExtendedProcessingMaint : SWKMapadocConnMaint
	{
	}

	public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
	{
		public static bool IsActiveSwitch => CheckThatFeatureIsActive1();

		public bool IsActiveFeature() => true;

		public static bool CheckThatFeatureIsActive1() => PXGraph.CreateInstance<SWKMapadocConnMaint>().IsActiveFeature();

		public static bool CheckThatFeatureIsActive2()
		{
			return SelectFrom<SWKCustomer>
					   .Where<SWKCustomer.processingEnabled.IsEqual<True>>
					   .View.ReadOnly
					   .Select(new PXGraph())
					   .Any();
		}
	}

	[PXCacheName("SWK Customer")]
	public class SWKCustomer : Customer
	{
		public abstract class processingEnabled : BqlBool.Field<processingEnabled> { }

		[PXDBBool]
		public bool? ProcessingEnabled { get; set; }
	}
}
