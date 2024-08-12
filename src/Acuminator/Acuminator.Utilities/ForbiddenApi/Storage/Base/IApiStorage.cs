#nullable enable

using Acuminator.Utilities.ForbiddenApi.Model;

namespace Acuminator.Utilities.ForbiddenApi.Storage
{
    /// <summary>
    /// An interface for the API storage.
    /// </summary>
    public interface IApiStorage
    {
        public int ApiKindsCount { get; }

        /// <summary>
        /// Count of APIs of the <paramref name="apiKind"/> kind.
        /// </summary>
        /// <param name="apiKind">The API kind.</param>
        /// <returns>
        /// The number of APIs of the <paramref name="apiKind"/> kind.
        /// </returns>
        public int CountOfApis(ApiKind apiKind);

        /// <summary>
        /// Gets the API or null if there is no such API in the storage.
        /// </summary>
        /// <param name="apiKind">The API kind.</param>
        /// <param name="apiDocId">The API Doc ID.</param>
        /// <returns>
        /// The API or null.
        /// </returns>
        public ApiWithAppliedBanKind? GetApi(ApiKind apiKind, string apiDocId);

        /// <summary>
        /// Query if the storage contains the API.
        /// </summary>
        /// <param name="apiKind">The API kind.</param>
        /// <param name="apiDocId">The API Doc ID.</param>
        /// <returns>
        /// True if the storage contains the API, false if not.
        /// </returns>
        public bool ContainsApi(ApiKind apiKind, string apiDocId);
    }
}
