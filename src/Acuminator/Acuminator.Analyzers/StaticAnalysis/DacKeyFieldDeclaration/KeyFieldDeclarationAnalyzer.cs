using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacDeclaration
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class KeyFieldDeclarationAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1055_DacKeyFieldBound
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(async symbolContext => await AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.NamedType);
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
