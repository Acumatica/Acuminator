using PX.Data;

namespace Acuminator.Tests.Sources
{
    public class LocalizationWithIncorrectStringsToFormatInMethods
    {
        public string All()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXLocalizer.LocalizeFormat(MyMessages.CommasInUserName, parameter);
            localizedString = PXMessages.LocalizeFormat(MyMessages.CommasInUserName, parameter);
            localizedString = PXMessages.LocalizeFormat(MyMessages.CommasInUserName, out string refix, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix(MyMessages.CommasInUserName, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefixNLA(MyMessages.CommasInUserName, parameter);

            return localizedString;
        }
    }
}
