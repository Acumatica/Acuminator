using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Analyzers.Settings.OutOfProcess;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		private const string AcuminatorTestsName = "Acuminator.Tests";

		private bool _settingsProvidedExternally;

		protected CodeAnalysisSettings CodeAnalysisSettings 
		{ 
			get;
			private set;
		}

		protected static ImmutableArray<string> UnitTestAssemblyMarkers { get; } = 
			new[] 
			{
				"TEST",
				"BENCHMARK"
			}
			.ToImmutableArray();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="codeAnalysisSettings">(Optional) The code analysis settings for unit tests.</param>
		protected PXDiagnosticAnalyzer(CodeAnalysisSettings codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings;
			_settingsProvidedExternally = codeAnalysisSettings != null;
		}
		
		public override void Initialize(AnalysisContext context)
		{
			AcuminatorVsixPackageLoader.EnsurePackageLoaded();

			if (!_settingsProvidedExternally)
				CodeAnalysisSettings = AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisSettings(); //Initialize settings form global values after potential package load

			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
				return;

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(compilationStartContext =>
			{
				var pxContext = new PXContext(compilationStartContext.Compilation, CodeAnalysisSettings);

				if (ShouldAnalyze(pxContext))
				{
					AnalyzeCompilation(compilationStartContext, pxContext);
				}
			});
		}

		protected virtual bool ShouldAnalyze(PXContext pxContext) =>  pxContext.IsPlatformReferenced && 
																	  pxContext.CodeAnalysisSettings.StaticAnalysisEnabled &&
																	  !IsUnitTestAssembly(pxContext.Compilation);

		/// <summary>
		/// Check that compillation is a unit test assembly. The check is implemented by searching for <c>Test</c> word in the assembly name. 
		/// It is a common pattern which on one hand is used almost everywhere and on the other hand allows us to distance from the concrete unit test frameworks
		/// and support not only xUnit but also others like NUnit.
		/// </summary>
		/// <param name="compilation">The compilation.</param>
		/// <returns/>
		protected virtual bool IsUnitTestAssembly(Compilation compilation)
		{
			string assemblyName = compilation?.AssemblyName;

			if (assemblyName.IsNullOrEmpty())
				return false;

			string assemblyNameUpperCase = assemblyName.ToUpperInvariant();

			for (int i = 0; i < UnitTestAssemblyMarkers.Length; i++)
			{
				if (assemblyNameUpperCase.Contains(UnitTestAssemblyMarkers[i]))
					return assemblyName != AcuminatorTestsName;
			}

			return false;
		}

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);
	}
}
