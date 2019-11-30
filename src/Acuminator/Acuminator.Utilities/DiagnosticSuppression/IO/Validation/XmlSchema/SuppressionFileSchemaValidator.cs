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

		/// <summary>
		/// The error processor. Can be null.
		/// </summary>
		public IIOErrorProcessor ErrorProcessor { get; }

		private readonly XmlSchemaSet _xmlSchemaSet;


		protected SuppressionFileSchemaValidator(XmlSchema schema, IIOErrorProcessor errorProcessor)
		{
			Schema = schema.CheckIfNull(nameof(schema));
			_xmlSchemaSet = new XmlSchemaSet();
			_xmlSchemaSet.Add(Schema);

			ErrorProcessor = errorProcessor;
		}

		public static SuppressionFileSchemaValidator Create(IIOErrorProcessor errorProcessor, IXmlSchemaProvider customXmlSchemaProvider = null)
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

			return new SuppressionFileSchemaValidator(schema, errorProcessor);
		}

		public bool ValidateSuppressionFile(XDocument document)
		{
			document.ThrowOnNull(nameof(document));
			int errorCounter = 0;
			StringBuilder logBuilder = new StringBuilder();

			try
			{
				document.Validate(_xmlSchemaSet, (sender, e) => OnSchemaError(logBuilder, errorCounter, sender, e));
			}
			catch (XmlSchemaValidationException schemaValidationException)
			{
				ErrorProcessor?.ProcessError(schemaValidationException);
				return false;
			}


		}

		private void OnSchemaError(StringBuilder logBuilder, int errorCounter, object sender, ValidationEventArgs e)
		{

		}	



		
	}
}
