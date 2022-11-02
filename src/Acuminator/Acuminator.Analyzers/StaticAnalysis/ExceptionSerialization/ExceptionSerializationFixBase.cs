#nullable enable

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.CodeGeneration;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.ExceptionSerialization
{
	public abstract class ExceptionSerializationFixBase : CodeFixProvider
	{		
		protected const string SerializationInfoParameterName = "info";
		protected const string StreamingContextParameterName = "context";

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

			if (diagnostic == null)
				return Task.CompletedTask;

			string addSerializationMemberTitle = GetCodeFixTitle(diagnostic);
			var addSerializationMemberCodeAction =
					CodeAction.Create(addSerializationMemberTitle,
									  cToken => AddMissingExceptionMembersForCorrectSerializationAsync(context.Document, context.Span, diagnostic, cToken),
									  equivalenceKey: addSerializationMemberTitle);

			context.CancellationToken.ThrowIfCancellationRequested();
			context.RegisterCodeFix(addSerializationMemberCodeAction, diagnostic);

			return Task.CompletedTask;
		}

		protected abstract string GetCodeFixTitle(Diagnostic diagnostic);

		protected virtual async Task<Document> AddMissingExceptionMembersForCorrectSerializationAsync(Document document, TextSpan span, Diagnostic diagnostic,
																									  CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var exceptionDeclaration = root?.FindNode(span)
										   ?.ParentOrSelf<ClassDeclarationSyntax>();

			if (exceptionDeclaration == null)
				return document;

			SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null)
				return document;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
			var generatedSerializationMemberDeclaration = GenerateSerializationMemberNode(generator, diagnostic);

			if (generatedSerializationMemberDeclaration == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			int positionToInsert = FindPositionToInsertGeneratedMember(exceptionDeclaration, semanticModel, pxContext, cancellationToken);
			ClassDeclarationSyntax? modifiedExceptionDeclaration = positionToInsert >= 0
				? generator.InsertMembers(exceptionDeclaration, positionToInsert, generatedSerializationMemberDeclaration) as ClassDeclarationSyntax
				: generator.AddMembers(exceptionDeclaration, generatedSerializationMemberDeclaration) as ClassDeclarationSyntax;

			if (modifiedExceptionDeclaration == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();
			var changedRoot = root.ReplaceNode(exceptionDeclaration, modifiedExceptionDeclaration) as CompilationUnitSyntax;

			if (changedRoot == null)
				return document;

			changedRoot = changedRoot.AddMissingUsingDirectiveForNamespace(NamespaceNames.DotNetSerializationNamespace);
			return document.WithSyntaxRoot(changedRoot);
		}

		/// <summary>
		/// Searches for the position to insert the generated exception type member responsible for the serialization.
		/// </summary>
		/// <param name="exceptionDeclaration">The exception declaration.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// The found position to insert the generated type member. <c>-1</c> if no preferrable position is found (member will be added as the last type member).
		/// </returns>
		protected abstract int FindPositionToInsertGeneratedMember(ClassDeclarationSyntax exceptionDeclaration, SemanticModel semanticModel,
																   PXContext pxContext, CancellationToken cancellationToken);

		protected abstract MemberDeclarationSyntax? GenerateSerializationMemberNode(SyntaxGenerator generator, Diagnostic diagnosticProperties);

		protected bool IsMethodUsedForSerialization(IMethodSymbol method, PXContext pxContext) =>
			method.Parameters.Length == 2 &&
			method.Parameters[0]?.Type == pxContext.Serialization.SerializationInfo &&
			method.Parameters[1]?.Type == pxContext.Serialization.StreamingContext;
	}
}
