using PX.Data;
using PX.Common;

namespace Acuminator.Tests.Sources
{
    public class LocalizationWithHardcodedStrings
    {
        [PXLocalizable]
        public class MyMessages
        {
            public const string CommasInUserName = "Usernames cannot contain commas.";
            public const string SomeString = "Some string";
            public const string StringToFormat = "Text with placeholder {0}";
        }

        public string PXMessagesSimple()
        {
            string localizedString;
            localizedString = PXMessages.Localize("Usernames cannot contain commas.");
            localizedString = PXMessages.Localize("Usernames cannot contain commas.", out string strPrefix);
            localizedString = PXMessages.LocalizeNoPrefix("Usernames cannot contain commas.");

            return localizedString;
        }

        public string PXMessagesFormat()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXMessages.LocalizeFormat("Text with placeholder {0}", parameter);
            localizedString = PXMessages.LocalizeFormat("Text with placeholder {0}", out string prefix, parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefix("Text with placeholder {0}", parameter);
            localizedString = PXMessages.LocalizeFormatNoPrefixNLA("Text with placeholder {0}", parameter);

            return localizedString;
        }

        public string PXLocalizerAll()
        {
            string localizedString;
            object parameter = new object();

            localizedString = PXLocalizer.Localize("Hardcoded String");
            localizedString = PXLocalizer.Localize("Hardcoded String", typeof(MyMessages).FullName);
            localizedString = PXLocalizer.LocalizeFormat("Hardcoded String To Format {0}", parameter);

            return localizedString;
        }
    }
}
