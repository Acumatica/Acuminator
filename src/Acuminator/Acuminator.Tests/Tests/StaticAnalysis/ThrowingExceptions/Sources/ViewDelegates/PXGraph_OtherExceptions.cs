using PX.Data;
using PX.SM;
using System;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions.Sources.ViewDelegates
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public IEnumerable users()
        {
            var dayNbr = DateTime.Now.Day;

            switch (dayNbr)
            {
                case 1:
                    throw new PXInvalidOperationException();
                case 2:
                    throw new PXArgumentException();
                case 3:
                    throw new PXException();
                default:
                    throw new Exception();
            }
        }
    }
}
