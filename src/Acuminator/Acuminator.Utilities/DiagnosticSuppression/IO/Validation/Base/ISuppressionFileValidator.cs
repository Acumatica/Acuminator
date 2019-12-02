using System.Xml.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// Interface for suppression file validator.
	/// </summary>
	public interface ISuppressionFileValidator
	{
		/// <summary>
		/// Validates the suppression file.
		/// </summary>
		/// <param name="document">The suppression file xml.</param>
		/// <param name="validationLog">The validation log.</param>
		/// <returns/>
		void ValidateSuppressionFile(XDocument document, ValidationLog validationLog);
	}
}
