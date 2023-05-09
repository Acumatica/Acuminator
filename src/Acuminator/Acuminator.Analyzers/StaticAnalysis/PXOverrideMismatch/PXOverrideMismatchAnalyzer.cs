using System.Collections.Generic;
using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXOverrideMismatchAnalyzer : SymbolAnalyzersAggregator<IPXGraphAnalyzer>
	{
		public PXOverrideMismatchAnalyzer(CodeAnalysisSettings settings, params IPXGraphAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
		{
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, Descriptors.PX1096_PXOverrideContainerIsNotPXGraphExtension);

		protected override SymbolKind SymbolKind => SymbolKind.Method;

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is IMethodSymbol methodSymbol)
			{
				if (!methodSymbol.IsStatic && methodSymbol.HasAttribute(pxContext.AttributeTypes.PXOverrideAttribute, false))
				{
					AnalyzeMethod(context, pxContext, methodSymbol);
				}
			}
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext, IMethodSymbol methodSymbol)
		{
			// Here we know that the method is not static and has the correct attribute.

			var extensionType = GetPXGraphExtension(pxContext.PXGraphExtensionTypes, methodSymbol.ContainingType);
			var graphType = FindFirstTypeArgumentOfPxGraphExtension(extensionType);

			if (graphType != null)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var allTypes = GetAllBaseTypes(pxContext.PXGraphExtensionTypes, extensionType, graphType);

				context.CancellationToken.ThrowIfCancellationRequested();

				var hasMatchingMethod = allTypes
					.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>())
					.Any(m => IsCompatibleMethod(m, methodSymbol));

				if (!hasMatchingMethod)
				{
					var diagnostic = Diagnostic.Create(
						Descriptors.PX1096_PXOverrideMustMatchSignature,
						methodSymbol.Locations.First());

					context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
				}
			}
			else
			{
				// PXOverride can only be used in a PXGraphExtension child type.
				var diagnostic = Diagnostic.Create(
					Descriptors.PX1096_PXOverrideContainerIsNotPXGraphExtension,
					methodSymbol.Locations.First());

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private static HashSet<INamedTypeSymbol> GetAllBaseTypes(
			List<INamedTypeSymbol> pxGraphExtensionTypes,
			params INamedTypeSymbol[] types)
		{
			var allTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Instance);

			foreach (var type in types)
			{
				GetBaseTypes(pxGraphExtensionTypes, type, allTypes);
			}

			return allTypes;
		}
		private static void GetBaseTypes(
			List<INamedTypeSymbol> pxGraphExtensionTypes,
			INamedTypeSymbol typeSymbol,
			HashSet<INamedTypeSymbol> hashSet)
		{
			if (typeSymbol == null || typeSymbol.SpecialType != SpecialType.None || hashSet.Contains(typeSymbol))
			{
				return;
			}

			hashSet.Add(typeSymbol);

			var extensionType = GetPXGraphExtension(pxGraphExtensionTypes, typeSymbol.BaseType);
			var graphType = FindFirstTypeArgumentOfPxGraphExtension(extensionType);

			if (graphType != null)
			{
				GetBaseTypes(pxGraphExtensionTypes, graphType, hashSet);
			}

			GetBaseTypes(pxGraphExtensionTypes, typeSymbol.BaseType, hashSet);
		}
		private static INamedTypeSymbol FindFirstTypeArgumentOfPxGraphExtension(INamedTypeSymbol type)
		{
			if (type == null)
			{
				return null;
			}

			if (type.TypeArguments.Any())
			{
				return type.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
			}

			return FindFirstTypeArgumentOfPxGraphExtension(type.ContainingType);
		}

		private static INamedTypeSymbol GetPXGraphExtension(List<INamedTypeSymbol> pxGraphExtensionTypes, INamedTypeSymbol typeSymbol)
		{
			if (typeSymbol == null)
			{
				return null;
			}

			if (Implements(typeSymbol, pxGraphExtensionTypes) && typeSymbol.IsGenericType)
			{
				return typeSymbol;
			}

			return GetPXGraphExtension(pxGraphExtensionTypes, typeSymbol.BaseType);
		}

		private static bool Implements(INamedTypeSymbol typeSymbol, List<INamedTypeSymbol> baseTypes)
		{
			if (typeSymbol == null)
			{
				return false;
			}

			if (baseTypes.Any(b => typeSymbol.MetadataName == b.MetadataName))
			{
				return true;
			}

			return Implements(typeSymbol.BaseType, baseTypes);
		}


		private static bool IsCompatibleMethod(IMethodSymbol src, IMethodSymbol ext)
		{
			var srcParams = src.Parameters;
			var extParams = ext.Parameters;

			var isSimple = srcParams.Length == extParams.Length;
			var isComplex = srcParams.Length + 1 == extParams.Length;

			if (!isSimple && !isComplex)
			{
				return false;
			}

			if (!SymbolEqualityComparer.Instance.Equals(ext.ReturnType, src.ReturnType))
			{
				return false;
			}

			for (var i = 0; i < srcParams.Length; i++)
			{
				if (!SymbolEqualityComparer.Instance.Equals(srcParams[i].Type, extParams[i].Type))
				{
					return false;
				}
			}

			if (!isComplex)
			{
				return true;
			}

			if (!(extParams.Last().Type is INamedTypeSymbol del))
			{
				return false;
			}

			if (!del.IsGenericType && del.TypeKind != TypeKind.Delegate)
			{
				return false;
			}

			var returnType = del.TypeKind == TypeKind.Delegate ? del.DelegateInvokeMethod.ReturnType : del.TypeArguments.Last();

			if (!SymbolEqualityComparer.Instance.Equals(returnType, src.ReturnType))
			{
				return false;
			}

			var delegateArguments = del.TypeKind == TypeKind.Delegate ? del.DelegateInvokeMethod.Parameters.Select(p => p.Type).ToList() : del.TypeArguments.Take(del.TypeArguments.Length - 1).ToList();

			if (srcParams.Length != delegateArguments.Count)
			{
				return false;
			}

			if (srcParams.Length == 0)
			{
				return true;
			}

			for (var i = 0; i < srcParams.Length; i++)
			{
				if (!SymbolEqualityComparer.Instance.Equals(srcParams[i].Type, delegateArguments[i]))
				{
					return false;
				}
			}

			return true;
		}
	}
}
