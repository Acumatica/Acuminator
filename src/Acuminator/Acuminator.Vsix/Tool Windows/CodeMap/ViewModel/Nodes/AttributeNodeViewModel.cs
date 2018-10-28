using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class AttributeNodeViewModel : TreeNodeViewModel
	{
		public AttributeData Attribute { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public AttributeNodeViewModel(GraphMemberNodeViewModel graphMemberVM, AttributeData attribute, 
									  bool isExpanded = false) :
								 base(graphMemberVM?.Tree, isExpanded)
		{
			attribute.ThrowOnNull(nameof(attribute));

			Attribute = attribute;
			string attributeName = Attribute.AttributeClass.Name.Split('.').FirstOrDefault();
			Name = $"[{attributeName}]";
		}

		public override void NavigateToItem()
		{
			if (Attribute.ApplicationSyntaxReference?.SyntaxTree == null)
				return;

			TextSpan span = Attribute.ApplicationSyntaxReference.Span;
			string filePath =  Attribute.ApplicationSyntaxReference.SyntaxTree.FilePath;
			Workspace workspace = AcuminatorVSPackage.Instance.GetVSWorkspace();

			if (workspace?.CurrentSolution == null)
				return;

			AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateToPosition(workspace.CurrentSolution, 
																			filePath, span.Start);
		}
	}
}
