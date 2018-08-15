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
}