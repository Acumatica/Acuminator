#nullable enable

using System.Collections.Immutable;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class DataViewInfo : SymbolItem<ISymbol>, IWriteableBaseItem<DataViewInfo>
	{
		/// <summary>
		/// Indicates whether the data view is processing data view
		/// </summary>
		public bool IsProcessing { get; }

		/// <summary>
		/// Indicates whether the data view is PXSetup data view
		/// </summary>
		public bool IsSetup { get; }

		/// <summary>
		/// Indicates whether the data view is PXFilter data view
		/// </summary>
		public bool IsFilter { get; }

		/// <summary>
		/// Indicates whether the data view is a custom PXSelect-derived data view
		/// </summary>
		public bool IsCustomView { get; }

		/// <summary>
		/// Indicates whether the data view is derived from PXSelectReadOnly
		/// </summary>
		public bool IsPXSelectReadOnly { get; }

		/// <summary>
		/// Indicates whether the data view is FBQL
		/// </summary>
		public bool IsFBQL { get; }

		/// <summary>
		/// The type of the data view symbol
		/// </summary>
		public INamedTypeSymbol Type { get; }
		
		public ITypeSymbol? DAC { get; }

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

		protected DataViewInfo? _baseInfo;

		/// <summary>
		/// The overriden item if any
		/// </summary>
		public DataViewInfo? Base => _baseInfo;

		DataViewInfo? IWriteableBaseItem<DataViewInfo>.Base
		{
			get => Base;
			set
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, int declarationOrder)
					 : base(symbol, declarationOrder)
		{
			pxContext.ThrowOnNull();

			Type = type.CheckIfNull();

			IsProcessing 	   = Type.IsProcessingView(pxContext);
			IsSetup 		   = Type.IsPXSetupBqlCommand(pxContext);
			IsFilter 		   = Type.InheritsFromOrEqualsGeneric(pxContext.BQL.PXFilter);
			IsFBQL 			   = Type.IsFbqlView(pxContext);
			IsCustomView 	   = !IsProcessing && !IsSetup && !IsFilter && !IsFBQL && Type.IsCustomBqlCommand(pxContext);
			IsPXSelectReadOnly = Type.IsPXSelectReadOnlyCommand();

			DAC = Type.GetDacFromView(pxContext);
		}

		public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, int declarationOrder, DataViewInfo baseInfo)
					 : this(symbol, type, pxContext, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull(nameof(baseInfo));
			CombineWithBaseInfo(_baseInfo);
		}

		void IWriteableBaseItem<DataViewInfo>.CombineWithBaseInfo(DataViewInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		private void CombineWithBaseInfo(DataViewInfo baseInfo)
		{
			
		}
	}
}