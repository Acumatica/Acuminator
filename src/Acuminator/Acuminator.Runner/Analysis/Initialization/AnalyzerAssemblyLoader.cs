using System;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Analysis.Initialization
{
	internal class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
	{
		public void AddDependencyLocation(string fullPath)
		{
		}

		public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
	}
}
