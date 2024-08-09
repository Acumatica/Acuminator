#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacSemanticModel : ISemanticModel
	{
		private readonly CancellationToken _cancellation;

		public PXContext PXContext { get; }

		public DacType DacType { get; }

		public ClassDeclarationSyntax Node { get; }

		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// The DAC symbol. For the DAC, the value is the same as <see cref="Symbol"/>. 
		/// For DAC extensions, the value is the symbol of the extension's base DAC.
		/// </summary>
		public ITypeSymbol DacSymbol { get; }

		/// <summary>
		/// An indicator of whether the DAC is a mapping DAC derived from the PXMappedCacheExtension class.
		/// </summary>
		public bool IsMappedCacheExtension { get; }

		/// <summary>
		/// An indicator of whether the DAC is fully unbound.
		/// </summary>
		public bool IsFullyUnbound { get; }

		/// <summary>
		/// An indicator of whether the DAC is a projection DAC.
		/// </summary>
		public bool IsProjectionDac { get; }

		public ImmutableDictionary<string, DacPropertyInfo> PropertiesByNames { get; }
		public IEnumerable<DacPropertyInfo> Properties => PropertiesByNames.Values;

		public IEnumerable<DacPropertyInfo> DacFieldProperties => Properties.Where(p => p.IsDacProperty);

		public IEnumerable<DacPropertyInfo> AllDeclaredProperties => Properties.Where(p => p.Symbol.IsDeclaredInType(Symbol));

		public IEnumerable<DacPropertyInfo> DeclaredDacFieldProperties => Properties.Where(p => p.IsDacProperty && p.Symbol.IsDeclaredInType(Symbol));

		public ImmutableDictionary<string, DacBqlFieldInfo> BqlFieldsByNames { get; }
		public IEnumerable<DacBqlFieldInfo> BqlFields => BqlFieldsByNames.Values;

		public IEnumerable<DacBqlFieldInfo> DeclaredBqlFields => BqlFields.Where(f => f.Symbol.IsDeclaredInType(Symbol));

		public ImmutableDictionary<string, DacFieldInfo> DacFieldsByNames { get; }

		public IEnumerable<DacFieldInfo> DacFields => DacFieldsByNames.Values;

		public IEnumerable<DacFieldInfo> DeclaredDacFields => DacFields.Where(f => f.IsDeclaredInType(Symbol));

		/// <summary>
		/// Information about the IsActive method of the DAC extensions. 
		/// The value can be <c>null</c>. The value is always <c>null</c> for DACs.
		/// <value>
		/// Information about the IsActive method.
		/// </value>
		public IsActiveMethodInfo? IsActiveMethodInfo { get; }

		/// <summary>
		/// The attributes declared on a DAC or a DAC extension.
		/// </summary>
		public ImmutableArray<DacAttributeInfo> Attributes { get; }

		private DacSemanticModel(PXContext pxContext, DacType dacType, INamedTypeSymbol symbol, ClassDeclarationSyntax node,
								 CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			PXContext = pxContext;
			_cancellation = cancellation;
			DacType = dacType;
			Node = node;
			Symbol = symbol;
			DacSymbol = DacType == DacType.Dac
				? Symbol
				: Symbol.GetDacFromDacExtension(PXContext)!;
			IsMappedCacheExtension = Symbol.InheritsFromOrEquals(PXContext.PXMappedCacheExtensionType);

			Attributes         = GetDacAttributes();
			BqlFieldsByNames   = GetDacBqlFields();
			PropertiesByNames  = GetDacProperties();
			DacFieldsByNames   = DacFieldsCollector.CollectDacFieldsFromDacPropertiesAndBqlFields(Symbol, DacType, PXContext,
																								   BqlFieldsByNames, PropertiesByNames);
			IsActiveMethodInfo = GetIsActiveMethodInfo();

			IsFullyUnbound  = DacFieldProperties.All(p => p.EffectiveDbBoundness is DbBoundnessType.Unbound or DbBoundnessType.NotDefined);
			IsProjectionDac = CheckIfDacIsProjection();
		}

		/// <summary>
		/// Returns the semantic model of DAC or DAC extension which is inferred from <paramref name="typeSymbol"/>.
		/// </summary>
		/// <param name="pxContext">Context instance</param>
		/// <param name="typeSymbol">Symbol which is DAC or DAC extension descendant</param>
		/// <param name="semanticModel">Semantic model</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <returns/>
		public static DacSemanticModel? InferModel(PXContext pxContext, INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
		{		
			pxContext.ThrowOnNull();
			typeSymbol.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			DacType? dacType = typeSymbol.IsDAC(pxContext)
				? DacType.Dac
				: typeSymbol.IsDacExtension(pxContext)
					? DacType.DacExtension
					: null;

			if (dacType == null ||
				typeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellation) is not ClassDeclarationSyntax node)
			{
				return null;
			}

			return new DacSemanticModel(pxContext, dacType.Value, typeSymbol, node, cancellation);
		}

		/// <summary>
		/// Gets the member nodes of the specified type from the declaration of a DAC or a DAC extension.
		/// The method does not perform boxing of <see cref="SyntaxList{TNode}"/> <see cref="DacNode.Members"/> which is good for performance.
		/// </summary>
		/// <typeparam name="TMemberNode">Type of the member node</typeparam>
		/// <returns/>
		public IEnumerable<TMemberNode> GetMemberNodes<TMemberNode>()
		where TMemberNode : MemberDeclarationSyntax
		{
			var memberList = Node.Members;

			for (int i = 0; i < memberList.Count; i++)
			{
				if (memberList[i] is TMemberNode memberNode)
					yield return memberNode;
			}
		}

		private ImmutableArray<DacAttributeInfo> GetDacAttributes()
		{
			var attributes = Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return ImmutableArray<DacAttributeInfo>.Empty;

			var attributeInfos = attributes.Select((attributeData, relativeOrder) => new DacAttributeInfo(PXContext, attributeData, relativeOrder));
			var builder = ImmutableArray.CreateBuilder<DacAttributeInfo>(attributes.Length);
			builder.AddRange(attributeInfos);

			return builder.ToImmutable();
		}

		private ImmutableDictionary<string, DacPropertyInfo> GetDacProperties() =>
			GetInfos(() => Symbol.GetDacPropertiesFromDac(PXContext, BqlFieldsByNames, cancellation: _cancellation),
					 () => Symbol.GetPropertiesFromDacExtensionAndBaseDac(PXContext, BqlFieldsByNames, _cancellation));

		private ImmutableDictionary<string, DacBqlFieldInfo> GetDacBqlFields() =>
			GetInfos(() => Symbol.GetDacBqlFieldsFromDac(PXContext, cancellation: _cancellation),
					 () => Symbol.GetDacBqlFieldsFromDacExtensionAndBaseDac(PXContext, _cancellation));

		private ImmutableDictionary<string, TInfo> GetInfos<TInfo>(Func<OverridableItemsCollection<TInfo>> dacInfosSelector,
																   Func<OverridableItemsCollection<TInfo>> dacExtInfosSelector)
		where TInfo : IOverridableItem<TInfo>
		{
			var infos = DacType == DacType.Dac
				? dacInfosSelector()
				: dacExtInfosSelector();

			return infos.ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);
		}

		private IsActiveMethodInfo? GetIsActiveMethodInfo()
		{
			if (DacType != DacType.DacExtension)
				return null;

			_cancellation.ThrowIfCancellationRequested();
			return IsActiveMethodInfo.GetIsActiveMethodInfo(Symbol, _cancellation);
		}

		private bool CheckIfDacIsProjection()
		{
			if (DacType != DacType.Dac || Attributes.IsDefaultOrEmpty)
				return false;

			return Attributes.Any(attrInfo => attrInfo.IsPXProjection);
		}
	}
}
