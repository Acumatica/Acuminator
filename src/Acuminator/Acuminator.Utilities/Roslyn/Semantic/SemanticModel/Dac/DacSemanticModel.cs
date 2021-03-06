﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacSemanticModel : ISemanticModel
	{
		private readonly CancellationToken _cancellation;
		private readonly PXContext _pxContext;

		public DacType DacType { get; }

		public ClassDeclarationSyntax Node { get; }

		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// The DAC symbol. For the DAC is the same as <see cref="Symbol"/>. For DAC extensions is the extension's base DAC.
		/// </summary>
		public ITypeSymbol DacSymbol { get; }

		/// <summary>
		/// True if the DAC is a mapping DAC derived from PXMappedCacheExtension class.
		/// </summary>
		public bool IsMappedCacheExtension { get; }

		public ImmutableDictionary<string, DacPropertyInfo> PropertiesByNames { get; }
		public IEnumerable<DacPropertyInfo> Properties => PropertiesByNames.Values;

		public IEnumerable<DacPropertyInfo> DacProperties => Properties.Where(p => p.IsDacProperty);

		public IEnumerable<DacPropertyInfo> AllDeclaredProperties => Properties.Where(p => p.Symbol.ContainingType == Symbol);

		public IEnumerable<DacPropertyInfo> DeclaredDacProperties => Properties.Where(p => p.IsDacProperty && p.Symbol.ContainingType == Symbol);

		public ImmutableDictionary<string, DacFieldInfo> FieldsByNames { get; }
		public IEnumerable<DacFieldInfo> Fields => FieldsByNames.Values;

		public IEnumerable<DacFieldInfo> DeclaredFields => Fields.Where(f => f.Symbol.ContainingType == Symbol);

		/// <summary>
		/// Gets the IsActive method symbol for DAC extension. Can be <c>null</c>. Always <c>null</c> for DACs.
		/// <value>
		/// The is active method symbol.
		/// </value>
		public IMethodSymbol IsActiveMethod { get; }

		private DacSemanticModel(PXContext pxContext, DacType dacType, INamedTypeSymbol symbol, ClassDeclarationSyntax node,
								 CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			_pxContext = pxContext;
			_cancellation = cancellation;
			DacType = dacType;
			Node = node;
			Symbol = symbol;		
			DacSymbol = DacType == DacType.Dac
				? Symbol
				: Symbol.GetDacFromDacExtension(_pxContext);
			IsMappedCacheExtension = Symbol.InheritsFromOrEquals(_pxContext.PXMappedCacheExtensionType);

			FieldsByNames = GetDacFields();
			PropertiesByNames = GetDacProperties();
			IsActiveMethod = GetIsActiveMethod();
		}

		/// <summary>
		/// Returns semantic model of DAC or DAC Extension which is inferred from <paramref name="typeSymbol"/>
		/// </summary>
		/// <param name="pxContext">Context instance</param>
		/// <param name="typeSymbol">Symbol which is DAC or DAC Extension descendant</param>
		/// <param name="semanticModel">Semantic model</param>
		/// <param name="cancellation">Cancellation</param>
		/// <returns/>
		public static DacSemanticModel InferModel(PXContext pxContext, INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
		{		
			pxContext.ThrowOnNull(nameof(pxContext));
			typeSymbol.ThrowOnNull(nameof(typeSymbol));
			cancellation.ThrowIfCancellationRequested();

			DacType? dacType = typeSymbol.IsDAC(pxContext)
				? DacType.Dac
				: typeSymbol.IsDacExtension(pxContext)
					? DacType.DacExtension
					: (DacType?)null;

			if (dacType == null ||
				!(typeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellation) is ClassDeclarationSyntax node))
			{
				return null;
			}

			return new DacSemanticModel(pxContext, dacType.Value, typeSymbol, node, cancellation);
		}

		/// <summary>
		/// Gets the member nodes of the specified type from the DAC/Dac extension declaration.
		/// Does not perform boxing of <see cref="SyntaxList{TNode}"/> <see cref="DacNode.Members"/> which is good for performance.
		/// </summary>
		/// <typeparam name="TMemberNode">Type of the member node.</typeparam>
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

		public bool IsFullyUnbound() =>
			DacProperties.All(p => p.EffectiveBoundType != BoundType.DbBound && p.EffectiveBoundType != BoundType.Unknown);

		private ImmutableDictionary<string, DacPropertyInfo> GetDacProperties() =>
			GetInfos(() => Symbol.GetDacPropertiesFromDac(_pxContext, FieldsByNames, cancellation: _cancellation),
					 () => Symbol.GetPropertiesFromDacExtensionAndBaseDac(_pxContext, FieldsByNames, _cancellation));

		private ImmutableDictionary<string, DacFieldInfo> GetDacFields() =>
			GetInfos(() => Symbol.GetDacFieldsFromDac(_pxContext, cancellation: _cancellation),
					 () => Symbol.GetDacFieldsFromDacExtensionAndBaseDac(_pxContext, _cancellation));

		private ImmutableDictionary<string, TInfo> GetInfos<TInfo>(Func<OverridableItemsCollection<TInfo>> dacInfosSelector,
																   Func<OverridableItemsCollection<TInfo>> dacExtInfosSelector)
		where TInfo : IOverridableItem<TInfo>
		{
			var infos = DacType == DacType.Dac
				? dacInfosSelector()
				: dacExtInfosSelector();

			return infos.ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);
		}

		private IMethodSymbol GetIsActiveMethod()
		{
			if (DacType != DacType.DacExtension)
				return null;

			_cancellation.ThrowIfCancellationRequested();
			ImmutableArray<ISymbol> isActiveCandidates = Symbol.GetMembers(DelegateNames.IsActive);

			if (isActiveCandidates.IsDefaultOrEmpty)
				return null;

			return isActiveCandidates.OfType<IMethodSymbol>()
									 .FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
															   method.Parameters.IsDefaultOrEmpty && !method.IsGenericMethod &&
															   method.ReturnType.SpecialType == SpecialType.System_Boolean);
		}
	}
}
