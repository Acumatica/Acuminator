using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// An embedded resource XML schema provider.
	/// </summary>
	public class EmbeddedResourceXmlSchemaProvider : IXmlSchemaProvider
	{
		/// <summary>
		/// Gets XML schema from the given assembly embedded resources.
		/// </summary>
		public  XmlSchema GetXmlSchema()
		{
			try
			{
				Assembly utilsAssembly = typeof(SuppressionFileSchemaValidator).GetTypeInfo().Assembly;
				string schemaResourceFullName = GetSchemaResourceFullName(utilsAssembly);

				if (schemaResourceFullName.IsNullOrWhiteSpace())
					return null;

				using (Stream stream = utilsAssembly.GetManifestResourceStream(schemaResourceFullName))
				{
					if (stream == null)
						return null;

					using (XmlReader schemaReader = XmlReader.Create(stream))
					{

						return XmlSchema.Read(schemaReader, null);
					}

					return null;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		protected virtual string GetSchemaResourceFullName(Assembly curentAssembly) =>
			curentAssembly.GetManifestResourceNames()
						 ?.FirstOrDefault(rName => rName.EndsWith(SharedConstants.SuppressionFileXmlSchemaFileName));
	}
}
