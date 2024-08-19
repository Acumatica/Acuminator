
using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal class ExcludeFromDocsAttributesChecker
	{
		private const string ObsoleteAttributeShortName = "Obsolete";
		private const string PXHiddenAttributeShortName = "PXHidden";
		private const string PXInternalUseOnlyAttributeShortName = "PXInternalUseOnly";

		public bool CheckIfAttributesDisableDiagnostic(INamedTypeSymbol typeSymbol, bool checkForPXHidden)
		{
			var attributes = typeSymbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return false;

			var shortAttributeNames = attributes.Select(attr => GetAttributeShortName(attr.AttributeClass.Name));

			return CheckAttributeNamesForDocumentationExclusion(shortAttributeNames, checkForPXHidden);
		}

		public bool CheckIfAttributesDisableDiagnostic(MemberDeclarationSyntax member, bool checkForPXHidden)
		{
			var shortAttributeNames = member.GetAttributes()
											.Select(attr => GetAttributeShortName(attr));

			return CheckAttributeNamesForDocumentationExclusion(shortAttributeNames, checkForPXHidden);
		}

		private bool CheckAttributeNamesForDocumentationExclusion(IEnumerable<string> attributeNames, bool checkForPXHidden)
		{
			foreach (string attrName in attributeNames)
			{
				if (ObsoleteAttributeShortName.Equals(attrName) || PXInternalUseOnlyAttributeShortName.Equals(attrName) ||
					(checkForPXHidden && PXHiddenAttributeShortName.Equals(attrName)))
				{
					return true;
				}
			}

			return false;
		}

		private static string GetAttributeShortName(AttributeSyntax attribute)
		{
			string shortName = attribute.Name is QualifiedNameSyntax qualifiedName
				? qualifiedName.Right.ToString()
				: attribute.Name.ToString();

			return GetAttributeShortName(shortName);
		}

		private static string GetAttributeShortName(string attributeName)
		{
			const string AttributeSuffix = "Attribute";
			const int minLengthWithSuffix = 17;

			// perfomance optimization to avoid checking the suffix of attribute names 
			// which are definitely shorter than any of the attributes we search with "Attribute" suffix
			if (attributeName.Length >= minLengthWithSuffix && attributeName.EndsWith(AttributeSuffix))
			{
				const int suffixLength = 9;
				return attributeName.Substring(0, attributeName.Length - suffixLength);
			}

			return attributeName;
		}
	}
}