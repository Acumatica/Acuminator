#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Information about the non capturable argument 
	/// </summary>
	internal class NonCapturableArgument
	{
		public int Index { get; }

		public bool UseThisReference { get; }

		public List<PassedParameter>? UsedParameters { get; }

		[MemberNotNullWhen(returnValue: true, nameof(UsedParameters))]
		public bool HasNonCapturableParameters => UsedParameters?.Count > 0;

		public bool CapturesNonCapturableElement => UseThisReference || HasNonCapturableParameters;

		public NonCapturableArgument(int argumentIndex, bool useThisReference, List<PassedParameter>? usedParameters)
		{
			if (argumentIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(argumentIndex));

			Index = argumentIndex;
			UseThisReference = useThisReference;
			UsedParameters = usedParameters;
		}

		public CapturedInstancesTypes GetCapturedTypes()
		{
			CapturedInstancesTypes capturedTypes = UseThisReference
				? CapturedInstancesTypes.PXGraph
				: CapturedInstancesTypes.None;

			if (!HasNonCapturableParameters)
				return capturedTypes;

			foreach (var parameter in UsedParameters)
			{
				capturedTypes = capturedTypes | parameter.CapturedTypes;
			}

			return capturedTypes;
		}
	}
}