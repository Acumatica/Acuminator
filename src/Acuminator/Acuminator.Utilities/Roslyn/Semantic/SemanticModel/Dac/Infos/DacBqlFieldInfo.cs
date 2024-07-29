#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacBqlFieldInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IWriteableBaseItem<DacBqlFieldInfo>
	{
		/// <summary>
		/// The overriden dac field if any
		/// </summary>
		public DacBqlFieldInfo? Base
		{
			get;
			internal set;
		}
	
		DacBqlFieldInfo? IWriteableBaseItem<DacBqlFieldInfo>.Base
		{
			get => Base;
			set => Base = value;
		}

		public DacBqlFieldInfo(ClassDeclarationSyntax node, INamedTypeSymbol symbol, int declarationOrder) :
					   base(node, symbol, declarationOrder)
		{
		}

		public DacBqlFieldInfo(ClassDeclarationSyntax node, INamedTypeSymbol symbol, int declarationOrder, DacBqlFieldInfo baseInfo) :
					   this(node, symbol, declarationOrder)
		{
			baseInfo.ThrowOnNull();
			Base = baseInfo;
		}
	}
}