using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using Acuminator.Utilities.Common;
using System.IO;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// A helper responsible for the suppression file validation.
	/// </summary>
	public class SuppressionFileValidation
	{
		protected IIOErrorProcessor ErrorProcessor { get; }

		protected ImmutableArray<ISuppressionFileValidator> AggregatedValidators { get; }

		public SuppressionFileValidation(IIOErrorProcessor errorProcessor)
		{
			ErrorProcessor = errorProcessor;
			AggregatedValidators = GetAggregatedValidators(errorProcessor)?.Where(validator => validator != null).ToImmutableArray() ?? 
								   ImmutableArray<ISuppressionFileValidator>.Empty;
		}

		protected virtual IEnumerable<ISuppressionFileValidator> GetAggregatedValidators(IIOErrorProcessor errorProcessor)
		{
			var xmlSchemaValidator = SuppressionFileSchemaValidator.Create(errorProcessor: errorProcessor);

			if (xmlSchemaValidator != null)
				yield return xmlSchemaValidator;
		}

		public virtual bool ValidateSuppressionFile(XDocument document)
		{
			document.ThrowOnNull(nameof(document));

			ValidationLog validationLog = new ValidationLog();

			foreach (ISuppressionFileValidator validator in AggregatedValidators)
			{
				validator.ValidateSuppressionFile(document, validationLog);
			}

			if (validationLog.ErrorsCount == 0)
				return true;

			if (ErrorProcessor == null)
				return false;

			string log = validationLog.GetValidationLog();
			
			if (log.IsNullOrWhiteSpace())
			{
				log = "Validation of suppression file failed";
			}

			ErrorProcessor.ProcessError(new Exception(log));
			return false;
		}
	}
}
