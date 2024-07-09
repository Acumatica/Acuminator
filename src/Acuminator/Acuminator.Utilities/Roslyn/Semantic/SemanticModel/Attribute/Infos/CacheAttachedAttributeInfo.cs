#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about attributes of a DAC field declared on a cache attached event.
	/// </summary>
	public class CacheAttachedAttributeInfo : AttributeInfoBase
	{
		private readonly DacFieldAttributeInfo _dacFieldAttributeInfo; 

		public override AttributePlacement Placement => AttributePlacement.CacheAttached;

		/// <summary>
		/// The merge method used for the attribute.
		/// </summary>
		public CacheAttachedAttributesMergeMethod MergeMethod { get; }

		/// <summary>
		/// The aggregated attribute metadata collection which is information from the flattened attributes set. 
		/// This information is mostly related to the attribute's relationship with the database.
		/// </summary>
		public ImmutableArray<DataTypeAttributeInfo> AggregatedAttributeMetadata => _dacFieldAttributeInfo.AggregatedAttributeMetadata;

		public DbBoundnessType DbBoundness => _dacFieldAttributeInfo.DbBoundness;

		public bool IsIdentity => _dacFieldAttributeInfo.IsIdentity;

		public bool IsKey => _dacFieldAttributeInfo.IsKey;

		public bool IsDefaultAttribute => _dacFieldAttributeInfo.IsDefaultAttribute;

		public bool IsAutoNumberAttribute => _dacFieldAttributeInfo.IsAutoNumberAttribute;

		protected CacheAttachedAttributeInfo(DacFieldAttributeInfo dacFieldAttributeInfo, CacheAttachedAttributesMergeMethod mergeMethod) :
										base(dacFieldAttributeInfo.CheckIfNull().AttributeData, dacFieldAttributeInfo.DeclarationOrder)
		{
			_dacFieldAttributeInfo = dacFieldAttributeInfo;
			MergeMethod 		   = mergeMethod;
		}

		public static CacheAttachedAttributeInfo Create(AttributeData attribute, CacheAttachedAttributesMergeMethod mergeMethod, 
														DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder)
		{
			var dacFieldAttributeInfo = DacFieldAttributeInfo.Create(attribute, dbBoundnessCalculator, declarationOrder);
			return new CacheAttachedAttributeInfo(dacFieldAttributeInfo, mergeMethod);
		}
	}
}