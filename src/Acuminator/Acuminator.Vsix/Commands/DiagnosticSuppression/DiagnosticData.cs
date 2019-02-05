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


using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;


namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A wrapper for the roslyn internal diagnostic data DTO. 
	/// Because roslyn diagnostic dto is immutable and stores mostly public data types we can make snapshot copy from it via dynamic only once.
	/// </summary>
	internal sealed class DiagnosticData
	{
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

		private DiagnosticData(dynamic roslynDTO)
		{
			Id = roslynDTO.Id;
			Category = roslynDTO.Category;
			Message = roslynDTO.Message;
			Description = roslynDTO.Description;
			Title = roslynDTO.Title;
			HelpLink = roslynDTO.HelpLink;
			Severity = roslynDTO.Severity;
			DefaultSeverity = roslynDTO.DefaultSeverity;
			IsEnabledByDefault = roslynDTO.IsEnabledByDefault;
			WarningLevel = roslynDTO.WarningLevel;
			CustomTags = roslynDTO.CustomTags;
			Properties = roslynDTO.Properties;
			IsSuppressed = roslynDTO.IsSuppressed;
			Workspace = roslynDTO.Workspace;
			ProjectId = roslynDTO.ProjectId;
			DocumentId = roslynDTO.DocumentId;
		}

		public static DiagnosticData Create(dynamic roslynDTO)
		{
			roslynDTO.ThrowOnNull(nameof(roslynDTO));

			try
			{
				return new DiagnosticData(roslynDTO);
			}
			catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException binderException)
			{
				return null;
			}
			
		}
	}
}
