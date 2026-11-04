#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.VisualStudio.Shell;

using AcuminatorConstants = Acuminator.Vsix.Utilities.Constants;

namespace Acuminator.Vsix
{
	// Part of the Acuminator package responsible for Acuminator options
	public sealed partial class AcuminatorVSPackage : AsyncPackage
	{
		private GeneralOptionsPage? _generalOptionsPage;
		private CodeMapOptionsPage? _codeMapOptionsPage;

		public async ValueTask<GeneralOptionsPage?> GetGeneralOptionsPageAsync()
		{
			var existingOptions = Volatile.Read(ref _generalOptionsPage);

			if (existingOptions != null)
				return existingOptions;

			// Ensure that we are on the UI thread before calling GetDialogPage
			if (!ThreadHelper.CheckAccess())
				await JTF.SwitchToMainThreadAsync();

			if (_generalOptionsPage != null)
				return _generalOptionsPage;

			// We are on main thread here, so we can safely call GetDialogPage to obtain the options page.
			// Also there is no need of thread-safety concerns because only one thread can access this code at a time due to the main thread synchronization.
			_generalOptionsPage = GetDialogPage(typeof(GeneralOptionsPage)) as GeneralOptionsPage;
			return _generalOptionsPage;
		}

		/// <summary>
		/// Attempts to get a cached general options page. Returns null if the page is not available or has not been obtained yet.
		/// </summary>
		/// <returns/>
		public GeneralOptionsPage? TryGetGeneralOptionsPage() => _generalOptionsPage;

		public async ValueTask<CodeMapOptionsPage?> GetCodeMapOptionsPageAsync()
		{
			var existingOptions = Volatile.Read(ref _codeMapOptionsPage);

			if (existingOptions != null)
				return existingOptions;

			// Ensure that we are on the UI thread before calling GetDialogPage
			if (!ThreadHelper.CheckAccess())
				await JTF.SwitchToMainThreadAsync();

			if (_codeMapOptionsPage != null)
				return _codeMapOptionsPage;

			// We are on main thread here, so we can safely call GetDialogPage to obtain the options page.
			// Also there is no need of thread-safety concerns because only one thread can access this code at a time due to the main thread synchronization.
			_codeMapOptionsPage = GetDialogPage(typeof(CodeMapOptionsPage)) as CodeMapOptionsPage;
			return _codeMapOptionsPage;
		}

		/// <summary>
		/// Attempts to get a cached code map options page. Returns null if the page is not available or has not been obtained yet.
		/// </summary>
		/// <returns/>
		public CodeMapOptionsPage? TryGetCodeMapOptionsPage() => _codeMapOptionsPage;

		public async ValueTask<bool> GetColoringEnabledAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.ColoringEnabled ?? AcuminatorConstants.Settings.Coloring.ColoringEnabledDefault;
		}
		public async ValueTask<bool> GetUseBqlOutliningAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.UseBqlOutlining ?? AcuminatorConstants.Settings.Outlining.UseBqlOutliningDefault;
		}

		public async ValueTask<bool> GetUseBqlDetailedOutliningAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.UseBqlDetailedOutlining ?? AcuminatorConstants.Settings.Outlining.UseBqlDetailedOutliningDefault;
		}

		public async ValueTask<bool> GetPXGraphColoringEnabledAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.PXGraphColoringEnabled ?? AcuminatorConstants.Settings.Coloring.PXGraphColoringEnabledDefault;
		}

		public async ValueTask<bool> GetPXActionColoringEnabledAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.PXActionColoringEnabled ?? AcuminatorConstants.Settings.Coloring.PXActionColoringEnabledDefault;
		}

		public async ValueTask<bool> GetColorOnlyInsideBQLAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.ColorOnlyInsideBQL ?? AcuminatorConstants.Settings.Coloring.ColorOnlyInsideBQLDefault;
		}

		public async ValueTask<string?> GetBannedApiFilePathAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.BannedApiFilePath;
		}
		public async ValueTask<string?> GetAllowedApiFilePathAsync()
		{
			var optionsPage = await GetGeneralOptionsPageAsync();
			return optionsPage?.AllowedApiFilePath;
		}

		public async ValueTask<bool> GetExpandRootNodesAsync()
		{
			var codeMapOptionsPage = await GetCodeMapOptionsPageAsync();
			return codeMapOptionsPage?.ExpandRootNodes ?? AcuminatorConstants.Settings.CodeMap.ExpandRootNodesDefault;
		}

		public async ValueTask<bool> GetExpandRegularNodesAsync()
		{
			var codeMapOptionsPage = await GetCodeMapOptionsPageAsync();
			return codeMapOptionsPage?.ExpandRegularNodes ?? AcuminatorConstants.Settings.CodeMap.ExpandRegularNodesDefault;
		}

		public async ValueTask<bool> GetExpandAttributeNodesAsync()
		{
			var codeMapOptionsPage = await GetCodeMapOptionsPageAsync();
			return codeMapOptionsPage?.ExpandAttributeNodes ?? AcuminatorConstants.Settings.CodeMap.ExpandAttributeNodesDefault;
		}
	}
}
