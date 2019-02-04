using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;


namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A roslyn diagnostic service.
	/// </summary>
	internal class RoslynDiagnosticService 
	{
		protected const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _isServiceInitialized = NOT_INITIALIZED;

		private const string RoslynDiagnosticServiceAssemblyName = "Microsoft.CodeAnalysis.Features";
		private const string RoslynDiagnosticServiceTypeName = "IDiagnosticAnalyzerService";
		private const string DiagnosticDataTypeName = "DiagnosticData";
		private const string GetDiagnosticsForSpanAsyncMethodName = "GetDiagnosticsForSpanAsync";


		private readonly Package _package;
		private readonly IComponentModel _componentModel;

		public Type DiagnosticAnalyzerServiceType { get; }

		public Type DiagnosticDataType { get; }

		private readonly object _roslynAnalyzersService;
		private readonly MethodInfo _getDiagnosticOnTextSpanMethod;
		private readonly PropertyInfo _taskResultPropertyInfo;


		private RoslynDiagnosticService(Package package, IComponentModel componentModel)
		{
			package.ThrowOnNull(nameof(package));
			componentModel.ThrowOnNull(nameof(componentModel));
			
			_package = package;
			_componentModel = componentModel;

			DiagnosticAnalyzerServiceType = GetInternalRoslynServiceType();
			DiagnosticDataType = GetDiagnosticDataType();
			DiagnosticAnalyzerServiceType.ThrowOnNull(nameof(DiagnosticAnalyzerServiceType));
			DiagnosticDataType.ThrowOnNull(nameof(DiagnosticDataType));

			_roslynAnalyzersService = GetDiagnosticServiceInstance(componentModel, DiagnosticAnalyzerServiceType);
			_roslynAnalyzersService.ThrowOnNull(nameof(_roslynAnalyzersService));

			_getDiagnosticOnTextSpanMethod = DiagnosticAnalyzerServiceType.GetMethod(GetDiagnosticsForSpanAsyncMethodName);
			_getDiagnosticOnTextSpanMethod.ThrowOnNull(nameof(_getDiagnosticOnTextSpanMethod));

			_taskResultPropertyInfo = GetTaskResultPropertyInfo(DiagnosticDataType);
			_taskResultPropertyInfo.ThrowOnNull(nameof(_taskResultPropertyInfo));
		}

		/// <summary>
		/// Gets the instance of the service.
		/// </summary>
		public static RoslynDiagnosticService Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(Package package)
		{
			if (Interlocked.CompareExchange(ref _isServiceInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				var componentModel = package?.GetService<SComponentModel, IComponentModel>();

				Instance = new RoslynDiagnosticService(package, componentModel);
			}
		}

		private static Type GetInternalRoslynServiceType()
		{
			Type diagnosticAnalyzerServiceType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
												  where assembly.FullName.StartsWith(RoslynDiagnosticServiceAssemblyName)
												  from type in assembly.GetTypes()
												  where type.IsInterface && type.Name == RoslynDiagnosticServiceTypeName
												  select type)
												 .FirstOrDefault();
							  
			return diagnosticAnalyzerServiceType;
		}

		private static Type GetDiagnosticDataType() =>
			(from type in typeof(DocumentId).Assembly.GetTypes()
			 where type.Name == DiagnosticDataTypeName
			 select type)
			.FirstOrDefault();

		private static object GetDiagnosticServiceInstance(IComponentModel componentModel, Type diagnosticAnalyzerServiceType)
		{
			Type componentModelType = componentModel.GetType();
			var getServiceMethodInfo = componentModelType.GetMethod(nameof(componentModel.GetService))
														?.MakeGenericMethod(diagnosticAnalyzerServiceType);
			if (getServiceMethodInfo == null)
				return null;

			try
			{
				return getServiceMethodInfo.Invoke(componentModel, null);
			}
			catch (Exception e)
			{
				return null;
			}
		}

		private static PropertyInfo GetTaskResultPropertyInfo(Type diagnosticDataType)
		{
			Type genericIEnumerableType = typeof(IEnumerable<>).MakeGenericType(diagnosticDataType);

			if (genericIEnumerableType == null)
				return null;

			Type genericTask = typeof(System.Threading.Tasks.Task<>).MakeGenericType(genericIEnumerableType);
			return genericTask?.GetProperty(nameof(System.Threading.Tasks.Task<object>.Result));
		}

		public async System.Threading.Tasks.Task<List<object>> GetCurrentDiagnosticForDocumentSpanAsync(Document document, TextSpan caretSpan)
		{		
			var componentModelType = _componentModel.GetType();
			
			try
			{
				dynamic dataTask = _getDiagnosticOnTextSpanMethod.Invoke(_roslynAnalyzersService, 
																		 new object[] { document, caretSpan, null, false, CancellationToken.None });
				if (!(dataTask is System.Threading.Tasks.Task task))
					return new List<object>();

				await task;
				object diagnosticsCollectionRaw = _taskResultPropertyInfo.GetValue(dataTask);

				if (!(diagnosticsCollectionRaw is IEnumerable<object> diagnostics) || diagnostics.IsNullOrEmpty())
					return new List<object>();

				return diagnostics.ToList();
			}
			catch (Exception exc)
			{
				return new List<object>();
			}
		}
	}
}
