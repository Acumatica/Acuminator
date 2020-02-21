using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected virtual DacNodeViewModel CreateDacNode(DacSemanticModel dacSemanticModel, TreeViewModel tree) =>
			new DacNodeViewModel(dacSemanticModel, tree, ExpandCreatedNodes);

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacNodeViewModel dac)
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
			yield return DacMemberCategory.Keys;
			yield return DacMemberCategory.Property;
			yield return DacMemberCategory.FieldsWithoutProperty;
		}

		protected virtual DacMemberCategoryNodeViewModel CreateCategory(DacNodeViewModel dac, DacMemberCategory dacMemberCategory)
		{
			switch (dacMemberCategory)
			{
				case DacMemberCategory.Keys:
					return new DacKeysCategoryNodeViewModel(dac, ExpandCreatedNodes);

				case DacMemberCategory.Property:
					return new DacPropertiesCategoryNodeViewModel(dac, ExpandCreatedNodes);

				case DacMemberCategory.FieldsWithoutProperty:
				default:
					return null;
			}
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacKeysCategoryNodeViewModel dacKeysCategory)
		{
			dacKeysCategory.ThrowOnNull(nameof(dacKeysCategory));
			return CreateDacMemberCategoryChildren<DacPropertyInfo>(dacKeysCategory,
																	propertyInfo => new PropertyNodeViewModel(dacKeysCategory, propertyInfo, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacPropertiesCategoryNodeViewModel dacPropertiesCategory)
		{
			dacPropertiesCategory.ThrowOnNull(nameof(dacPropertiesCategory));
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

		public override IEnumerable<TreeNodeViewModel> VisitNode(PropertyNodeViewModel property)
		{
			property.ThrowOnNull(nameof(property));
			return property.PropertyInfo.Attributes.Select(a => new AttributeNodeViewModel(property, a.AttributeData, ExpandCreatedNodes));
		}
	}
}