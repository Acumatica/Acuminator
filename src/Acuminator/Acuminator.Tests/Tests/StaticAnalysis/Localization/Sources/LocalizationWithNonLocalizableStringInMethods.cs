using PX.Data;

namespace Acuminator.Tests.Sources
{
    public class LocalizationWithNonLocalizableStringInMethods
    {
        public string PXMessagesSimple()
        {
            string localizedString;

            localizedString = PXMessages.Localize(InnerNamespace.NonLocalizableMessagesInNamespace.CommasInUserName);
            localizedString = PXMessages.Localize(NonLocalizableMessages.CommasInUserName, out string strPrefix);
            localizedString = PXMessages.LocalizeNoPrefix(NonLocalizableMessages.CommasInUserName);

            return localizedString;
        }

        public string PXMessagesFormat()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXMessages.LocalizeFormat(InnerNamespace.NonLocalizableMessagesInNamespace.StringToFormat, parameter);
            localizedString = PXMessages.LocalizeFormat(NonLocalizableMessages.StringToFormat, out string prefix, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix(NonLocalizableMessages.StringToFormat, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefixNLA(NonLocalizableMessages.StringToFormat, parameter);

            return localizedString;
        }

        public string PXLocalizerAll()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXLocalizer.Localize(NonLocalizableMessages.CommasInUserName);
            localizedString = PXLocalizer.Localize(NonLocalizableMessages.CommasInUserName, typeof(NonLocalizableMessages).FullName);
            localizedString = PXLocalizer.LocalizeFormat(NonLocalizableMessages.StringToFormat, parameter);
            localizedString = PXLocalizer.LocalizeFormatWithKey(NonLocalizableMessages.StringToFormat, typeof(NonLocalizableMessages).FullName, parameter);

            return localizedString;
        }
    }
}
