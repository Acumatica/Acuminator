using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A wrapper for the roslyn internal diagnostic data location DTO. 
	/// Because roslyn diagnostic data location dto is immutable and stores mostly public data types we can make snapshot copy from it via dynamic only once.
	/// </summary>
	internal sealed class DiagnosticDataLocation : RoslynDTOWrapperBase<DiagnosticDataLocation>
	{
		public static Type DiagnosticDataLocationType
		{
			get;
			private set;
		}

		private static Dictionary<string, FieldInfo> _publicInternalFields;
		private static Dictionary<string, PropertyInfo> _publicInternalProperties;


		#region DTO properties
		public DocumentId DocumentId { get; }

		/// <summary>
		/// Text can be either given or calculated from original line/column.
		/// </summary>
		public TextSpan? SourceSpan { get; }

		/// <summary>
		/// Null if path is not mapped and <see cref="OriginalFilePath"/> contains the actual path. 
		/// Note that the value might be a relative path. In that case <see cref="OriginalFilePath"/> should be used as a base path for path resolution.
		/// </summary>
		public string MappedFilePath { get; }

		public int MappedStartLine { get; }

		public int MappedStartColumn { get; }

		public int MappedEndLine { get; }

		public int MappedEndColumn { get; }

		public string OriginalFilePath { get; }

		public int OriginalStartLine { get; }

		public int OriginalStartColumn { get; }

		public int OriginalEndLine { get; }

		public int OriginalEndColumn { get; }
		#endregion

		public static DiagnosticDataLocation Create(object roslynLocationDTO)
		{
			roslynLocationDTO.ThrowOnNull(nameof(roslynLocationDTO));

			InitializeDiagnosticDataLocationReflectionInfo(roslynLocationDTO);

			try
			{
				return new DiagnosticData(roslynLocationDTO);
			}	
			catch (Exception e) when (e is MissingMemberException || e is KeyNotFoundException || e is InvalidCastException)
			{
				AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: false, 
																		   Logger.LogMode.Warning);
				return null;
			}
		}


		private DiagnosticDataLocation(object roslynLocationDTO)
		{
			Id = _publicInternalFields[nameof(Id)].GetValue<string>(roslynLocationDTO);
			Category = _publicInternalFields[nameof(Category)].GetValue<string>(roslynLocationDTO); 
			 
			Message = _publicInternalFields[nameof(Message)].GetValue<string>(roslynLocationDTO);
			Description = _publicInternalFields[nameof(Description)].GetValue<string>(roslynLocationDTO);
			Title = _publicInternalFields[nameof(Title)].GetValue<string>(roslynLocationDTO);
			HelpLink = _publicInternalFields[nameof(HelpLink)].GetValue<string>(roslynLocationDTO);

			Severity = _publicInternalFields[nameof(Severity)].GetValue<DiagnosticSeverity>(roslynLocationDTO);
			DefaultSeverity = _publicInternalFields[nameof(DefaultSeverity)].GetValue<DiagnosticSeverity>(roslynLocationDTO);

			IsEnabledByDefault = _publicInternalFields[nameof(IsEnabledByDefault)].GetValue<bool>(roslynLocationDTO);
			WarningLevel = _publicInternalFields[nameof(WarningLevel)].GetValue<int>(roslynLocationDTO);
			CustomTags = _publicInternalFields[nameof(CustomTags)].GetValue<IList<string>>(roslynLocationDTO);

			Properties = _publicInternalFields[nameof(Properties)].GetValue<ImmutableDictionary<string, string>>(roslynLocationDTO);
			IsSuppressed = _publicInternalFields[nameof(IsSuppressed)].GetValue<bool>(roslynLocationDTO);
			Workspace = _publicInternalFields[nameof(Workspace)].GetValue<Workspace>(roslynLocationDTO);
			ProjectId = _publicInternalFields[nameof(ProjectId)].GetValue<ProjectId>(roslynLocationDTO);

			DocumentId = _publicInternalProperties[nameof(DocumentId)].GetValue<DocumentId>(roslynLocationDTO);
		}

		private static void InitializeDiagnosticDataLocationReflectionInfo(object roslynLocationDTO)
		{
			DiagnosticDataLocationType = roslynLocationDTO.GetType();
			var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			_publicInternalFields = DiagnosticDataLocationType.GetFields(bindingFlags).ToDictionary(field => field.Name);
			_publicInternalProperties = DiagnosticDataLocationType.GetProperties(bindingFlags).ToDictionary(property => property.Name);
		}
	}
}
