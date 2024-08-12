#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.BannedApi.Model
{
	/// <summary>
	/// API data.
	/// </summary>
	public class Api : IEquatable<Api>
	{
		private const int NameOffset = 2;

		public string RawApiData { get; }

		public string DocID { get; }

		public string FullName { get; }

		public string Namespace { get; }

		public ImmutableArray<string> AllContainingTypes { get; }

		public string TypeName { get; }

		public string FullTypeName { get; }

		public string MemberName { get; }

		public ApiKind Kind { get; }

		public ApiBanKind BanKind { get; }

		public string? BanReason { get; }

		internal Api(ISymbol apiSymbol, ApiBanKind apiBanKind, string? apiBanReason)
		{
			Kind	   = apiSymbol.CheckIfNull().GetApiKind();
			BanKind	   = apiBanKind;
			BanReason  = apiBanReason.NullIfWhiteSpace();
			DocID	   = apiSymbol.GetDocumentationCommentId().NullIfWhiteSpace() ??
						 throw new InvalidOperationException($"Failed to get Doc ID for symbol {apiSymbol.ToString()}");
			FullName   = DocID.Substring(NameOffset);
			Namespace  = apiSymbol.ContainingNamespace?.ToString() ?? string.Empty;

			AllContainingTypes = apiSymbol is INamespaceSymbol
				? ImmutableArray<string>.Empty
				: apiSymbol.GetContainingTypes()
						   .Select(type => type.Name)
						   .ToImmutableArray();

			switch (apiSymbol)
			{
				case ITypeSymbol:
					MemberName 	 = string.Empty;
					TypeName  	 = apiSymbol.Name;
					FullTypeName = apiSymbol.ToString();
					break;

				case IMethodSymbol:
				case IFieldSymbol:
				case IPropertySymbol:
				case IEventSymbol:
					MemberName 	 = apiSymbol.Name;
					TypeName 	 = apiSymbol.ContainingType?.Name ?? string.Empty;
					FullTypeName = apiSymbol.ContainingType?.ToString() ?? string.Empty;
					break;

				case INamespaceSymbol:
				default:
					MemberName 	 = string.Empty;
					TypeName 	 = string.Empty;
					FullTypeName = string.Empty;
					break;
			}

			RawApiData = GetRawApiData();
		}

		private string GetRawApiData()
		{
			int estimatedCapacity = DocID.Length + 2;		// 2 for the Api Kind prefix and separator

			if (BanReason != null)
				estimatedCapacity += BanReason.Length + 1; // 1 for separator

			char prefix = DocID[0];
			var sb		= new StringBuilder($"{prefix}:{Namespace}", estimatedCapacity);

			if (Kind is not (ApiKind.Namespace or ApiKind.Undefined))
			{
				string combinedTypeName;

				if (Kind == ApiKind.Type)
				{
					combinedTypeName = AllContainingTypes.IsDefaultOrEmpty
						? TypeName
						: AllContainingTypes.AppendItem(TypeName)
											.Join(ApiFormatConstants.Strings.NestedTypesSeparator);
				}
				else
					combinedTypeName = AllContainingTypes.Join(ApiFormatConstants.Strings.NestedTypesSeparator);

				if (!combinedTypeName.IsNullOrWhiteSpace())
				{
					sb.Append($"{ApiFormatConstants.Strings.NamespaceSeparator}{combinedTypeName}");

					if (Kind != ApiKind.Type && !MemberName.IsNullOrWhiteSpace())
						sb.Append($".{MemberName}");
				}
			}

			if (Kind == ApiKind.Method)
			{
				int startBraceIndex = DocID.LastIndexOf('(');

				if (startBraceIndex >= 0)
				{
					string parameters = DocID[startBraceIndex..];
					sb.Append(parameters);
				}
			}

			sb.Append($" {BanKind.ToString()}");

			if (BanReason != null)
				sb.Append($" {BanReason}");

			string rawApiData = sb.ToString();
			return rawApiData;
		}

		public Api(Api sourceApi, ApiBanKind banKind, string? apiBanReason)
		{
			RawApiData 		   = sourceApi.CheckIfNull().RawApiData;
			Kind 			   = sourceApi.Kind;
			DocID 			   = sourceApi.DocID;
			FullName 		   = sourceApi.FullName;
			Namespace 		   = sourceApi.Namespace;
			AllContainingTypes = sourceApi.AllContainingTypes;
			TypeName 		   = sourceApi.TypeName;
			FullTypeName 	   = sourceApi.FullTypeName;
			MemberName 		   = sourceApi.MemberName;
			BanKind 		   = banKind;
			BanReason 		   = apiBanReason.NullIfWhiteSpace();
		}

		public Api(string rawApiData)
		{
			RawApiData = rawApiData.CheckIfNullOrWhiteSpace().Trim();
			var(apiDataWithoutExtraInfo, BanKind, BanReason) = ParseRawApiData(rawApiData);
			Kind = apiDataWithoutExtraInfo.GetApiKind();

			if (Kind == ApiKind.Undefined || apiDataWithoutExtraInfo.Length < NameOffset)
				throw InvalidInputStringFormatException(RawApiData);

			DocID 	 = GetDocID(apiDataWithoutExtraInfo, Kind);
			FullName = DocID.Substring(NameOffset);

			string apiDataWithoutExtraInfoAndPrefix = apiDataWithoutExtraInfo.Substring(NameOffset);
			(Namespace, string combinedTypeName, MemberName) = GetNameParts(apiDataWithoutExtraInfoAndPrefix, Kind);
			(TypeName, AllContainingTypes) 					 = GetTypeParts(combinedTypeName, Kind);

			if (Kind is ApiKind.Namespace or ApiKind.Undefined)
				FullTypeName = string.Empty;
			else
			{
				string preparedCombinedTypeName = combinedTypeName.Replace(ApiFormatConstants.Chars.NestedTypesSeparator, '.');
				FullTypeName = $"{Namespace}.{preparedCombinedTypeName}";
			}
		}

		private static (string ApiDataWithoutExtraInfo, ApiBanKind BanKind, string? BanReason) ParseRawApiData(string rawApiData)
		{
			if (rawApiData.Length < NameOffset)
				throw InvalidInputStringFormatException(rawApiData);

			int firstSpaceIndex = rawApiData.IndexOf(' ');

			if (firstSpaceIndex < 0)
				throw InvalidInputStringFormatException(rawApiData);

			string apiDataWithoutExtraInfo = rawApiData[..firstSpaceIndex];
			int secondSpaceIndex = rawApiData.IndexOf(' ', firstSpaceIndex + 1);
			string banKindString;
			string? banReason;

			if (secondSpaceIndex >= 0)
			{
				banKindString = rawApiData[(firstSpaceIndex + 1)..secondSpaceIndex];
				banReason = rawApiData[(secondSpaceIndex + 1)..].NullIfWhiteSpace()?.Trim();
			}
			else
			{
				banKindString = rawApiData[(firstSpaceIndex + 1)..].Trim();
				banReason = null;
			}

			if (!Enum.TryParse(banKindString, ignoreCase: true, out ApiBanKind banKind))
				throw InvalidInputStringFormatException(rawApiData);

			return (apiDataWithoutExtraInfo, banKind, banReason);
		}

		private static string GetDocID(string apiDataWithoutExtraInfo, ApiKind apiKind)
		{
			if (apiKind == ApiKind.Namespace)
				return apiDataWithoutExtraInfo;

			var sb = new StringBuilder(apiDataWithoutExtraInfo)
							.Replace(ApiFormatConstants.Chars.NamespaceSeparator, '.')
							.Replace(ApiFormatConstants.Chars.NestedTypesSeparator, '.');
			return sb.ToString();
		}

		private static (string Namespace, string CombinedTypeName, string MemberName) GetNameParts(string apiDataWithoutExtraInfoAndPrefix, ApiKind apiKind)
		{
			if (apiKind == ApiKind.Namespace)
				return (apiDataWithoutExtraInfoAndPrefix, CombinedTypeName: string.Empty, MemberName: string.Empty);

			int namespaceSeparatorIndex = apiDataWithoutExtraInfoAndPrefix.IndexOf(ApiFormatConstants.Chars.NamespaceSeparator);
			string @namespace = namespaceSeparatorIndex > 0
				? apiDataWithoutExtraInfoAndPrefix[..namespaceSeparatorIndex]
				: string.Empty;

			if (namespaceSeparatorIndex == apiDataWithoutExtraInfoAndPrefix.Length - 1)
				return (@namespace, CombinedTypeName: string.Empty, MemberName: string.Empty);

			string typeAndMemberName = apiDataWithoutExtraInfoAndPrefix[(namespaceSeparatorIndex + 1)..];
			var (combinedTypeName, memberName) = GetTypeAndMemberNameParts(typeAndMemberName, apiKind);
			return (@namespace, combinedTypeName, memberName);
		}

		private static (string CombinedTypeName, string MemberName) GetTypeAndMemberNameParts(string typeAndMemberName, ApiKind apiKind)
		{
			if (apiKind == ApiKind.Type)
				return (typeAndMemberName, MemberName: string.Empty);
			else if (apiKind != ApiKind.Method)
				return GetTypeAndMemberNamesForMemberApi(typeAndMemberName);

			int startBraceIndex = typeAndMemberName.LastIndexOf('(');

			if (startBraceIndex <= 0)
				return GetTypeAndMemberNamesForMemberApi(typeAndMemberName);

			string typeAndMemberNameWithoutBraces = typeAndMemberName[..startBraceIndex];
			string parameters 					  = typeAndMemberName[startBraceIndex..];
			var (combinedTypeName, memberName) 	  = GetTypeAndMemberNamesForMemberApi(typeAndMemberNameWithoutBraces);
			memberName							 += parameters;

			return (combinedTypeName, memberName);
		}

		private static (string CombinedTypeName, string MemberName) GetTypeAndMemberNamesForMemberApi(string typeAndMemberNameWithoutBraces)
		{
			int lastDotIndex = typeAndMemberNameWithoutBraces.LastIndexOf('.');

			if (lastDotIndex < 0)
				return (CombinedTypeName: string.Empty, MemberName: typeAndMemberNameWithoutBraces);

			string combinedTypeName = typeAndMemberNameWithoutBraces[..lastDotIndex];
			string memberName = lastDotIndex < (typeAndMemberNameWithoutBraces.Length - 1)
				? typeAndMemberNameWithoutBraces[(lastDotIndex + 1)..]
				: string.Empty;
			
			return (combinedTypeName, memberName);
		}

		private static (string TypeName, ImmutableArray<string> ContainingTypes) GetTypeParts(string combinedTypeName, ApiKind apiKind)
		{
			if (combinedTypeName.Length == 0 || apiKind == ApiKind.Namespace)
				return (TypeName: string.Empty, ContainingTypes: ImmutableArray<string>.Empty);

			int typesSeparatorIndex = combinedTypeName.IndexOf(ApiFormatConstants.Chars.NestedTypesSeparator);

			if (typesSeparatorIndex < 0)
			{
				if (apiKind == ApiKind.Type)
					return (TypeName: combinedTypeName, ContainingTypes: ImmutableArray<string>.Empty);
				else
					return (TypeName: combinedTypeName, ContainingTypes: ImmutableArray.Create(combinedTypeName));
			}

			string[] types 		= combinedTypeName.Split(new[] { ApiFormatConstants.Chars.NestedTypesSeparator }, StringSplitOptions.None);
			string typeName 	= types[^1];
			var containingTypes = apiKind == ApiKind.Type
				? types.Take(types.Length - 1).ToImmutableArray()
				: types.ToImmutableArray();
			
			return (typeName, containingTypes);
		}

		private static ArgumentException InvalidInputStringFormatException(string rawApiData) =>
			 new ($"The input API data string \"{rawApiData}\" has unknown format.{Environment.NewLine}" +
					"Please check the following link for a list of supported formats: " +
					"https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/#id-strings" + 
					$"{Environment.NewLine + Environment.NewLine}Acuminator extends this DocID string format.{Environment.NewLine}" + 
					"The namespace part of the API name is separated from the rest of its name with" + 
					$" \"{ApiFormatConstants.Strings.NamespaceSeparator}\",{Environment.NewLine}" +
					$"and names of nested types are separated with \"{ApiFormatConstants.Strings.NestedTypesSeparator}\".{Environment.NewLine}" +
					$"Acuminator also adds two extra strings after the DocID string and separated by whitespaces:{Environment.NewLine}" +
					$"- First, Mandatory banned API data - info for whom the API is banned: Acumatica, ISV, All or None;{Environment.NewLine}" +
					$"- Second, optional string description of why the banned API is forbidden.{Environment.NewLine}{Environment.NewLine}" +
					"Example: \"T:System.Web-IHttpModule ISV Declaring Http Modules in Acumatica Customization is forbidden.\"",
					paramName: nameof(rawApiData));

		public override bool Equals(object obj) => Equals(obj as Api);

		public bool Equals(Api? other) =>
			DocID.Equals(other?.DocID) && BanKind == other.BanKind;

		public override string ToString()
		{	
			string banKind = BanKind.ToString();
			return BanReason != null
				? $"{DocID} {banKind} {BanReason}"
				: $"{DocID} {banKind}";
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + DocID.GetHashCode();
				hash = 23 * hash + (int)BanKind;
			}

			return hash;
		}
	}
}
