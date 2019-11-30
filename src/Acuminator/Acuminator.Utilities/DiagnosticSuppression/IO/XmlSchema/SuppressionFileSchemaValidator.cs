using System;
using System.Collections.Generic;
using System.Reflection;
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
		/// The error processor.
		/// </summary>
		public IIOErrorProcessor ErrorProcessor { get; }

		/// <summary>
		/// The XML schema.
		/// </summary>
		public XmlSchema Schema { get; }

		public SuppressionFileSchemaValidator(XmlSchema schema, IIOErrorProcessor errorProcessor)
		{
			Schema = schema.CheckIfNull(nameof(schema));
			ErrorProcessor = errorProcessor ?? new DefaultIOErrorProcessor();
		}

		public static SuppressionFileSchemaValidator Create(IIOErrorProcessor errorProcessor)
		{
			return null;
		}

		private static XDocument LoadSuppressionFileWithSchemaCheck(string fileName, XmlSchema xmlSchema)
		{
			XDocument doc;

			try
			{
				doc = XDocument.Load(fileName);
			}
			catch (XmlSchemaValidationException)
			{
				return null;
			}
			catch (XmlException)
			{
				return null;
			}

			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			xmlSchemaSet.Add(xmlSchema);

			try
			{
				doc.Validate(xmlSchemaSet, OnSchemaError);
				return doc;
			}
			catch (XmlSchemaValidationException)
			{
				return null;
			}
		}

		private static void OnSchemaError(object sender, ValidationEventArgs e)
		{

		}

		public bool ValidateSuppressionFile(XDocument document)
		{
			throw new NotImplementedException();
		}
	}
}
