using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class ITypeSymbolExtensions
	{
		private const char DefaultGenericArgsCountSeparator = '`';
		private const char DefaultNestedTypesSeparator = '+';

		/// <summary>
		/// Gets the base types and this in this collection. The types are returned from the most derived ones to the most base <see cref="Object"/> type
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
		{
			type.ThrowOnNull(nameof(type));

			if (type is ITypeParameterSymbol typeParameter)
			{
				IEnumerable<ITypeSymbol> constraintTypes = typeParameter.GetAllConstraintTypes(includeInterfaces: false)
																	    .SelectMany(GetBaseTypesImplementation)
																	    .Distinct();
				return typeParameter.ToEnumerable()
									.Concat(constraintTypes);
			}
			else
			{
				return type.GetBaseTypesAndThisImplementation();
			}		
		}

		/// <summary>
		/// Gets the base types and this in this collection. The types are returned from the most derived ones to the most base <see cref="Object"/> type
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <returns/>
		public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol type)
		{
			type.ThrowOnNull(nameof(type));

			if (type is ITypeParameterSymbol typeParameter)
			{
				return typeParameter.GetAllConstraintTypes(includeInterfaces: false)
									.SelectMany(GetBaseTypesImplementation)
									.Distinct();
			}

			return type.GetBaseTypesImplementation();			
		}

		private static IEnumerable<ITypeSymbol> GetBaseTypesAndThisImplementation(this ITypeSymbol typeToUse)
		{
			var current = typeToUse;

			while (current != null)
			{
				yield return current;
				current = current.BaseType;
			}
		}

		private static IEnumerable<INamedTypeSymbol> GetBaseTypesImplementation(this ITypeSymbol typeToUse)
		{
			var current = typeToUse.BaseType;

			while (current != null)
			{
				yield return current;
				current = current.BaseType;
			}
		}

		public static IEnumerable<INamedTypeSymbol> GetFlattenedNestedTypes(this ITypeSymbol type, CancellationToken cancellationToken)
		{
			type.ThrowOnNull(nameof(type));
			cancellationToken.ThrowIfCancellationRequested();
			return type.GetFlattenedNestedTypesImplementation(shouldWalkThroughNestedTypesPredicate: null, cancellationToken);			
		}

		public static IEnumerable<INamedTypeSymbol> GetFlattenedNestedTypes(this ITypeSymbol type, Func<ITypeSymbol, bool> shouldWalkThroughNestedTypesPredicate, 
																			CancellationToken cancellationToken)
		{
			type.ThrowOnNull(nameof(type));
			cancellationToken.ThrowIfCancellationRequested();
			shouldWalkThroughNestedTypesPredicate.ThrowOnNull(nameof(shouldWalkThroughNestedTypesPredicate));

			return type.GetFlattenedNestedTypesImplementation(shouldWalkThroughNestedTypesPredicate, cancellationToken);
		}

		private static IEnumerable<INamedTypeSymbol> GetFlattenedNestedTypesImplementation(this ITypeSymbol type, 
																						   Func<ITypeSymbol, bool> shouldWalkThroughNestedTypesPredicate,
																						   CancellationToken cancellationToken)
		{
			var nestedTypes = type.GetTypeMembers();

			if (nestedTypes.IsDefaultOrEmpty)
				yield break;

			var typesQueue = new Queue<INamedTypeSymbol>(nestedTypes);

			while (typesQueue.Count > 0)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var currentType = typesQueue.Dequeue();
				bool shouldWalkThroughChildNestedTypes = shouldWalkThroughNestedTypesPredicate?.Invoke(currentType) ?? true;

				if (shouldWalkThroughChildNestedTypes)
				{
					var declaredNestedTypes = currentType.GetTypeMembers();

					if (!declaredNestedTypes.IsDefaultOrEmpty)
					{
						foreach (var nestedType in declaredNestedTypes)
						{
							typesQueue.Enqueue(nestedType);
						}
					}
				}

				yield return currentType;
			}
		}

		public static IEnumerable<ITypeSymbol> GetContainingTypesAndThis(this ITypeSymbol type)
		{
			var current = type;

			while (current != null)
			{
				yield return current;
				current = current.ContainingType;
			}
		}

		public static IEnumerable<INamedTypeSymbol> GetContainingTypes(this ITypeSymbol type)
		{
			var current = type.ContainingType;

			while (current != null)
			{
				yield return current;
				current = current.ContainingType;
			}
		}

		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(this ITypeSymbol type)
		{
			var currentNamespace = type?.ContainingNamespace;

			while (currentNamespace != null)
			{
				yield return currentNamespace;
				currentNamespace = currentNamespace.ContainingNamespace;
			}
		}

		/// <summary>
		///  Determine if "type" inherits from "baseType", ignoring constructed types and interfaces, dealing only with original types.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="baseType">The base type.</param>
		/// <returns/>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType) =>
			InheritsFromOrEquals(type, baseType, includeInterfaces: false);

		/// <summary>
		/// Determine if "type" inherits from "baseType", ignoring constructed types, optionally including interfaces, dealing only with original types.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="baseType">The base type.</param>
		/// <param name="includeInterfaces">True to include, false to exclude the interfaces.</param>
		/// <returns/>
		public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(baseType));

			var typeList = type.GetBaseTypesAndThis();

			if (includeInterfaces)
			{
				typeList = typeList.ConcatStructList(type.AllInterfaces);
			}

			return typeList.Any(t => t.Equals(baseType));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsFromOrEqualsGeneric(this ITypeSymbol type, ITypeSymbol baseType) =>
			InheritsFromOrEqualsGeneric(type, baseType, includeInterfaces: false);

		public static bool InheritsFromOrEqualsGeneric(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(baseType));

			var typeList = type.GetBaseTypesAndThis();

			if (includeInterfaces)
				typeList = typeList.ConcatStructList(type.AllInterfaces);

			return typeList.Select(t => t.OriginalDefinition)
						   .Any(t => t.Equals(baseType.OriginalDefinition));
		}

		public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces = false)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(baseType));

			IEnumerable<ITypeSymbol> baseTypes = type.GetBaseTypes();

			if (includeInterfaces)
			{
				baseTypes = baseTypes.ConcatStructList(type.AllInterfaces);
			}
			
			return baseTypes.Any(t => t.Equals(baseType));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ImplementsInterface(this ITypeSymbol type, ITypeSymbol interfaceType)
		{
			type.ThrowOnNull(nameof(type));
			interfaceType.ThrowOnNull(nameof(interfaceType));

			if (interfaceType.TypeKind != TypeKind.Interface)
			{
				throw new ArgumentException("Invalid interface type", nameof(interfaceType));
			}

			return type.AllInterfaces.Any(t => t.Equals(interfaceType));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsFrom(this ITypeSymbol symbol, string baseType)
		{
			symbol.ThrowOnNull(nameof(symbol));
			baseType.ThrowOnNullOrWhiteSpace(nameof(baseType));

			return symbol.GetBaseTypesAndThis()
						 .Any(t => t.Name == baseType);
		}
		
		/// <summary>
		/// Determine if "type" inherits from "baseType", ignoring constructed types, optionally including interfaces, dealing only with original
		/// types.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="baseTypeName">Name of the base type.</param>
		/// <param name="includeInterfaces">(Optional) True to include, false to exclude the interfaces.</param>
		/// <returns/>
		public static bool InheritsOrImplementsOrEquals(this ITypeSymbol type, string baseTypeName,
														bool includeInterfaces = true)
		{
			if (type == null)
				return false;

			IEnumerable<ITypeSymbol> baseTypes = type.GetBaseTypesAndThis();

			if (includeInterfaces)
			{
				baseTypes = baseTypes.ConcatStructList(type.AllInterfaces);
			}

			return baseTypes.Any(typeSymbol => typeSymbol.Name == baseTypeName);					
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ImplementsInterface(this ITypeSymbol type, string interfaceName)
		{
			if (type == null)
				return false;
			else if (type.TypeKind == TypeKind.Interface && type.Name == interfaceName)
				return true;
			else
				return type.AllInterfaces.Any(interfaceType => interfaceType.Name == interfaceName);
		}
			

		/// <summary>
		/// Gets <paramref name="typeParameterSymbol"/> and its all constraint types.
		/// </summary>
		/// <param name="typeParameterSymbol">The typeParameterSymbol to act on.</param>
		/// <param name="includeInterfaces">(Optional) True to include, false to exclude the interfaces.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetTypeWithAllConstraintTypes(this ITypeParameterSymbol typeParameterSymbol,
																			 bool includeInterfaces = true)
		{
			var constraintTypes = typeParameterSymbol.GetAllConstraintTypes(includeInterfaces);
			return typeParameterSymbol.ToEnumerable()
									  .Concat(constraintTypes);
		}

		/// <summary>
		/// Gets all constraint types for the given <paramref name="typeParameterSymbol"/>.
		/// </summary>
		/// <param name="typeParameterSymbol">The typeParameterSymbol to act on.</param>
		/// <param name="includeInterfaces">(Optional) True to include, false to exclude the interfaces.</param>
		/// <returns/>
		public static IEnumerable<ITypeSymbol> GetAllConstraintTypes(this ITypeParameterSymbol typeParameterSymbol, bool includeInterfaces = true)
		{
			typeParameterSymbol.ThrowOnNull(nameof(typeParameterSymbol));
			
			var constraintTypes = includeInterfaces
				? GetAllConstraintTypesImplementation(typeParameterSymbol)
				: GetAllConstraintTypesImplementation(typeParameterSymbol)
							.Where(type => type.TypeKind != TypeKind.Interface);

			return constraintTypes.Distinct();

			//---------------------------------Local Functions--------------------------------------------------------
			IEnumerable<ITypeSymbol> GetAllConstraintTypesImplementation(ITypeParameterSymbol typeParameter, int recursionLevel = 0)
			{
				const int maxRecursionLevel = 40;

				if (recursionLevel > maxRecursionLevel || typeParameter.ConstraintTypes.Length == 0)
					yield break;

				foreach (ITypeSymbol constraintType in typeParameter.ConstraintTypes)
				{
					if (constraintType is ITypeParameterSymbol constraintTypeParameter)
					{
						var nextOrderTypeParams = GetAllConstraintTypesImplementation(constraintTypeParameter, recursionLevel + 1);

						foreach (ITypeSymbol type in nextOrderTypeParams)
						{
							yield return type;
						}
					}
					else
					{
						yield return constraintType;
					}
				}
			}
		}

		/// <summary>
		/// Gets the depth of inheritance between <paramref name="type"/> and its <paramref name="baseType"/>.
		/// If <paramref name="baseType"/> is not an ancestor of type returns <c>null</c>.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="baseType">The base type.</param>
		/// <returns>
		/// The inheritance depth.
		/// </returns>
		public static int? GetInheritanceDepth(this ITypeSymbol type, ITypeSymbol baseType)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(type));

			ITypeSymbol current = type;
			int depth = 0;

			while (current != null && !current.Equals(baseType))
			{
				current = current.BaseType;
				depth++;
			}

			return current != null ? depth : (int?)null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<ITypeSymbol> GetAllAttributesDefinedOnThisAndBaseTypes(this ITypeSymbol typeSymbol)
		{
			typeSymbol.ThrowOnNull(nameof(typeSymbol));
			return typeSymbol.GetBaseTypesAndThis()
							 .SelectMany(t => t.GetAttributes())
							 .Select(a => a.AttributeClass);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ITypeSymbol GetUnderlyingTypeFromNullable(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			if (!typeSymbol.IsNullable(pxContext) || !(typeSymbol is INamedTypeSymbol namedTypeSymbol))
				return null;

			ImmutableArray<ITypeSymbol> typeArgs = namedTypeSymbol.TypeArguments;
			return typeArgs.Length == 1
				? typeArgs[0]
				: null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullable(this ITypeSymbol typeSymbol, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			return typeSymbol?.OriginalDefinition?.Equals(pxContext.SystemTypes.Nullable) ?? false;
		}

		/// <summary>
		/// An INamedTypeSymbol extension method that gets CLR-style full type name from type.
		/// </summary>
		/// <param name="typeSymbol">The typeSymbol to act on.</param>
		/// <returns/>
		public static string GetCLRTypeNameFromType(this ITypeSymbol typeSymbol)
		{
			if (typeSymbol == null)
				return string.Empty;
			else if (typeSymbol.ContainingType == null)
				return typeSymbol.GetClrStyleTypeFullNameForNotNestedType();

			Stack<ITypeSymbol> containingTypesStack = typeSymbol.GetContainingTypesAndThis().ToStack();
			string notNestedTypeName = containingTypesStack.Pop().GetClrStyleTypeFullNameForNotNestedType();
			StringBuilder nameBuilder = new StringBuilder(notNestedTypeName, capacity: 128);

			while (containingTypesStack.Count > 0)
			{
				ITypeSymbol nestedType = containingTypesStack.Pop();
				nameBuilder.AppendClrStyleNestedTypeShortName(nestedType);
			}

			return nameBuilder.ToString();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static StringBuilder AppendClrStyleNestedTypeShortName(this StringBuilder builder, ITypeSymbol typeSymbol)
		{
			builder.Append(DefaultNestedTypesSeparator)
				   .Append(typeSymbol.Name);

			if (!(typeSymbol is INamedTypeSymbol namedType) || !namedType.IsGenericType)
				return builder;

			var typeArgs = namedType.TypeArguments;
			return builder.Append(DefaultGenericArgsCountSeparator)
						  .Append(typeArgs.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string GetClrStyleTypeFullNameForNotNestedType(this ITypeSymbol notNestedTypeSymbol)
		{
			if (!(notNestedTypeSymbol is INamedTypeSymbol namedType) || !namedType.IsGenericType)
				return notNestedTypeSymbol.ToDisplayString();

			var typeArgs = namedType.TypeArguments;
			var displayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
														genericsOptions: SymbolDisplayGenericsOptions.None);
			string typeNameWithoutGeneric = namedType.ToDisplayString(displayFormat);
			return typeNameWithoutGeneric + DefaultGenericArgsCountSeparator + typeArgs.Length;
		}

		internal static IEnumerable<(ConstructorDeclarationSyntax Node, IMethodSymbol Symbol)> GetDeclaredInstanceConstructors(
			this INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
		{
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

			List<(ConstructorDeclarationSyntax, IMethodSymbol)> initializers = new List<(ConstructorDeclarationSyntax, IMethodSymbol)>();

			foreach (IMethodSymbol ctr in typeSymbol.InstanceConstructors)
			{
				cancellation.ThrowIfCancellationRequested();

				if (!ctr.IsDefinition)
					continue;

				SyntaxReference reference = ctr.DeclaringSyntaxReferences.FirstOrDefault();
				if (reference == null)
					continue;

				if (!(reference.GetSyntax(cancellation) is ConstructorDeclarationSyntax node))
					continue;

				initializers.Add((node, ctr));
			}

			return initializers;
		}

		public static ImmutableArray<StaticConstructorInfo> GetStaticConstructors(this INamedTypeSymbol typeSymbol,
																				  CancellationToken cancellation = default)
		{
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

			int order = 0;
			List<StaticConstructorInfo> staticCtrs = new List<StaticConstructorInfo>();

			foreach (IMethodSymbol ctr in typeSymbol.StaticConstructors)
			{
				cancellation.ThrowIfCancellationRequested();

				SyntaxReference reference = ctr.DeclaringSyntaxReferences.FirstOrDefault();

				if (!(reference?.GetSyntax(cancellation) is ConstructorDeclarationSyntax node))
					continue;

				staticCtrs.Add(new StaticConstructorInfo(node, ctr, order));
				order++;
			}

			return staticCtrs.ToImmutableArray();
		}

		/// <summary>Get all the methods of this symbol.</summary>
		/// <returns>An ImmutableArray containing all the methods of this symbol. If this symbol has no methods,
		/// returns an empty ImmutableArray. Never returns Null.</returns>
		public static ImmutableArray<IMethodSymbol> GetMethods(this ITypeSymbol type)
		{
			type.ThrowOnNull(nameof(type));

			return type
				.GetMembers()
				.OfType<IMethodSymbol>()
				.ToImmutableArray();
		}

		/// <summary>
		/// Get all the methods of this symbol that have a particular name.
		/// </summary>
		/// <returns>An ImmutableArray containing all the methods of this symbol with the given name. If there are
		/// no methods with this name, returns an empty ImmutableArray. Never returns Null.</returns>
		public static ImmutableArray<IMethodSymbol> GetMethods(this ITypeSymbol type, string methodName)
		{
			type.ThrowOnNull(nameof(type));

			return type
				.GetMembers(methodName)
				.OfType<IMethodSymbol>()
				.ToImmutableArray();
		}

		/// <summary>
		/// Returns true if a type is declared in Acumatica root namespace
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInAcumaticaRootNamespace(this ITypeSymbol type)
		{
			type.ThrowOnNull(nameof(type));

			var typeRootNamespace = type
				.GetContainingNamespaces()
				.Where(n => !string.IsNullOrEmpty(n.Name))
				.Last();

			return NamespaceNames.AcumaticaRootNamespace.Equals(typeRootNamespace.Name, StringComparison.Ordinal);
		}


		/// <summary>
		/// An ITypeSymbol extension method that gets a simplified name for type if it is a primitive type.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <returns/>
		public static string GetSimplifiedName(this ITypeSymbol type)
		{
			type.ThrowOnNull(nameof(type));

			switch (type.SpecialType)
			{
				case SpecialType.None when type.TypeKind == TypeKind.Array:
				case SpecialType.System_Object:
				case SpecialType.System_Void:
				case SpecialType.System_Boolean:
				case SpecialType.System_Char:
				case SpecialType.System_SByte:
				case SpecialType.System_Byte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Decimal:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
				case SpecialType.System_String:
				case SpecialType.System_Array:
				case SpecialType.System_Nullable_T:
					return type.ToString();				
				default:
					return type.Name;
			}
		}
	}
}