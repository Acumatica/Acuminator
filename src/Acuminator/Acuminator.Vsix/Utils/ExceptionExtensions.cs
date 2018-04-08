using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;



namespace Acuminator.Vsix.Utilities
{
    public static class ExceptionExtensions
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowOnNull<T>(this T obj, string parameter = null, string message = null)
        where T : class
        {
            if (obj != null)
                return;

            throw NewException(parameter, message);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowOnNullOrWhiteSpace(this string str, string parameter = null, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return;

            throw NewException(parameter, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
