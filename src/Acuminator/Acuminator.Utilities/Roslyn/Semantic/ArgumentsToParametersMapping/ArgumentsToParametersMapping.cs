﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.ArgumentsToParametersMapping
{
	/// <summary>
	/// The arguments to parameters mapping struct. 
	/// </summary>
	/// <remarks>
	/// Here we use a readonly struct to avoid memory allocations in the most frequent case when all method call arguments are positional
	/// and the argument position is the parameter position.<br/>
	/// The approach is inspired by similar functionality in Roslyn repo:<br/>
	/// https://github.com/dotnet/roslyn/blob/4b957187b2fda9545981021533ea01bff3f03062/src/Compilers/CSharp/Portable/Binder/Semantics/OverloadResolution/OverloadResolution_ArgsToParameters.cs#L27
	/// </remarks>
	public readonly struct ArgumentsToParametersMapping
	{
		private readonly int _length;
		private readonly int[]? _parametersMapping;

		/// <summary>
		/// Gets the number of mapped arguments.
		/// </summary>
		/// <value>
		/// The number of mapped arguments.
		/// </value>
		public int Length => _length;

		/// <summary>
		/// Checks the mapping is a trivial mapping where all arguments are positional and the argument position is the parameter position.
		/// </summary>
		/// <value>
		/// True if the maping is trivial, false if not.
		/// </value>
		[MemberNotNullWhen(returnValue: false, nameof(_parametersMapping))]
		public bool IsTrivial => _parametersMapping == null;

		public int this[int argIndex] => GetMappedParameterPosition(argIndex);

		public ArgumentsToParametersMapping(int length)
		{
			_length = length;
			_parametersMapping = null;
		}

		public ArgumentsToParametersMapping(int[] parametersMapping)
		{
			_parametersMapping = parametersMapping.CheckIfNull();
			_length = parametersMapping.Length;
		}

		public IParameterSymbol GetMappedParameter(IMethodSymbol methodSymbol, int argIndex)
		{
			methodSymbol.ThrowOnNull();
			int parameterIndex = GetMappedParameterPosition(argIndex);
			return methodSymbol.Parameters[parameterIndex];
		}

		public int GetMappedParameterPosition(int argIndex)
		{
			if (IsTrivial)
			{
				return argIndex >= 0 && argIndex < _length
					? argIndex
					: throw new ArgumentOutOfRangeException(nameof(argIndex), $"Argument index can't be negative and must be less than {_length}");
			}

			return _parametersMapping[argIndex];
		}

		public static ArgumentsToParametersMapping Trivial(int length) => new ArgumentsToParametersMapping(length);

		public ImmutableArray<int> ToImmutableArray() =>
			_parametersMapping?.ToImmutableArray() ?? ImmutableArray<int>.Empty;
	}
}
