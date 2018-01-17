using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers.Coloriser
{
    public static class ExceptionExtensions
    {
        public static void ThrowOnNull<T>(this T obj, string parameter = null, string message = null)
        where T : class
        {
            if (obj != null)
                return;

            throw NewException(parameter, message);
        }

        public static void ThrowOnNullOrWhiteSpace(this string str, string parameter = null, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return;

            throw NewException(parameter, message);
        }

        private static ArgumentNullException NewException(string parameter = null, string message = null)
        {
            return parameter == null
               ? throw new ArgumentNullException()
               : message == null
                   ? new ArgumentNullException(parameter)
                   : new ArgumentNullException(parameter, message);
        }
    }
}
