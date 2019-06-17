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
		private const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _areStaticMembersInitialized = NOT_INITIALIZED;

		public static Type DiagnosticDataType
		{
			get;
			private set;
		}

		private static Dictionary<string, FieldInfo> _publicInternalFields;
		private static Dictionary<string, PropertyInfo> _publicInternalProperties;


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

			InitializeStaticMembers(roslynDTO);

			try
			{
				return new DiagnosticData(roslynDTO);
			}	
			catch (MissingMemberException reflectionException)
			{
				return null;
			}
			catch(KeyNotFoundException missingInfoException)
			{
				return null;
			}
			catch (InvalidCastException wrongDataTypeException)
			{
				return null;
			}
		}

		private DiagnosticData(object roslynDTO)
		{
			Id = _publicInternalFields[nameof(Id)].GetValue<string>(roslynDTO);
			Category = _publicInternalFields[nameof(Category)].GetValue<string>(roslynDTO); 
			 
			Message = _publicInternalFields[nameof(Message)].GetValue<string>(roslynDTO);
			Description = _publicInternalFields[nameof(Description)].GetValue<string>(roslynDTO);
			Title = _publicInternalFields[nameof(Title)].GetValue<string>(roslynDTO);
			HelpLink = _publicInternalFields[nameof(HelpLink)].GetValue<string>(roslynDTO);

			Severity = _publicInternalFields[nameof(Severity)].GetValue<DiagnosticSeverity>(roslynDTO);
			DefaultSeverity = _publicInternalFields[nameof(DefaultSeverity)].GetValue<DiagnosticSeverity>(roslynDTO);

			IsEnabledByDefault = _publicInternalFields[nameof(IsEnabledByDefault)].GetValue<bool>(roslynDTO);
			WarningLevel = _publicInternalFields[nameof(WarningLevel)].GetValue<int>(roslynDTO);
			CustomTags = _publicInternalFields[nameof(CustomTags)].GetValue<IList<string>>(roslynDTO);

			Properties = _publicInternalFields[nameof(Properties)].GetValue<ImmutableDictionary<string, string>>(roslynDTO);
			IsSuppressed = _publicInternalFields[nameof(IsSuppressed)].GetValue<bool>(roslynDTO);
			Workspace = _publicInternalFields[nameof(Workspace)].GetValue<Workspace>(roslynDTO);
			ProjectId = _publicInternalFields[nameof(ProjectId)].GetValue<ProjectId>(roslynDTO);

			DocumentId = _publicInternalProperties[nameof(DocumentId)].GetValue<DocumentId>(roslynDTO);
		}

		private static void InitializeStaticMembers(object roslynDTO)
		{
			if (Interlocked.CompareExchange(ref _areStaticMembersInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				DiagnosticDataType = roslynDTO.GetType();
				var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

				_publicInternalFields = DiagnosticDataType.GetFields(bindingFlags)
														  .ToDictionary(field => field.Name);

				_publicInternalProperties = DiagnosticDataType.GetProperties(bindingFlags)
															  .ToDictionary(property => property.Name);
			}
		}
	}
}
