using PX.Data;

namespace Acuminator.Tests.Tests
{
    public class SecureRedirectException : PXBaseRedirectException
    {
        public SecureRedirectException(string message)
            :base(message)
        {
        }
    }

    public class NonLocalizationExceptionUsage
    {
        public void CheckCondition(bool condition)
        {
            if (!condition)
                return;

            throw new SecureRedirectException("This redirect is secure");
        }
    }
}
