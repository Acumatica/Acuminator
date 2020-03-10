using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphSymbolUtils
	{
		/// <summary>
		/// Gets the graph type from graph extension type.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The graph from graph extension.
		/// </returns>
		public static ITypeSymbol GetGraphFromGraphExtension(this ITypeSymbol graphExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			if (graphExtension == null || !graphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return null;

			var baseGraphExtensionType = graphExtension.GetBaseTypesAndThis()
													   .OfType<INamedTypeSymbol>()
													   .FirstOrDefault(type => type.IsGraphExtensionBaseType());
			if (baseGraphExtensionType == null)
				return null;

			var graphExtTypeArgs = baseGraphExtensionType.TypeArguments;

			if (graphExtTypeArgs.Length == 0)
				return null;

			ITypeSymbol firstTypeArg = graphExtTypeArgs.Last();
			return firstTypeArg.IsPXGraph(pxContext)
				? firstTypeArg
				: null;
		}

		public static bool IsDelegateForViewInPXGraph(this IMethodSymbol method, PXContext pxContext)
		{
			if (method == null || method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable)
				return false;

			INamedTypeSymbol containingType = method.ContainingType;

			if (containingType == null || !containingType.IsPXGraphOrExtension(pxContext))
				return false;

			return containingType.GetMembers()
								 .OfType<IFieldSymbol>()
								 .Where(field => field.Type.InheritsFrom(pxContext.PXSelectBase.Type))
								 .Any(field => string.Equals(field.Name, method.Name, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsValidActionHandler(this IMethodSymbol method, PXContext pxContext)
		{
			method.ThrowOnNull(nameof(method));
			pxContext.ThrowOnNull(nameof(pxContext));

			if (method.Parameters.Length == 0)
				return method.ReturnsVoid;
			else
			{
				return method.Parameters[0].Type.InheritsFromOrEquals(pxContext.PXAdapterType) &&
					   method.ReturnType.InheritsFromOrEquals(pxContext.SystemTypes.IEnumerable, includeInterfaces: true);
			}
		}

		public static bool IsValidViewDelegate(this IMethodSymbol method, PXContext pxContext)
		{
			method.ThrowOnNull(nameof(method));
			pxContext.ThrowOnNull(nameof(pxContext));

			return method.ReturnType.Equals(pxContext.SystemTypes.IEnumerable) &&
				   method.Parameters.All(p => p.RefKind != RefKind.Ref);
		}

		/// <summary>
		/// Get declared primary DAC from graph or graph extension.
		/// </summary>
		/// <param name="graphOrExtension">The graph or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The declared primary DAC from graph or graph extension.
		/// </returns>
		public static ITypeSymbol GetDeclaredPrimaryDacFromGraphOrGraphExtension(this ITypeSymbol graphOrExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			bool isGraph = graphOrExtension?.InheritsFrom(pxContext.PXGraph.Type) ?? false;

			if (!isGraph && !graphOrExtension?.InheritsFrom(pxContext.PXGraphExtensionType) != true)
				return null;

			ITypeSymbol graph = isGraph
				? graphOrExtension
				: graphOrExtension.GetGraphFromGraphExtension(pxContext);

			var baseGraphType = graph.GetBaseTypesAndThis()
									 .OfType<INamedTypeSymbol>()
									 .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type));

			if (baseGraphType == null || baseGraphType.TypeArguments.Length < 2)
				return null;

			ITypeSymbol primaryDacType = baseGraphType.TypeArguments[1];
			return primaryDacType.IsDAC() ? primaryDacType : null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsGraphWithPrimaryDacBaseGenericType(INamedTypeSymbol type) =>
			type.TypeArguments.Length >= 2 && type.Name == TypeNames.PXGraph;

		internal static (MethodDeclarationSyntax Node, IMethodSymbol Symbol) GetGraphExtensionInitialization
			(this INamedTypeSymbol typeSymbol, PXContext pxContext, CancellationToken cancellation = default)
		{
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

			IMethodSymbol initialize = typeSymbol.GetMembers()
									   .OfType<IMethodSymbol>()
									   .Where(m => pxContext.PXGraphExtensionInitializeMethod.Equals(m.OverriddenMethod))
									   .FirstOrDefault();
			if (initialize == null)
				return (null, null);

			SyntaxReference reference = initialize.DeclaringSyntaxReferences.FirstOrDefault();
			if (reference == null)
				return (null, null);

			if (!(reference.GetSyntax(cancellation) is MethodDeclarationSyntax node))
				return (null, null);

			return (node, initialize);
		}

		/// <summary>
		/// Check if <paramref name="eventType"/> is DAC field event.
		/// </summary>
		/// <param name="eventType">The eventType to check.</param>
		/// <returns/>
		public static bool IsDacFieldEvent(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.FieldUpdated:
				case EventType.CacheAttached:
				case EventType.CommandPreparing:
				case EventType.ExceptionHandling:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Check if <paramref name="eventType"/> is DAC row event.
		/// </summary>
		/// <param name="eventType">The eventType to check.</param>
		/// <returns/>
		public static bool IsDacRowEvent(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.RowSelecting:
				case EventType.RowSelected:
				case EventType.RowInserting:
				case EventType.RowInserted:
				case EventType.RowUpdating:
				case EventType.RowUpdated:
				case EventType.RowDeleting:
				case EventType.RowDeleted:
				case EventType.RowPersisting:
				case EventType.RowPersisted:
					return true;
				default:
					return false;
			}
		}
	}
}