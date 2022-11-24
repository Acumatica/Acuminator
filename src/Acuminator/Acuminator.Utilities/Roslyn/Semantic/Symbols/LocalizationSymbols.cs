#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class LocalizationSymbols : SymbolsSetBase
	{
		private const string PXMessagesMetadataName             = "PX.Data.PXMessages";
		private const string PXLocalizerMetadataName            = "PX.Data.PXLocalizer";
		private const string PXLocalizableAttributeMetadataName = "PX.Common.PXLocalizableAttribute";

		private static readonly string[] _pxMessagesSimpleMethodNames =
		{
			"Localize",
			"LocalizeNoPrefix"
		};

		private static readonly string[] _pxMessagesFormatMethodNames =
		{
			"LocalizeFormat",
			"LocalizeFormatNoPrefix",
			"LocalizeFormatNoPrefixNLA"
		};

		private static readonly string[] _pxLocalizerSimpleMethodNames = 
		{ 
			"Localize"
		};

		private static readonly string[] _pxLocalizerFormatMethodNames =
		{
			"LocalizeFormat",
			"LocalizeFormatWithKey"
		};

		public INamedTypeSymbol PXMessages { get; }

		public INamedTypeSymbol PXLocalizer { get; }

		public INamedTypeSymbol PXLocalizableAttribute { get; }

		public ImmutableArray<IMethodSymbol> PXMessagesSimpleMethods { get; }

		public ImmutableArray<IMethodSymbol> PXMessagesFormatMethods { get; }

		public ImmutableArray<IMethodSymbol> PXLocalizerSimpleMethods { get; }

		public ImmutableArray<IMethodSymbol> PXLocalizerFormatMethods { get; }

		internal LocalizationSymbols(Compilation compilation) : base(compilation)
		{
			PXMessages = Compilation.GetTypeByMetadataName(PXMessagesMetadataName);
			PXLocalizer = Compilation.GetTypeByMetadataName(PXLocalizerMetadataName);
			PXLocalizableAttribute = Compilation.GetTypeByMetadataName(PXLocalizableAttributeMetadataName);

			(PXMessagesSimpleMethods, PXMessagesFormatMethods) = 
				GetLocalizationMethodsFromType(PXMessages, _pxMessagesSimpleMethodNames, _pxMessagesFormatMethodNames);
			(PXLocalizerSimpleMethods, PXLocalizerFormatMethods) = 
				GetLocalizationMethodsFromType(PXLocalizer, _pxLocalizerSimpleMethodNames, _pxLocalizerFormatMethodNames);
		}

		private (ImmutableArray<IMethodSymbol> SimpleMethods, ImmutableArray<IMethodSymbol> FormatMethods) GetLocalizationMethodsFromType(INamedTypeSymbol type,
																																		  string[] simpleMethodNames,
																																		  string[] formatMethodNames)
		{
			var methods		  = type.GetMembers().OfType<IMethodSymbol>();
			var simpleMethods = ImmutableArray.CreateBuilder<IMethodSymbol>(initialCapacity: simpleMethodNames.Length);
			var formatMethods = ImmutableArray.CreateBuilder<IMethodSymbol>(initialCapacity: formatMethodNames.Length);

			foreach (IMethodSymbol method in methods)
			{
				if (simpleMethodNames.Contains(method.MetadataName))
				{
					simpleMethods.Add(method);
				}
				else if (formatMethodNames.Contains(method.MetadataName))
				{
					formatMethods.Add(method);
				}
			}

			return (simpleMethods.ToImmutable(), formatMethods.ToImmutable());
		}
	}
}
