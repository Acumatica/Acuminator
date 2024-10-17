using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.Settings.OutOfProcess;
using Acuminator.Utilities;
using Acuminator.Utilities.BannedApi.ApiInfoRetrievers;
using Acuminator.Utilities.BannedApi.Providers;
using Acuminator.Utilities.BannedApi.Storage;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BannedApi;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class BannedApiAnalyzer : PXDiagnosticAnalyzer
{
	[MemberNotNullWhen(returnValue: true, nameof(BannedApiSettings))]
	private bool BannedApiSettingsProvidedExternally { get; }

	protected BannedApiSettings? BannedApiSettings { get; set; }

	protected virtual string? SharedMemorySlotName => null;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
		ImmutableArray.Create
		(
			Descriptors.PX1099_ForbiddenApiUsage_WithoutReason,
			Descriptors.PX1099_ForbiddenApiUsage_WithReason
		);

	private readonly IApiStorage? _customBannedApiStorage;
	private readonly IApiDataProvider? _customBannedApiDataProvider;

	private readonly IApiStorage? _customAllowedApiStorage;
	private readonly IApiDataProvider? _customAllowedApiDataProvider;

	private readonly IApiInfoRetriever? _customBanInfoRetriever;
	private readonly IApiInfoRetriever? _customAllowedInfoRetriever;

	public BannedApiAnalyzer() : this(customBannedApiStorage: null, customBannedApiDataProvider: null,
									  customAllowedApiStorage: null, customAllowedApiDataProvider: null,
									  customBanInfoRetriever: null, customAllowedInfoRetriever: null,
									  codeAnalysisSettings: null, bannedApiSettings: null)
	{
	}

	public BannedApiAnalyzer(IApiStorage? customBannedApiStorage, IApiDataProvider? customBannedApiDataProvider,
							 IApiStorage? customAllowedApiStorage, IApiDataProvider? customAllowedApiDataProvider,
							 IApiInfoRetriever? customBanInfoRetriever, IApiInfoRetriever? customAllowedInfoRetriever,
							 CodeAnalysisSettings? codeAnalysisSettings, BannedApiSettings? bannedApiSettings) :
						base(codeAnalysisSettings)
	{
		BannedApiSettings = bannedApiSettings;
		BannedApiSettingsProvidedExternally = BannedApiSettings != null;

		_customBannedApiStorage 	  = customBannedApiStorage;
		_customBannedApiDataProvider  = customBannedApiDataProvider;
		_customAllowedApiStorage 	  = customAllowedApiStorage;
		_customAllowedApiDataProvider = customAllowedApiDataProvider;
		_customBanInfoRetriever 	  = customBanInfoRetriever;
		_customAllowedInfoRetriever	  = customAllowedInfoRetriever;
	}

	[MemberNotNull(nameof(BannedApiSettings))]
	protected override void ReadAcuminatorSettingsFromSharedMemory()
	{
		if (!SettingsProvidedExternally || !BannedApiSettingsProvidedExternally)
		{
			var (externalCodeAnalysisSettings, externalBannedApiSettings) =
			   AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisAndBannedApiSettings(SharedMemorySlotName);

			if (!SettingsProvidedExternally)
				CodeAnalysisSettings = externalCodeAnalysisSettings;

			if (!BannedApiSettingsProvidedExternally)
				BannedApiSettings = externalBannedApiSettings;
		}
	}

	[MemberNotNullWhen(returnValue: true, nameof(BannedApiSettings))]
	protected override bool ShouldRegisterAnalysisActions() => 
		base.ShouldRegisterAnalysisActions() && BannedApiSettings?.BannedApiAnalysisEnabled == true;

	protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
	{
		compilationStartContext.CancellationToken.ThrowIfCancellationRequested();

		var banInfoRetriever = GetApiInfoRetriever(_customBanInfoRetriever, _customBannedApiStorage, _customBannedApiDataProvider,
													BannedApiSettings?.BannedApiFilePath, globalApiDataRetriever: GlobalApiData.GetBannedApiData,
													pxContext.CodeAnalysisSettings, compilationStartContext.CancellationToken);
		if (banInfoRetriever == null)
			return;

		var whiteListInfoRetriever = GetApiInfoRetriever(_customAllowedInfoRetriever, _customAllowedApiStorage, _customAllowedApiDataProvider,
														 BannedApiSettings?.WhiteListApiFilePath, globalApiDataRetriever: GlobalApiData.GetWhiteListApiData,
														 pxContext.CodeAnalysisSettings, compilationStartContext.CancellationToken);

		compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeSyntaxTree(context, pxContext, banInfoRetriever, whiteListInfoRetriever),
														 SyntaxKind.CompilationUnit);
	}

	private static IApiInfoRetriever? GetApiInfoRetriever(IApiInfoRetriever? customApiInfoRetriever, IApiStorage? customApiStorage,
														  IApiDataProvider? customApiDataProvider, string? apiFilePath, 
														  Func<string?, CancellationToken, IApiStorage> globalApiDataRetriever,
														  CodeAnalysisSettings codeAnalysisSettings, CancellationToken cancellation)
	{
		if (customApiInfoRetriever != null)
			return customApiInfoRetriever;

		if (customApiStorage != null)
			return GetHierarchicalApiInfoRetrieverWithCache(customApiStorage, codeAnalysisSettings);

		if (customApiDataProvider != null)
		{
			var customStorageProvider = new ApiStorageProvider(customApiDataProvider);
			var storageFromCustomApiDataProvider = customStorageProvider.GetStorage(cancellation);

			return GetHierarchicalApiInfoRetrieverWithCache(storageFromCustomApiDataProvider, codeAnalysisSettings);
		}

		var apiDataStorageFromGlobalSettings = globalApiDataRetriever(apiFilePath, cancellation);
		return GetHierarchicalApiInfoRetrieverWithCache(apiDataStorageFromGlobalSettings, codeAnalysisSettings);
	}

	protected static IApiInfoRetriever? GetHierarchicalApiInfoRetrieverWithCache(IApiStorage storage, CodeAnalysisSettings codeAnalysisSettings)
	{
		if (storage.ApiKindsCount == 0)
			return null;

		return new ApiInfoRetrieverWithWeakCache(
					new HierarchicalApiBanInfoRetriever(storage, codeAnalysisSettings));
	}

	private void AnalyzeSyntaxTree(in SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
								   IApiInfoRetriever apiBanInfoRetriever, IApiInfoRetriever? whiteListInfoRetriever)
	{
		syntaxContext.CancellationToken.ThrowIfCancellationRequested();

		if (syntaxContext.Node is CompilationUnitSyntax compilationUnitSyntax)
		{
			var apiNodesWalker = new ApiNodesWalker(syntaxContext, pxContext, apiBanInfoRetriever, whiteListInfoRetriever, checkInterfaces: false);
			apiNodesWalker.CheckSyntaxTree(compilationUnitSyntax);
		}
	}
}