#nullable enable

using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;
using System.Collections.Generic;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXOverrideMismatchAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1096_PXOverrideMustMatchSignature);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type == GraphType.PXGraphExtension;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var allMethods = pxGraphExtension.Symbol.GetMethods();

			var methodsWithPxOverrideAttribute = allMethods
					.Where(m => m.HasAttribute(pxContext.AttributeTypes.PXOverrideAttribute, checkOverrides: false))
					.ToList();

			if (methodsWithPxOverrideAttribute.Any())
			{
				var directBaseTypesAndThis = pxGraphExtension.Symbol.GetBaseTypesAndThis().ToList(capacity: 4);

				var allBaseTypes = pxGraphExtension.Symbol
					.GetGraphExtensionWithBaseExtensions(pxContext, SortDirection.Ascending, includeGraph: true)
					.SelectMany(t => t.GetBaseTypesAndThis())
					.OfType<INamedTypeSymbol>()
					.Distinct()
					.Where(baseType => !directBaseTypesAndThis.Contains(baseType));

				methodsWithPxOverrideAttribute.ForEach(m => AnalyzeMethod(context, pxContext, allBaseTypes, m!));
			}
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext, IEnumerable<INamedTypeSymbol> allBaseTypes, IMethodSymbol methodSymbol)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!methodSymbol.IsStatic && !methodSymbol.IsGenericMethod)
			{
				foreach (var baseType in allBaseTypes)
				{
					bool hasSuitablePXOverride = baseType.GetMethods(methodSymbol.Name)
														 .Any(m => PXOverrideHelper.IsSuitable(methodSymbol, m));
					if (hasSuitablePXOverride)
						return;
				}
			}

			var location = methodSymbol.Locations.FirstOrDefault();

			if (location != null)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, location);

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private class PXOverrideHelper
		{
			/// <summary>
			/// Special signature check between a derived method with the PXOverride attribute and the base method.
			/// </summary>
			/// <param name="pxOverrideMethod">The method from the derived class, with the PXOverride attribute</param>
			/// <param name="baseMethod">The method from the base</param>
			/// <returns></returns>
			public static bool IsSuitable(IMethodSymbol pxOverrideMethod, IMethodSymbol baseMethod)
			{
				var methodsCompatibility = GetMethodsCompatibility(baseMethod.Parameters.Length, pxOverrideMethod.Parameters.Length);

				if (methodsCompatibility == MethodsCompatibility.NotCompatible)
				{
					return false;
				}

				if (!baseMethod.CanBeOverriden() || !baseMethod.IsAccessibleOutsideOfAssembly())
				{
					return false;
				}

				if (methodsCompatibility == MethodsCompatibility.ParametersMatch)
				{
					return CheckExactMatch(pxOverrideMethod, baseMethod);
				}

				if (methodsCompatibility == MethodsCompatibility.ParametersMatchWithDelegate)
				{
					if (pxOverrideMethod.Parameters[pxOverrideMethod.Parameters.Length - 1].Type is not INamedTypeSymbol @delegate || @delegate.TypeKind != TypeKind.Delegate)
					{
						return false;
					}

					return
						DoParametersMatch(baseMethod.Parameters, pxOverrideMethod.Parameters) &&
						CheckExactMatch(baseMethod, @delegate.DelegateInvokeMethod);
				}

				return false;
			}

			private static bool CheckExactMatch(IMethodSymbol pxOverrideMethod, IMethodSymbol delegateInvokeMethod)
			{
				if (!pxOverrideMethod.ReturnType.Equals(delegateInvokeMethod.ReturnType))
				{
					return false;
				}

				if (pxOverrideMethod.Parameters.Length != delegateInvokeMethod.Parameters.Length)
				{
					return false;
				}

				if (!DoParametersMatch(pxOverrideMethod.Parameters, delegateInvokeMethod.Parameters))
				{
					return false;
				}

				return true;
			}

			private static bool DoParametersMatch(ImmutableArray<IParameterSymbol> sourceParameters, ImmutableArray<IParameterSymbol> targetParameters)
			{
				for (var i = 0; i < sourceParameters.Length; i++)
				{
					if (!sourceParameters[i].Type.Equals(targetParameters[i].Type))
					{
						return false;
					}
				}

				return true;
			}

			private static MethodsCompatibility GetMethodsCompatibility(int baseMethodParametersCount, int pxOverrideMethodParametersCount)
			{
				return baseMethodParametersCount == pxOverrideMethodParametersCount ? MethodsCompatibility.ParametersMatch :
					baseMethodParametersCount + 1 == pxOverrideMethodParametersCount ? MethodsCompatibility.ParametersMatchWithDelegate : MethodsCompatibility.NotCompatible;
			}

			private enum MethodsCompatibility
			{
				NotCompatible,
				ParametersMatch,
				ParametersMatchWithDelegate
			}
		}
	}
}
