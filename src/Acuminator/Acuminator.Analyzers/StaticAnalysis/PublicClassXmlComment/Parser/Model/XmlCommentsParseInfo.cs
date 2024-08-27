
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal readonly struct XmlCommentsParseInfo
	{
		public XmlCommentParseResult ParseResult { get; }

		public DiagnosticDescriptor? DiagnosticToReport { get; }

		public bool StepIntoChildren { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(DiagnosticToReport))]
		public bool HasError => DiagnosticToReport != null;

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren)
		{
			ParseResult 				  = parseResult;
			DiagnosticToReport 			  = diagnosticToReport;
			StepIntoChildren 			  = stepIntoChildren;
		}
	}
}