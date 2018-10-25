using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using AcumaticaPlagiarism;


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
			ReferenceLocation = ExtractShortLocation(_plagiarismInfo.Reference.Location, referenceSolutionDir);
			SourceLocation = ExtractShortLocation(_plagiarismInfo.Source.Location, sourceSolutionDir);
		}

		private string ExtractShortLocation(FileLinePositionSpan location, string solutionDir)
		{
			string preparedLocation = location.Path;

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
