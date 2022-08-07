#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Information about parameters passed into the method that shouldn't be captured in a delegate closure 
	/// </summary>
	internal class PassedParametersToNotBeCaptured
	{
		public HashSet<string> PassedInstances { get; }

		public int PassedInstancesCount => PassedInstances.Count;

		public PassedParametersToNotBeCaptured(HashSet<string>? passedParameters)
		{
			PassedInstances = passedParameters ?? new HashSet<string>();
		}
	}
}