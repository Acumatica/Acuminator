#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Information about parameters passed into the method that shouldn't be captured in a delegate closure 
	/// </summary>
	internal class PassedParametersToNotBeCaptured : ICollection<string>
	{
		private readonly HashSet<string> _passedInstances;

		public int PassedInstancesCount => _passedInstances.Count;

		int ICollection<string>.Count => _passedInstances.Count;

		public bool IsReadOnly => true;

		public PassedParametersToNotBeCaptured(HashSet<string>? passedParameters)
		{
			_passedInstances = passedParameters ?? new HashSet<string>();
		}

		public bool Contains(string parameterName) => _passedInstances.Contains(parameterName);

		void ICollection<string>.Add(string item) => throw new NotSupportedException($"Changing {nameof(PassedParametersToNotBeCaptured)} is not supported");

		void ICollection<string>.Clear() => throw new NotSupportedException($"Changing {nameof(PassedParametersToNotBeCaptured)} is not supported");

		bool ICollection<string>.Remove(string item) => throw new NotSupportedException($"Changing {nameof(PassedParametersToNotBeCaptured)} is not supported");

		void ICollection<string>.CopyTo(string[] array, int arrayIndex) => 
			((ICollection<string>)_passedInstances).CopyTo(array, arrayIndex);

		public IEnumerator<string> GetEnumerator() => _passedInstances.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}