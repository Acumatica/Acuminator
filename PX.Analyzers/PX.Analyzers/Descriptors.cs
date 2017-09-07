using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace PX.Analyzers
{
	internal enum Category
	{
		Default,
	}

	internal static class Descriptors
	{
		static readonly ConcurrentDictionary<Category, string> categoryMapping = new ConcurrentDictionary<Category, string>();
        static DiagnosticDescriptor Rule(string id, LocalizableString title, Category category, DiagnosticSeverity defaultSeverity, LocalizableString messageFormat = null, LocalizableString description = null)
		{
			var helpLink = "";
			var isEnabledByDefault = true;
			messageFormat = messageFormat ?? title;
			return new DiagnosticDescriptor(id, title, messageFormat, categoryMapping.GetOrAdd(category, c => c.ToString()), defaultSeverity, isEnabledByDefault, description, helpLink);
		}

		internal static DiagnosticDescriptor PX1000_InvalidPXActionHandlerSignature { get; } = Rule("PX1000", nameof(Resources.PX1000Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
		internal static DiagnosticDescriptor PX1001_PXGraphCreateInstance { get; } = Rule("PX1001", nameof(Resources.PX1001Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
		internal static DiagnosticDescriptor PX1002_MissingTypeListAttributeAnalyzer { get; } = Rule("PX1002", nameof(Resources.PX1002Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
		internal static DiagnosticDescriptor PX1003_NonSpecificPXGraphCreateInstance { get; } = Rule("PX1003", nameof(Resources.PX1003Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);
        internal static DiagnosticDescriptor PX1004_ViewDeclarationOrder { get; } = Rule("PX1004", nameof(Resources.PX1004Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
		internal static DiagnosticDescriptor PX1005_TypoInViewDelegateName { get; } = Rule("PX1005", nameof(Resources.PX1005Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning, nameof(Resources.PX1005MessageFormat).GetLocalized());
		internal static DiagnosticDescriptor PX1006_ViewDeclarationOrder { get; } = Rule("PX1006", nameof(Resources.PX1006Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
		internal static DiagnosticDescriptor PX1007_DACCreateInstance{ get; } = Rule("PX1007", nameof(Resources.PX1007Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning, nameof(Resources.PX1007MessageFormat).GetLocalized());
        internal static DiagnosticDescriptor PX1008_LongOperationDelegateClosures { get; } = Rule("PX1008", nameof(Resources.PX1008Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);
    }
}
