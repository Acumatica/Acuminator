using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.Common
{
	public static class ExceptionExtensions
	{
		private const string TheCollectionCannotBeEmptyErrorMsg = "The collection cannot be empty";

		/// <summary>
		/// An extension method for fluent patterns that throws <see cref="ArgumentNullException"/> if <paramref name="obj"/> is null.
		/// Otherwise returns the <paramref name="obj"/>.
		/// </summary>
		/// <typeparam name="T">Object type.</typeparam>
		/// <param name="obj">The object to act on.</param>
		/// <param name="paramName">(Optional) Name of the parameter for <see cref="ArgumentNullException"/>.</param>
		/// <param name="message">(Optional) The error message.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T CheckIfNull<T>(this T obj, string paramName = null, string message = null)
		{
			obj.ThrowOnNull(paramName, message);
			return obj;
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ThrowOnNull<T>(this T obj, string parameter = null, string message = null)
		{
			if (obj != null)
				return;

			throw NewArgumentNullException(parameter, message);
		}

		/// <summary>
		/// An extension method for fluent patterns that throws <see cref="ArgumentNullException"/> if <paramref name="collection"/> is null,
		/// throws <see cref="ArgumentException"/> if collection is empty.
		/// Otherwise returns the <paramref name="collection"/>.
		/// </summary>
		/// <typeparam name="T">Object type.</typeparam>
		/// <param name="collection">The collection to act on.</param>
		/// <param name="paramName">(Optional) Name of the parameter for <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/>.</param>
		/// <param name="message">(Optional) The error message.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> CheckIfNullOrEmpty<T>(this IEnumerable<T> collection, string paramName = null, string message = null)
		{
			collection.ThrowOnNullOrEmpty(paramName, message);
			return collection;
		}

		/// <summary>
		/// An extension method that throws <see cref="ArgumentNullException"/> if <paramref name="collection"/> is null and
		/// throws <see cref="ArgumentException"/> if collection is empty.
		/// </summary>
		/// <typeparam name="T">Object type.</typeparam>
		/// <param name="collection">The collection to act on.</param>
		/// <param name="paramName">(Optional) Name of the parameter for <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/>.</param>
		/// <param name="message">(Optional) The error message.</param>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ThrowOnNullOrEmpty<T>(this IEnumerable<T> collection, string paramName = null, string message = null)
		{
			if (collection == null)
			{
				throw NewArgumentNullException(paramName, message);
			}
			else if (collection.IsEmpty())
			{
				message = message.IsNullOrWhiteSpace()
							? TheCollectionCannotBeEmptyErrorMsg
							: message;
				throw NewArgumentException(message, paramName);
			}
		}

		/// <summary>
		/// An extension method for fluent patterns that throws <see cref="ArgumentNullException"/> if <paramref name="str"/> is null and
		/// throws <see cref="ArgumentException"/> if <paramref name="str"/> contains only whitespaces or is empty.
		/// Otherwise returns the <paramref name="str"/>.
		/// </summary>
		/// <param name="str">The string to act on.</param>
		/// <param name="paramName">(Optional) Name of the parameter for <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/>.</param>
		/// <param name="message">(Optional) The error message.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string CheckIfNullOrWhiteSpace(this string str, string paramName = null, string message = null)
		{
			str.ThrowOnNullOrWhiteSpace(paramName, message);
			return str;
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ThrowOnNullOrWhiteSpace(this string str, string parameter = null, string message = null)
		{
			if (!string.IsNullOrWhiteSpace(str))
				return;

			throw str == null
				? NewArgumentNullException(parameter, message)
				: NewArgumentException(parameter, message);
		}

		private static ArgumentNullException NewArgumentNullException(string parameter = null, string message = null)
		{
			return parameter == null
			   ? throw new ArgumentNullException()
			   : message == null
				   ? new ArgumentNullException(parameter)
				   : new ArgumentNullException(parameter, message);
		}

		private static ArgumentException NewArgumentException(string parameter = null, string message = null)
		{
			return parameter == null
			   ? throw new ArgumentNullException()
			   : message == null
				   ? new ArgumentNullException(parameter)
				   : new ArgumentNullException(parameter, message);
		}
	}
}
