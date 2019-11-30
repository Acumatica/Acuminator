using System.Xml.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// Interface for suppression file validator.
	/// </summary>
	public interface ISuppressionFileValidator
	{
		/// <summary>
		/// The error processor.
		/// </summary>
		IIOErrorProcessor ErrorProcessor { get; }

		/// <summary>
		/// Validates the suppression file.
		/// </summary>
		/// <param name="document">The suppression file xml.</param>
		/// <returns/>
		bool ValidateSuppressionFile(XDocument document);
	}
}
