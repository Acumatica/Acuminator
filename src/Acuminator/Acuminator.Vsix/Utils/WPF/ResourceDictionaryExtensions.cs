using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Resources;
using System.Windows;


namespace Acuminator.Vsix.Utilities
{
	/// <summary>
	/// A static helper for the WPF <see cref="ResourceDictionary"/>.
	/// </summary>
	public static class ResourceDictionaryHelper
	{
		/// <summary>
		/// The TryGetValue method for the WPF <see cref="ResourceDictionary"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="resources">Resource dictionary.</param>
		/// <param name="key">Key.</param>
		/// <param name="value">Returned value in case of succeed, default otherwise.</param>
		/// <returns></returns>
		public static bool TryGetValue<T>(this ResourceDictionary resources, object key, out T value)
		{
			value = default(T);
			
			if (resources == null || key == null)
				return false;

			try
			{
				value = (T)resources[key];
			}
			catch
			{				
				return false;
			}

			return true;
		}
	}
}
