using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
    internal static class StackGuard
    {
        public const int MaxUncheckedRecursionDepth = 20;

		/// <summary>
		/// Ensures that the remaining stack space is large enough to execute the average function.
		/// </summary>
		/// <param name="recursionDepth">How many times the calling function has recursed</param>
		/// <exception cref="InsufficientExecutionStackException">
		/// The available stack space is insufficient to execute the average function.
		/// </exception>
		[DebuggerStepThrough]
		public static void EnsureSufficientExecutionStack(int recursionDepth)
		{
			if (recursionDepth > MaxUncheckedRecursionDepth)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
			}
		}
	}
}
