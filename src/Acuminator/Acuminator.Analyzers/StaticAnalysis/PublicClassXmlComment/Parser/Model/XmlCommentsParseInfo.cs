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

		public XmlCommentTagsInfo TagsInfo { get; }

		public DiagnosticDescriptor? DiagnosticToReport { get; }

		public bool StepIntoChildNodes { get; }

		public ImmutableArray<SyntaxNode> NodesWithErrors { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(DiagnosticToReport))]
		public bool HasError => DiagnosticToReport != null;

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, XmlCommentTagsInfo tagsInfo, DiagnosticDescriptor? diagnosticToReport,
									bool stepIntoChildNodes, SyntaxNode? nodeWithErrors) :
							   this(parseResult, tagsInfo, diagnosticToReport, stepIntoChildNodes,
									nodeWithErrors != null
										? ImmutableArray.Create(nodeWithErrors)
										: ImmutableArray<SyntaxNode>.Empty)
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, XmlCommentTagsInfo tagsInfo, DiagnosticDescriptor? diagnosticToReport,
									bool stepIntoChildNodes, IEnumerable<SyntaxNode>? nodesWithErrors) :
							   this(parseResult, tagsInfo, diagnosticToReport, stepIntoChildNodes,
									nodesWithErrors?.ToImmutableArray() ?? ImmutableArray<SyntaxNode>.Empty)
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, XmlCommentTagsInfo tagsInfo, DiagnosticDescriptor? diagnosticToReport,
									bool stepIntoChildNodes, ImmutableArray<SyntaxNode> nodesWithErrors)
		{
			ParseResult = parseResult;
			TagsInfo = tagsInfo;
			DiagnosticToReport = diagnosticToReport;
			StepIntoChildNodes = stepIntoChildNodes;
			NodesWithErrors = nodesWithErrors;
		}
	}
}