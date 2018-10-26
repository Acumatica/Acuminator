using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;



namespace Acuminator.Vsix.ToolWindows.DacExplorer
{
	/// <summary>
	/// Open AntiPlagiator window command.
	/// </summary>
	internal sealed class OpenDacExplorerWindowCommand : OpenToolWindowCommandBase<DacExplorerWindow>
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0203;

		private OpenDacExplorerWindowCommand(Package package) : base(package, CommandId)
		{		
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static OpenDacExplorerWindowCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(Package package)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new OpenDacExplorerWindowCommand(package);
			}
		}
	}
}
