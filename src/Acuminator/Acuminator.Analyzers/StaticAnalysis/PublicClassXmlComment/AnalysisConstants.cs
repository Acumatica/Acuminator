#nullable enable

using System;
using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.Parser.Model;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal static class AnalysisConstants
	{
		public const string ParseResultKey = nameof(XmlCommentParseResult);
	}
}