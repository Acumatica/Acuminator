using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ChangesClassification;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A class for Code Map specific document changes classification. Contains optimizations for code map.
	/// </summary>
	internal class CodeMapDocChangesClassifier : DocumentChangesClassifier
	{
		private readonly CodeMapWindowViewModel _codeMapViewModel;

		public CodeMapDocChangesClassifier(CodeMapWindowViewModel codeMapWindowViewModel)
		{
			_codeMapViewModel = codeMapWindowViewModel.CheckIfNull(nameof(codeMapWindowViewModel));
		}

		public async Task<CodeMapRefreshMode> ShouldRefreshCodeMapAsync(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																		CancellationToken cancellationToken = default)
		{
			ChangeLocationScope changeLocation = await GetChangesLocationAsync(oldDocument, newRoot, newDocument, cancellationToken);

			if (changeLocation.ContainsLocation(ChangeLocationScope.Namespace))
			{
				return newRoot.ContainsDiagnostics
					? CodeMapRefreshMode.Clear
					: CodeMapRefreshMode.Recalculate;
			}
			else if (changeLocation.ContainsLocation(ChangeLocationScope.Class))
			{
				return CodeMapRefreshMode.Recalculate;
			}


			
			return CodeMapRefreshMode.NoRefresh;
		}

		protected override ChangeLocationScope GetChangesLocationImpl(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																 IEnumerable<TextChange> textChanges, CancellationToken cancellationToken = default)
		{
			ChangeLocationScope accumulatedChangeLocation = ChangeLocationScope.None;

			foreach (TextChange change in textChanges)
			{
				ChangeLocationScope changeLocation = GetTextChangeLocation(change, newRoot);

				//Early exit if we found a change which require the refresh of code map 
				if (changeLocation.ContainsLocation(ChangeLocationScope.Class) || changeLocation.ContainsLocation(ChangeLocationScope.Namespace))
					return changeLocation;

				accumulatedChangeLocation = accumulatedChangeLocation | changeLocation;
				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeLocation;
		}

		protected override ChangeLocationScope? GetChangeLocationFromMethodBaseSyntaxNode(BaseMethodDeclarationSyntax methodNodeBase, in TextChange textChange, 
																					 ContainmentModeChange containingModeChange)
		{
			var changeLocation = base.GetChangeLocationFromMethodBaseSyntaxNode(methodNodeBase, textChange, containingModeChange);

			if (changeLocation != ChangeLocationScope.Attributes || !(methodNodeBase is MethodDeclarationSyntax methodDeclaration))
			{
				return changeLocation;
			}

			// In Acumatica one of the most frequent attributes placed on method is the PXOverride attribute. 
			// If there are changes in attributes then there is a high probability that it was change related to PXOverride attribute.
			// There could be a more complex and error-prone attempt to detect that the text change is related exactly to the PXOverride attribute
			// However, currently we do not perform such check because the PXOverride is one of the most frequent cases in Acumatica when the method has attributes. 
			// The situations which may benefit from complex analysis of change (multiple attributes on a method or an attribute different from attribute on a method) are just too rare.
			// On the other hand the amount of work for such analysis is quite significant. 
			// The simple solution is to just refresh code map whenever method attributes are changed, we do not expect that such changes happen frequently.
			
			return ChangeLocationScope.Class;	//Increase change location class
		}

		/// <summary>
		/// Override change location detrction .
		/// </summary>
		/// <param name="propertyNodeBase">The property node base.</param>
		/// <param name="textChange">The text change.</param>
		/// <param name="containingModeChange">The containing mode change.</param>
		/// <returns>
		/// The change location from property base syntax node.
		/// </returns>
		protected override ChangeLocationScope? GetChangeLocationFromPropertyBaseSyntaxNode(BasePropertyDeclarationSyntax propertyNodeBase, 
																					   in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			var changeLocation = base.GetChangeLocationFromPropertyBaseSyntaxNode(propertyNodeBase, textChange, containingModeChange);

			if (changeLocation != ChangeLocationScope.Attributes || !(propertyNodeBase is PropertyDeclarationSyntax property))
				return changeLocation;


		}
	}
}
