using System;
using System.Text;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using Acuminator.Utilities.Common;
using System.IO;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// Suppression file validator by XML schema.
	/// </summary>
	public class SuppressionFileSchemaValidator : ISuppressionFileValidator
	{
		/// <summary>
		/// The XML schema.
		/// </summary>
		public XmlSchema Schema { get; }

		private readonly XmlSchemaSet _xmlSchemaSet;

		protected SuppressionFileSchemaValidator(XmlSchema schema)
		{
			Schema = schema.CheckIfNull(nameof(schema));
			_xmlSchemaSet = new XmlSchemaSet();
			_xmlSchemaSet.Add(Schema);
		}

		public static SuppressionFileSchemaValidator Create(IXmlSchemaProvider customXmlSchemaProvider = null, 
															IIOErrorProcessor errorProcessor = null)
		{
			IXmlSchemaProvider xmlSchemaProvider = customXmlSchemaProvider ?? new EmbeddedResourceXmlSchemaProvider();
			XmlSchema schema;

			try
			{
				schema = xmlSchemaProvider.GetXmlSchema();
			}
			catch (Exception e)
			{
				schema = null;
				errorProcessor?.ProcessError(e);
			}
			
			if (schema == null)
			{
				Exception exception = new Exception("Failed to load schema for the suppression file. The suppression file won't be validated");
				errorProcessor?.ProcessError(exception);
				return null;
			}

			return new SuppressionFileSchemaValidator(schema);
		}

		/// <summary>
		/// Validates the suppression file.
		/// </summary>
		/// <param name="document">The suppression file xml.</param>
		/// <param name="validationLog">The validation log.</param>
		/// <returns/>
		public void ValidateSuppressionFile(XDocument document, ValidationLog validationLog)
		{
			document.ThrowOnNull(nameof(document));
			validationLog.ThrowOnNull(nameof(validationLog));

			try
			{
				document.Validate(_xmlSchemaSet, (sender, e) => OnSchemaError(validationLog, sender, e));
			}
			catch (XmlSchemaValidationException schemaValidationException)
			{
				validationLog.LogError(schemaValidationException.Message);
			}
		}

		protected virtual void OnSchemaError(ValidationLog validationLog, object sender, ValidationEventArgs e)
		{
			string errorMsg = $"Validation event: {e.Message} | Severity: {e.Severity} | Line Number: {e.Exception.LineNumber}" + 
							  $" | Line Position: {e.Exception.LinePosition} | Error Message: { e.Exception.Message}";

			validationLog.LogError(errorMsg);
		}		
	}
}
