﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.CodeAnalysis;

namespace Acuminator.Tests.Verification
{
	public static class FluentAssertionsExtensions
	{
		public static LocationsAssertions Should(this IEnumerable<Location> actualValue)
		{
			return new LocationsAssertions(actualValue);
		}
	}

	public class LocationsAssertions : GenericCollectionAssertions<Location>
	{
		public LocationsAssertions(IEnumerable<Location> actualValue) 
			: base(actualValue)
		{
		}

		public AndConstraint<GenericCollectionAssertions<Location>> BeEquivalentTo(params (int Line, int Column)[] expected)
		{
			var actual = Subject.ToArray();
			actual.Should().HaveCount(expected.Length);

			for (int i = 0; i < actual.Length; i++)
			{
				Assert(expected[i], actual[i]);
			}

			return new AndConstraint<GenericCollectionAssertions<Location>>(this);

			//------------------------------------Local Function--------------------------------------
			static void Assert((int Line, int Column) expectedLocation, Location actualLocation)
			{
				var actualSpan = actualLocation.GetLineSpan();
				var actualLinePosition = actualSpan.StartLinePosition;
				int actualLine = actualLinePosition.Line + 1;
				int actualColumn = actualLinePosition.Character + 1;

				actualLine.Should().Be(expectedLocation.Line);
				actualColumn.Should().Be(expectedLocation.Column);
			}
		}
	}
}
