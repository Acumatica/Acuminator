#nullable enable

using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.BannedApi.Model
{
	public static class ApiUtils
	{
		/// <summary>
		/// An extension method that gets API kind from the Doc ID.
		/// </summary>
		/// <param name="docId">The docId to act on.</param>
		/// <returns>
		/// The API kind.
		/// </returns>
		public static ApiKind GetApiKind(this string docId)
		{
			if (string.IsNullOrWhiteSpace(docId) || docId!.Length <= 2 || docId[1] != ':')
				return ApiKind.Undefined;

			return docId[0] switch
			{
				'N' => ApiKind.Namespace,
				'T' => ApiKind.Type,
				'M' => ApiKind.Method,
				'F' => ApiKind.Field,
				'P' => ApiKind.Property,
				'E' => ApiKind.Event,
				_   => ApiKind.Undefined
			};
		}

		public static char GetDocIdPrefixChar(this ApiKind apiKind) => apiKind switch
			{
				ApiKind.Namespace => 'N',
				ApiKind.Type 	  => 'T',
				ApiKind.Method 	  => 'M',
				ApiKind.Field 	  => 'F',
				ApiKind.Property  => 'P',
				ApiKind.Event 	  => 'E',
				_ 				  => throw new NotSupportedException($"{apiKind} value is not supported"),
			};


		public static ApiKind GetApiKind(this ISymbol symbol) =>
			symbol.CheckIfNull() switch
			{
				INamespaceSymbol => ApiKind.Namespace,
				ITypeSymbol 	 => ApiKind.Type,
				IMethodSymbol 	 => ApiKind.Method,
				IFieldSymbol 	 => ApiKind.Field,
				IPropertySymbol  => ApiKind.Property,
				IEventSymbol 	 => ApiKind.Event,
				_ 				 => ApiKind.Undefined
			};
	}
}
