using System;
using Acuminator.Utilities.Common;
using System.Reflection;




namespace Acuminator.Vsix.Utilities
{
	/// <summary>
	/// A reflection utilities used by the <see cref="Acuminator.Vsix.DiagnosticSuppression.DiagnosticData"/>.
	/// </summary>
	internal static class ReflectionUtils
	{
		public static T GetValue<T>(this FieldInfo fieldInfo, object instance) => (T)fieldInfo.GetValue(instance);


		public static T GetValue<T>(this PropertyInfo propertyInfo, object instance) => (T)propertyInfo.GetValue(instance);
	}
}
