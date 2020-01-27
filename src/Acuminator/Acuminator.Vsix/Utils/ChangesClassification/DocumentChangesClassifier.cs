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



namespace Acuminator.Vsix.ChangesClassification
{
	/// <summary>
	/// A base class for document changes classification.
	/// </summary>
	public class DocumentChangesClassifier
	{
		/// <summary>
		/// Values that represent containment mode changes for a <see cref="TextSpan"/> which was containing another <see cref="TextSpan"/> before changes.
		/// </summary>
		protected enum ContainmentModeChange : byte
		{
			StillContaining,
			NotContaining,		
		} 

		public async Task<ChangeLocationScope> GetChangesLocationAsync(Document oldDocument, SyntaxNode newRoot, Document newDocument, 
																  CancellationToken cancellationToken = default)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			newRoot.ThrowOnNull(nameof(newRoot));
			newDocument.ThrowOnNull(nameof(newDocument));

			IEnumerable<TextChange> textChanges = await newDocument.GetTextChangesAsync(oldDocument, cancellationToken)
																   .ConfigureAwait(false);
			if (textChanges.IsNullOrEmpty())
				return ChangeLocationScope.None;

			return GetChangesLocationImpl(oldDocument, newRoot, newDocument, textChanges, cancellationToken);
		}

