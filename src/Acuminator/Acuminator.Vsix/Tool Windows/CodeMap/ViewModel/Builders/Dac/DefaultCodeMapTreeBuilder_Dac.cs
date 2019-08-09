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
		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacNodeViewModel dac, bool expandChildren, CancellationToken cancellation)
		{
			return base.VisitNodeAndBuildChildren(dac, expandChildren, cancellation);
		}

		protected virtual IEnumerable<DacMemberCategory> GetDacMemberCategoriesInOrder()
		{
			yield return DacMemberCategory.Keys;
			yield return DacMemberCategory.Property;
			yield return DacMemberCategory.FieldsWithoutProperty;
		}


	}
}