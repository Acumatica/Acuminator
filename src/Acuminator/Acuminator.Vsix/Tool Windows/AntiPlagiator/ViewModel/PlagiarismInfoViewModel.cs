using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;
using AcumaticaPlagiarism;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.AntiPlagiator
{
	public class PlagiarismInfoViewModel : ViewModelBase
	{
		private const string LocationPrefix = "SourceFile(";
		private const string LocationSuffix = ")";

		private readonly PlagiarismInfo _plagiarismInfo;

		public AntiPlagiatorWindowViewModel ParentViewModel { get; }

		public string Type => _plagiarismInfo.Type.ToString();

		public double Similarity => _plagiarismInfo.Similarity;

		public bool IsThresholdExceeded
		{
			get
			{
				double threshholdFraction = ParentViewModel.ThreshholdPercent / 100.0;
				return Similarity >= threshholdFraction;
			}
		}

		public string ReferenceName => _plagiarismInfo.Reference.Name;

		public string ReferenceLocation { get; }

		public string SourceName => _plagiarismInfo.Source.Name;

		public string SourceLocation { get; }

		public PlagiarismInfoViewModel(AntiPlagiatorWindowViewModel parentViewModel, PlagiarismInfo plagiarismInfo,
									   string referenceSolutionDir, string sourceSolutionDir)
		{
			parentViewModel.ThrowOnNull(nameof(parentViewModel));
			plagiarismInfo.ThrowOnNull(nameof(plagiarismInfo));
			referenceSolutionDir.ThrowOnNullOrWhiteSpace(nameof(referenceSolutionDir));
			sourceSolutionDir.ThrowOnNullOrWhiteSpace(nameof(sourceSolutionDir));

			_plagiarismInfo = plagiarismInfo;
			ParentViewModel = parentViewModel;
			ReferenceLocation = ExtractShortLocation(_plagiarismInfo.Reference.Path, referenceSolutionDir);
			SourceLocation = ExtractShortLocation(_plagiarismInfo.Source.Path, sourceSolutionDir);
		}

		public void OpenLocation(LocationType locationType)
		{
			if (!ThreadHelper.CheckAccess())
				return;

			Index location;

			switch (locationType)
			{
				case LocationType.Reference:
					location = _plagiarismInfo.Reference;
					break;
				case LocationType.Source:
					location = _plagiarismInfo.Source;
					break;
				default:
					return;
			}

			var vsWorkspace = AcuminatorVSPackage.Instance.GetVSWorkspace();

			if (vsWorkspace?.CurrentSolution == null)
				return;

			AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateByLineAndChar(vsWorkspace.CurrentSolution, location.Path,
																			  location.Line, location.Character);
		}

		private string ExtractShortLocation(string location, string solutionDir)
		{
            string preparedLocation = location;

			if (preparedLocation.StartsWith(LocationPrefix))
			{
				preparedLocation = preparedLocation.Substring(LocationPrefix.Length);
			}

			if (preparedLocation.EndsWith(LocationSuffix))
			{
				preparedLocation = preparedLocation.Substring(0, preparedLocation.Length - LocationSuffix.Length);
			}

			if (preparedLocation.StartsWith(solutionDir))
			{
				preparedLocation = preparedLocation.Substring(solutionDir.Length);
			}

			return preparedLocation;
		}
	}
}
