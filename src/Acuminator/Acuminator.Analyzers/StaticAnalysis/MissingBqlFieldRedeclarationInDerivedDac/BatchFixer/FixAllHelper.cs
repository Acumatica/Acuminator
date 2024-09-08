using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

using Acuminator.Utilities.Common;

using FixAllScope = Microsoft.CodeAnalysis.CodeFixes.FixAllScope;

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;

internal static class FixAllHelper
{
	private const string WorkspaceExtensionsResourcesFullTypeName = "Microsoft.CodeAnalysis.WorkspaceExtensionsResources";

	private static readonly Dictionary<string, string> _fixAllStrings;
	private static readonly Dictionary<string, string> _defaultFixAllStrings = new()
	{
		{ "Fix_all_0_in_1"                , "Fix all '{0}' in '{1}'" },
		{ "Fix_all_0_in_Solution"         , "Fix all '{0}' in Solution" },
		{ "Fix_all_0_in_Containing_member", "Fix all '{0}' in Containing member" },
		{ "Fix_all_0_in_Containing_type"  , "Fix all '{0}' in Containing type" },
		{ "Fix_all_0"                     , "Fix all '{0}'" }
	};

	static FixAllHelper()
	{
		try
		{
			_fixAllStrings = GetFixAllStrings();
		}
		catch (Exception e)
		{
			_fixAllStrings = _defaultFixAllStrings;
		}
	}

	private static Dictionary<string, string> GetFixAllStrings()
	{
		Assembly microsoftCodeAnalysisWorkspacesAssembly = typeof(CodeAction).Assembly;
		var workspaceExtensionsResourcesType = 
			microsoftCodeAnalysisWorkspacesAssembly.GetTypes()
												   .FirstOrDefault(type => type.FullName == WorkspaceExtensionsResourcesFullTypeName);
		
		if (workspaceExtensionsResourcesType == null)
			return _defaultFixAllStrings;

		var properties = workspaceExtensionsResourcesType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		if (properties.Length == 0)
			return _defaultFixAllStrings;

		var fixAllStrings = new Dictionary<string, string>(capacity: 5);
		bool somethingChanged = false;

		foreach (var (propertyName, defaultPropertyValue) in _defaultFixAllStrings)
		{
			PropertyInfo? property	  = properties.FirstOrDefault(p => p.Name == propertyName);
			string? realPropertyValue = property?.GetValue(null) as string;

			if (realPropertyValue != null)
			{
				somethingChanged = true;
				fixAllStrings[propertyName] = realPropertyValue;
			}
			else
				fixAllStrings[propertyName] = defaultPropertyValue;
		}

		return somethingChanged 
			? fixAllStrings 
			: _defaultFixAllStrings;
	}

	public static string GetDefaultFixAllTitle(FixAllScope fixAllScope, string title, Document triggerDocument, Project triggerProject)
	{
		int fixAllScopeIntValue = (int)fixAllScope;

		switch (fixAllScopeIntValue)
		{
			case FixAllScopePolyfillConstants.Document:
				return string.Format(_fixAllStrings["Fix_all_0_in_1"], title, triggerDocument.Name);
			case FixAllScopePolyfillConstants.Project:
				return string.Format(_fixAllStrings["Fix_all_0_in_1"], title, triggerProject.Name);
			case FixAllScopePolyfillConstants.Solution:
				return string.Format(_fixAllStrings["Fix_all_0_in_Solution"], title);
			case FixAllScopePolyfillConstants.ContainingMember:
				return string.Format(_fixAllStrings["Fix_all_0_in_Containing_member"], title);
			case FixAllScopePolyfillConstants.ContainingType:
				return string.Format(_fixAllStrings["Fix_all_0_in_Containing_type"], title);
			case FixAllScopePolyfillConstants.Custom:
				return string.Format(_fixAllStrings["Fix_all_0"], title);
			default:
				throw new ArgumentOutOfRangeException(nameof(fixAllScope), fixAllScope, $"The \"{fixAllScope}\" is not supported.");
		}
	}
}