#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;

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
		private static readonly Dictionary<ISymbol, bool> _markedInternalApi = new Dictionary<ISymbol, bool>();

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

		public override void VisitGenericName(GenericNameSyntax genericName)
		{
			CancellationToken.ThrowIfCancellationRequested();

			if (!(_semanticModel.GetSymbolInfo(genericName, CancellationToken).Symbol is ITypeSymbol typeSymbol))
			{
				base.VisitGenericName(genericName);
				return;
			}

			if (IsInternalApiType(typeSymbol))
			{
				ReportInternalApiDiagnostic(genericName.Identifier.GetLocation());
			}

			base.VisitGenericName(genericName);
		}

		public override void VisitIdentifierName(IdentifierNameSyntax identifierName)
		{
			CancellationToken.ThrowIfCancellationRequested();

			if (!(_semanticModel.GetSymbolInfo(identifierName, CancellationToken).Symbol is ITypeSymbol typeSymbol))
				return;

			if (IsInternalApiType(typeSymbol))
			{
				ReportInternalApiDiagnostic(identifierName.Identifier.GetLocation());
			}
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
		{
			CancellationToken.ThrowIfCancellationRequested();

			var symbolInfo = _semanticModel.GetSymbolInfo(invocation, CancellationToken);

			if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
			{
				base.VisitInvocationExpression(invocation);
				return;
			}

			if (IsInternalApiMethod(methodSymbol))
			{
				ReportInternalApiDiagnostic(invocation.GetMethodNameLocation());
			}

			base.VisitInvocationExpression(invocation);
		}

		private bool IsInternalApiType(ITypeSymbol typeSymbol)
		{
			if (typeSymbol.IsNamespace || typeSymbol.IsTupleType || typeSymbol.IsAnonymousType || !typeSymbol.IsAccessibleOutsideOfAssembly() || 
				!CheckSpecialType(typeSymbol))
			{
				return false;
			}

			bool? isInternal = CheckAttributesAndCacheForInternal(typeSymbol);

			if (isInternal.HasValue)
				return isInternal.Value;

			return IsInternalApiImpl(typeSymbol, recursionDepth: 0);

			//-------------------------------------------------Local Function--------------------------------------------------------
			bool IsInternalApiImpl(ITypeSymbol type, int recursionDepth)
			{
				const int maxRecursionDepth = 100;

				if (recursionDepth > maxRecursionDepth)
					return false;

				bool? isInternalApiType = CheckAttributesAndCacheForInternal(type);

				if (isInternalApiType.HasValue)
					return isInternalApiType.Value;

				CancellationToken.ThrowIfCancellationRequested();

				if (type.IsReferenceType && type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object &&
					IsInternalApiImpl(type.BaseType, recursionDepth + 1))
				{
					_markedInternalApi[type] = true;
					return true;
				}

				CancellationToken.ThrowIfCancellationRequested();

				if (type.ContainingType != null && IsInternalApiImpl(type.ContainingType, recursionDepth + 1))
				{
					_markedInternalApi[type] = true;
					return true;
				}

				_markedInternalApi[type] = false;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CheckSpecialType(ITypeSymbol typeSymbol) =>
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

		private bool IsInternalApiMethod(IMethodSymbol methodSymbol)
		{
			if (!CheckMethodKind(methodSymbol) || !methodSymbol.IsAccessibleOutsideOfAssembly())
				return false;

			if (_markedInternalApi.TryGetValue(methodSymbol, out bool isInternalMethod))
				return isInternalMethod;

			if (methodSymbol.ContainingType != null && IsInternalApiType(methodSymbol.ContainingType))
			{
				_markedInternalApi[methodSymbol] = true;
				return true;
			}

			IMethodSymbol? curMethod = methodSymbol;

			while (curMethod != null)
			{
				bool? isInternal = CheckAttributesAndCacheForInternal(curMethod);

				if (isInternal.HasValue)
					return isInternal.Value;

				curMethod = curMethod.IsOverride
					? curMethod.OverriddenMethod
					: null;
			}

			_markedInternalApi[methodSymbol] = false;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CheckMethodKind(IMethodSymbol methodSymbol) =>
			methodSymbol.MethodKind switch
			{
				MethodKind.LambdaMethod      => false,
				MethodKind.Constructor       => false,
				MethodKind.Conversion        => false,
				MethodKind.Destructor        => false,
				MethodKind.EventAdd          => false,
				MethodKind.EventRaise        => false,
				MethodKind.EventRemove       => false,
				MethodKind.StaticConstructor => false,
				MethodKind.BuiltinOperator   => false,
				MethodKind.DeclareMethod     => false,
				MethodKind.LocalFunction     => false,
				_                            => true
			};

		private bool? CheckAttributesAndCacheForInternal(ISymbol symbol)
		{
			if (_markedInternalApi.TryGetValue(symbol, out bool isInternalApi))
				return isInternalApi;

			bool isInternal = symbol.GetAttributes().Any(a => a.AttributeClass == _pxInternalUseOnlyAttribute);

			if (isInternal)
			{
				_markedInternalApi[symbol] = true;
				return true;
			}

			return null;
		}

		private void ReportInternalApiDiagnostic(Location location)
		{
			CancellationToken.ThrowIfCancellationRequested();

			if (location != null)
			{
				var internalApiDiagnostic = Diagnostic.Create(Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV, location);
				_syntaxContext.ReportDiagnosticWithSuppressionCheck(internalApiDiagnostic, _pxContext.CodeAnalysisSettings);
			}
		}
	}
}
