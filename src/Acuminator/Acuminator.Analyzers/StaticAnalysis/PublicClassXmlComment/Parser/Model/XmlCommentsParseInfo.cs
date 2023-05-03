#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

		public ImmutableArray<SyntaxNode> DocCommentNodesWithErrors { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(DiagnosticToReport))]
		public bool HasError => DiagnosticToReport != null;

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, bool stepIntoChildren) :
							   this(parseResult, diagnosticToReport: null, stepIntoChildren,
									nodesWithErrors: ImmutableArray<SyntaxNode>.Empty)
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren,
									SyntaxNode? nodeWithErrors) :
							   this(parseResult, diagnosticToReport, stepIntoChildren,
									nodeWithErrors != null
										? ImmutableArray.Create(nodeWithErrors)
										: ImmutableArray<SyntaxNode>.Empty)
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren, 
									IEnumerable<SyntaxNode>? nodesWithErrors) :
							   this(parseResult, diagnosticToReport, stepIntoChildren,
									nodesWithErrors?.ToImmutableArray() ?? ImmutableArray<SyntaxNode>.Empty)
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport,
									bool stepIntoChildren, ImmutableArray<SyntaxNode> nodesWithErrors)
		{
			ParseResult               = parseResult;
			DiagnosticToReport        = diagnosticToReport;
			StepIntoChildren          = stepIntoChildren;
			DocCommentNodesWithErrors = nodesWithErrors;
		}
	}
}