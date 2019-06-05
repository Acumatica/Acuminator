using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Vsix.Coloriser
{
	internal class ParsedSymbolsCache
    {
        public static Dictionary<string, HashSet<string>> DocTypes { get; } = new Dictionary<string, HashSet<string>>
        {
            [TypeNames.BqlCommand] = new HashSet<string>(),
            [TypeNames.IBqlTable] = new HashSet<string>(),
            [TypeNames.IBqlField] = new HashSet<string>(),
            [TypeNames.IBqlParameter] = new HashSet<string>(),
            [TypeNames.IBqlCreator] = new HashSet<string>(),
            [TypeNames.Constant] = new HashSet<string>()
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNodeToCache(string nodeText, string nodeType)
        {       
            if (DocTypes.TryGetValue(nodeType, out HashSet<string> cachedNodes))
                cachedNodes.Add(nodeText);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBqlCommand(string nodeString) => DocTypes[TypeNames.BqlCommand].Contains(nodeString);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBqlParameter(string nodeString) => DocTypes[TypeNames.IBqlParameter].Contains(nodeString);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBqlOperator(string nodeString) => DocTypes[TypeNames.IBqlCreator].Contains(nodeString);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDAC(string nodeString) => DocTypes[TypeNames.IBqlTable].Contains(nodeString);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDacField(string nodeString) => DocTypes[TypeNames.IBqlField].Contains(nodeString);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBqlConstant(string nodeString) => DocTypes[TypeNames.Constant].Contains(nodeString);    
    }
}
