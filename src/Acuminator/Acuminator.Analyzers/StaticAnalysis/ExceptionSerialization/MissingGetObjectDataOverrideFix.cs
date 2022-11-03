#nullable enable

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
	public class MissingGetObjectDataOverrideFix : ExceptionSerializationFixBase
	{		
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1064_NoGetObjectDataOverrideInExceptionWithNewFields.Id);

		protected override string GetCodeFixTitle(Diagnostic diagnostic) => 
			nameof(Resources.PX1064Fix).GetLocalized().ToString();

		protected override int FindPositionToInsertGeneratedMember(ClassDeclarationSyntax exceptionDeclaration, SemanticModel semanticModel,
																	PXContext pxContext, CancellationToken cancellationToken)
		{
			int serializationConstructorIndex = FindSerializationConstructorIndex(exceptionDeclaration, semanticModel, pxContext, cancellationToken);
			return serializationConstructorIndex >= 0
				? serializationConstructorIndex + 1
				: -1;
		}

		private int FindSerializationConstructorIndex(ClassDeclarationSyntax exceptionDeclaration, SemanticModel semanticModel,
													  PXContext pxContext, CancellationToken cancellationToken)
		{
			for (int i = 0; i < exceptionDeclaration.Members.Count; i++)
			{
				if (exceptionDeclaration.Members[i] is not ConstructorDeclarationSyntax constructorDeclaration ||
					constructorDeclaration.IsStatic() || constructorDeclaration.ParameterList.Parameters.Count != 2)
				{
					continue;
				}

				var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorDeclaration, cancellationToken);

				if (constructorSymbol?.MethodKind == MethodKind.Constructor && IsMethodUsedForSerialization(constructorSymbol, pxContext))
					return i;
			}

			return -1;
		}

		protected override MemberDeclarationSyntax? GenerateSerializationMemberNode(SyntaxGenerator generator, ClassDeclarationSyntax exceptionDeclaration,
																					PXContext pxContext, Diagnostic diagnostic)
		{
			if (pxContext.Serialization.ReflectionSerializer == null && pxContext.Serialization.PXReflectionSerializer == null)
				return null;

			return GenerateGetObjectDataOverrideNode(generator, pxContext);
		}

		private MethodDeclarationSyntax? GenerateGetObjectDataOverrideNode(SyntaxGenerator generator, PXContext pxContext)
		{
			SyntaxNode[] parameters = GenerateSerializationMemberParameters(generator, pxContext);
			SyntaxNode[] statements =
			{
				GenerateReflectionSerializerMethodCall(generator, DelegateNames.Serialization.ReflectionSerializer_GetObjectData, pxContext),
				GenerateCallToBaseGetObjectData(generator)
			};

			PredefinedTypeSyntax voidReturnType = PredefinedType(Token(SyntaxKind.VoidKeyword));
			var getObjectDataOverride = generator.MethodDeclaration(name: DelegateNames.Serialization.GetObjectData, parameters,
																	typeParameters: null, returnType: voidReturnType,
																	Accessibility.Public, DeclarationModifiers.Override,
																	statements);
			return getObjectDataOverride as MethodDeclarationSyntax;
		}

		private SyntaxNode GenerateCallToBaseGetObjectData(SyntaxGenerator generator)
		{
			SyntaxNode[] arguments =
			{
				generator.Argument(
					IdentifierName(SerializationInfoParameterName)),
				generator.Argument(
					IdentifierName(StreamingContextParameterName))
			};

			return generator.ExpressionStatement
					(
						generator.InvocationExpression(
							generator.MemberAccessExpression(
								generator.BaseExpression(),
								memberName: DelegateNames.Serialization.GetObjectData),
							arguments)
					);
		}

		protected override CompilationUnitSyntax AddMissingUsingDirectives(CompilationUnitSyntax root, Diagnostic diagnostic) =>
			root.AddMissingUsingDirectiveForNamespace(NamespaceNames.DotNetSerializationNamespace)
				.AddMissingUsingDirectiveForNamespace(NamespaceNames.PXCommon);
	}
}
