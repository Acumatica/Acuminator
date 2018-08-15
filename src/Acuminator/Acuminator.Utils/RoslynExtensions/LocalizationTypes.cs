using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers
{
    public class LocalizationTypes
    {
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

        private INamedTypeSymbol PXMessages => _compilation.GetTypeByMetadataName("PX.Data.PXMessages");
        private INamedTypeSymbol PXLocalizer => _compilation.GetTypeByMetadataName("PX.Data.PXLocalizer");
        public INamedTypeSymbol PXLocalizableAttribute => _compilation.GetTypeByMetadataName("PX.Common.PXLocalizableAttribute");

        private void InitMethods()
        {
            IEnumerable<ISymbol> pxMessagesMembers = PXMessages.GetMembers();
            Tuple<ImmutableArray<ISymbol>, ImmutableArray<ISymbol>> pxMessagesMethods =
                GetMethods(pxMessagesMembers, _pxMessagesSimpleMethodNames, _pxMessagesFormatMethodNames);

            PXMessagesSimpleMethods = pxMessagesMethods.Item1;
            PXMessagesFormatMethods = pxMessagesMethods.Item2;

            IEnumerable<ISymbol> pxLocalizerMembers = PXLocalizer.GetMembers();
            IEnumerable<string> pxLocalizerSimpleMethodNames = new[] { _pxLocalizerSimpleMethodName };
            Tuple<ImmutableArray<ISymbol>, ImmutableArray<ISymbol>> pxLocalizerMethods =
                GetMethods(pxLocalizerMembers, pxLocalizerSimpleMethodNames, _pxLocalizerFormatMethodNames);

            PXLocalizerSimpleMethods = pxLocalizerMethods.Item1;
            PXLocalizerFormatMethods = pxLocalizerMethods.Item2;
        }

        private Tuple<ImmutableArray<ISymbol>, ImmutableArray<ISymbol>> GetMethods(IEnumerable<ISymbol> membersToAnalyze,
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

            return new Tuple<ImmutableArray<ISymbol>, ImmutableArray<ISymbol>>(simpleMethods.ToImmutableArray(), formatMethods.ToImmutableArray());
        }
    }
}
