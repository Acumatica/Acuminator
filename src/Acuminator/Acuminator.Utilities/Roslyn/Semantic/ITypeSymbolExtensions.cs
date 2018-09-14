using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class ITypeSymbolExtensions
	{
		private const char DefaultGenericArgsCountSeparator = '`';
		private const char DefaultNestedTypesSeparator = '+';

		public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
		{
			var current = type;
			
			while (current != null)
			{
				yield return current;
				current = current.BaseType;
			}
		}

		public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol type)
		{
			var current = type.BaseType;

			while (current != null)
			{
				yield return current;
				current = current.BaseType;
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
				typeList = typeList.Concat(type.AllInterfaces);
			}

			return typeList.Any(t => t.Equals(baseType));
		}

		/// <summary>
		///  Determine if "type" inherits from "baseType", ignoring constructed types and interfaces, dealing only with original types.
		/// </summary>
		/// <param name="type">The type to act on.</param>
		/// <param name="baseType">The base type.</param>
		/// <returns/>    
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType) =>
			type.GetBaseTypesAndThis().Any(t => t.Equals(baseType));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsFromOrEqualsGeneric(this ITypeSymbol type, ITypeSymbol baseType) =>
			InheritsFromOrEqualsGeneric(type, baseType, false);

		public static bool InheritsFromOrEqualsGeneric(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(baseType));

			var typeList = type.GetBaseTypesAndThis();

			if (includeInterfaces)
				typeList = typeList.Concat(type.AllInterfaces);

			return typeList.Select(t => t.OriginalDefinition)
						   .Any(t => t.Equals(baseType.OriginalDefinition));
		}

		public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces = false)
		{
			type.ThrowOnNull(nameof(type));
			baseType.ThrowOnNull(nameof(baseType));

			var list = type.GetBaseTypes();

			if (includeInterfaces)
				list = list.Concat(type.AllInterfaces);

			return list.Any(t => t.Equals(baseType));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ImplementsInterface(this ITypeSymbol type, ITypeSymbol interfaceType)
		{
			type.ThrowOnNull(nameof(type));
			interfaceType.ThrowOnNull(nameof(interfaceType));

			if (!interfaceType.IsAbstract)
				throw new ArgumentException("Invalid interface type", nameof(interfaceType));

			return type.AllInterfaces.Any(t => t.Equals(interfaceType));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsFrom(this ITypeSymbol symbol, string baseType)
		{
			symbol.ThrowOnNull(nameof(symbol));
			baseType.ThrowOnNullOrWhiteSpace(nameof(baseType));

			ITypeSymbol current = symbol;

			while (current != null)
			{
				if (current.Name == baseType)
					return true;

				current = current.BaseType;
			}

			return false;
		}

		// Determine if "type" inherits from "baseType", ignoring constructed types, optionally including interfaces,
		// dealing only with original types.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InheritsOrImplementsOrEquals(this ITypeSymbol type, string baseTypeName,
														bool includeInterfaces = true)
		{
			if (type == null)
				return false;

			IEnumerable<ITypeSymbol> baseTypes = includeInterfaces
				? type.GetBaseTypesAndThis().ConcatStructList(type.AllInterfaces)
				: type.GetBaseTypesAndThis();

			return baseTypes.Select(typeSymbol => typeSymbol.Name)
							.Contains(baseTypeName);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ImplementsInterface(this ITypeSymbol type, string interfaceName)
		{
			if (type == null)
				return false;

			foreach (var interfaceSymbol in type.AllInterfaces)
			{
				if (interfaceSymbol.Name == interfaceName)
					return true;
			}

			return false;
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
		public static ITypeSymbol GetUnderlyingTypeFromNullable(this INamedTypeSymbol typeSymbol, PXContext pxContext)
		{
			if (!typeSymbol.IsNullable(pxContext))
				return null;
			
			ImmutableArray<ITypeSymbol> typeArgs = typeSymbol.TypeArguments;
			return typeArgs.Length == 1 
				? typeArgs[0]
				: null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullable(this INamedTypeSymbol typeSymbol, PXContext pxContext)
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

        public static IEnumerable<(ConstructorDeclarationSyntax node, IMethodSymbol symbol)> GetDeclaredInstanceConstructors(
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

                initializers.Add(new Tuple<ConstructorDeclarationSyntax, IMethodSymbol>(node, ctr));
            }

            return initializers;
        }

        public static (ConstructorDeclarationSyntax node, IMethodSymbol symbol) GetDeclaredStaticConstructor
            (this INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
        {
            typeSymbol.ThrowOnNull(nameof(typeSymbol));

            foreach(IMethodSymbol ctr in typeSymbol.StaticConstructors)
            {
                cancellation.ThrowIfCancellationRequested();

                if (!ctr.IsDefinition)
                    continue;

                SyntaxReference reference = ctr.DeclaringSyntaxReferences.FirstOrDefault();
                if (reference == null)
                    continue;

                if (!(reference.GetSyntax(cancellation) is ConstructorDeclarationSyntax node))
                    continue;

                return (node, ctr);
            }

            return (null, null);
        }
    }
}