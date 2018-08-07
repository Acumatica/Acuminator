using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;



namespace Acuminator.Vsix.Utilities
{
    /// <summary>
    /// The Visual Studio services extensions.
    /// </summary>
    internal static class VSServicesExtensions
	{
		public static TService GetService<TService>(this IServiceProvider serviceProvider)
		where TService : class
		{
			return serviceProvider?.GetService(typeof(TService)) as TService;
		}

		public static TActual GetService<TRequested, TActual>(this IServiceProvider serviceProvider)
		where TRequested : class
		where TActual : class
		{
			return serviceProvider?.GetService(typeof(TRequested)) as TActual;
		}
    }
}
