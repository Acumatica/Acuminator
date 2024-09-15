#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ChangesClassification;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
			_codeMapViewModel = codeMapWindowViewModel.CheckIfNull();
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

			if (changeScope != ChangeInfluenceScope.Attributes || methodNodeBase is not MethodDeclarationSyntax)
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
		/// Get change scope from property-like declaration. For Dac Properties contains more complex logic.
		/// </summary>
		/// <param name="propertyNodeBase">The property node base.</param>
		/// <param name="textChange">The text change.</param>
		/// <param name="containingModeChange">The containing mode change.</param>
		/// <returns/>
		protected override ChangeInfluenceScope? GetChangeScopeFromPropertyBaseSyntaxNode(BasePropertyDeclarationSyntax propertyNodeBase, 
																						  in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			var changeScope = base.GetChangeScopeFromPropertyBaseSyntaxNode(propertyNodeBase, textChange, containingModeChange);

			//We look for changes in DAC property attributes
			if (changeScope != ChangeInfluenceScope.Attributes || propertyNodeBase is not PropertyDeclarationSyntax changedProperty ||
				_codeMapViewModel.DocumentModel?.CodeMapSemanticModels == null)
			{
				return changeScope;
			}

			for (int i = 0; i < _codeMapViewModel.DocumentModel.CodeMapSemanticModels.Count; i++)
			{
				DacSemanticModel? dacSemanticModel = _codeMapViewModel.DocumentModel.CodeMapSemanticModels[i] switch
				{
					DacSemanticModelForCodeMap dacModelForCodeMap => dacModelForCodeMap.DacModel,
					DacSemanticModel dacModel					  => dacModel,
					_ 											  => null
				};

				if (dacSemanticModel == null)
					continue;
				else if (IsPropertyFromDAC(changedProperty, dacSemanticModel))
					return ChangeInfluenceScope.Class;
			}

			return changeScope;
		}

		private bool IsPropertyFromDAC(PropertyDeclarationSyntax changedProperty, DacSemanticModel dacCandidate)
		{
			if (dacCandidate.IsInMetadata)
				return false;

			//basic fast check for bounds and file
			if (!dacCandidate.Node.Span.Contains(changedProperty.Span) || dacCandidate.Node.SyntaxTree.FilePath != changedProperty.SyntaxTree.FilePath)
				return false;

			//Check that declaring type is the same
			if (changedProperty.Parent is not ClassDeclarationSyntax changedDac || changedDac.Identifier.Text != dacCandidate.Node.Identifier.Text)
				return false;

			if (!dacCandidate.PropertiesByNames.TryGetValue(changedProperty.Identifier.Text, out DacPropertyInfo? dacPropertyInfoCandidate))
				return false;

			return dacPropertyInfoCandidate.IsInSource && 
				   Equals(dacPropertyInfoCandidate.Node.ExplicitInterfaceSpecifier, changedProperty.ExplicitInterfaceSpecifier);
		}
	}
}
