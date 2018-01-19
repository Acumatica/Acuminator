using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers
{
	public static class EnumerableHelpers
	{
		/// <summary>Adds a single element to the end of an IEnumerable.</summary>
		/// <typeparam name="T">Type of enumerable to return.</typeparam>
		/// <returns>IEnumerable containing all the input elements, followed by the
		/// specified additional element.</returns>
		public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T element)
		{
			return concatIterator(element, source, false);
		}

		/// <summary>Adds a single element to the start of an IEnumerable.</summary>
		/// <typeparam name="T">Type of enumerable to return.</typeparam>
		/// <returns>IEnumerable containing the specified additional element, followed by
		/// all the input elements.</returns>
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> tail, T head)
		{
			return concatIterator(head, tail, true);
		}

		public static T[] Append<T>(this T[] source, T element)
		{
			return concatArray(element, source, false);
		}

		public static T[] Prepend<T>(this T[] tail, T head)
		{
			return concatArray(head, tail, true);
		}

		public static T[] Append<T>(this T[] source, params T[] elements)
		{
			return concatArrays(elements, source, false);
		}

		public static T[] Prepend<T>(this T[] tail, params T[] head)
		{
			return concatArrays(head, tail, true);
		}

		private static IEnumerable<T> concatIterator<T>(T extraElement,
			IEnumerable<T> source, bool insertAtStart)
		{
			if (insertAtStart)
				yield return extraElement;
			if (source != null)
				foreach (var e in source)
					yield return e;
			if (!insertAtStart)
				yield return extraElement;
		}

		private static T[] concatArray<T>(T extraElement, T[] source, bool insertAtStart)
		{
			if (source == null)
				source = new T[0];
			T[] result = new T[source.Length + 1];
			source.CopyTo(result, insertAtStart ? 1 : 0);
			result[insertAtStart ? 0 : source.Length] = extraElement;
			return result;
		}

		private static T[] concatArrays<T>(T[] extraElements, T[] source, bool insertAtStart)
		{
			if (source == null)
				source = new T[0];
			T[] result = new T[source.Length + extraElements.Length];
			source.CopyTo(result, insertAtStart ? extraElements.Length : 0);
			extraElements.CopyTo(result, insertAtStart ? 0 : source.Length);
			return result;
		}
	}
}
