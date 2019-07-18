﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacSemanticModel
	{
		private readonly CancellationToken _cancellation;
		private readonly PXContext _pxContext;

		public DacType DacType { get; }

		public ClassDeclarationSyntax DacNode { get; }

		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// The DAC symbol. For the DAC is the same as <see cref="Symbol"/>. For DAC extensions is the extension's base DAC.
		/// </summary>
		public ITypeSymbol DacSymbol { get; }

		public ImmutableDictionary<string, DacPropertyInfo> PropertiesByNames { get; }
		public IEnumerable<DacPropertyInfo> Properties => PropertiesByNames.Values;

		public IEnumerable<DacPropertyInfo> DeclaredProperties => Properties.Where(p => p.Symbol.ContainingType == Symbol);

		public ImmutableDictionary<string, DacFieldInfo> FieldsByNames { get; }
		public IEnumerable<DacFieldInfo> Fields => FieldsByNames.Values;

		public IEnumerable<DacFieldInfo> DeclaredFields => Fields.Where(f => f.Symbol.ContainingType == Symbol);

		private DacSemanticModel(PXContext pxContext, DacType dacType, INamedTypeSymbol symbol, ClassDeclarationSyntax dacNode,
								 CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			_pxContext = pxContext;
			_cancellation = cancellation;
			DacType = dacType;
			Symbol = symbol;		
			DacSymbol = DacType == DacType.Dac
				? Symbol
				: Symbol.GetDacFromDacExtension(_pxContext);

			PropertiesByNames = GetDacProperties();
			FieldsByNames = GetDacFields();
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

			if (typeSymbol.IsDAC(pxContext))
				return new DacSemanticModel(pxContext, DacType.Dac, typeSymbol, cancellation);
			else if (typeSymbol.IsDacExtension(pxContext))
				return new DacSemanticModel(pxContext, DacType.DacExtension, typeSymbol, cancellation);
			else
				return null;
			{
				dacType = DacType.DacExtension;
			}

			if (dacType == DacType.None ||
				!(typeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellation) is ClassDeclarationSyntax dacNode))
			{
				return null;
			}

			return new DacSemanticModel(pxContext, dacType, typeSymbol, dacNode,  cancellation);
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
			var memberList = DacNode.Members;

			for (int i = 0; i < memberList.Count; i++)
			{
				if (memberList[i] is TMemberNode memberNode)
					yield return memberNode;
			}
		}

		private ImmutableDictionary<string, DacPropertyInfo> GetDacProperties() =>
			GetInfos(() => Symbol.GetDacPropertiesFromDac(_pxContext, cancellation: _cancellation),
					 () => Symbol.GetPropertiesFromDacExtensionAndBaseDac(_pxContext, _cancellation));

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
	}
}
