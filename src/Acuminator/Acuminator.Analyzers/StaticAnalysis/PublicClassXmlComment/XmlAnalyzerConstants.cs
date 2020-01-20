using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal enum FixOption
	{
		NoXmlComment,
		NoSummaryTag,
		EmptySummaryTag
	}

	public static class XmlAnalyzerConstants
	{
		public const string XmlCommentExcludeTag = "exclude";
		public static readonly string XmlCommentSummaryTag = SyntaxFactory.XmlSummaryElement().StartTag.Name.ToFullString();
		internal const string FixOptionKey = nameof(FixOption);
	}
}
