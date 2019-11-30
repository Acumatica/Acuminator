using System;
using System.Text;
using System.Xml.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	public class ValidationLog
	{
		private const string LogSeparator = "*******************************************************************"

		public int ErrorsCount
		{
			get;
			private set;
		}

		protected StringBuilder Log { get; } = new StringBuilder();

		public string GetValidationLog() => Log.ToString();

		public void LogError(string errorMessage)
		{
			if (errorMessage.IsNullOrWhiteSpace())
				return;

			try
			{
				if (Log.Length > 0)
				{
					Log.AppendLine(LogSeparator);
				}

				Log.AppendLine(errorMessage);
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			finally
			{
				if (ErrorsCount < Int32.MaxValue)
					ErrorsCount++;
			}
		}
	}
}
