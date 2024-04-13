﻿using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public partial class SuppressionManager
	{
		private class SuppressionFileCreator(SuppressionManager suppressionManager)
		{
			private readonly SuppressionManager _suppressionManager = suppressionManager.CheckIfNull();

			public SuppressionFile CreateSuppressionFileForProjectFromCommand(Project project)
			{
				project.ThrowOnNull();

				//First check if file already exists to dismiss threads withou acquiring the lock
				var existingSuppressionFile =  _suppressionManager.GetSuppressionFile(project.AssemblyName);

				if (existingSuppressionFile != null)
					return existingSuppressionFile;

				lock (_suppressionManager._fileSystemService)
				{
					//Second check inside the lock if file already exists 
					_suppressionManager._fileByAssembly.TryGetValue(project.AssemblyName, out existingSuppressionFile);
					return existingSuppressionFile ?? AddNewSuppressionFileAndApplyChangesToWorkspace(project);
				}
			}

			private SuppressionFile AddNewSuppressionFileAndApplyChangesToWorkspace(Project project)
			{
				TextDocument roslynSuppressionFile = AddAdditionalSuppressionDocumentToProject(project);

				if (roslynSuppressionFile == null || !project.Solution.Workspace.TryApplyChanges(roslynSuppressionFile.Project.Solution))
					return null;

				return Instance.LoadSuppressionFileFrom(roslynSuppressionFile.FilePath);
			}

			public TextDocument AddAdditionalSuppressionDocumentToProject(Project project)
			{
				project.ThrowOnNull();

				string suppressionFileName = project.AssemblyName + SuppressionFile.SuppressionFileExtension;
				string projectDir = Instance._fileSystemService.GetFileDirectory(project.FilePath);
				string suppressionFilePath = Path.Combine(projectDir, suppressionFileName);

				//Create new xml document and get its text
				var newXDocument = SuppressionFile.NewDocumentFromMessages(Enumerable.Empty<SuppressMessage>());

				if (newXDocument == null) //-V3022
					return null;

				string docText = newXDocument.GetXDocumentStringWithDeclaration();

				//Add file to project without applying changes to workspace
				return project.AddAdditionalDocument(suppressionFileName, docText, filePath: suppressionFilePath);
			}
		}
	}
}