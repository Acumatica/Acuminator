using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[PXDBInt]
	public class IntAggregatorAttribute : PXAggregateAttribute
	{
	}

	[PXInt]
	public class IntDerivedAggregatorAttribute : IntAggregatorAttribute
	{
	}

	[PXDBScalar(typeof(Search<DacWithInvalidAggregatorAttributes.someField>))]
	public class SpecialAttributesAggregatorAttribute : PXAggregateAttribute
	{
	}

	[PXDBCalced(typeof(Search<DacWithInvalidAggregatorAttributes.someField>), typeof(bool))]
	public class SpecialAttributesDerivedAggregatorAttribute : SpecialAttributesAggregatorAttribute
	{
	}


	public class DacWithInvalidAggregatorAttributes : IBqlTable
	{
		public abstract class someField : IBqlField { }
		
		[SpecialAttributesDerivedAggregator]
		public int? SomeField { get; set; }

		public abstract class otherField : IBqlField { }

		[IntDerivedAggregator]
		public int? OtherField { get; set; }
	}
}