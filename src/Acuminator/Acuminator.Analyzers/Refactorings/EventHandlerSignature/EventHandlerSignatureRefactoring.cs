using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.Refactorings.EventHandlerSignature
{
	public class EventHandlerSignatureRefactoring : CodeRefactoringProvider
	{
		private const string ArgsParameterName = "e";
		private const string EventHandlerMethodName = "_";

		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			var pxContext = new PXContext(semanticModel.Compilation);

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var methodNode = root?.FindNode(context.Span)?.GetDeclaringMethodNode();

			if (methodNode != null)
			{
				IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodNode, context.CancellationToken);

				if (methodSymbol?.ContainingType?.OriginalDefinition != null 
					&& methodSymbol.ContainingType.OriginalDefinition.IsPXGraphOrExtension(pxContext))
				{
					var eventHandlerInfo = methodSymbol.GetEventHandlerInfo(pxContext);

					if (eventHandlerInfo.eventSignatureType == EventHandlerSignatureType.Default
						&& pxContext.Events.EventHandlerSignatureTypeMap.TryGetValue(
							(eventHandlerInfo.eventType, EventHandlerSignatureType.Generic), out var genericArgsSymbol)
						&& methodSymbol.Name.EndsWith("_" + eventHandlerInfo.eventType, StringComparison.Ordinal))
					{
						string dacName = methodSymbol.Name.Substring(0, methodSymbol.Name.IndexOf("_", StringComparison.Ordinal));

						string title = nameof (Resources.EventHandlerSignatureCodeActionTitle).GetLocalized().ToString();
						context.RegisterRefactoring(CodeAction.Create(title,
							ct => ChangeSignatureAsync(context, root, semanticModel, pxContext, methodNode, 
								eventHandlerInfo.eventType, genericArgsSymbol, dacName, ct), title));
					}
				}
			}
		}

		private async Task<Document> ChangeSignatureAsync(CodeRefactoringContext context, 
			SyntaxNode root, SemanticModel semanticModel,
			PXContext pxContext, MethodDeclarationSyntax methodDeclaration, 
			EventType eventType, INamedTypeSymbol genericArgsSymbol, string dacName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var newParameters = methodDeclaration.ParameterList.Parameters;

			var cacheParameter = newParameters[0];

			if (eventType != EventType.CacheAttached)
			{
				newParameters = newParameters.RemoveAt(0); // "PXCache sender" parameter
			}
			
			var argsParameter = newParameters[0];

			//var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);

			newParameters = newParameters.Replace(argsParameter, CreateArgsParameter(genericArgsSymbol, dacName));

			var newMethodDeclaration = methodDeclaration
				.WithIdentifier(Identifier(EventHandlerMethodName))
				.WithParameterList(methodDeclaration.ParameterList.WithParameters(newParameters));

			return context.Document.WithSyntaxRoot(root.ReplaceNode(methodDeclaration, newMethodDeclaration));
		}

		private ParameterSyntax CreateArgsParameter(INamedTypeSymbol genericArgsSymbol, string dacName)
		{
			return Parameter(
				Identifier(ArgsParameterName))
				.WithType(
					QualifiedName(
						IdentifierName(genericArgsSymbol.ContainingType.Name),
						GenericName(
								Identifier(genericArgsSymbol.Name))
							.WithTypeArgumentList(
								TypeArgumentList(
									SingletonSeparatedList<TypeSyntax>(
										IdentifierName(dacName))))));
		}
	}
}
