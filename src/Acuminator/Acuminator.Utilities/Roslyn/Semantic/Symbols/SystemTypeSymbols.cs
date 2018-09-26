using System;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class SystemTypeSymbols
	{
		private readonly Compilation _compilation;

		internal SystemTypeSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol Array => _compilation.GetSpecialType(SpecialType.System_Array);

		public IArrayTypeSymbol ByteArray => _compilation.CreateArrayTypeSymbol(Byte);
		public IArrayTypeSymbol StringArray => _compilation.CreateArrayTypeSymbol(String);

		public INamedTypeSymbol String => _compilation.GetSpecialType(SpecialType.System_String);
		public INamedTypeSymbol Bool => _compilation.GetSpecialType(SpecialType.System_Boolean);
		public INamedTypeSymbol Int64 => _compilation.GetSpecialType(SpecialType.System_Int64);
		public INamedTypeSymbol Int32 => _compilation.GetSpecialType(SpecialType.System_Int32);
		public INamedTypeSymbol Int16 => _compilation.GetSpecialType(SpecialType.System_Int16);
		public INamedTypeSymbol Byte => _compilation.GetSpecialType(SpecialType.System_Byte);
		public INamedTypeSymbol Double => _compilation.GetSpecialType(SpecialType.System_Double);
		public INamedTypeSymbol Float => _compilation.GetSpecialType(SpecialType.System_Single);
		public INamedTypeSymbol Decimal => _compilation.GetSpecialType(SpecialType.System_Decimal);
		public INamedTypeSymbol DateTime => _compilation.GetSpecialType(SpecialType.System_DateTime);
		public INamedTypeSymbol Nullable => _compilation.GetSpecialType(SpecialType.System_Nullable_T);
		public INamedTypeSymbol Enum => _compilation.GetSpecialType(SpecialType.System_Enum);

		public INamedTypeSymbol IEnumerable => _compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);

		public INamedTypeSymbol Guid => _compilation.GetTypeByMetadataName(typeof(Guid).FullName);
		public INamedTypeSymbol TimeSpan => _compilation.GetTypeByMetadataName(typeof(TimeSpan).FullName);
	}
}
