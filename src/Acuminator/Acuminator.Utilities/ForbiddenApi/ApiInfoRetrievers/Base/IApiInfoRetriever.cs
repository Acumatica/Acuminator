#nullable enable

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.ForbiddenApi.ApiInfoRetrievers
{
	/// <summary>
	/// Interface for a retriever of info for APIs.
	/// </summary>
	public interface IApiInfoRetriever
	{
		/// <summary>
		/// Gets the information for API. Returns <c>null</c> if the API is not found.
		/// </summary>
		/// <param name="apiSymbol">The API symbol to check.</param>
		/// <returns>
		/// Returns the information about banned API or <c>null</c> if the API is not found.
		/// </returns>
		ApiSearchResult? GetInfoForApi(ISymbol apiSymbol);
	}
}