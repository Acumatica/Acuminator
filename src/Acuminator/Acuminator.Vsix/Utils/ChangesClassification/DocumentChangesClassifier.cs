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

		public async Task<ChangeInfluenceScope> GetChangesScopeAsync(Document oldDocument, SyntaxNode newRoot, Document newDocument, 
																  CancellationToken cancellationToken = default)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			newRoot.ThrowOnNull(nameof(newRoot));
			newDocument.ThrowOnNull(nameof(newDocument));

			IEnumerable<TextChange> textChanges = await newDocument.GetTextChangesAsync(oldDocument, cancellationToken)
																   .ConfigureAwait(false);
			if (textChanges.IsNullOrEmpty())
				return ChangeInfluenceScope.None;

			return GetChangesScopeImpl(oldDocument, newRoot, newDocument, textChanges, cancellationToken);
		}

		protected virtual ChangeInfluenceScope GetChangesScopeImpl(Document oldDocument, SyntaxNode newRoot, Document newDocument,
																IEnumerable<TextChange> textChanges, 
																CancellationToken cancellationToken = default)
		{
			ChangeInfluenceScope accumulatedChangeScope = ChangeInfluenceScope.None;

			foreach (TextChange change in textChanges)
			{
				ChangeInfluenceScope changeScope = GetTextChangeInfluenceScope(change, newRoot);
				accumulatedChangeScope = accumulatedChangeScope | changeScope;

				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeScope;
		}

		protected virtual ChangeInfluenceScope GetTextChangeInfluenceScope(in TextChange textChange, SyntaxNode newRoot)
		{
			// Performing the same check as FindNode to prevent ArgumentOutOfRange exception. 
			// If check fails then we can't classify changes and assume that they have max possible scope and influence on the code
			if (!newRoot.FullSpan.Contains(textChange.Span)) 
			{
				return ChangeInfluenceScope.Namespace;
			}

			var containingNode = newRoot.FindNode(textChange.Span);

			if (containingNode == null)
				return ChangeInfluenceScope.None;

			while (!containingNode.IsKind(SyntaxKind.CompilationUnit))
			{
				ContainmentModeChange containingModeChange = GetContainingSpanNewContainmentModeForTextChange(textChange, containingNode.Span);
				ChangeInfluenceScope? changesScope = null;

				switch (containingNode)
				{
					case MemberDeclarationSyntax memberDeclaration:
						changesScope = GetChangeScopeFromTypeMemberNode(memberDeclaration, textChange, containingModeChange);
						break;

					case BlockSyntax blockNode:
						changesScope = GetChangeScopeFromBlockNode(blockNode, textChange, containingModeChange);
						break;

					case LocalFunctionStatementSyntax localFunctionDeclaration:
						changesScope = GetChangeScopeFromLocalFunctionDeclaration(localFunctionDeclaration, textChange, containingModeChange);
						break;
					
					case StatementSyntax statementNode:
						changesScope = GetChangeScopeFromStatementNode(statementNode, textChange, containingModeChange);
						break;							
				}
				
				if (changesScope.HasValue)
					return changesScope.Value;

				containingNode = containingNode.Parent;
			}

			return ChangeInfluenceScope.Namespace;
		}

		protected virtual ChangeInfluenceScope? GetChangeScopeFromBlockNode(BlockSyntax blockNode, in TextChange textChange,
																			ContainmentModeChange containingModeChange)
		{
			if (!(blockNode.Parent is MemberDeclarationSyntax))
				return ChangeInfluenceScope.StatementsBlock;

			bool changeContainedOpenBrace = textChange.Span.Contains(blockNode.OpenBraceToken.Span);
			bool changeContainedCloseBrace = textChange.Span.Contains(blockNode.CloseBraceToken.Span);
			int openBracesNewCount = textChange.NewText.Count(c => c == '{');
			int closeBracesNewCount = textChange.NewText.Count(c => c == '}');

			if (changeContainedOpenBrace && openBracesNewCount != 1)
				return ChangeInfluenceScope.Class;
			else if (changeContainedCloseBrace && closeBracesNewCount != 1)
				return ChangeInfluenceScope.Class;
			else
				return ChangeInfluenceScope.StatementsBlock;
		}

		protected virtual ChangeInfluenceScope? GetChangeScopeFromLocalFunctionDeclaration(LocalFunctionStatementSyntax localFunction, in TextChange textChange,
																						  ContainmentModeChange containingModeChange)
		{
			return containingModeChange == ContainmentModeChange.StillContaining
				? ChangeInfluenceScope.StatementsBlock
				: (ChangeInfluenceScope?) null;
		}

		protected virtual ChangeInfluenceScope? GetChangeScopeFromStatementNode(StatementSyntax statementNode, in TextChange textChange,
																			    ContainmentModeChange containingModeChange) =>
			containingModeChange == ContainmentModeChange.StillContaining
				? ChangeInfluenceScope.StatementsBlock
				: (ChangeInfluenceScope?)null;

		protected virtual ChangeInfluenceScope? GetChangeScopeFromTypeMemberNode(MemberDeclarationSyntax memberDeclaration,
																				 in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			switch (memberDeclaration)
			{
				case BaseMethodDeclarationSyntax baseMethodNode:
					return GetChangeScopeFromMethodBaseSyntaxNode(baseMethodNode, textChange, containingModeChange);

				case BasePropertyDeclarationSyntax basePropertyNode:
					return GetChangeScopeFromPropertyBaseSyntaxNode(basePropertyNode, textChange, containingModeChange);

				default:
					return GetChangeScopeFromNodeTrivia(memberDeclaration, textChange) ?? ChangeInfluenceScope.Class;
			}
		}

		protected virtual ChangeInfluenceScope? GetChangeScopeFromMethodBaseSyntaxNode(BaseMethodDeclarationSyntax methodNodeBase,
																					   in TextChange textChange, ContainmentModeChange containingModeChange)
		{
			TextSpan spanToCheck = new TextSpan(textChange.Span.Start, textChange.NewText.Length);	
			TextSpan? methodBodySpan = methodNodeBase.Body?.Span;       //First check body of the property because it is most common place for changes

			if (methodBodySpan == null && methodNodeBase is MethodDeclarationSyntax methodNode)
			{
				methodBodySpan = methodNode.ExpressionBody?.Span;
			}

			ChangeInfluenceScope? changeScope = methodBodySpan == null
					? ChangeInfluenceScope.Class
					: methodBodySpan.Value.Contains(spanToCheck)
						? ChangeInfluenceScope.StatementsBlock
						: (ChangeInfluenceScope?) null;

			if (changeScope.HasValue)
				return changeScope;

			//Now check trivia
			changeScope = GetChangeScopeFromNodeTrivia(methodNodeBase, textChange);

			if (changeScope.HasValue)
				return changeScope;

			//Now check attributes because it is the least frequent case
			if (methodNodeBase.AttributeLists.Span.Contains(spanToCheck))
			{
				return ChangeInfluenceScope.Attributes;
			}

			return ChangeInfluenceScope.Class;
		}
		
		protected virtual ChangeInfluenceScope? GetChangeScopeFromPropertyBaseSyntaxNode(BasePropertyDeclarationSyntax propertyNodeBase, 
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

			ChangeInfluenceScope? changeScope = bodySpan == null
				? ChangeInfluenceScope.Class
				: bodySpan.Value.Contains(spanToCheck)
					? ChangeInfluenceScope.StatementsBlock
					: (ChangeInfluenceScope?) null;

			if (changeScope.HasValue)
				return changeScope;

			//Now check trivia
			changeScope = GetChangeScopeFromNodeTrivia(propertyNodeBase, textChange);

			if (changeScope.HasValue)
				return changeScope;

			if (propertyNodeBase.AttributeLists.Span.Contains(spanToCheck))
			{
				return ChangeInfluenceScope.Attributes;
			}

			return ChangeInfluenceScope.Class;
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

		protected virtual ChangeInfluenceScope? GetChangeScopeFromNodeTrivia(SyntaxNode syntaxNode, in TextChange textChange)
		{
			if (!IsNewLineOrWhitespaceChange(textChange))
				return null;

			ChangeInfluenceScope? changeScope = GetNewLineOrWhitespaceChangeScopeFromTriviaList(syntaxNode.GetLeadingTrivia(), textChange);

			if (changeScope.HasValue)
				return changeScope;

			return GetNewLineOrWhitespaceChangeScopeFromTriviaList(syntaxNode.GetTrailingTrivia(), textChange);
		}

		protected static bool IsNewLineOrWhitespaceChange(in TextChange textChange) =>
			textChange.Span.IsEmpty && (textChange.NewText == Environment.NewLine || textChange.NewText.IsNullOrWhiteSpace());

		protected static ChangeInfluenceScope? GetNewLineOrWhitespaceChangeScopeFromTriviaList(in SyntaxTriviaList triviaList, 
																							   in TextChange newLineOrWhitespaceChange) =>
			triviaList.Span.Contains(newLineOrWhitespaceChange.Span.Start)
				? ChangeInfluenceScope.Trivia
				: (ChangeInfluenceScope?)null;
	}
}