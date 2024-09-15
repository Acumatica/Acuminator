#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected virtual DacNodeViewModel CreateDacNode(DacSemanticModelForCodeMap dacSemanticModel, TreeViewModel tree) =>
			new DacNodeViewModel(dacSemanticModel, tree, ExpandCreatedNodes);

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacNodeViewModel dac)
		{
			var dacAttributesGroup = GetDacAttributesGroupNode(dac);

			if (dacAttributesGroup != null)
				yield return dacAttributesGroup;

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

		protected virtual DacAttributesGroupNodeViewModel GetDacAttributesGroupNode(DacNodeViewModel dac) =>
			new DacAttributesGroupNodeViewModel(dac.DacModelForCodeMap.DacModel, dac, ExpandCreatedNodes);

		protected virtual IEnumerable<DacMemberCategory> GetDacMemberCategoriesInOrder()
		{
			yield return DacMemberCategory.BaseTypes;
			yield return DacMemberCategory.InitializationAndActivation;
			yield return DacMemberCategory.Keys;
			yield return DacMemberCategory.Property;
			yield return DacMemberCategory.FieldsWithoutProperty;
		}

		protected virtual DacMemberCategoryNodeViewModel? CreateCategory(DacNodeViewModel dac, DacMemberCategory dacMemberCategory) =>
			dacMemberCategory switch
			{
				DacMemberCategory.BaseTypes					  => new DacBaseTypesCategoryNodeViewModel(dac, dac, ExpandCreatedNodes),
				DacMemberCategory.InitializationAndActivation => new DacInitializationAndActivationCategoryNodeViewModel(dac, dac, ExpandCreatedNodes),
				DacMemberCategory.Keys 						  => new KeyDacFieldsCategoryNodeViewModel(dac, dac, ExpandCreatedNodes),
				DacMemberCategory.Property 					  => new AllDacFieldsDacCategoryNodeViewModel(dac, dac, ExpandCreatedNodes),
				_ 											  => null,
			};

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacAttributesGroupNodeViewModel attributeGroupNode) =>
			attributeGroupNode.AttributeInfos()
							  .Select(attrInfo => new DacAttributeNodeViewModel(attributeGroupNode, attrInfo, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacBaseTypesCategoryNodeViewModel dacBaseTypesCategory)
		{
			Cancellation.ThrowIfCancellationRequested();

			if (dacBaseTypesCategory.BaseDacInfo != null)
			{
				yield return new BaseDacNodeViewModel(dacBaseTypesCategory.BaseDacInfo, dacBaseTypesCategory.DacViewModel,
													  dacBaseTypesCategory, ExpandCreatedNodes);
			}

			if (dacBaseTypesCategory.BaseDacExtensionInfo != null)
			{
				yield return new BaseDacNodeViewModel(dacBaseTypesCategory.BaseDacExtensionInfo, dacBaseTypesCategory.DacViewModel, 
													  dacBaseTypesCategory, ExpandCreatedNodes);
			}
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacInitializationAndActivationCategoryNodeViewModel dacInitializationAndActivationCategory)
		{
			Cancellation.ThrowIfCancellationRequested();

			if (dacInitializationAndActivationCategory?.DacModel.IsActiveMethodInfo != null)
			{
				var isActiveNode = new IsActiveDacMethodNodeViewModel(dacInitializationAndActivationCategory,
																	  dacInitializationAndActivationCategory.DacModel.IsActiveMethodInfo, 
																	  ExpandCreatedNodes);
				return [isActiveNode];
			}
			else
				return [];
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(KeyDacFieldsCategoryNodeViewModel dacKeyFieldsCategory) =>
			CreateDacFieldsCategoryChildren(dacKeyFieldsCategory);

		public override IEnumerable<TreeNodeViewModel>? VisitNode(AllDacFieldsDacCategoryNodeViewModel allDacFieldsCategory) =>
			CreateDacFieldsCategoryChildren(allDacFieldsCategory);

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacFieldsCategoryChildren(DacFieldCategoryNodeViewModel dacFieldCategory)
		{
			var categorySymbols = dacFieldCategory.CheckIfNull().GetCategoryDacFields();

			if (categorySymbols == null)
				yield break;

			foreach (DacFieldInfo fieldInfo in categorySymbols)
			{
				Cancellation.ThrowIfCancellationRequested();
				TreeNodeViewModel childNode = new DacFieldNodeViewModel(dacFieldCategory, parent: dacFieldCategory, 
																		fieldInfo, ExpandCreatedNodes);
				if (childNode != null)
					yield return childNode;
			}
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacFieldNodeViewModel dacField)
		{
			if (dacField.FieldInfo.PropertyInfo != null)
			{
				yield return new DacFieldPropertyNodeViewModel(dacField.MemberCategory, parent: dacField, 
															   dacField.FieldInfo.PropertyInfo, ExpandCreatedNodes);
			}

			if (dacField.FieldInfo.BqlFieldInfo != null)
			{
				yield return new DacBqlFieldNodeViewModel(dacField.MemberCategory, parent: dacField, 
														   dacField.FieldInfo.BqlFieldInfo, ExpandCreatedNodes);
			}
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacFieldPropertyNodeViewModel dacFieldProperty)
		{
			var attributes = dacFieldProperty.CheckIfNull().PropertyInfo.Attributes;
			return !attributes.IsDefaultOrEmpty
				? attributes.Select(attrInfo => new DacFieldAttributeNodeViewModel(dacFieldProperty, attrInfo, ExpandCreatedNodes))
				: [];
		}
	}
}