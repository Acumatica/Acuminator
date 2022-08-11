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
	internal class PassedParametersToNotBeCaptured : IReadOnlyDictionary<string, PassedParameter>
	{
		private readonly Dictionary<string, PassedParameter> _passedParametersByName;

		public int Count => _passedParametersByName.Count;

		public IReadOnlyCollection<string> PassedParametersNames => _passedParametersByName.Keys;

		public IReadOnlyCollection<PassedParameter> PassedParameters => _passedParametersByName.Values;

		IEnumerable<string> IReadOnlyDictionary<string, PassedParameter>.Keys => PassedParametersNames;

		IEnumerable<PassedParameter> IReadOnlyDictionary<string, PassedParameter>.Values => PassedParameters;

		public PassedParameter this[string key] => _passedParametersByName[key];

		public PassedParametersToNotBeCaptured(IEnumerable<PassedParameter>? passedParameters)
		{
			_passedParametersByName = passedParameters?.ToDictionary(parameter => parameter.Name) ?? 
									  new Dictionary<string, PassedParameter>();
		}

		public bool Contains(string parameterName) => _passedParametersByName.ContainsKey(parameterName);

		public PassedParameter? GetPassedParameter(string parameterName) =>
			_passedParametersByName.TryGetValue(parameterName, out PassedParameter passedParameter)
				? passedParameter
				: null;

		bool IReadOnlyDictionary<string, PassedParameter>.TryGetValue(string parameterName, out PassedParameter parameter)
		{
			var foundParameter = GetPassedParameter(parameterName);

			if (foundParameter.HasValue)
			{
				parameter = foundParameter.Value;
				return true;
			}
			else
			{
				parameter = default;
				return false;
			}
		}

		bool IReadOnlyDictionary<string, PassedParameter>.ContainsKey(string parameterName) => Contains(parameterName);
	
		public Dictionary<string, PassedParameter>.Enumerator GetEnumerator() => _passedParametersByName.GetEnumerator();

		IEnumerator<KeyValuePair<string, PassedParameter>> IEnumerable<KeyValuePair<string, PassedParameter>>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}