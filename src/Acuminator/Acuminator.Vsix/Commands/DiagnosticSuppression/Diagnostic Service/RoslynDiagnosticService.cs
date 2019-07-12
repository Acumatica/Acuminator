using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using Shell = Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A roslyn diagnostic service.
	/// </summary>
	internal class RoslynDiagnosticService 
	{
		private static readonly object _locker = new object();

		private const string RoslynDiagnosticServiceAssemblyName = "Microsoft.CodeAnalysis.Features";
		private const string RoslynDiagnosticServiceTypeName = "IDiagnosticAnalyzerService";
		private const string DiagnosticDataTypeName = "DiagnosticData";
		private const string DiagnosticDataLocationTypeName = "DiagnosticDataLocation";
		private const string GetDiagnosticsForSpanAsyncMethodName = "GetDiagnosticsForSpanAsync";

		private readonly IComponentModel _componentModel;

		public Type DiagnosticAnalyzerServiceType { get; }

		public Type DiagnosticDataType { get; }

		public Type DiagnosticDataLocationType { get; }

		private readonly object _roslynAnalyzersService;
		private readonly MethodInfo _getDiagnosticOnTextSpanMethod;
		private readonly PropertyInfo _taskResultPropertyInfo;

		/// <summary>
		/// Cached instance of the service.
		/// </summary>
		private static RoslynDiagnosticService _instance;

		/// <summary>
		/// Create wrapper for Roslyn internal diagnostic service.
		/// </summary>
		/// <param name="componentModel">The component model service.</param>
		public static RoslynDiagnosticService Create(IComponentModel componentModel)
		{
			if (_instance != null)
				return _instance;

			lock (_locker)
			{
				if (_instance != null)
					return _instance;

				try
				{
					_instance = new RoslynDiagnosticService(componentModel);
				}
				catch (Exception)
				{
					_instance = null;
				}

				return _instance;
			}		
		}

		private RoslynDiagnosticService(IComponentModel componentModel)
		{
			componentModel.ThrowOnNull(nameof(componentModel));
			_componentModel = componentModel;

			DiagnosticAnalyzerServiceType = GetInternalRoslynServiceType();

			foreach (Type type in typeof(DocumentId).Assembly.GetTypes())
			{
				if (type.Name == DiagnosticDataTypeName)
					DiagnosticDataType = type;
				else if (type.Name == DiagnosticDataLocationTypeName)
					DiagnosticDataLocationType = type;
			}

			DiagnosticAnalyzerServiceType.ThrowOnNull(nameof(DiagnosticAnalyzerServiceType));
			DiagnosticDataType.ThrowOnNull(nameof(DiagnosticDataType));
			DiagnosticDataLocationType.ThrowOnNull(nameof(DiagnosticDataLocationType));

			_roslynAnalyzersService = GetDiagnosticServiceInstance(componentModel, DiagnosticAnalyzerServiceType);
			_roslynAnalyzersService.ThrowOnNull(nameof(_roslynAnalyzersService));

			_getDiagnosticOnTextSpanMethod = DiagnosticAnalyzerServiceType.GetMethod(GetDiagnosticsForSpanAsyncMethodName);
			_getDiagnosticOnTextSpanMethod.ThrowOnNull(nameof(_getDiagnosticOnTextSpanMethod));

			_taskResultPropertyInfo = GetTaskResultPropertyInfo(DiagnosticDataType);
			_taskResultPropertyInfo.ThrowOnNull(nameof(_taskResultPropertyInfo));
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

			Type genericTask = typeof(Task<>).MakeGenericType(genericIEnumerableType);
			return genericTask?.GetProperty(nameof(Task<object>.Result));
		}

		public async Task<List<DiagnosticData>> GetCurrentDiagnosticForDocumentSpanAsync(Document document, TextSpan caretSpan,
																						 CancellationToken cancellationToken = default)
		{					
			try
			{
				object dataTask = _getDiagnosticOnTextSpanMethod.Invoke(_roslynAnalyzersService, 
																new object[] { document, caretSpan, null, false, cancellationToken });
				if (!(dataTask is Task task))
					return new List<DiagnosticData>();

				await task;
				object rawResult = _taskResultPropertyInfo.GetValue(dataTask);

				if (!(rawResult is IEnumerable<object> diagnosticsRaw) || diagnosticsRaw.IsNullOrEmpty())
					return new List<DiagnosticData>();

				return diagnosticsRaw.Select(rawData => DiagnosticData.Create(rawData))
									 .Where(diagnosticData => diagnosticData != null)
									 .ToList(capacity: 1);
			}
			catch (Exception e)
			{
				return new List<DiagnosticData>();
			}
		}

		public async Task<List<DiagnosticData>> GetCurrentAcuminatorDiagnosticForDocumentSpanAsync(Document document, TextSpan caretSpan,
																								   CancellationToken cancellationToken = default)
		{
			var componentModelType = _componentModel.GetType();

			try
			{
				object dataTask = _getDiagnosticOnTextSpanMethod.Invoke(_roslynAnalyzersService,
																new object[] { document, caretSpan, null, false, cancellationToken });
				if (!(dataTask is Task task))
					return new List<DiagnosticData>();

				await task;
				object rawResult = _taskResultPropertyInfo.GetValue(dataTask);

				if (!(rawResult is IEnumerable<object> diagnosticsRaw) || diagnosticsRaw.IsNullOrEmpty())
					return new List<DiagnosticData>();

				return diagnosticsRaw.Select(rawData => DiagnosticData.Create(rawData))
									 .Where(diagnosticData => diagnosticData != null && IsAcuminatorDiagnostic(diagnosticData))
									 .ToList(capacity: 1);
			}
			catch (Exception e)
			{
				return new List<DiagnosticData>();
			}
		}

		private static bool IsAcuminatorDiagnostic(DiagnosticData diagnosticData) => 
			diagnosticData.Id.StartsWith(SharedConstants.AcuminatorDiagnosticPrefix);
	}
}
