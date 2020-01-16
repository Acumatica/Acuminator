using System;
using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Tests.Tests.DiagnosticSuppression.SuppressionFileOrdering
{
	/// <summary>
	/// A test implementation of suppression file service.
	/// </summary>
	internal class SuppressionFileTestService : SuppressionFileSystemServiceBase
	{
		public SuppressionFileTestService() :
									 base(null, null)
		{
		}

		public SuppressionFileTestService(IIOErrorProcessor errorProcessor) :
									 base(errorProcessor, null)
		{
		}

		public SuppressionFileTestService(IIOErrorProcessor errorProcessor, SuppressionFileValidation customValidation) :
									 base(errorProcessor, customValidation)
		{
		}

		public override ISuppressionFileWatcherService CreateWatcher(string path) => null;
	}
}
