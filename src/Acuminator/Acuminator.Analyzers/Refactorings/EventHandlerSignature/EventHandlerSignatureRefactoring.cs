using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
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
	[ExportCodeRefactoringProvider(LanguageNames.CSharp), Shared]
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

				if (IsSuitableForRefactoring(methodSymbol, pxContext))
				{
					var eventHandlerInfo = methodSymbol.GetEventHandlerInfo(pxContext);

					if (eventHandlerInfo.EventSignatureType == EventHandlerSignatureType.Default
						&& pxContext.Events.EventHandlerSignatureTypeMap.TryGetValue(
							(eventHandlerInfo.EventType, EventHandlerSignatureType.Generic), out var genericArgsSymbol)
						&& methodSymbol.Name.EndsWith("_" + eventHandlerInfo.EventType, StringComparison.Ordinal))
					{
						(string dacName, string fieldName) = ParseMethodName(methodSymbol.Name, eventHandlerInfo.EventType);

						string title = nameof (Resources.EventHandlerSignatureCodeActionTitle).GetLocalized().ToString();
						context.RegisterRefactoring(CodeAction.Create(title,
							ct => ChangeSignatureAsync(context, root, semanticModel, methodNode, methodSymbol,
								eventHandlerInfo.EventType, genericArgsSymbol, dacName, fieldName, ct), title));
					}
				}
			}
		}

		private bool IsSuitableForRefactoring(IMethodSymbol symbol, PXContext pxContext)
		{
			return symbol?.ContainingType?.OriginalDefinition != null
			       && symbol.ContainingType.OriginalDefinition.IsPXGraphOrExtension(pxContext)
			       && symbol.Parameters.Length <= 2
			       && !symbol.IsOverride
				   && !symbol.GetAttributes().Any(
					   attr => attr.AttributeClass != null && attr.AttributeClass.Equals(pxContext.AttributeTypes.PXOverrideAttribute));
		}

		private Task<Document> ChangeSignatureAsync(CodeRefactoringContext context, 
			SyntaxNode root, SemanticModel semanticModel,
			MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol,
			EventType eventType, INamedTypeSymbol genericArgsSymbol,
			string dacName, string fieldName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var newParameters = methodDeclaration.ParameterList.Parameters;
			var cacheParameterSymbol = methodSymbol.Parameters[0];
			ParameterSyntax argsParameter;
			SyntaxToken parameterName;

			if (eventType == EventType.CacheAttached)
			{
				parameterName = Identifier(ArgsParameterName);
				argsParameter = newParameters[0];
			}
			else
			{
				newParameters = newParameters.RemoveAt(0); // "PXCache sender" parameter
				argsParameter = newParameters[0];
				parameterName = argsParameter.Identifier;
			}

			newParameters = newParameters.Replace(argsParameter, 
				CreateArgsParameter(genericArgsSymbol, parameterName, dacName, fieldName));

			var newMethodDeclaration = 
				ReplaceParameterUsages(methodDeclaration, cacheParameterSymbol,
					MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
						IdentifierName(parameterName), IdentifierName("Cache")), semanticModel)
				.WithIdentifier(Identifier(EventHandlerMethodName))
				.WithParameterList(methodDeclaration.ParameterList.WithParameters(newParameters));

			root = root.ReplaceNode(methodDeclaration, newMethodDeclaration);

			return Task.FromResult(context.Document.WithSyntaxRoot(root));
		}

		private ParameterSyntax CreateArgsParameter(INamedTypeSymbol genericArgsSymbol, SyntaxToken parameterName, 
			string dacName, string fieldName)
		{
			TypeSyntax identifier;

			if (String.IsNullOrEmpty(fieldName))
				identifier = IdentifierName(dacName);
			else
				identifier = QualifiedName(IdentifierName(dacName), IdentifierName(fieldName));

			return Parameter(parameterName)
				.WithType(
					QualifiedName(
						IdentifierName(genericArgsSymbol.ContainingType.Name),
						GenericName(
								Identifier(genericArgsSymbol.Name))
							.WithTypeArgumentList(
								TypeArgumentList(
									SingletonSeparatedList<TypeSyntax>(
										identifier)))));
		}

		private (string DacName, string FieldName) ParseMethodName(string methodName, EventType eventType)
		{
			var match = Regex.Match(methodName, @"([\w-[_]]+)_(\w+_){0,1}" + eventType,
				RegexOptions.CultureInvariant | RegexOptions.Singleline);

			if (match.Success)
			{
				string dacName = match.Groups[1].Value;
				string fieldName = match.Groups[2].Success ? match.Groups[2].Value.TrimEnd('_') : null;
				if (!String.IsNullOrEmpty(fieldName))
					fieldName = Char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1, fieldName.Length - 1);
				return (dacName, fieldName);
			}

			return (null, null);
		}

		private MethodDeclarationSyntax ReplaceParameterUsages(MethodDeclarationSyntax methodDeclaration, 
			IParameterSymbol parameterSymbol, SyntaxNode replaceWith, SemanticModel semanticModel)
		{
			var rewriter = new ParameterUsagesRewriter(parameterSymbol, replaceWith, semanticModel);
			return (MethodDeclarationSyntax) methodDeclaration.Accept(rewriter);
		}
	}
}
