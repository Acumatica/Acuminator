using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces.Sources
{
    public class SWKMapadocConnMaintExt1 : PXGraphExtension<SWKMapadocConnMaint>
    {
       public static bool IsActive() => PXGraph.CreateInstance<SWKMapadocConnMaint>().IsActiveFeature();

	   public static bool IsActiveForGraph<TGraph>()
		where TGraph : PXGraph	=> 
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
							SWKMapadocConnMaint.CheckThatFeatureIsActive1();
			return isActive;
		}
	}

	public class SWKMapadocConnExtendedProcessingMaint : SWKMapadocConnMaint
	{
	}

	public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
    {
		public bool IsActiveFeature() => true; 

		public static bool CheckThatFeatureIsActive1() => PXGraph.CreateInstance<SWKMapadocConnMaint>().IsActiveFeature();

		public static bool CheckThatFeatureIsActive2()
		{
			var graph = PXGraph.CreateInstance<SWKMapadocConnMaint>();
			return graph.IsActiveFeature();
		}
	}
}
