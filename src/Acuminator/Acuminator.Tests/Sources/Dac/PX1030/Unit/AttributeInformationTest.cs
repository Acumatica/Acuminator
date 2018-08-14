using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{

	[PXDBInt]
	[PXDefault(0)]
	public class NonNullableIntAttribute : PXAggregateAttribute
	{
	}

	[NonNullableInt]
	[PXIntList(new[] { 1, 2, 3 }, new[] { "First", "Second", "Third" })]
	public class NonNullableIntListAttribute : PXAggregateAttribute
	{
	}

	public class Foo : IBqlTable
	{
		public abstract class someField : IBqlField { }
		[NonNullableIntList]
		public int? SomeField { get; set; }
	}


	/* Class to test get information from*/
	[PXPrimaryGraph(typeof(IIGPOALCLandedCostEntry)), PXCacheName("Containers")]
	public class IIGPOALCLandedCost : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.IBqlField { }
		protected bool? _Selected;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region Cost
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Cost;
		[PXDBDecimal]
		[PXDefault]
		[PXUIField(DisplayName = "Cost")]
		public virtual decimal? Cost { get; set; }
		#endregion
	}
}