﻿using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

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

		/// <summary>
		/// The overriden item if any
		/// </summary>
		public DataViewInfo Base
		{
			get;
			internal set;
		}

		DataViewInfo IWriteableBaseItem<DataViewInfo>.Base
		{
			get => Base;
			set => Base = value;
		}

		public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, int declarationOrder)
					 : base(symbol, declarationOrder)
		{
			type.ThrowOnNull(nameof(type));
			pxContext.ThrowOnNull(nameof(pxContext));

			Type = type;

			IsProcessing = type.IsProcessingView(pxContext);
			IsSetup = type.IsPXSetupBqlCommand(pxContext);
			IsFilter = type.InheritsFromOrEqualsGeneric(pxContext.BQL.PXFilter);
			IsFBQL = type.IsFbqlView(pxContext);
			IsCustomView = !IsProcessing && !IsSetup && !IsFilter && !IsFBQL && type.IsCustomBqlCommand(pxContext);
			IsPXSelectReadOnly = type.IsPXSelectReadOnlyCommand();

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