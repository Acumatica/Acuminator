using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.Utils
{
	internal static class Utils
	{
		private const string AcuminatorTestsName = "Acuminator.Tests";

		private static ImmutableArray<string> UnitTestAssemblyMarkers { get; } =
			new[]
			{
				"TEST",
				"BENCHMARK"
			}
			.ToImmutableArray();

		/// <summary>
		/// Check that compillation is a unit test assembly. The check is implemented by searching for <c>Test</c> word in the assembly name. 
		/// It is a common pattern which on one hand is used almost everywhere and on the other hand allows us to distance from the concrete unit test frameworks
		/// and support not only xUnit but also others like NUnit.
		/// </summary>
		/// <param name="compilation">The compilation.</param>
		/// <returns/>
		public static bool IsUnitTestAssembly(this Compilation compilation)
		{
			string assemblyName = compilation?.AssemblyName;

			if (assemblyName.IsNullOrEmpty())
				return false;

			string assemblyNameUpperCase = assemblyName.ToUpperInvariant();

			for (int i = 0; i < UnitTestAssemblyMarkers.Length; i++)
			{
				if (assemblyNameUpperCase.Contains(UnitTestAssemblyMarkers[i]))
					return assemblyName != AcuminatorTestsName;
			}

			return false;
		}
	}
}
