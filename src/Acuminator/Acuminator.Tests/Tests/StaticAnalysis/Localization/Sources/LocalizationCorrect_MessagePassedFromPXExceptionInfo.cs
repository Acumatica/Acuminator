using PX.Data;
using PX.Objects.Common.Exceptions;

namespace Acuminator.Tests.Tests
{
	public class ExceptionHelper
	{
		public void ThrowException(PXExceptionInfo exceptionInfo)
		{
			throw new PXSetPropertyException(exceptionInfo.MessageFormat, exceptionInfo.ErrorLevel, exceptionInfo.MessageArguments);  //no alert
		}
	}

	// Acuminator disable once PX1063 ExceptionWithoutSerializationConstructor [Justification]
	// Acuminator disable once PX1064 ExceptionWithNewFieldsAndNoGetObjectDataOverride [Justification]
	public class DetailNonLocalizableBypassedException : PXException
	{
		public object ItemToBypass { get; }
		public DetailNonLocalizableBypassedException(object item, PXExceptionInfo exceptionInfo)
			: base(exceptionInfo.MessageFormat)
		{
			ItemToBypass = item;
		}
	}
}
