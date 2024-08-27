#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.BannedApi.ApiInfoRetrievers;
using Acuminator.Utilities.BannedApi.Providers;
using Acuminator.Utilities.BannedApi.Storage;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BannedApi
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class BannedApiAnalyzer : PXDiagnosticAnalyzer
	{
		private static ApiStorageProvider BannedApiProvider { get; }
		
		private static ApiStorageProvider WhiteListProvider { get; }

		private static IApiDataProvider DefaultBannedApiDataProvider { get; }

		private static IApiDataProvider DefaultWhiteListDataProvider { get; }

		private static readonly bool _isSuccessfullyInitialized;

		static BannedApiAnalyzer()
		{
			try
			{
				BannedApiProvider = new(_bannedApiFileRelativePath, _bannedApiAssemblyResourceName);
				WhiteListProvider = new(_whiteListFileRelativePath, _whiteListAssemblyResourceName);

				BannedApiProvider.GetStorage(CancellationToken.None);

				DefaultBannedApiDataProvider = GetApiBanInfoRetriever(Banned)

				_isSuccessfullyInitialized = true;
			}
			catch (Exception)
			{
				_isSuccessfullyInitialized = false;
			}
		}

		protected static IApiInfoRetriever GetApiBanInfoRetriever(IApiStorage bannedApiStorage) =>
			GetHierarchicalApiInfoRetrieverWithCache(bannedApiStorage);

		protected static IApiInfoRetriever? GetWhiteListInfoRetriever(IApiStorage whiteListStorage) =>
			whiteListStorage.ApiKindsCount > 0
				? GetHierarchicalApiInfoRetrieverWithCache(whiteListStorage)
				: null;

		

		protected static IApiInfoRetriever GetHierarchicalApiInfoRetrieverWithCache(IApiStorage storage) =>
			new ApiInfoRetrieverWithWeakCache(
				new HierarchicalApiBanInfoRetriever(storage));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1099_ForbiddenApiUsage_NoDetails,
				Descriptors.PX1099_ForbiddenApiUsage_WithDetails
			);

		private readonly IApiStorage? _customBannedApi;
		private readonly IApiDataProvider? _customBannedApiDataProvider;

		private readonly IApiStorage? _customWhiteList;
		private readonly IApiDataProvider? _customWhiteListDataProvider;

		private readonly IApiInfoRetriever? _customBanInfoRetriever;
		private readonly IApiInfoRetriever? _customWhiteListInfoRetriever;

		public BannedApiAnalyzer() : this(customBannedApi: null, customBannedApiDataProvider: null,
											 customWhiteList: null, customWhiteListDataProvider: null,
											 customBanInfoRetriever: null, customWhiteListInfoRetriever: null,
											 codeAnalysisSettings: null)
		{
		}

		public BannedApiAnalyzer(IApiStorage? customBannedApi, IApiDataProvider? customBannedApiDataProvider,
									IApiStorage? customWhiteList, IApiDataProvider? customWhiteListDataProvider,
									IApiInfoRetriever? customBanInfoRetriever, IApiInfoRetriever? customWhiteListInfoRetriever,
									CodeAnalysisSettings? codeAnalysisSettings) : 
								base(codeAnalysisSettings)
		{
			_customBannedApi 			  = customBannedApi;
			_customBannedApiDataProvider  = customBannedApiDataProvider;
			_customWhiteList 			  = customWhiteList;
			_customWhiteListDataProvider  = customWhiteListDataProvider;
			_customBanInfoRetriever 	  = customBanInfoRetriever;
			_customWhiteListInfoRetriever = customWhiteListInfoRetriever;
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.CancellationToken.ThrowIfCancellationRequested();

			if (!_isSuccessfullyInitialized)
				return;

			var bannedApiStorage = _customBannedApi ?? GetBannedApiStorage(compilationStartContext.CancellationToken);

			if (bannedApiStorage.ApiKindsCount == 0)
				return;

			compilationStartContext.CancellationToken.ThrowIfCancellationRequested();

			var apiBanInfoRetriever = _customBanInfoRetriever ?? GetApiBanInfoRetriever(bannedApiStorage);

			if (apiBanInfoRetriever == null)
				return;

			var whiteListStorage = _customWhiteList ?? GetWhiteListStorage(compilationStartContext.CancellationToken);
			var whiteListInfoRetriever = _customWhiteListInfoRetriever ?? GetWhiteListInfoRetriever(whiteListStorage);

			compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeSyntaxTree(context, apiBanInfoRetriever, whiteListInfoRetriever),
															 SyntaxKind.CompilationUnit);
		}

		protected override bool ShouldAnalyze(PXContext pxContext) => 
			base.ShouldAnalyze(pxContext) && _isSuccessfullyInitialized;


		private IApiStorage GetCustomBannedApiStorage(CancellationToken cancellation) =>
			BannedApiProvider.GetStorage(cancellation, _customBannedApiDataProvider);

		private IApiStorage GetCustomWhiteListStorage(CancellationToken cancellation) =>
			WhiteListProvider.GetStorage(cancellation, _customWhiteListDataProvider);


		private void AnalyzeSyntaxTree(in SyntaxNodeAnalysisContext syntaxContext, IApiInfoRetriever apiBanInfoRetriever,
									   IApiInfoRetriever? whiteListInfoRetriever)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is CompilationUnitSyntax compilationUnitSyntax)
			{
				var apiNodesWalker = new ApiNodesWalker(syntaxContext, apiBanInfoRetriever, whiteListInfoRetriever, checkInterfaces: false);
				apiNodesWalker.CheckSyntaxTree(compilationUnitSyntax);
			}
		}
	}
}
