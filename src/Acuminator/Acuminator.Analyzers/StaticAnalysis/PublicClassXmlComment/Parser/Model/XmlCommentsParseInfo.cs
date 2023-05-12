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

		public IReadOnlyCollection<Location> DocCommentLocationsWithErrors { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(DiagnosticToReport))]
		public bool HasError => DiagnosticToReport != null;

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, bool stepIntoChildren) :
							   this(parseResult, diagnosticToReport: null, stepIntoChildren, locationsWithErrors: null)
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren,
									SyntaxNode? nodeWithErrors) :
							   this(parseResult, diagnosticToReport, stepIntoChildren, locationWithErrors: nodeWithErrors?.GetLocation())
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren,
									IEnumerable<SyntaxNode>? nodeWithErrors) :
							   this(parseResult, diagnosticToReport, stepIntoChildren,
									locationsWithErrors: nodeWithErrors?.Select(node => node.GetLocation())
																		.Where(location => location != null))
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren,
									Location? locationWithErrors) :
							   this(parseResult, diagnosticToReport, stepIntoChildren,
									locationWithErrors != null
										? new[] { locationWithErrors }
										: Array.Empty<Location>())
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren, 
									IEnumerable<Location>? locationsWithErrors) :
									this(parseResult, diagnosticToReport, stepIntoChildren, locationsWithErrors: locationsWithErrors?.ToList())
		{ }

		public XmlCommentsParseInfo(XmlCommentParseResult parseResult, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren,
									IReadOnlyCollection<Location>? locationsWithErrors)
		{
			ParseResult 				  = parseResult;
			DiagnosticToReport 			  = diagnosticToReport;
			StepIntoChildren 			  = stepIntoChildren;
			DocCommentLocationsWithErrors = locationsWithErrors ?? Array.Empty<Location>();
		}
	}
}