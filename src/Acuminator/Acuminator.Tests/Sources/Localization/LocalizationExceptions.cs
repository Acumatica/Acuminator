using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources
{
    public class LocalizationExceptions
    {
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
