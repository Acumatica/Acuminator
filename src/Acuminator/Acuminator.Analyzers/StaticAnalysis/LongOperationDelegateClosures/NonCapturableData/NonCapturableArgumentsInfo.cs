#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Information about the non capturable arguments of a method call 
	/// </summary>
	internal class NonCapturableArgumentsInfo : IReadOnlyCollection<NonCapturableArgument>
	{
		private readonly List<NonCapturableArgument> _arguments = new List<NonCapturableArgument>();
		private Dictionary<string, List<NonCapturableArgument>>? _usedParametersWithUsingArguments;

		public int Count => _arguments.Count;

		[MemberNotNullWhen(returnValue: true, nameof(UsedParameters))]
		public bool HasNonCapturableParameters => _usedParametersWithUsingArguments?.Count > 0;

		public IReadOnlyCollection<string>? UsedParameters => _usedParametersWithUsingArguments?.Keys;

		public void Add(NonCapturableArgument nonCapturableArgument)
		{
			_arguments.Add(nonCapturableArgument.CheckIfNull(nameof(nonCapturableArgument)));

			if (nonCapturableArgument.HasNonCapturableParameters)
			{
				_usedParametersWithUsingArguments ??= new Dictionary<string, List<NonCapturableArgument>>();

				foreach (PassedParameter usedParameter in nonCapturableArgument.UsedParameters)
				{
					if (_usedParametersWithUsingArguments.TryGetValue(usedParameter.Name, out List<NonCapturableArgument> parameterIndexes))
						parameterIndexes.Add(nonCapturableArgument);
					else
						_usedParametersWithUsingArguments.Add(usedParameter.Name, new List<NonCapturableArgument> { nonCapturableArgument });
				}
			}
		}

		public void RemoveParameterUsageFromArguments(string parameterName)
		{
			var affectedArguments = GetArgumentsUsingParameter(parameterName.CheckIfNull(nameof(parameterName)));

			if (affectedArguments.IsNullOrEmpty())
				return;

			for (int i = affectedArguments.Count - 1; i >= 0; i--)
			{
				NonCapturableArgument argument = affectedArguments[i];

				argument.UsedParameters?.RemoveAll(p => p.Name == parameterName);

				if (!argument.CapturesNonCapturableElement)
				{
					affectedArguments.RemoveAt(i);
					_arguments.Remove(argument);
				}
			}
		}

		private List<NonCapturableArgument>? GetArgumentsUsingParameter(string parameterName)
		{
			return _usedParametersWithUsingArguments?.TryGetValue(parameterName, out var arguments) == true
				? arguments
				: null;
		}

		public List<NonCapturableArgument>.Enumerator GetEnumerator() => _arguments.GetEnumerator();

		IEnumerator<NonCapturableArgument> IEnumerable<NonCapturableArgument>.GetEnumerator() => _arguments.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}