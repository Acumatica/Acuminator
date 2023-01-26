using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.HackathonDemo
{
	[NonNullableIntList]
	[PXDefault(0)]
	public class NonNullableIntAttribute : PXAggregateAttribute
	{
	}

	[NonNullableInt]
	[PXIntList(new[] { 1, 2, 3 }, new[] { "First", "Second", "Third" })]
	[PeriodID(IsDBField = false)]
	public class NonNullableIntListAttribute : PXAggregateAttribute
	{
	}

	[PXDBInt]
	[DBBoundNonNullableIntList]
	public class DBBoundNonNullableIntAttribute : PXAggregateAttribute
	{
	}

	[DBBoundNonNullableInt]
	[PXIntList(new[] { 1, 2, 3 }, new[] { "First", "Second", "Third" })]
	public class DBBoundNonNullableIntListAttribute : PXAggregateAttribute
	{
	}

	[UnknownBoundnessIntList]
	public class UnknownBoundnessNonNullableIntAttribute : PXAggregateAttribute
	{
		public bool IsDbField { get; set; }
	}

	[UnknownBoundnessNonNullableInt]
	[PXUIField(DisplayName = "Some Name")]

	public class UnknownBoundnessIntListAttribute : PXAggregateAttribute
	{
	}


	public class Foo : IBqlTable
	{
		public abstract class unboundField : IBqlField { }

		[NonNullableIntList]
		public int? UnboundField { get; set; }

		public abstract class boundField : IBqlField { }

		[DBBoundNonNullableIntList]
		public int? BoundField { get; set; }

		public abstract class unknownBoundnessField : IBqlField { }

		[UnknownBoundnessIntList]
		public int? UnknownBoundnessField { get; set; }
	}
}