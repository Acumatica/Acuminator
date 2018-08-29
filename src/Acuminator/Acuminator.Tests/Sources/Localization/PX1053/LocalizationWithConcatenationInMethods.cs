using PX.Data;
using System;

namespace Acuminator.Tests.Sources.Localization
{
    public class LocalizationWithConcatenation
    {
        public string All()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXLocalizer.Localize(MyMessages.CommasInUserName + MyMessages.SomeString);
            localizedString = PXLocalizer.Localize(string.Format(MyMessages.StringToFormat, parameter), typeof(MyMessages).FullName);
            localizedString = PXLocalizer.LocalizeFormat(String.Concat(MyMessages.CommasInUserName, MyMessages.SomeString), parameter);

            localizedString = PXMessages.Localize(MyMessages.CommasInUserName + "123");
            localizedString = PXMessages.Localize(string.Format(MyMessages.StringToFormat, 123), out string strPrefix);
            localizedString = PXMessages.LocalizeNoPrefix(string.Concat(MyMessages.CommasInUserName, "123"));

            localizedString = PXMessages.LocalizeFormat(MyMessages.StringToFormat + "456", parameter);
            localizedString = PXMessages.LocalizeFormat(string.Format(MyMessages.StringToFormat, parameter), out string prefix, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix(string.Concat(MyMessages.StringToFormat, "456"), parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefixNLA(string.Concat(MyMessages.StringToFormat, "456", "789"), parameter);

            return localizedString;
        }
    }
}
