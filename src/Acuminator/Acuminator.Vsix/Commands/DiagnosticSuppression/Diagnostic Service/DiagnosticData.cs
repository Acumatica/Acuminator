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
		#endregion

		public static DiagnosticData Create(object roslynDTO)
		{
			roslynDTO.ThrowOnNull(nameof(roslynDTO));

			InitializeSharedStaticData(roslynDTO);

			try
			{
				return new DiagnosticData(roslynDTO);
			}	
			catch (Exception e) when (e is MissingMemberException || e is KeyNotFoundException || e is InvalidCastException)
			{
				AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: false, 
																		   Logger.LogMode.Warning);
				return null;
			}
		}

		private DiagnosticData(object roslynDTO)
		{
			Id = DtoFields[nameof(Id)].GetValue<string>(roslynDTO);
			Category = DtoFields[nameof(Category)].GetValue<string>(roslynDTO); 
			 
			Message = DtoFields[nameof(Message)].GetValue<string>(roslynDTO);
			Description = DtoFields[nameof(Description)].GetValue<string>(roslynDTO);
			Title = DtoFields[nameof(Title)].GetValue<string>(roslynDTO);
			HelpLink = DtoFields[nameof(HelpLink)].GetValue<string>(roslynDTO);

			Severity = DtoFields[nameof(Severity)].GetValue<DiagnosticSeverity>(roslynDTO);
			DefaultSeverity = DtoFields[nameof(DefaultSeverity)].GetValue<DiagnosticSeverity>(roslynDTO);

			IsEnabledByDefault = DtoFields[nameof(IsEnabledByDefault)].GetValue<bool>(roslynDTO);
			WarningLevel = DtoFields[nameof(WarningLevel)].GetValue<int>(roslynDTO);
			CustomTags = DtoFields[nameof(CustomTags)].GetValue<IList<string>>(roslynDTO);

			Properties = DtoFields[nameof(Properties)].GetValue<ImmutableDictionary<string, string>>(roslynDTO);
			IsSuppressed = DtoFields[nameof(IsSuppressed)].GetValue<bool>(roslynDTO);
			Workspace = DtoFields[nameof(Workspace)].GetValue<Workspace>(roslynDTO);
			ProjectId = DtoFields[nameof(ProjectId)].GetValue<ProjectId>(roslynDTO);

			DocumentId = DtoProperties[nameof(DocumentId)].GetValue<DocumentId>(roslynDTO);
		}
	}
}
