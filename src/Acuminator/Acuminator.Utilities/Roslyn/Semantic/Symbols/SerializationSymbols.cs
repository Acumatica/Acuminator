#nullable enable

using System;
using System.Runtime.Serialization;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class SerializationSymbols : SymbolsSetBase
    {
        internal SerializationSymbols(Compilation compilation) : base(compilation)
        {
			NonSerializedAttribute = Compilation.GetTypeByMetadataName(typeof(NonSerializedAttribute).FullName);
			SerializationInfo      = Compilation.GetTypeByMetadataName(typeof(SerializationInfo).FullName);
			StreamingContext       = Compilation.GetTypeByMetadataName(typeof(StreamingContext).FullName);
		}

		public INamedTypeSymbol NonSerializedAttribute { get; }

		public INamedTypeSymbol SerializationInfo { get; }

		public INamedTypeSymbol StreamingContext { get; }

		public INamedTypeSymbol ReflectionSerializer => Compilation.GetTypeByMetadataName(TypeFullNames.Serialization.ReflectionSerializer);

		public INamedTypeSymbol PXReflectionSerializer => Compilation.GetTypeByMetadataName(TypeFullNames.Serialization.PXReflectionSerializer);
	}
}
