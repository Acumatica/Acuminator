using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[NonNullableIntList]
	[PXDefault(0)]
	public class NonNullableIntAttribute : PXAggregateAttribute
	{
	}

	[NonNullableInt]
	[PXIntList(new[] { 1, 2, 3 }, new[] { "First", "Second", "Third" })]
	public class NonNullableIntListAttribute : PXAggregateAttribute
	{
	}

	[_NonNullableIntList]
	[PXDBInt]
	public class _NonNullableIntAttribute : PXAggregateAttribute
	{
	}

	[_NonNullableInt]
	[PXIntList(new[] { 1, 2, 3 }, new[] { "First", "Second", "Third" })]
	public class _NonNullableIntListAttribute : PXAggregateAttribute
	{
	}
	public class Foo : PXCacheExtension<SOOrder>
	{
		public abstract class someField : IBqlField { }
		[NonNullableIntList]
		public int? SomeField { get; set; }

		public abstract class _someField : IBqlField { }
		[_NonNullableIntList]
		public int? _SomeField { get; set; }
	}
}