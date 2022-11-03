#nullable enable

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn.CodeGeneration;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.ExceptionSerialization
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MissingSerializationConstructorFix : ExceptionSerializationFixBase
	{	
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1063_NoSerializationConstructorInException.Id);

		protected override string GetCodeFixTitle(Diagnostic diagnostic) =>
			nameof(Resources.PX1063Fix).GetLocalized().ToString();

		protected override int FindPositionToInsertGeneratedMember(ClassDeclarationSyntax exceptionDeclaration, SemanticModel semanticModel, 
																   PXContext pxContext, CancellationToken cancellationToken)
		{
			int getObjectDataOverrideIndex = FindGetObjectDataOverrideIndex(exceptionDeclaration, semanticModel, pxContext, cancellationToken);
			return getObjectDataOverrideIndex >= 0
				? getObjectDataOverrideIndex
				: -1;
		}

		private int FindGetObjectDataOverrideIndex(ClassDeclarationSyntax exceptionDeclaration, SemanticModel semanticModel,
												   PXContext pxContext, CancellationToken cancellationToken)
		{
			for (int i = 0; i < exceptionDeclaration.Members.Count; i++)
			{
				if (exceptionDeclaration.Members[i] is not MethodDeclarationSyntax methodDeclaration ||
					methodDeclaration.ParameterList.Parameters.Count != 2 ||
					methodDeclaration.Identifier.Text != DelegateNames.Serialization.GetObjectData)
				{
					continue;
				}

				var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

				if (methodSymbol?.MethodKind == MethodKind.Ordinary && methodSymbol.IsOverride &&
					methodSymbol.DeclaredAccessibility == Accessibility.Public &&
					methodSymbol.Name == DelegateNames.Serialization.GetObjectData &&
					IsMethodUsedForSerialization(methodSymbol, pxContext))
				{
					return i;
				}
			}

			return -1;
		}

		protected override MemberDeclarationSyntax? GenerateSerializationMemberNode(SyntaxGenerator generator, ClassDeclarationSyntax exceptionDeclaration, 
																					PXContext pxContext, Diagnostic diagnostic)
		{
			bool hasNewSerializableData = diagnostic.IsFlagSet(ExceptionSerializationDiagnosticProperties.HasNewDataForSerialization);

			if (hasNewSerializableData)
			{
				if (pxContext.Serialization.ReflectionSerializer == null && pxContext.Serialization.PXReflectionSerializer == null)
					return null;

				return GenerateSerializationConstructorWithNewSerializableDataProcessing(generator, exceptionDeclaration, pxContext);
			}
			else
				return GenerateEmptySerializationConstructor(generator, exceptionDeclaration, pxContext);
		}

		private ConstructorDeclarationSyntax? GenerateSerializationConstructorWithNewSerializableDataProcessing(SyntaxGenerator generator, 
																											ClassDeclarationSyntax exceptionDeclaration,
																											PXContext pxContext)
		{
			SyntaxNode[] statements =
			{
				GenerateReflectionSerializerMethodCall(generator, DelegateNames.Serialization.ReflectionSerializer_RestoreObjectProps, pxContext)
			};

			return GenerateSerializationConstructor(generator, exceptionDeclaration, pxContext, statements);
		}

		private ConstructorDeclarationSyntax? GenerateEmptySerializationConstructor(SyntaxGenerator generator, ClassDeclarationSyntax exceptionDeclaration, 
																					PXContext pxContext)
		{
			return GenerateSerializationConstructor(generator, exceptionDeclaration, pxContext, statements: null);
		}

		private ConstructorDeclarationSyntax? GenerateSerializationConstructor(SyntaxGenerator generator, ClassDeclarationSyntax exceptionDeclaration,
																			   PXContext pxContext, SyntaxNode[]? statements)
		{
			SyntaxNode[] parameters = GenerateSerializationMemberParameters(generator, pxContext);
			SyntaxNode[] baseConstructorArguments =
			{
				generator.Argument(
					IdentifierName(SerializationInfoParameterName)),
				generator.Argument(
					IdentifierName(StreamingContextParameterName))
			};

			var generatedConstructor = generator.ConstructorDeclaration(containingTypeName: exceptionDeclaration.Identifier.Text,
																		parameters, Accessibility.Protected, 
																		baseConstructorArguments: baseConstructorArguments,
																		statements: statements);
			return generatedConstructor as ConstructorDeclarationSyntax;
		}

		protected override CompilationUnitSyntax AddMissingUsingDirectives(CompilationUnitSyntax root, Diagnostic diagnostic)
		{
			var changedRoot = root.AddMissingUsingDirectiveForNamespace(NamespaceNames.DotNetSerializationNamespace);
			bool hasNewSerializableData = diagnostic.IsFlagSet(ExceptionSerializationDiagnosticProperties.HasNewDataForSerialization);

			return hasNewSerializableData
				? changedRoot.AddMissingUsingDirectiveForNamespace(NamespaceNames.PXCommon)
				: changedRoot;
		}
	}
}
