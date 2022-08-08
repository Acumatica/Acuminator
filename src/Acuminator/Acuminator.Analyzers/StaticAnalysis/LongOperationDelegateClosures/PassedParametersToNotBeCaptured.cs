#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Information about parameters passed into the method that shouldn't be captured in a delegate closure 
	/// </summary>
	internal class PassedParametersToNotBeCaptured
	{
		private readonly HashSet<string> _passedInstances;

		public int PassedInstancesCount => _passedInstances.Count;

		public PassedParametersToNotBeCaptured(HashSet<string>? passedParameters)
		{
			_passedInstances = passedParameters ?? new HashSet<string>();
		}

		public bool Contains(string parameterName) => _passedInstances.Contains(parameterName);
	}
}