using System;
using System.Reflection;

namespace Acuminator.Utilities.Common.Reflection
{
	/// <summary>
	/// A reflection utilities.
	/// </summary>
	public static class ReflectionUtils
	{
		public static T GetValue<T>(this FieldInfo fieldInfo, object instance) => (T)fieldInfo.GetValue(instance);


		public static T GetValue<T>(this PropertyInfo propertyInfo, object instance) => (T)propertyInfo.GetValue(instance);
	}
}
