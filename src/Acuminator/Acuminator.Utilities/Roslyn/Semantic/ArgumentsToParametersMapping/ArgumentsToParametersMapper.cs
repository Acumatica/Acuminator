#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.ArgumentsToParametersMapping
{
	/// <summary>
	/// The helper that maps method call arguments to method parameters.
	/// </summary>
	public static class ArgumentsToParametersMapper
	{
		/// <summary>
		/// Map method call arguments to method parameter symbols.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <param name="arguments">The call arguments list. Must be arguments from the call to <paramref name="method"/>.</param>
		/// <returns>
		/// An <see cref="ArgumentsToParametersMapping?"/> struct which stores mapping from each argument index to each <paramref name="method"/> parameter index.<br/>
		/// If mapping failed returns <see langword="null"/>.
		/// </returns>
		public static ArgumentsToParametersMapping? MapArgumentsToParameters(this IMethodSymbol method, BaseArgumentListSyntax argumentsList)
		{
			method.ThrowOnNull(nameof(method));
			argumentsList.ThrowOnNull(nameof(argumentsList));
			
			// Need to also filter out unsafe methods here but there is no flag on IMethodSymbol to do this
			// And obtaining syntax node and checking modifiers will be expensive considering that unsafe code is not used in Acumatica
			if (method.IsExtern || method.IsVararg)
				return null;

			// We can't map correctly arguments with syntax errors
			if (argumentsList.ContainsDiagnostics || argumentsList.IsMissing)
				return null;

			var arguments = argumentsList.Arguments;

			if (arguments.Count == 0 || method.Parameters.Length == 0)
				return ArgumentsToParametersMapping.Trivial(length: 0);
	
			// We can't map correctly arguments if they have syntax issues and we won't try to do partial mapping
			if (arguments.Any(argument => argument.IsMissing))
				return null;

			int indexOfFirstNamedParameterOutOfPosition = IndexOfFirstNamedParameterOutOfPosition(method, arguments);
			bool useNamedParametersOutOfPosition = indexOfFirstNamedParameterOutOfPosition >= 0;

			// Simple and most frequent case when all arguments are positional and no argument uses a named parameter 
			// or there are arguments that use named parameters but the argument's position is the same as parameter's position
			// In this case we can use trivial mapping between arguments and parameters where argument position is the same as parameter's
			if (!useNamedParametersOutOfPosition)
			{
				return MapAllPositionalArgumentsToParameters(method, arguments);
			}

			var parametersMapping = new int[arguments.Count];

			//Map all positional arguments first
			for (int argIndex = 0; argIndex < indexOfFirstNamedParameterOutOfPosition; argIndex++)
				parametersMapping[argIndex] = argIndex;

			// Map arguments that use named parameters out of position.
			// In C# a positonal argument can't be passed after the first such argument with out of position named parameter
			// All arguments starting from indexOfFirstNamedParameterOutOfPosition must specify the parameter name.
			// This has to be done even for the params parameter. 
			// Additionally, the arguments with named parameters can't refer to parameters before indexOfFirstNamedParameterOutOfPosition
			// because the positional arguments were already specified for them
			for (int argIndex = indexOfFirstNamedParameterOutOfPosition; argIndex < arguments.Count; argIndex++)
			{
				ArgumentSyntax argument = arguments[argIndex];
				string? namedParameterName = argument.NameColon?.Name?.Identifier.ValueText;

				if (namedParameterName.IsNullOrWhiteSpace())
					return null;    //Something went wrong with the mapping or there is a syntax error in the method call node

				int parameterIndex = 
					method.Parameters.FindIndex(startInclusive: indexOfFirstNamedParameterOutOfPosition,
												p => p.Name == namedParameterName);
				if (parameterIndex < 0)
					return null;    //The method parameter with the name provided by the argument is not found. Something is wrong with the method call.

				parametersMapping[argIndex] = parameterIndex;
			}

			return new ArgumentsToParametersMapping(parametersMapping);
		}

		private static int IndexOfFirstNamedParameterOutOfPosition(IMethodSymbol method, SeparatedSyntaxList<ArgumentSyntax> arguments)
		{
			// In case method has N parameters and the last one is params parameter the number of arguments can be greater than the number of parameters.
			// But only first N arguments can specify the parameter name. 
			// And the number of arguments can be less than the number of parameters because the method may have optional parameters.
			int numberOfArgumentsThatCanUseNamedParameters = Math.Min(arguments.Count, method.Parameters.Length);

			for (int argIndex = 0; argIndex < numberOfArgumentsThatCanUseNamedParameters; argIndex++)
			{
				ArgumentSyntax argument = arguments[argIndex];
				string? namedParameterName = argument.NameColon?.Name?.Identifier.ValueText;

				if (namedParameterName.IsNullOrWhiteSpace())
					continue;

				var parameterWithSamePosition = method.Parameters[argIndex];

				if (parameterWithSamePosition.Name != namedParameterName)
					return argIndex;
			}

			return -1;
		}

		private static ArgumentsToParametersMapping MapAllPositionalArgumentsToParameters(IMethodSymbol method, SeparatedSyntaxList<ArgumentSyntax> arguments)
		{
			var lastParameter = method.Parameters.Last();

			// If there is no params parameter (most frequent case) then the mapping is trivial. 
			// Even if the method has optional parameters, when there are no named parameters the arguments in C# are mapped to parameters with the same position.
			// It's not possible to skip one optional parameter and pass argument to another without named parameters
			if (!lastParameter.IsParams)
				return ArgumentsToParametersMapping.Trivial(arguments.Count);

			// Now we know that the last parameter is params and can accept variable number of args - zero, one or many
			// Suppose there are optional parameters before the params parameter like this:
			// 
			// private static void Foo(int x, double g, bool y = false, bool z = true, params string[] p)
			// 
			// Without usage of named parameters C# doesn't allow to call the method with arguments for params parameter without specifying arguments for all optional parameters
			// even if parameters have different types and it is possible to map them in theory:
			// 
			// Foo(1, 2.0, "abc");   //Displays error
			// 
			// Thus, if the number of arguments is less or equals to the number of method parameters, then all arguments are mapped positionally
			// This covers the case when there are zero or one argument for the method params parameter
			if (arguments.Count <= method.Parameters.Length)
				return ArgumentsToParametersMapping.Trivial(arguments.Count);

			// If the number of arguments is greater than the number of parameters then fill the positional mapping first for method parameters
			// and map remaining arguments to the params parameter
			int[] parametersMapping = new int[arguments.Count];

			for (int argIndex = 0; argIndex < method.Parameters.Length; argIndex++)
				parametersMapping[argIndex] = argIndex;

			int paramsParameterIndex = method.Parameters.Length - 1;

			for (int argIndex = method.Parameters.Length; argIndex < arguments.Count; argIndex++)
				parametersMapping[argIndex] = paramsParameterIndex;

			return new ArgumentsToParametersMapping(parametersMapping);
		}
	}
}
