using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Acuminator.Utilities.DiagnosticSuppression.BuildAction
{
	/// <summary>
	/// Interface for custom build action setter for new suppression file.
	/// </summary>
	public interface ICustomBuildActionSetter
	{
		Task<bool> SetBuildActionAsync(string roslynSuppressionFilePath);
	}
}
