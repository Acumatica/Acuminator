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
	/// Information about the non capturable elements passed to a method call.
	/// </summary>
	internal class NonCapturableElementsInfo
	{
		private List<NonCapturableArgument>? _arguments;
		private Dictionary<string, List<NonCapturableArgument>>? _parametersUsedInCallArguments;
		private readonly Dictionary<string, PassedParameter>? _nonCapturableContainingMethodsParameters;

		public IReadOnlyCollection<NonCapturableArgument>? ArgumentsWithNonCapturableElements => 
			_arguments;

		public IReadOnlyCollection<PassedParameter>? NonCapturableContainingMethodsParameters =>
			_nonCapturableContainingMethodsParameters?.Values;


		[MemberNotNullWhen(returnValue: true, nameof(_arguments), nameof(ArgumentsWithNonCapturableElements))]
		public bool HasArgumentsWithNonCapturableElements =>
			_arguments?.Count > 0;

		[MemberNotNullWhen(returnValue: true, nameof(_parametersUsedInCallArguments), nameof(_arguments), nameof(ArgumentsWithNonCapturableElements))]
		public bool HasNonCapturableParametersInArguments => 
			_parametersUsedInCallArguments?.Count > 0;

		[MemberNotNullWhen(returnValue: true, nameof(_nonCapturableContainingMethodsParameters), nameof(NonCapturableContainingMethodsParameters))]
		public bool HasNonCapturableContainingMethodsParameters => 
			_nonCapturableContainingMethodsParameters?.Count > 0;

		public bool HasNonCapturableElements =>
			HasArgumentsWithNonCapturableElements || HasNonCapturableContainingMethodsParameters;

		public bool HasNonCapturableParameters =>
			HasNonCapturableParametersInArguments || HasNonCapturableContainingMethodsParameters;

		public NonCapturableElementsInfo()
		{

		}

		public NonCapturableElementsInfo(IReadOnlyDictionary<string, PassedParameter> nonCapturableContainingMethodsParameters)
		{
			nonCapturableContainingMethodsParameters.ThrowOnNull(nameof(nonCapturableContainingMethodsParameters));

			if (nonCapturableContainingMethodsParameters is Dictionary<string, PassedParameter> dictionary)
				_nonCapturableContainingMethodsParameters = dictionary;
			else
			{
				_nonCapturableContainingMethodsParameters = new Dictionary<string, PassedParameter>(capacity: nonCapturableContainingMethodsParameters.Count);

				foreach (var (name, parameter) in nonCapturableContainingMethodsParameters)
					_nonCapturableContainingMethodsParameters.Add(name, parameter);
			}
		}

		[MemberNotNull(nameof(_arguments))]
		public void AddCallArgument(NonCapturableArgument nonCapturableArgument)
		{
			nonCapturableArgument.ThrowOnNull(nameof(nonCapturableArgument));

			_arguments ??= new List<NonCapturableArgument>(capacity: 1);
			_arguments.Add(nonCapturableArgument);

			if (nonCapturableArgument.HasNonCapturableParameters)
			{
				_parametersUsedInCallArguments ??= new Dictionary<string, List<NonCapturableArgument>>();

				foreach (PassedParameter usedParameter in nonCapturableArgument.UsedParameters)
				{
					if (_parametersUsedInCallArguments.TryGetValue(usedParameter.Name, out List<NonCapturableArgument> parameterIndexes))
						parameterIndexes.Add(nonCapturableArgument);
					else
						_parametersUsedInCallArguments.Add(usedParameter.Name, new List<NonCapturableArgument> { nonCapturableArgument });
				}
			}
		}

		public IReadOnlyCollection<string>? GetNamesOfUsedNonCapturableParameters()
		{
			if (!HasNonCapturableContainingMethodsParameters)
				return _parametersUsedInCallArguments?.Keys;
			else if (!HasNonCapturableParametersInArguments)
				return _nonCapturableContainingMethodsParameters.Keys;

			return _parametersUsedInCallArguments.Keys.Concat(_nonCapturableContainingMethodsParameters.Keys)
													  .Distinct()
													  .ToList(capacity: _parametersUsedInCallArguments.Count + _nonCapturableContainingMethodsParameters.Count);
		}

		public void RemoveParameterUsage(string parameterName)
		{
			_nonCapturableContainingMethodsParameters?.Remove(parameterName);

			var affectedArguments = GetCallArgumentsUsingParameter(parameterName.CheckIfNull(nameof(parameterName)));

			if (affectedArguments.IsNullOrEmpty())
				return;

			for (int i = affectedArguments.Count - 1; i >= 0; i--)
			{
				NonCapturableArgument argument = affectedArguments[i];

				argument.UsedParameters?.RemoveAll(p => p.Name == parameterName);

				if (!argument.CapturesNonCapturableElement)
				{
					affectedArguments.RemoveAt(i);
					_arguments?.Remove(argument);
				}
			}

			_parametersUsedInCallArguments?.Remove(parameterName);
		}

		private List<NonCapturableArgument>? GetCallArgumentsUsingParameter(string parameterName)
		{
			return _parametersUsedInCallArguments?.TryGetValue(parameterName, out var arguments) == true
				? arguments
				: null;
		}
	}
}