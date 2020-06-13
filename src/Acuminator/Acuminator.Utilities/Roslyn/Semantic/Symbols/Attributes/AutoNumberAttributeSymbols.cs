using System;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Acuminator.Utilities.Roslyn.Constants;


namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class AutoNumberAttributeSymbols : SymbolsSetForTypeBase
    {
        private const int DefaultMinAutoNumberLength = 15;
        private const string NumberingSequenceStartNbrFieldName = "StartNbr";

        private static object _locker = new object();
        private static volatile bool _isMinAutoNumberLengthInitialized;

        private static int _minAutoNumberLength;

        public int MinAutoNumberLength => _minAutoNumberLength;

        public INamedTypeSymbol NumberingSequence => Compilation.GetTypeByMetadataName(TypeFullNames.NumberingSequence);

        internal AutoNumberAttributeSymbols(Compilation compilation) : base(compilation, TypeFullNames.AutoNumberAttribute)
        {
            InitializeMinAutoNumberLength();
        }

        private void InitializeMinAutoNumberLength()
        {
            if (!_isMinAutoNumberLengthInitialized)
            {
                lock (_locker)
                {
                    if (!_isMinAutoNumberLengthInitialized)
                    {
                        _minAutoNumberLength = ReadMinAutoNumberLengthFromNumberingSequence();
                    }
                }
            }
        }

        private int ReadMinAutoNumberLengthFromNumberingSequence()
        {
            var numberingSequenceDac = NumberingSequence;

            if (numberingSequenceDac == null)
                return DefaultMinAutoNumberLength;

            var startNbrProperty = numberingSequenceDac.GetMembers(NumberingSequenceStartNbrFieldName)
                                                       .FirstOrDefault() as IPropertySymbol;
            if (startNbrProperty == null)
                return DefaultMinAutoNumberLength;

            var stringAttribute = Compilation.GetTypeByMetadataName(TypeFullNames.PXDBStringAttribute);

            if (stringAttribute == null)
                return DefaultMinAutoNumberLength;

            var stringAttributeData = startNbrProperty.GetAttributes()
                                                      .FirstOrDefault(a => a.AttributeClass.InheritsFromOrEquals(stringAttribute));

            if (stringAttributeData == null || stringAttributeData.ConstructorArguments.IsDefaultOrEmpty)
                return DefaultMinAutoNumberLength;

            TypedConstant lengthArg = stringAttributeData.ConstructorArguments[0];
            return lengthArg.Value is int length
                ? length
                : DefaultMinAutoNumberLength;
        }
    }
}
