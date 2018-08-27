using PX.Data;

namespace Acuminator.Tests.Sources
{
    public class LocalizationExceptions
    {
        public void ExceptionsLocalization()
        {
            throw new PXArgumentException(nameof(ExceptionsLocalization), "Usernames cannot contain commas.");
        }
    }

    public class DetailNonLocalizableBypassedException : PXException
    {
        public object ItemToBypass { get; }
        public DetailNonLocalizableBypassedException(object itemToBypass)
            : base("Hardcoded String, Ups!")
        {
            ItemToBypass = itemToBypass;
        }
    }
}
