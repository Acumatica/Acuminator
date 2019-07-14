using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;


namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacFieldInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IWriteableBaseItem<DacFieldInfo>
	{
		/// <summary>
		/// The overriden dac field if any
		/// </summary>
		public DacFieldInfo Base
		{
			get;
			internal set;
		}
	
		DacFieldInfo IOverridableItem<DacFieldInfo>.Base => Base;

		DacFieldInfo IWriteableBaseItem<DacFieldInfo>.Base
		{
			get => Base;
			set => Base = value;
		}

		public string Name => Symbol.Name;

		public DacFieldInfo(ClassDeclarationSyntax node, INamedTypeSymbol symbol, int declarationOrder) :
					   base(node, symbol, declarationOrder)
		{
		}

		public DacFieldInfo(ClassDeclarationSyntax node, INamedTypeSymbol symbol, int declarationOrder, DacFieldInfo baseInfo) :
					   this(node, symbol, declarationOrder)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));
			Base = baseInfo;
		}
	}
}