using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class DataViewInfo : GraphNodeSymbolItem<ISymbol>
	{
		/// <summary>
		/// The overriden item if any
		/// </summary>
		public DataViewInfo Base { get; }

		/// <summary>
		/// Indicates whether the data view is processing data view
		/// </summary>
		public bool IsProcessing { get; }

		/// <summary>
		/// The type of the data view symbol
		/// </summary>
		public INamedTypeSymbol Type { get; }
		
		public ITypeSymbol DAC { get; }

		/// <summary>
		/// The process delegates
		/// </summary>
		public ImmutableArray<ProcessingDelegateInfo> ProcessDelegates { get; internal set; } =
			ImmutableArray<ProcessingDelegateInfo>.Empty;

		/// <summary>
		/// The parameters process delegate
		/// </summary>
		public ImmutableArray<ProcessingDelegateInfo> ParametersDelegates { get; internal set; } =
			ImmutableArray<ProcessingDelegateInfo>.Empty;

		/// <summary>
		/// The finally process delegate
		/// </summary>
		public ImmutableArray<ProcessingDelegateInfo> FinallyProcessDelegates { get; internal set; } =
			ImmutableArray<ProcessingDelegateInfo>.Empty;

		protected override string DebuggerDisplay => $"{base.DebuggerDisplay} |Type: {Type.ToString()}";

		public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, int declarationOrder)
					 : base(symbol, declarationOrder)
		{
			type.ThrowOnNull(nameof(type));
			pxContext.ThrowOnNull(nameof(pxContext));

			Type = type;
			IsProcessing = type.IsProcessingView(pxContext);
			DAC = Type.GetDacFromView(pxContext);
		}

		public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, int declarationOrder, DataViewInfo baseInfo)
					 : this(symbol, type, pxContext, declarationOrder)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));

			Base = baseInfo;
		}
	}
}