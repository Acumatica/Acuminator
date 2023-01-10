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
}
