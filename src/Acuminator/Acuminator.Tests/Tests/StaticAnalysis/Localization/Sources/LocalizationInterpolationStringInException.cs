using PX.Data;

namespace Acuminator.Tests.Tests
{
    public class SomeException : PXException
    {
        public SomeException(string message)
            :base($"Message is {message}")
        {
        }
    }

    public class ExceptionUsage
    {
        public void CheckCondition(bool condition)
        {
            if (!condition)
                return;

            throw new SomeException($"The conditon is {condition}");
        }
    }
}
