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
        public void ExceptionsLocalization()
        {
            throw new PXArgumentException(nameof(ExceptionsLocalization), MyMessages.CommasInUserName + MyMessages.SomeString);
        }
    }

    public class NonLocalizableMessageException : PXException
    {
        public NonLocalizableMessageException()
            : base(MyMessages.CommasInUserName + MyMessages.SomeString)
        {
        }
    }
}
