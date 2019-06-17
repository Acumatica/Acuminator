using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A generator of supression file for project.
	/// </summary>
	public static class ProjectSuppressionFileGenerator
	{
		private const string CustomSuppresionFileSuffix = "Custom";

		public static void GenerateSuppressionFile(Project project, string customFilePath = null)
		{
			project.ThrowOnNull(nameof(project));
			project.FilePath.ThrowOnNullOrWhiteSpace($"{nameof(project)}.{nameof(project.FilePath)}");

			string suppressionFilePath = GetSuppressionFilePath(project, customFilePath);
			TextDocument suppressionFile = project.AdditionalDocuments
												  .FirstOrDefault(doc => doc.FilePath == suppressionFilePath);

			if (suppressionFile != null)
				return;


		}

		private static string GetSuppressionFilePath(Project project, string customFilePath)
		{
			if (!customFilePath.IsNullOrWhiteSpace())
			{
				return ValidateCustomFilePath(customFilePath)
					? Path.GetFullPath(customFilePath)
					: throw new ArgumentException("Invalid custom supression file path", nameof(customFilePath));
			}
			else
			{
				string projectDir = Path.GetDirectoryName(project.FilePath);
				string defaultFileName = $"{project.Name}.{CustomSuppresionFileSuffix}{SuppressionFile.SuppressionFileExtension}";
				return Path.Combine(projectDir, defaultFileName);
			}
		}

		private static bool ValidateCustomFilePath(string filePath)
		{
			try
			{
				string fullPath = Path.GetFullPath(filePath);
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (PathTooLongException)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}
			catch (System.Security.SecurityException)
			{
				return false;
			}

			return true;
		}
	}
}
