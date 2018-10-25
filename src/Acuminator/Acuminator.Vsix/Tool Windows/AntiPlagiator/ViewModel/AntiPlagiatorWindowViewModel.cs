using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using AcumaticaPlagiarism;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;



namespace Acuminator.Vsix.ToolWindows.AntiPlagiator
{
	public class AntiPlagiatorWindowViewModel : ToolWindowViewModelBase
	{
		private CancellationTokenSource _cancellationTokenSource;

		public ExtendedObservableCollection<PlagiarismInfoViewModel> PlagiatedItems { get; }
		 
		private PlagiarismInfoViewModel _selectedItem;

		public PlagiarismInfoViewModel SelectedItem
		{
			get => _selectedItem; 
			set
			{
				if (_selectedItem != value)
				{
					_selectedItem = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool _isAnalysisRunning;

		public bool IsAnalysisRunning
		{
			get => _isAnalysisRunning;
			private set
			{
				if (_isAnalysisRunning != value)
				{
					_isAnalysisRunning = value;
					NotifyPropertyChanged();
				}
			}
		}

        private string _referenceSolutionPath;

        public string ReferenceSolutionPath
        {
            get => _referenceSolutionPath;
            private set
            {
                if (_referenceSolutionPath != value)
                {
                    _referenceSolutionPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

		private double _threshholdPercent = PlagiarismScanner.SimilarityThresholdDefault * 100;

		public double ThreshholdPercent
		{
			get => _threshholdPercent;
			set
			{
				if (_threshholdPercent == value)
					return;
			
				if (value < 0 || value > 100)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "The value should be in range between 0 and 100");
				}

				_threshholdPercent = value;
				NotifyPropertyChanged();

				PlagiatedItems.ForEach(itemVm => itemVm.NotifyPropertyChanged(nameof(itemVm.IsThresholdExceeded)));
			}
		}

		public Command OpenReferenceSolutionCommand { get; }

        public Command RunAnalysisCommand { get; }

		public Command CancelAnalysisCommand { get; }

		public AntiPlagiatorWindowViewModel()
		{
			PlagiatedItems = new ExtendedObservableCollection<PlagiarismInfoViewModel>();

            OpenReferenceSolutionCommand = new Command(p => OpenReferenceSolution());
			RunAnalysisCommand = new Command(p => RunAntiplagiatorAsync());
			CancelAnalysisCommand = new Command(p => CancelAntiplagiator());
		}

		public override void FreeResources()
		{
			base.FreeResources();
			_cancellationTokenSource?.Dispose();
		}

		private void OpenReferenceSolution()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Solution files (*.sln)|*.sln|All files (*.*)|*.*",
				DefaultExt = "sln",
				AddExtension = true,
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = false,
				Title = "Select reference solution file"
			};

			if (openFileDialog.ShowDialog() != true || openFileDialog.FileName.IsNullOrWhiteSpace() || !File.Exists(openFileDialog.FileName))
				return;

			string extension = Path.GetExtension(openFileDialog.FileName);

			if (extension != ".sln")
				return;

			ReferenceSolutionPath = openFileDialog.FileName;
		}

		private async Task RunAntiplagiatorAsync()
		{
			try
			{
				IsAnalysisRunning = true;
				
				using (_cancellationTokenSource = new CancellationTokenSource())
				{
					CancellationToken cancellationToken = _cancellationTokenSource.Token;
					await FillItemsAsync(cancellationToken).ConfigureAwait(false);
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				}
			}
			finally
			{
				IsAnalysisRunning = false;
				_cancellationTokenSource = null;
			}
		}

		private void CancelAntiplagiator()
		{
			if (!IsAnalysisRunning)
				return;

			_cancellationTokenSource?.Cancel();
		}

		private async Task FillItemsAsync(CancellationToken cancellationToken)
		{
			PlagiatedItems.Clear();
			string solutionPath = AcuminatorVSPackage.Instance.GetSolutionPath();

			if (ReferenceSolutionPath.IsNullOrWhiteSpace() || solutionPath.IsNullOrWhiteSpace() || cancellationToken.IsCancellationRequested)
				return;

			await TaskScheduler.Default; //switch to background thread
			string sourceSolutionDir = Path.GetDirectoryName(solutionPath) + Path.DirectorySeparatorChar;
			string referenceSolutionDir = Path.GetDirectoryName(ReferenceSolutionPath) + Path.DirectorySeparatorChar;
			double threshholdFraction = ThreshholdPercent / 100.0;
			PlagiarismScanner plagiarismScanner = new PlagiarismScanner(ReferenceSolutionPath, solutionPath, threshholdFraction);
			var plagiatedItems = plagiarismScanner.Scan(callFromVS: true)
												  .Select(item => new PlagiarismInfoViewModel(this, item, referenceSolutionDir, sourceSolutionDir));
			
			if (cancellationToken.IsCancellationRequested)
				return;

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			PlagiatedItems.AddRange(plagiatedItems);
		}	
	}
}
