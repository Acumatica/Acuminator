#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

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

namespace Acuminator.Analyzers.StaticAnalysis.CallsToInternalAPI
{
	internal class InternalApiCallsWalker : CSharpSyntaxWalker
	{
		private static readonly Dictionary<ITypeSymbol, bool> _markedTypes = new Dictionary<ITypeSymbol, bool>();
		private static readonly Dictionary<ISymbol, bool> _markedNonTypes = new Dictionary<ISymbol, bool>();

		private readonly PXContext _pxContext;
		private readonly SyntaxNodeAnalysisContext _syntaxContext;
		private readonly INamedTypeSymbol _pxInternalUseOnlyAttribute;
		private readonly SemanticModel _semanticModel;

		private CancellationToken CancellationToken => _syntaxContext.CancellationToken;

		public InternalApiCallsWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, SemanticModel semanticModel)
		{
			_syntaxContext = syntaxContext;
			_pxContext = pxContext;
			_pxInternalUseOnlyAttribute = _pxContext.AttributeTypes.PXInternalUseOnlyAttribute;
			_semanticModel = semanticModel;
		}

		public override void VisitGenericName(GenericNameSyntax node)
		{
			CancellationToken.ThrowIfCancellationRequested();

			base.VisitGenericName(node);
		}

		public override void VisitIdentifierName(IdentifierNameSyntax node)
		{
			CancellationToken.ThrowIfCancellationRequested();

			base.VisitIdentifierName(node);
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			CancellationToken.ThrowIfCancellationRequested();

			base.VisitInvocationExpression(node);
		}


		private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, MemberDeclarationSyntax memberDeclaration,
									  Location location, XmlCommentParseResult parseResult)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var memberCategory = GetMemberCategory(memberDeclaration);
			var properties = ImmutableDictionary<string, string>.Empty
																.Add(XmlAnalyzerConstants.XmlCommentParseResultKey, parseResult.ToString());
			var noXmlCommentDiagnostic = Diagnostic.Create(Descriptors.PX1007_PublicClassXmlComment, location, properties, memberCategory);

			syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, _pxContext.CodeAnalysisSettings);
		}

		


		private bool IsInternalApi(ITypeSymbol typeSymbol)
		{
			if (typeSymbol.IsNamespace || typeSymbol.IsTupleType || typeSymbol.IsAnonymousType || !CheckSpecialType(typeSymbol))
				return false;

			bool? isInternal = CheckTypeAttributesAndCacheForInternal(typeSymbol);

			if (isInternal.HasValue)
				return isInternal.Value;

			return IsInternalApiImpl(typeSymbol, recursionDepth: 0);

			//-------------------------------------------------Local Function--------------------------------------------------------
			bool IsInternalApiImpl(ITypeSymbol type, int recursionDepth)
			{
				const int maxRecursionDepth = 100;

				if (recursionDepth > maxRecursionDepth)
					return false;

				bool? isInternalApiType = CheckTypeAttributesAndCacheForInternal(type);

				if (isInternalApiType.HasValue)
					return isInternalApiType.Value;

				if (type.IsReferenceType && type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object && 
					IsInternalApiImpl(type.BaseType, recursionDepth + 1))
				{
					_markedTypes[type] = true;
					return true;
				}

				if (type.ContainingType != null && IsInternalApiImpl(type.ContainingType, recursionDepth + 1))
				{
					_markedTypes[type] = true;
					return true;
				}

				_markedTypes[type] = false;
				return false;
			}


			bool? CheckTypeAttributesAndCacheForInternal(ITypeSymbol type)
			{
				if (_markedTypes.TryGetValue(type, out bool isInternalApi))
					return isInternalApi;

				bool isInternal = type.GetAttributes().Any(a => a.AttributeClass == _pxInternalUseOnlyAttribute);

				if (isInternal)
				{
					_markedTypes[type] = true;
					return true;
				}

				return null;
			}
		}

		private bool CheckSpecialType(ITypeSymbol typeSymbol) =>
			typeSymbol.SpecialType switch
			{
				SpecialType.System_Object   => false,
				SpecialType.System_Void     => false,
				SpecialType.System_Boolean  => false,
				SpecialType.System_Char     => false,
				SpecialType.System_SByte    => false,
				SpecialType.System_Byte     => false,
				SpecialType.System_Int16    => false,
				SpecialType.System_UInt16   => false,
				SpecialType.System_Int32    => false,
				SpecialType.System_UInt32   => false,
				SpecialType.System_Int64    => false,
				SpecialType.System_UInt64   => false,
				SpecialType.System_Decimal  => false,
				SpecialType.System_Single   => false,
				SpecialType.System_Double   => false,
				SpecialType.System_String   => false,
				SpecialType.System_IntPtr   => false,
				SpecialType.System_UIntPtr  => false,
				SpecialType.System_DateTime => false,
				_                           => true,
			};
	}
}