		protected virtual ChangeLocationScope GetChangesLocationImpl(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																IEnumerable<TextChange> textChanges, 
																CancellationToken cancellationToken = default)
		{
			ChangeLocationScope accumulatedChangeLocation = ChangeLocationScope.None;

			foreach (TextChange change in textChanges)
			{
				ChangeLocationScope changeLocation = GetTextChangeLocation(change, newRoot);
				accumulatedChangeLocation = accumulatedChangeLocation | changeLocation;

				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeLocation;
		}

		protected virtual ChangeLocationScope GetTextChangeLocation(in TextChange textChange, SyntaxNode newRoot)
		{
			// Performing the same check as FindNode to prevent ArgumentOutOfRange exception. 
			// If check fails then we can't classify changes and assume that they have max possible scope and influence on the code
			if (!newRoot.FullSpan.Contains(textChange.Span)) 
			{
				return ChangeLocationScope.Namespace;
			}

			var containingNode = newRoot.FindNode(textChange.Span);

			if (containingNode == null)
				return ChangeLocationScope.None;

			while (!containingNode.IsKind(SyntaxKind.CompilationUnit))
			{
				ContainmentModeChange containingModeChange = GetContainingSpanNewContainmentModeForTextChange(textChange, containingNode.Span);
				ChangeLocationScope? changesLocation = null;

				switch (containingNode)
				{
					case BlockSyntax blockNode:
						changesLocation = GetChangeLocationFromBlockNode(blockNode, textChange, containingModeChange);
						break;			

					//TODO need to add check for local function declaration node here after Roslyn upgrade
					
					case StatementSyntax statementNode:
						changesLocation = GetChangeLocationFromStatementNode(statementNode, textChange, containingModeChange);
						break;				
					case MemberDeclarationSyntax memberDeclaration:
						changesLocation = GetChangeLocationFromTypeMemberNode(memberDeclaration, textChange, containingModeChange);
						break;			
				}
				
				if (changesLocation.HasValue)
					return changesLocation.Value;

				containingNode = containingNode.Parent;
			}

			return ChangeLocationScope.Namespace;
		}

		protected virtual ChangeLocationScope? GetChangeLocationFromBlockNode(BlockSyntax blockNode, in TextChange textChange,
																		 ContainmentModeChange containingModeChange)
		{
			if (!(blockNode.Parent is MemberDeclarationSyntax))
				return ChangeLocationScope.StatementsBlock;

			bool changeContainedOpenBrace = textChange.Span.Contains(blockNode.OpenBraceToken.Span);
			bool changeContainedCloseBrace = textChange.Span.Contains(blockNode.CloseBraceToken.Span);
			int openBracesNewCount = textChange.NewText.Count(c => c == '{');
			int closeBracesNewCount = textChange.NewText.Count(c => c == '}');

			if (changeContainedOpenBrace && openBracesNewCount != 1)
				return ChangeLocationScope.Class;
			else if (changeContainedCloseBrace && closeBracesNewCount != 1)
				return ChangeLocationScope.Class;
			else
				return ChangeLocationScope.StatementsBlock;
		}

		protected virtual ChangeLocationScope? GetChangeLocationFromStatementNode(StatementSyntax statementNode, in TextChange textChange,
																			 ContainmentModeChange containingModeChange) =>
			containingModeChange == ContainmentModeChange.StillContaining
				? ChangeLocationScope.StatementsBlock
				: (ChangeLocationScope?)null;

		protected virtual ChangeLocationScope? GetChangeLocationFromTypeMemberNode(MemberDeclarationSyntax memberDeclaration,
																			  in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			switch (memberDeclaration)
			{
				case BaseMethodDeclarationSyntax baseMethodNode:
					return GetChangeLocationFromMethodBaseSyntaxNode(baseMethodNode, textChange, containingModeChange);

				case BasePropertyDeclarationSyntax basePropertyNode:
					return GetChangeLocationFromPropertyBaseSyntaxNode(basePropertyNode, textChange, containingModeChange);

				default:
					return GetChangeLocationFromNodeTrivia(memberDeclaration, textChange) ?? ChangeLocationScope.Class;
			}
		}

		protected virtual ChangeLocationScope? GetChangeLocationFromMethodBaseSyntaxNode(BaseMethodDeclarationSyntax methodNodeBase,
																					in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			TextSpan spanToCheck = new TextSpan(textChange.Span.Start, textChange.NewText.Length);	
			TextSpan? methodBodySpan = methodNodeBase.Body?.Span;       //First check body of the property because it is most common place for changes

			if (methodBodySpan == null && methodNodeBase is MethodDeclarationSyntax methodNode)
			{
				methodBodySpan = methodNode.ExpressionBody?.Span;
			}

			ChangeLocationScope? changeLocation = methodBodySpan == null
					? ChangeLocationScope.Class
					: methodBodySpan.Value.Contains(spanToCheck)
						? ChangeLocationScope.StatementsBlock
						: (ChangeLocationScope?) null;

			if (changeLocation.HasValue)
				return changeLocation;

			//Now check trivia
			changeLocation = GetChangeLocationFromNodeTrivia(methodNodeBase, textChange);

			if (changeLocation.HasValue)
				return changeLocation;

			//Now check attributes because it is the least frequent case
			if (methodNodeBase.AttributeLists.Span.Contains(spanToCheck))
			{
				return ChangeLocationScope.Attributes;
			}

			return ChangeLocationScope.Class;
		}

		protected virtual ChangeLocationScope? GetChangeLocationFromPropertyBaseSyntaxNode(BasePropertyDeclarationSyntax propertyNodeBase, 
																					  in TextChange textChange, ContainmentModeChange containingModeChange)
		{	
			TextSpan spanToCheck = new TextSpan(textChange.Span.Start, textChange.NewText.Length);		
			TextSpan? bodySpan = propertyNodeBase.AccessorList?.Span;       //First check body of the property because it is most common place for changes

			if (bodySpan == null)
			{
				switch (propertyNodeBase)
				{
					case PropertyDeclarationSyntax property:
						bodySpan = property.ExpressionBody?.Span;
						break;
					case IndexerDeclarationSyntax indexer:
						bodySpan = indexer.ExpressionBody?.Span;
						break;
				}
			}

			ChangeLocationScope? changeLocation = bodySpan == null
				? ChangeLocationScope.Class
				: bodySpan.Value.Contains(spanToCheck)
					? ChangeLocationScope.StatementsBlock
					: (ChangeLocationScope?) null;

			if (changeLocation.HasValue)
				return changeLocation;

			//Now check trivia
			changeLocation = GetChangeLocationFromNodeTrivia(propertyNodeBase, textChange);

			if (changeLocation.HasValue)
				return changeLocation;

			if (propertyNodeBase.AttributeLists.Span.Contains(spanToCheck))
			{
				return ChangeLocationScope.Attributes;
			}

			return ChangeLocationScope.Class;
		}

		protected ContainmentModeChange GetContainingSpanNewContainmentModeForTextChange(in TextChange textChange, in TextSpan existingContainingSpan)
		{
			var lengthFromChangeStart = existingContainingSpan.End - textChange.Span.Start;

			return textChange.NewText.Length == lengthFromChangeStart
				? ContainmentModeChange.StillContaining
				: lengthFromChangeStart < textChange.NewText.Length
						? ContainmentModeChange.NotContaining
						: ContainmentModeChange.StillContaining;
		}	

		protected virtual ChangeLocationScope? GetChangeLocationFromNodeTrivia(SyntaxNode syntaxNode, in TextChange textChange)
		{
			if (!IsNewLineOrWhitespaceChange(textChange))
				return null;

			ChangeLocationScope? changeLocation = GetNewLineOrWhitespaceChangeLocationFromTriviaList(syntaxNode.GetLeadingTrivia(), textChange);

			if (changeLocation.HasValue)
				return changeLocation;

			return GetNewLineOrWhitespaceChangeLocationFromTriviaList(syntaxNode.GetTrailingTrivia(), textChange);
		}

		protected static bool IsNewLineOrWhitespaceChange(in TextChange textChange) =>
			textChange.Span.IsEmpty && (textChange.NewText == Environment.NewLine || textChange.NewText.IsNullOrWhiteSpace());

		protected static ChangeLocationScope? GetNewLineOrWhitespaceChangeLocationFromTriviaList(in SyntaxTriviaList triviaList, 
																							in TextChange newLineOrWhitespaceChange) =>
			triviaList.Span.Contains(newLineOrWhitespaceChange.Span.Start)
				? ChangeLocationScope.Trivia
				: (ChangeLocationScope?)null;
	}
}