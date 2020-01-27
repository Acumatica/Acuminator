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
			ChangeInfluenceScope changeScope = await GetChangesScopeAsync(oldDocument, newRoot, newDocument, cancellationToken);

			if (changeScope.ContainsLocation(ChangeInfluenceScope.Namespace))
			{
				return newRoot.ContainsDiagnostics
					? CodeMapRefreshMode.Clear
					: CodeMapRefreshMode.Recalculate;
			}
			else if (changeScope.ContainsLocation(ChangeInfluenceScope.Class))
			{
				return CodeMapRefreshMode.Recalculate;
			}

			return CodeMapRefreshMode.NoRefresh;
		}

		protected override ChangeInfluenceScope GetChangesScopeImpl(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																	IEnumerable<TextChange> textChanges, CancellationToken cancellationToken = default)
		{
			ChangeInfluenceScope accumulatedChangeScope = ChangeInfluenceScope.None;

			foreach (TextChange change in textChanges)
			{
				ChangeInfluenceScope changeScope = GetTextChangeInfluenceScope(change, newRoot);

				//Early exit if we found a change which require the refresh of code map 
				if (changeScope.ContainsLocation(ChangeInfluenceScope.Class) || changeScope.ContainsLocation(ChangeInfluenceScope.Namespace))
					return changeScope;

				accumulatedChangeScope = accumulatedChangeScope | changeScope;
				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeScope;
		}

		protected override ChangeInfluenceScope? GetChangeScopeFromMethodBaseSyntaxNode(BaseMethodDeclarationSyntax methodNodeBase, in TextChange textChange, 
																						ContainmentModeChange containingModeChange)
		{
			var changeScope = base.GetChangeScopeFromMethodBaseSyntaxNode(methodNodeBase, textChange, containingModeChange);

			if (changeScope != ChangeInfluenceScope.Attributes || !(methodNodeBase is MethodDeclarationSyntax))
			{
				return changeScope;
			}

			// In Acumatica one of the most frequent attributes placed on method is the PXOverride attribute. 
			// If there are changes in attributes then there is a high probability that it was change related to PXOverride attribute.
			// There could be a more complex and error-prone attempt to detect that the text change is related exactly to the PXOverride attribute
			// However, currently we do not perform such check because the PXOverride is one of the most frequent cases in Acumatica when the method has attributes. 
			// The situations which may benefit from complex analysis of change (multiple attributes on a method or an attribute different from attribute on a method) are just too rare.
			// On the other hand the amount of work for such analysis is quite significant. 
			// The simple solution is to just refresh code map whenever method attributes are changed, we do not expect that such changes happen frequently.
			
			return ChangeInfluenceScope.Class;	//Increase change location class
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
		protected override ChangeInfluenceScope? GetChangeScopeFromPropertyBaseSyntaxNode(BasePropertyDeclarationSyntax propertyNodeBase, 
																					   in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			var changeScope = base.GetChangeScopeFromPropertyBaseSyntaxNode(propertyNodeBase, textChange, containingModeChange);

			if (changeScope != ChangeInfluenceScope.Attributes || !(propertyNodeBase is PropertyDeclarationSyntax property))
				return changeScope;


		}
	}
}
