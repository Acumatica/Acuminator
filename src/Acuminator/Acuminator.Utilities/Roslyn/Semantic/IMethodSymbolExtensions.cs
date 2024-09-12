#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class IMethodSymbolExtensions
	{
		public static bool IsInstanceConstructor(this IMethodSymbol methodSymbol)
		{
			methodSymbol.ThrowOnNull();

			return !methodSymbol.IsStatic && methodSymbol.MethodKind == MethodKind.Constructor;
		}

		/// <summary>
		/// Gets the topmost non-local method containing the local function declaration. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localFunction">The method that can be local function.</param>
		/// <returns>
		/// The non-local method containing the local function.
		/// </returns>
		public static IMethodSymbol? GetContainingNonLocalMethod(this IMethodSymbol localFunction) =>
			GetStaticOrNonLocalContainingMethod(localFunction, stopOnStaticMethod: false, CancellationToken.None);

		/// <summary>
		/// Gets the topmost static or non-local method containing the <paramref name="localFunction"/>. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localFunction">The method that can be local function.</param>
		/// <param name="cancellation">A token that allows processing to be cancelled.</param>
		/// <returns>
		/// the topmost static or non-local method containing the <paramref name="localFunction"/>.
		/// </returns>
		public static IMethodSymbol? GetStaticOrNonLocalContainingMethod(this IMethodSymbol localFunction, CancellationToken cancellation) =>
			GetStaticOrNonLocalContainingMethod(localFunction, stopOnStaticMethod: true, cancellation);

		private static IMethodSymbol? GetStaticOrNonLocalContainingMethod(IMethodSymbol localFunction, bool stopOnStaticMethod,
																		  CancellationToken cancellation)
		{
			localFunction.ThrowOnNull();

			if (localFunction.MethodKind != MethodKind.LocalFunction)
				return localFunction;

			IMethodSymbol? current = localFunction;

			while (current != null && current.MethodKind == MethodKind.LocalFunction && (!stopOnStaticMethod || !localFunction.IsDefinitelyStatic(cancellation)))
				current = current.ContainingSymbol as IMethodSymbol;

			return current;
		}

		public static IEnumerable<IMethodSymbol> GetContainingMethodsAndThis(this IMethodSymbol localFunction) =>
			localFunction.CheckIfNull().MethodKind == MethodKind.LocalFunction
				? localFunction.GetContainingMethods(includeThis: true)
				: new[] { localFunction };

		public static IEnumerable<IMethodSymbol> GetContainingMethods(this IMethodSymbol localFunction) =>
			localFunction.CheckIfNull().MethodKind == MethodKind.LocalFunction
				? localFunction.GetContainingMethods(includeThis: false)
				: Enumerable.Empty<IMethodSymbol>();

		private static IEnumerable<IMethodSymbol> GetContainingMethods(this IMethodSymbol localFunction, bool includeThis)
		{
			IMethodSymbol? current = includeThis
				? localFunction
				: localFunction.ContainingSymbol as IMethodSymbol;

			while (current != null)
			{
				yield return current;
				current = current.ContainingSymbol as IMethodSymbol;
			}
		}

		/// <summary>
		/// Gets all parameters available for local function including parameters from containing methods.
		/// </summary>
		/// <param name="localFunction">The method that can be a local function.</param>
		/// <param name="includeOwnParameters">True to include, false to exclude <paramref name="localFunction"/>'s own parameters.</param>
		/// <returns>
		/// All parameters available for the local function including parameters from containing methods.
		/// </returns>
		public static ImmutableArray<IParameterSymbol> GetAllParametersAvailableForLocalFunction(this IMethodSymbol localFunction, bool includeOwnParameters,
																								 CancellationToken cancellation)
		{
			if (localFunction.CheckIfNull().MethodKind != MethodKind.LocalFunction)
				return localFunction.Parameters;

			ImmutableArray<IParameterSymbol>.Builder parametersBuilder;

			if (localFunction.Parameters.IsDefaultOrEmpty || !includeOwnParameters)
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>();
			else
			{
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>(initialCapacity: localFunction.Parameters.Length);
				parametersBuilder.AddRange(localFunction.Parameters);
			}

			if (localFunction.IsStatic)
				return parametersBuilder.ToImmutable();

			IMethodSymbol? current = localFunction;

			do
			{
				cancellation.ThrowIfCancellationRequested();

				var containingMethod = current.ContainingSymbol as IMethodSymbol;

				// For a non static nested local function we can add parameters from its containing local function even if it is static
				// But we must stop after that and won't take parameters from the methods containing static local function
				if (containingMethod != null && !containingMethod.Parameters.IsDefaultOrEmpty)
				{
					// If we do not include parameters from the local function then check if the outer parameters are redefined by the local function parameters.
					// Redefined parameters won't be available to the local function
					var notReassignedParameters = from parameter in containingMethod.Parameters
												  where !parametersBuilder.Contains(parameter) && 												
														(includeOwnParameters || !localFunction.Parameters.Any(localParameter => localParameter.Name == parameter.Name))
												  select parameter;
					parametersBuilder.AddRange(notReassignedParameters);
				}

				current = containingMethod;
			}
			while (current?.MethodKind == MethodKind.LocalFunction && !current.IsDefinitelyStatic(cancellation));	

			return parametersBuilder.ToImmutable();
		}

		/// <summary>
		/// Check if  the parameter <paramref name="parameterName"/> from the non local method is redefined.
		/// </summary>
		/// <param name="localMethod">The method that can be local function.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>
		/// True if non local method parameter is redefined in a local method, false if not.
		/// </returns>
		public static bool IsNonLocalMethodParameterRedefined(this IMethodSymbol localMethod, string parameterName, CancellationToken cancellation)
		{
			localMethod.ThrowOnNull();

			if (parameterName.IsNullOrWhiteSpace() || localMethod.MethodKind != MethodKind.LocalFunction)
				return false;

			IMethodSymbol? current = localMethod;

			while (current?.MethodKind == MethodKind.LocalFunction)
			{
				if ((!current.Parameters.IsDefaultOrEmpty && current.Parameters.Any(p => p.Name == parameterName)) ||
					current.IsDefinitelyStatic(cancellation))
				{
					return true;
				}

				current = current.ContainingSymbol as IMethodSymbol;
			}

			return false;
		}


		/// <summary>
		/// Check if <paramref name="method"/> definitely static.
		/// </summary>
		/// <remarks>
		/// There is a bug in older versions of Roslyn that local functions are always static: https://github.com/dotnet/roslyn/issues/27719 This code attempts to workaround it. <br/>
		/// TODO: we need to remove this method after migration to more modern version of Roslyn.
		/// </remarks>
		/// <param name="method">The method to act on.</param>
		/// <param name="cancellation">A token that allows processing to be cancelled.</param>
		/// <returns>
		/// True if <paramref name="method"/> is definitely static, false if not.
		/// </returns>
		public static bool IsDefinitelyStatic(this IMethodSymbol method, CancellationToken cancellation)
		{
			if (method.MethodKind != MethodKind.LocalFunction)
				return method.IsStatic;

			var methodDeclaration = method.GetSyntax(cancellation);
			return methodDeclaration?.IsStatic() ?? method.IsStatic;
		}

		/// <summary>
		/// Check if <paramref name="method"/> definitely static.
		/// </summary>
		/// <remarks>
		/// There is a bug in older versions of Roslyn that local functions are always static: https://github.com/dotnet/roslyn/issues/27719 This code attempts to workaround it. <br/>
		/// TODO: we need to remove this method after migration to more modern version of Roslyn.
		/// </remarks>
		/// <param name="method">The method to act on.</param>
		/// <param name="methodDeclaration">The method declaration node.</param>
		/// <returns>
		/// True if <paramref name="method"/> is definitely static, false if not.
		/// </returns>
		public static bool IsDefinitelyStatic(this IMethodSymbol method, SyntaxNode methodDeclaration)
		{
			method.ThrowOnNull();
			methodDeclaration.ThrowOnNull();

			if (method.MethodKind != MethodKind.LocalFunction)
				return method.IsStatic;

			return methodDeclaration.IsStatic();
		}

		/// <summary>
		/// Check if <paramref name="method"/> has either virtual, override or abstract signature.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <returns>True if <paramref name="method"/> </returns>
		public static bool CanBeOverriden(this IMethodSymbol method)
		{
			method.ThrowOnNull();

			return method.IsVirtual || method.IsOverride || method.IsAbstract;
		}
	}
}
