using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class KeyFieldDeclarationAnalyzer : PXDiagnosticAnalyzer
	{
		private const string IsKey = "IsKey";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1055_DacKeyFieldBound
			);
		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				AnalyzeDacOrDacExtensionDeclaration(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeDacOrDacExtensionDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (!(syntaxContext.Node is ClassDeclarationSyntax dacOrDacExtNode) || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			INamedTypeSymbol dacOrDacExt = syntaxContext.SemanticModel.GetDeclaredSymbol(dacOrDacExtNode, syntaxContext.CancellationToken);

			if (dacOrDacExt == null || (!dacOrDacExt.IsDAC() && !dacOrDacExt.IsDacExtension()) ||
				syntaxContext.CancellationToken.IsCancellationRequested)
				return;
		}

		private Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExt) || !dacOrDacExt.IsDacOrExtension(pxContext))
				return Task.FromResult(false);

			AttributeInformation attributeInformation = new AttributeInformation(pxContext);

			Task[] allTasks = dacOrDacExt.GetMembers()
				.OfType<IPropertySymbol>()
				.Select(property => CheckDacPropertyAsync(property, symbolContext, pxContext, attributeInformation))
				.ToArray();

			return Task.WhenAll(allTasks);
		}

		private static async Task CheckDacPropertyAsync(IPropertySymbol property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														AttributeInformation attributeInformation)
		{
			ImmutableArray<AttributeData> attributes = property.GetAttributes();

			if (attributes.Length == 0)
				return;

			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var IsBoundField = attributeInformation.ContainsBoundAttributes(attributes.Select(a => a));

			if (IsBoundField == BoundFlag.DbBound)
			{
				foreach (var attribute in attributes)
				{
					foreach(var argument in attribute.NamedArguments)
					{
						if(argument.Key == IsKey &&
							!argument.Value.IsNull &&
							argument.Value.Value is bool boolValue &&
							boolValue == true )
						{
							/*Location attributeLocation = await AttributeInformation.GetAttributeLocationAsync(attribute, symbolContext.CancellationToken);
							
							symbolContext.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.PX1055_DacKeyFieldBound, attributeLocation));
						*/
						}
					}
				}
			}
		}
	}


	/*
	 * public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.PX1055_DacKeyFieldBound.Id);
		
		public override FixAllProvider GetFixAllProvider()
		{
			return base.GetFixAllProvider();
		}
	
	public override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		return Task.Run(() =>
		{
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1055_DacKeyFieldBound.Id);

			if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
				return;

			string codeActionName = nameof(Resources.PX1055)

		}, context.CancellationToken);
	}
	 * */
}
