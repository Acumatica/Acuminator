using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers
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

		internal static DiagnosticDescriptor PX1000_InvalidPXActionHandlerSignature { get; } = 
            Rule("PX1000", nameof(Resources.PX1000Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		internal static DiagnosticDescriptor PX1001_PXGraphCreateInstance { get; } = 
            Rule("PX1001", nameof(Resources.PX1001Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		internal static DiagnosticDescriptor PX1002_MissingTypeListAttributeAnalyzer { get; } = 
            Rule("PX1002", nameof(Resources.PX1002Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		internal static DiagnosticDescriptor PX1003_NonSpecificPXGraphCreateInstance { get; } = 
            Rule("PX1003", nameof(Resources.PX1003Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

        internal static DiagnosticDescriptor PX1004_ViewDeclarationOrder { get; } = 
            Rule("PX1004", nameof(Resources.PX1004Title).GetLocalized(), Category.Default, DiagnosticSeverity.Info);

		internal static DiagnosticDescriptor PX1005_TypoInViewDelegateName { get; } = 
            Rule("PX1005", nameof(Resources.PX1005Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning, 
                nameof(Resources.PX1005MessageFormat).GetLocalized());

		internal static DiagnosticDescriptor PX1006_ViewDeclarationOrder { get; } = 
            Rule("PX1006", nameof(Resources.PX1006Title).GetLocalized(), Category.Default, DiagnosticSeverity.Info);

        internal static DiagnosticDescriptor PX1008_LongOperationDelegateClosures { get; } = 
            Rule("PX1008", nameof(Resources.PX1008Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

        internal static DiagnosticDescriptor PX1010_StartRowResetForPaging { get; } = 
            Rule("PX1010", nameof(Resources.PX1010Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		internal static DiagnosticDescriptor PX1009_InheritanceFromPXCacheExtension { get; } = 
            Rule("PX1009", nameof(Resources.PX1009Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		internal static DiagnosticDescriptor PX1011_InheritanceFromPXCacheExtension { get; } =
            Rule("PX1011", nameof(Resources.PX1011Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		internal static DiagnosticDescriptor PX1014_NonNullableTypeForBqlField { get; } = 
            Rule("PX1014", nameof(Resources.PX1014Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		internal static DiagnosticDescriptor PX1015_PXBqlParametersMismatch { get; } = 
            Rule("PX1015", nameof(Resources.PX1015Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
	}
}
