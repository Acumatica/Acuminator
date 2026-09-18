using System;

namespace Acuminator.Runner
{
    /// <summary>
    /// Run result
    /// </summary>
    internal enum RunResult
    {
        /// <summary>
        /// The run finished successfully.
        /// </summary>
        Success = 0,

		/// <summary>
		/// The run finished successfully but the analyzed code source did not pass the validation.
		/// </summary>
		RequirementsNotMet = 1,

		/// <summary>
		/// The run was cancelled.
		/// </summary>
		Cancelled = 2,

		/// <summary>
		/// The run was interrupted by a runtime error.
		/// </summary>
		RunTimeError = 4
    }

    /// <summary>
    /// The helper class for <see cref="RunResult"/>.
    /// </summary>
    internal static class RunResultHelper
    {
		public static bool IsError(this RunResult result) => 
			result != RunResult.Success && result != RunResult.Cancelled;

		public static bool IsCancelled(this RunResult result) => result == RunResult.Cancelled;

		public static RunResult Combine(this RunResult x, RunResult y) =>
		   x >= y ? x : y;

		public static int ToExitCode(this RunResult result) => (int)result;
    }
}
