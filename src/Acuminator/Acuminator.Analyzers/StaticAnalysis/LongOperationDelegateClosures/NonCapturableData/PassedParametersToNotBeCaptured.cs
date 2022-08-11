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
	internal class PassedParametersToNotBeCaptured
	{
		private readonly Dictionary<string, PassedParameter> _passedParametersByName;

		public int PassedParametersCount => _passedParametersByName.Count;

		public IReadOnlyCollection<string> PassedParametersNames => _passedParametersByName.Keys;

		public PassedParametersToNotBeCaptured(IEnumerable<PassedParameter>? passedParameters)
		{
			_passedParametersByName = passedParameters?.ToDictionary(parameter => parameter.Name) ?? new Dictionary<string, PassedParameter>();
		}

		public bool Contains(string parameterName) => _passedParametersByName.ContainsKey(parameterName);

		public PassedParameter? GetPassedParameter(string parameterName) =>
			_passedParametersByName.TryGetValue(parameterName, out PassedParameter passedParameter)
				? passedParameter
				: null;
	}
}