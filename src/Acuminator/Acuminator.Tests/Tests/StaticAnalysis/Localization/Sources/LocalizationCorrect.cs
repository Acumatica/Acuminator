using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
    public class LocalizationCorrect
    {
        public string PXMessagesLocalizationSimpleMethods()
        {
            string localizedString;

            localizedString = PXMessages.Localize(MyMessages.CommasInUserName);
            localizedString = PXMessages.Localize(MyMessages.CommasInUserName, out string strPrefix);
            localizedString = PXMessages.LocalizeNoPrefix(MyMessages.CommasInUserName);

            return localizedString;
        }

        public string PXMessagesLocalizationFormatMethods()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXMessages.LocalizeFormat(MyMessages.StringToFormat, parameter);
            localizedString = PXMessages.LocalizeFormat(MyMessages.StringToFormat, out string prefix, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix(MyMessages.StringToFormat, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefixNLA(MyMessages.StringToFormat, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix(ComplexMessages.DocDiscountExceedLimit, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix(ComplexMessages.DateTimeStr, DateTime.Now);

            return localizedString;
        }

        public string PXLocalizerLocalization()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXLocalizer.Localize(MyMessages.CommasInUserName);
            localizedString = PXLocalizer.Localize(MyMessages.CommasInUserName, typeof(MyMessages).FullName);
            localizedString = PXLocalizer.LocalizeFormat(MyMessages.StringToFormat, parameter);
            localizedString = PXLocalizer.LocalizeFormat(ComplexMessages.DocDiscountExceedLimit, parameter);
            localizedString = PXLocalizer.LocalizeFormatWithKey(MyMessages.StringToFormat, typeof(MyMessages).FullName, parameter);

            return localizedString;
        }

        public void ThrowPXException()
        {
            throw new PXArgumentException(nameof(ThrowPXException), MyMessages.CommasInUserName);
        }
    }

    public class DetailBypassedException : PXException
    {
        public object ItemToBypass { get; }
        public DetailBypassedException(object itemToBypass)
            : base(MyMessages.CommasInUserName)
        {
            ItemToBypass = itemToBypass;
        }
    }
}
