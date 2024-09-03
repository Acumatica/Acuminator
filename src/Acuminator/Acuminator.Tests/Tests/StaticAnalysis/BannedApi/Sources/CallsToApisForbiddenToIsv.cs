#nullable enable

using System;
using System.Linq;
using System.Reflection;

namespace PX.Objects.HackathonDemo
{
	public class InfoService
	{
		public string PrintMethods(Type type)
		{
			var methodNames = type.GetMethods().Select(method => method.Name);
			string methodsString = string.Join(Environment.NewLine, methodNames);
			return methodsString;
		}

		public MethodInfo? GetMethod(Type type, string name)
		{
			var method = type.GetMethod(name);
			return method;
		}

		public string PrintSystemInfo()
		{
			return $"OS: {Environment.OSVersion.VersionString},{Environment.NewLine}Processor count: {Environment.ProcessorCount}";
		}
	}
}
