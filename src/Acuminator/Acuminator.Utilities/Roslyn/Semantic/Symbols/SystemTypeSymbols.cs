#nullable enable

using System;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class SystemTypeSymbols : SymbolsSetBase
	{
		private readonly Lazy<StringSymbols> _string;

		public StringSymbols String => _string.Value;

		public INamedTypeSymbol Array => Compilation.GetSpecialType(SpecialType.System_Array);

		public IArrayTypeSymbol ByteArray => Compilation.CreateArrayTypeSymbol(Byte);
		public IArrayTypeSymbol UInt16Array => Compilation.CreateArrayTypeSymbol(UInt16);
		public IArrayTypeSymbol StringArray => Compilation.CreateArrayTypeSymbol(String.Type);

		public INamedTypeSymbol Bool => Compilation.GetSpecialType(SpecialType.System_Boolean);
		public INamedTypeSymbol Int64 => Compilation.GetSpecialType(SpecialType.System_Int64);	
		public INamedTypeSymbol Int32 => Compilation.GetSpecialType(SpecialType.System_Int32);
		public INamedTypeSymbol Int16 => Compilation.GetSpecialType(SpecialType.System_Int16);
		public INamedTypeSymbol UInt16 => Compilation.GetSpecialType(SpecialType.System_UInt16);
		public INamedTypeSymbol UInt32 => Compilation.GetSpecialType(SpecialType.System_UInt32);
		public INamedTypeSymbol UInt64 => Compilation.GetSpecialType(SpecialType.System_UInt64);
		public INamedTypeSymbol Byte => Compilation.GetSpecialType(SpecialType.System_Byte);
		public INamedTypeSymbol Double => Compilation.GetSpecialType(SpecialType.System_Double);
		public INamedTypeSymbol Float => Compilation.GetSpecialType(SpecialType.System_Single);
		public INamedTypeSymbol Decimal => Compilation.GetSpecialType(SpecialType.System_Decimal);
		public INamedTypeSymbol DateTime => Compilation.GetSpecialType(SpecialType.System_DateTime);
		public INamedTypeSymbol Nullable => Compilation.GetSpecialType(SpecialType.System_Nullable_T);
		public INamedTypeSymbol Enum => Compilation.GetSpecialType(SpecialType.System_Enum);

		public INamedTypeSymbol IEnumerable => Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);

		public INamedTypeSymbol Guid => Compilation.GetTypeByMetadataName(typeof(Guid).FullName);
		public INamedTypeSymbol TimeSpan => Compilation.GetTypeByMetadataName(typeof(TimeSpan).FullName);

		internal SystemTypeSymbols(Compilation compilation) : base(compilation)
		{
			_string = new Lazy<StringSymbols>(() => new StringSymbols(Compilation));
		}
	}
}
