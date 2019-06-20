using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A wrapper for the roslyn internal diagnostic data DTO. 
	/// Because roslyn diagnostic dto is immutable and stores mostly public data types we can make snapshot copy from it via dynamic only once.
	/// </summary>
	internal sealed class DiagnosticData : RoslynDTOWrapperBase<DiagnosticData>
	{
		public static Type DiagnosticDataType
		{
			get;
			private set;
		}

		#region DTO properties
		public string Id { get; }

		public string Category { get; }

		public string Message { get; }
 
		public string Description { get; }

		public string Title { get; }

		public string HelpLink { get; }

		public DiagnosticSeverity Severity { get; }

		public DiagnosticSeverity DefaultSeverity { get; }

		public bool IsEnabledByDefault { get; }

		public int WarningLevel { get; }

		public IList<string> CustomTags { get; }

		public ImmutableDictionary<string, string> Properties { get; }

		public bool IsSuppressed { get; }

		public Workspace Workspace { get; }

		public ProjectId ProjectId { get; }

		public DocumentId DocumentId { get; }

		public DiagnosticDataLocation DataLocation { get; }
		#endregion

		public static DiagnosticData Create(object roslynDiagnosticDTO)
		{
			roslynDiagnosticDTO.ThrowOnNull(nameof(roslynDiagnosticDTO));

			InitializeSharedStaticData(roslynDiagnosticDTO);

			try
			{
				return new DiagnosticData(roslynDiagnosticDTO);
			}	
			catch (Exception e) when (e is MissingMemberException || e is KeyNotFoundException || e is InvalidCastException)
			{
				AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: false, 
																		   Logger.LogMode.Warning);
				return null;
			}
		}

		private DiagnosticData(object roslynDiagnosticDTO)
		{
			Id = DtoFields[nameof(Id)].GetValue<string>(roslynDiagnosticDTO);
			Category = DtoFields[nameof(Category)].GetValue<string>(roslynDiagnosticDTO); 
			 
			Message = DtoFields[nameof(Message)].GetValue<string>(roslynDiagnosticDTO);
			Description = DtoFields[nameof(Description)].GetValue<string>(roslynDiagnosticDTO);
			Title = DtoFields[nameof(Title)].GetValue<string>(roslynDiagnosticDTO);
			HelpLink = DtoFields[nameof(HelpLink)].GetValue<string>(roslynDiagnosticDTO);

			Severity = DtoFields[nameof(Severity)].GetValue<DiagnosticSeverity>(roslynDiagnosticDTO);
			DefaultSeverity = DtoFields[nameof(DefaultSeverity)].GetValue<DiagnosticSeverity>(roslynDiagnosticDTO);

			IsEnabledByDefault = DtoFields[nameof(IsEnabledByDefault)].GetValue<bool>(roslynDiagnosticDTO);
			WarningLevel = DtoFields[nameof(WarningLevel)].GetValue<int>(roslynDiagnosticDTO);
			CustomTags = DtoFields[nameof(CustomTags)].GetValue<IList<string>>(roslynDiagnosticDTO);

			Properties = DtoFields[nameof(Properties)].GetValue<ImmutableDictionary<string, string>>(roslynDiagnosticDTO);
			IsSuppressed = DtoFields[nameof(IsSuppressed)].GetValue<bool>(roslynDiagnosticDTO);
			Workspace = DtoFields[nameof(Workspace)].GetValue<Workspace>(roslynDiagnosticDTO);
			ProjectId = DtoFields[nameof(ProjectId)].GetValue<ProjectId>(roslynDiagnosticDTO);
			DocumentId = DtoProperties[nameof(DocumentId)].GetValue<DocumentId>(roslynDiagnosticDTO);

			DataLocation = GetDiagnosticLocationWrapper(roslynDiagnosticDTO);
		}

		private DiagnosticDataLocation GetDiagnosticLocationWrapper(object roslynDiagnosticDTO)
		{
			object roslynDiagnosticLocationDTO = DtoFields[nameof(DataLocation)].GetValue(roslynDiagnosticDTO);
			return roslynDiagnosticLocationDTO != null
				? DiagnosticDataLocation.Create(roslynDiagnosticLocationDTO)
				: null;
		}
	}
}
