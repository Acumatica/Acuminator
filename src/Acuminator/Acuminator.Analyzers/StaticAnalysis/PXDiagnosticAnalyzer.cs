using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Reflection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		private const string VsixPackageType = "AcuminatorVSPackage";
		private const string ForceLoadPackageAsync = "ForceLoadPackageAsync";

		private static volatile bool _vsixPackageLoadWasDone;
		private static object _acuminatorVsixPackageLoaderLock = new object();

		private const string AcuminatorTestsName = "Acuminator.Tests";

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
		}
		
		public override void Initialize(AnalysisContext context)
		{
			EnsurePackageLoaded();
			CodeAnalysisSettings = CodeAnalysisSettings ?? GlobalCodeAnalysisSettings.Instance;  //Initialize settings form global values after potential package load

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

		/// <summary>
		/// Ensures that package loaded. A hack - the only known way to force the package load due to completely random default loading of packages by Visual Studio 
		/// </summary>
		private static void EnsurePackageLoaded()
		{
			if (!_vsixPackageLoadWasDone)
			{
				lock (_acuminatorVsixPackageLoaderLock)
				{
					if (!_vsixPackageLoadWasDone)
					{
						_vsixPackageLoadWasDone = true;
						SearchForVsixAndEnsureItIsLoadedPackageLoaded();
					}
				}
			}
		}

		/// <summary>
		/// Searches for the Visual Studio vsix package and if found (case when working via VSIX in Visual Studio IDE) ensures that package is loaded.
		/// Calls special method <see cref="ForceLoadPackageAsync"/> to load package provided by AcuminatorVSPackage type.
		/// </summary>
		private static void SearchForVsixAndEnsureItIsLoadedPackageLoaded()
		{	
			var vsixAssembly = AppDomain.CurrentDomain.GetAssemblies()
													  .FirstOrDefault(a => a.GetName().Name == SharedConstants.PackageName);
			if (vsixAssembly == null)
				return;

			var acuminatorPackageType = vsixAssembly.GetExportedTypes().FirstOrDefault(t => t.Name == VsixPackageType);

			if (acuminatorPackageType == null)
				return;

			var dummyServiceCaller = acuminatorPackageType.GetMethod(ForceLoadPackageAsync, BindingFlags.Static | BindingFlags.Public);

			if (dummyServiceCaller == null)
				return;

			object loadTask = null;

			try
			{
				loadTask = dummyServiceCaller.Invoke(null, Array.Empty<object>());
			}
			catch
			{
				return;
			}
			
			if (loadTask is Task task)
			{
				const int defaultTimeoutSeconds = 20;
				task.Wait(TimeSpan.FromSeconds(defaultTimeoutSeconds));
			}
		}
	}
}
