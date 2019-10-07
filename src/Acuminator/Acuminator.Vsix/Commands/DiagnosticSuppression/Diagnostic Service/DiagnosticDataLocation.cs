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
using Acuminator.Utilities.Common.Reflection;

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

			InitializeSharedStaticData(roslynLocationDTO);

			try
			{
				return new DiagnosticDataLocation(roslynLocationDTO);
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
			DocumentId          = DtoFields[nameof(DocumentId)].GetValue<DocumentId>(roslynLocationDTO);
			SourceSpan          = DtoFields[nameof(SourceSpan)].GetValue<TextSpan?>(roslynLocationDTO);
			MappedFilePath      = DtoFields[nameof(MappedFilePath)].GetValue<string>(roslynLocationDTO);
			MappedStartLine     = DtoFields[nameof(MappedStartLine)].GetValue<int>(roslynLocationDTO);
			MappedStartColumn   = DtoFields[nameof(MappedStartColumn)].GetValue<int>(roslynLocationDTO);
			MappedEndLine       = DtoFields[nameof(MappedEndLine)].GetValue<int>(roslynLocationDTO);
			MappedEndColumn     = DtoFields[nameof(MappedEndColumn)].GetValue<int>(roslynLocationDTO);
			OriginalFilePath    = DtoFields[nameof(OriginalFilePath)].GetValue<string>(roslynLocationDTO);
			OriginalStartLine   = DtoFields[nameof(OriginalStartLine)].GetValue<int>(roslynLocationDTO);
			OriginalStartColumn = DtoFields[nameof(OriginalStartColumn)].GetValue<int>(roslynLocationDTO);
			OriginalEndLine     = DtoFields[nameof(OriginalEndLine)].GetValue<int>(roslynLocationDTO);
			OriginalEndColumn   = DtoFields[nameof(OriginalEndColumn)].GetValue<int>(roslynLocationDTO);
		}
	}
}
