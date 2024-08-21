#nullable enable
using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class DataViewDelegateInfo : NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>, IWriteableBaseItem<DataViewDelegateInfo>
	{
		protected DataViewDelegateInfo? _baseInfo;

		/// <summary>
		/// The overriden item if any
		/// </summary>
		public DataViewDelegateInfo? Base => _baseInfo;

		DataViewDelegateInfo? IWriteableBaseItem<DataViewDelegateInfo>.Base
		{
			get => Base;
			set 
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		public DataViewDelegateInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder)
			: base(node, symbol, declarationOrder)
		{
		}

		public DataViewDelegateInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder, DataViewDelegateInfo baseInfo)
			: this(node, symbol, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(_baseInfo);
		}

		/// <summary>
		/// Gets a sequence of view delegates starting from the most derived override (including/excluding the instance depending on <paramref name="includeSelf"/> argument) and
		/// continuing with all overrides to the most base one.
		/// </summary>
		/// <param name="includeSelf">(Optional) True to include, false to exclude the self.</param>
		/// <returns/>
		public IEnumerable<DataViewDelegateInfo> GetDelegateWithAllOverrides(bool includeSelf = true)
		{
			const int recursionMax = 100;
			int counter = 0;
			DataViewDelegateInfo? currentDelegate = includeSelf 
					? this 
					: Base;

			while (currentDelegate != null && counter <= recursionMax)
			{
				yield return currentDelegate;
				currentDelegate = currentDelegate.Base;
				counter++;
			}
		}

		void IWriteableBaseItem<DataViewDelegateInfo>.CombineWithBaseInfo(DataViewDelegateInfo baseInfo) => 
			CombineWithBaseInfo(baseInfo);

		private void CombineWithBaseInfo(DataViewDelegateInfo baseInfo)
		{
		}
	}
}
