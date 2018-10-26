using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Threading;
using Acuminator.Utilities.Common;
using ForceGraph;


using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.DacExplorer
{
	public class DacViewModel : ToolWindowViewModelBase
	{
		public DacExplorerViewModel DacExplorerViewModel { get; }

		public string DacName { get; }


		public DacViewModel(DacExplorerViewModel dacExplorerViewModel, string dac)
		{
			DacExplorerViewModel = dacExplorerViewModel;
			DacName = dac;
		}
	}
}
