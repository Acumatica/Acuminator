using System.Xml.Linq;
using System.Xml.Schema;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// Interface for the helper used to get xml schema.
	/// </summary>
	public interface IXmlSchemaProvider
	{
		/// <summary>
		/// Gets XML schema.
		/// </summary>
		XmlSchema GetXmlSchema();
	}
}
