using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers
{
    public class LocalizationTypes
    {
        private const string _pxMessagesMetadataName = "PX.Data.PXMessages";
        private const string _pxLocalizerMetadataName = "PX.Data.PXLocalizer";
        private const string _pxLocalizableAttributeMetadataName = "PX.Common.PXLocalizableAttribute";

        private readonly Compilation _compilation;
        private readonly string[] _pxMessagesSimpleMethodNames = new[]
        {
                "Localize",
                "LocalizeNoPrefix"
            };
        private readonly string[] _pxMessagesFormatMethodNames = new[]
        {
                "LocalizeFormat",
                "LocalizeFormatNoPrefix",
                "LocalizeFormatNoPrefixNLA"
            };
        private const string _pxLocalizerSimpleMethodName = "Localize";
        private readonly string[] _pxLocalizerFormatMethodNames = new[]
        {
                "LocalizeFormat",
                "LocalizeFormatWithKey"
            };

        public ImmutableArray<ISymbol> PXMessagesSimpleMethods { get; private set; }
        public ImmutableArray<ISymbol> PXMessagesFormatMethods { get; private set; }
        public ImmutableArray<ISymbol> PXLocalizerSimpleMethods { get; private set; }
        public ImmutableArray<ISymbol> PXLocalizerFormatMethods { get; private set; }

        public LocalizationTypes(Compilation compilation)
        {
            _compilation = compilation;
            InitMethods();
        }

        private INamedTypeSymbol PXMessages => _compilation.GetTypeByMetadataName(_pxMessagesMetadataName);
        private INamedTypeSymbol PXLocalizer => _compilation.GetTypeByMetadataName(_pxLocalizerMetadataName);
        public INamedTypeSymbol PXLocalizableAttribute => _compilation.GetTypeByMetadataName(_pxLocalizableAttributeMetadataName);

        private void InitMethods()
        {
            IEnumerable<ISymbol> pxMessagesMembers = PXMessages.GetMembers();
            (ImmutableArray<ISymbol> simple, ImmutableArray<ISymbol> format) =
                GetMethods(pxMessagesMembers, _pxMessagesSimpleMethodNames, _pxMessagesFormatMethodNames);

            PXMessagesSimpleMethods = simple;
            PXMessagesFormatMethods = format;

            IEnumerable<ISymbol> pxLocalizerMembers = PXLocalizer.GetMembers();
            IEnumerable<string> pxLocalizerSimpleMethodNames = _pxLocalizerSimpleMethodName.ToEnumerable();
            (simple, format) = GetMethods(pxLocalizerMembers, pxLocalizerSimpleMethodNames, _pxLocalizerFormatMethodNames);

            PXLocalizerSimpleMethods = simple;
            PXLocalizerFormatMethods = format;
        }

        private (ImmutableArray<ISymbol>, ImmutableArray<ISymbol>) GetMethods(IEnumerable<ISymbol> membersToAnalyze,
            IEnumerable<string> simpleMethodNames, IEnumerable<string> formatMethodNames)
        {
            List<ISymbol> simpleMethods = new List<ISymbol>();
            List<ISymbol> formatMethods = new List<ISymbol>();

            foreach (ISymbol m in membersToAnalyze)
            {
                if (m.Kind != SymbolKind.Method)
                    continue;

                if (simpleMethodNames.Contains(m.MetadataName))
                {
                    simpleMethods.Add(m);
                }
                else if (formatMethodNames.Contains(m.MetadataName))
                {
                    formatMethods.Add(m);
                }
            }

            return (simpleMethods.ToImmutableArray(), formatMethods.ToImmutableArray());
        }
    }
}
