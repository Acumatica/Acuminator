using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources.Localization
{
    [PXLocalizable]
    public static class MyMessages
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string SomeString = "Some string";
        public const string StringToFormat = "Text with placeholder {0}";
    }

    public static class NonLocalizableMessages
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string StringToFormat = "Text with placeholder {0}";
    }

    public class LocalizationSamples
    {
        public void PXMessagesLocalizationSimpleMethods()
        {
            PXMessages.Localize(MyMessages.CommasInUserName);
            PXMessages.Localize(MyMessages.CommasInUserName, out string strPrefix);
            PXMessages.LocalizeNoPrefix(MyMessages.CommasInUserName);
        }

        public void PXMessagesLocalizationSimpleMethodsWithHardcodedStrings()
        {
            PXMessages.Localize("Usernames cannot contain commas.");
            PXMessages.Localize("Usernames cannot contain commas.", out string strPrefix);
            PXMessages.LocalizeNoPrefix("Usernames cannot contain commas.");
        }

        public void PXMessagesLocalizationSimpleMethodsWithNonLocalizableMessages()
        {
            PXMessages.Localize(NonLocalizableMessages.CommasInUserName);
            PXMessages.Localize(NonLocalizableMessages.CommasInUserName, out string strPrefix);
            PXMessages.LocalizeNoPrefix(NonLocalizableMessages.CommasInUserName);
        }

        public void PXMessagesLocalizationFormatMethods()
        {
            object parameter = new object();

            PXMessages.LocalizeFormat(MyMessages.StringToFormat, parameter);
            PXMessages.LocalizeFormat(MyMessages.StringToFormat, out string prefix, parameter);
            PXMessages.LocalizeFormatNoPrefix(MyMessages.StringToFormat, parameter);
            PXMessages.LocalizeFormatNoPrefixNLA(MyMessages.StringToFormat, parameter);
        }

        public void PXMessagesLocalizationFormatMethodsWithHardcodedStrings()
        {
            object parameter = new object();

            PXMessages.LocalizeFormat("Text with placeholder {0}", parameter);
            PXMessages.LocalizeFormat("Text with placeholder {0}", out string prefix, parameter);
            PXMessages.LocalizeFormatNoPrefix("Text with placeholder {0}", parameter);
            PXMessages.LocalizeFormatNoPrefixNLA("Text with placeholder {0}", parameter);
        }

        public void PXMessagesLocalizationFormatMethodsWithNonLocalizableMessages()
        {
            object parameter = new object();

            PXMessages.LocalizeFormat(NonLocalizableMessages.StringToFormat, parameter);
            PXMessages.LocalizeFormat(NonLocalizableMessages.StringToFormat, out string prefix, parameter);
            PXMessages.LocalizeFormatNoPrefix(NonLocalizableMessages.StringToFormat, parameter);
            PXMessages.LocalizeFormatNoPrefixNLA(NonLocalizableMessages.StringToFormat, parameter);
        }

        public void PXLocalizerLocalization()
        {
            object parameter = new object();

            PXLocalizer.Localize(MyMessages.CommasInUserName);
            PXLocalizer.Localize(MyMessages.CommasInUserName, typeof(MyMessages).FullName);
            PXLocalizer.LocalizeFormat(MyMessages.StringToFormat, parameter);
        }

        public void PXLocalizerLocalizationWithHardcodedStrings()
        {
            object parameter = new object();

            PXLocalizer.Localize("Hardcoded String");
            PXLocalizer.Localize("Hardcoded String", typeof(MyMessages).FullName);
            PXLocalizer.LocalizeFormat("Hardcoded String To Format {0}", parameter);
        }

        public void PXLocalizerLocalizationWithNonLocalizableMessages()
        {
            object parameter = new object();

            PXLocalizer.Localize(NonLocalizableMessages.CommasInUserName);
            PXLocalizer.Localize(NonLocalizableMessages.CommasInUserName, typeof(NonLocalizableMessages).FullName);
            PXLocalizer.LocalizeFormat(NonLocalizableMessages.StringToFormat, parameter);
        }

        public void LocalizatonMethodsWithIncorrectStringToFormat()
        {
            object parameter = new object();

            PXLocalizer.LocalizeFormat(MyMessages.CommasInUserName, parameter);
            PXMessages.LocalizeFormat(MyMessages.CommasInUserName, parameter);
            PXMessages.LocalizeFormat(MyMessages.CommasInUserName, out string refix, parameter);
            PXMessages.LocalizeFormatNoPrefix(MyMessages.CommasInUserName, parameter);
            PXMessages.LocalizeFormatNoPrefixNLA(MyMessages.CommasInUserName, parameter);
        }

        //To enrich method according to ATR-38
        public void LocalizationMethodsWithConcatinationPriorToLocalization()
        {
            object parameter = new object();

            PXLocalizer.Localize(MyMessages.CommasInUserName + MyMessages.SomeString);
            PXLocalizer.Localize(string.Format(MyMessages.StringToFormat, parameter), typeof(MyMessages).FullName);
            PXLocalizer.LocalizeFormat(string.Concat(MyMessages.CommasInUserName, MyMessages.SomeString), parameter);

            PXMessages.Localize(MyMessages.CommasInUserName + "123");
            PXMessages.Localize(string.Format(MyMessages.StringToFormat, 123), out string strPrefix);
            PXMessages.LocalizeNoPrefix(string.Concat(MyMessages.CommasInUserName, "123"));

            PXMessages.Localize(string.Concat(MyMessages.CommasInUserName, MyMessages.SomeString));
        }

        public void ExceptionsLocalization(object a, object b)
        {
            if (a == b)
            {
                throw new PXArgumentException(nameof(a), ErrorMessages.CommasInUserName);
            }

            throw new PXArgumentException(nameof(b), "Usernames cannot contain commas.");
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
