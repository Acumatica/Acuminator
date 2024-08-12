#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.BannedApi.Model;
using Acuminator.Utilities.BannedApi.Storage;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.BannedApi.ApiInfoRetrievers
{
    /// <summary>
    /// A retriever of the ban API info that also checks for banned containing APIs.
    /// </summary>
    public class HierarchicalApiBanInfoRetriever : DirectApiInfoRetriever
	{
        public HierarchicalApiBanInfoRetriever(IApiStorage bannedApiStorage) : base(bannedApiStorage)
        { }

		protected override ApiSearchResult? GetInfoForApiImpl(ISymbol apiSymbol, ApiKind apiKind)
		{
			var directInfo = base.GetInfoForApiImpl(apiSymbol, apiKind);

			if (directInfo != null)
				return directInfo;

			// We just checked API for directly. There is no containing API for namespaces and undefined APIs.
			if (apiKind is ApiKind.Namespace or ApiKind.Undefined)
				return null;

			Api? namespaceInfo = GetInfoForApiNamespace(apiSymbol.ContainingNamespace);

			if (namespaceInfo != null)
			{
				Api apiInBannedNamespace = new Api(apiSymbol, namespaceInfo.BanKind, namespaceInfo.BanReason);
				return new ApiSearchResult(closestBannedApi: apiInBannedNamespace, namespaceInfo);
			}

			// We checked API for info directly and for namespaces. Non nested types don't have other parent APIs
			if (apiSymbol is ITypeSymbol typeSymbol && typeSymbol.ContainingType == null) 
				return null;

			Api? containingTypeInfo = GetInfoForContainingTypes(apiSymbol.ContainingType);

			if (containingTypeInfo != null)
				return new ApiSearchResult(closestBannedApi: containingTypeInfo, containingTypeInfo);

			// We checked API directly and its containing namespace and types. 
			// Fields, events, properties and normal methods don't have other parent APIs
			if ((apiKind is ApiKind.Field or ApiKind.Property or ApiKind.Event) ||
				apiSymbol is not IMethodSymbol methodSymbol)
			{
				return null;
			}

			// The only API kind left to check are property and event accessors, since they are contained inside their corresponding property/event
			Api? accessorBanInfo = GetInfoForAccessorMethod(methodSymbol);
			return accessorBanInfo != null
				? new ApiSearchResult(closestBannedApi: accessorBanInfo, accessorBanInfo)
				: null;
		}

		private Api? GetInfoForApiNamespace(INamespaceSymbol? apiNamespaceSymbol) =>
			apiNamespaceSymbol != null && !apiNamespaceSymbol.IsGlobalNamespace
				? GetInfoForSymbol(apiNamespaceSymbol, ApiKind.Namespace)
				: null;

		private Api? GetInfoForContainingTypes(INamedTypeSymbol? firstContainingType)
		{
			INamedTypeSymbol? currentType = firstContainingType;

			while (currentType != null) 
			{
				var typeInfo = GetInfoForSymbol(currentType, ApiKind.Type);

				if (typeInfo != null)
					return typeInfo;

				currentType = currentType.ContainingType;
			}

			return null;
		}

		private Api? GetInfoForAccessorMethod(IMethodSymbol acessorMethod) =>
			acessorMethod.AssociatedSymbol switch
			{
				IPropertySymbol propertySymbol => GetInfoForSymbol(propertySymbol, ApiKind.Property),
				IEventSymbol eventSymbol       => GetInfoForSymbol(eventSymbol, ApiKind.Event),
				_                              => null
			};
	}
}