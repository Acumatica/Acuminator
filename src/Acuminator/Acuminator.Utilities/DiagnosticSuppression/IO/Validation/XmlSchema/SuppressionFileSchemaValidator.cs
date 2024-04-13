﻿using System;
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
			Schema = schema.CheckIfNull();
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
			string errorMsg = $"Validation event: {e.Message} | Severity: {e.Severity}";
			string location = GetLocationDescription(sender);

			if (location != null)
			{
				errorMsg += " | " + location;
			}

			validationLog.LogError(errorMsg);
		}		

		protected virtual string GetLocationDescription(object sender)
		{
			XElement suppressionMesage = sender is XAttribute attribute
				? GetSuppressionMessageElement(attribute.Parent)
				: sender is XElement element
					? GetSuppressionMessageElement(element)
					: null;

			if (suppressionMesage == null)
				return null;

			return $"Node: {suppressionMesage.ToString()} | " +
				   $"Prev Node: {suppressionMesage.PreviousNode?.ToString()} | " +
				   $"Next Node: {suppressionMesage.NextNode?.ToString()}";
		}

		private XElement GetSuppressionMessageElement(XElement element)
		{
			while (element != null && element.Name != SuppressionFile.SuppressMessageElement)
				element = element.Parent;

			return element;
		}
	}
}
