﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected virtual DacNodeViewModel CreateDacNode(DacSemanticModel dacSemanticModel, TreeViewModel tree) =>
			new DacNodeViewModel(dacSemanticModel, tree, ExpandCreatedNodes);

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacNodeViewModel dac)
		{
			foreach (DacMemberCategory dacMemberCategory in GetDacMemberCategoriesInOrder())
			{
				Cancellation.ThrowIfCancellationRequested();
				var dacCategory = CreateCategory(dac, dacMemberCategory);

				if (dacCategory != null)
				{
					yield return dacCategory;
				}
			}
		}

		protected virtual IEnumerable<DacMemberCategory> GetDacMemberCategoriesInOrder()
		{
			yield return DacMemberCategory.InitializationAndActivation;
			yield return DacMemberCategory.Keys;
			yield return DacMemberCategory.Property;
			yield return DacMemberCategory.FieldsWithoutProperty;
		}

		protected virtual DacMemberCategoryNodeViewModel? CreateCategory(DacNodeViewModel dac, DacMemberCategory dacMemberCategory) =>
			dacMemberCategory switch
			{
				DacMemberCategory.InitializationAndActivation => new DacInitializationAndActivationCategoryNodeViewModel(dac, ExpandCreatedNodes),
				DacMemberCategory.Keys 						  => new DacKeysCategoryNodeViewModel(dac, ExpandCreatedNodes),
				DacMemberCategory.Property 					  => new DacPropertiesCategoryNodeViewModel(dac, ExpandCreatedNodes),
				_ 											  => null,
			};

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory)
		{
			return CreateDacMemberCategoryChildren<IsActiveMethodInfo>(dacInitializationAndActivationCategory,
						constructor: isActiveMethodInfo => new IsActiveDacMethodNodeViewModel(dacInitializationAndActivationCategory,
																							  isActiveMethodInfo, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory)
		{
			dacKeysCategory.ThrowOnNull();
			return CreateDacMemberCategoryChildren<DacPropertyInfo>(dacKeysCategory,
																	propertyInfo => new PropertyNodeViewModel(dacKeysCategory, propertyInfo, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory)
		{
			dacPropertiesCategory.ThrowOnNull();
			return CreateDacMemberCategoryChildren<DacPropertyInfo>(dacPropertiesCategory,
																	propertyInfo => new PropertyNodeViewModel(dacPropertiesCategory, propertyInfo, ExpandCreatedNodes));
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacMemberCategoryChildren<TInfo>(DacMemberCategoryNodeViewModel dacMemberCategory,
																								Func<TInfo, TreeNodeViewModel> constructor)
		where TInfo : SymbolItem
		{
			var categorySymbols = dacMemberCategory?.GetCategoryDacNodeSymbols();

			if (categorySymbols == null)
			{
				yield break;
			}

			foreach (TInfo info in categorySymbols)
			{
				Cancellation.ThrowIfCancellationRequested();
				TreeNodeViewModel childNode = constructor(info);

				if (childNode != null)
				{
					yield return childNode;
				}
			}
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(PropertyNodeViewModel property)
		{
			var attributes = property.CheckIfNull().PropertyInfo.Attributes;
			return !attributes.IsDefaultOrEmpty
				? attributes.Select(attrInfo => new DacFieldAttributeNodeViewModel(property, attrInfo, ExpandCreatedNodes))
				: [];
		}
	}
}