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

	[PXDBString]
	public class StringDerivedAggregatorAttribute : IntDerivedAggregatorAttribute
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
		#region SomeField
		public abstract class someField : IBqlField { }
		
		[SpecialAttributesDerivedAggregator]
		public int? SomeField { get; set; }
		#endregion

		#region OtherField
		public abstract class otherField : IBqlField { }

		[IntDerivedAggregator]
		public int? OtherField { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : IBqlField { }

		[StringDerivedAggregator]
		public string? RefNbr { get; set; }
		#endregion
	}
}