using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	/// <summary>
	/// A base class for suppression code action operations.
	/// </summary>
	internal abstract class SuppressionOperationBase : CodeActionOperation
	{
		protected string AssemblyName { get; }

		protected SuppressionOperationBase(string assemblyName)
		{
			AssemblyName = assemblyName.CheckIfNullOrWhiteSpace(nameof(assemblyName));
		}

		protected void ShowLocalizedError(string resourceName, params string[] formatArgs)
		{		
			var errorMessage = new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources), formatArgs);
			ShowError(errorMessage.ToString());
		}

		protected void ShowError(string errorMessage) =>
			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage}");
	}
}
