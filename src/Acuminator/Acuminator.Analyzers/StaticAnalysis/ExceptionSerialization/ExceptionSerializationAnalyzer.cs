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
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ExceptionSerialization
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ExceptionSerializationAnalyzer : PXDiagnosticAnalyzer
	{
		public ExceptionSerializationAnalyzer() : this(codeAnalysisSettings: null)
		{ }

		public ExceptionSerializationAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(
				Descriptors.PX1063_NoSerializationConstructorInException,
				Descriptors.PX1064_NoGetObjectDataOverrideInExceptionWithNewFields);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(context => AnalyzeExceptionTypeForCorrectSerialization(context, pxContext), 
														 SymbolKind.NamedType);
		}

		private static void AnalyzeExceptionTypeForCorrectSerialization(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is not INamedTypeSymbol exceptionType || exceptionType.TypeKind != TypeKind.Class || !IsException(exceptionType, pxContext))
				return;

			bool? hasNewSerializableData = null;
			Location? location = null;

			if (!HasSerializationConstructor(pxContext, exceptionType))
			{
				hasNewSerializableData = GetSerializableFieldsAndProperties(exceptionType, pxContext).Any();
				location = GetDiagnosticLocation(exceptionType, context.CancellationToken);

				if (location == null)
					return;

				ReportMissingSerializationConstructor(context, pxContext, location, hasNewSerializableData.Value);
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			if (!HasGetObjectDataOverride(pxContext, exceptionType))
			{
				hasNewSerializableData ??= GetSerializableFieldsAndProperties(exceptionType, pxContext).Any();

				if (hasNewSerializableData.Value)
				{
					location ??= GetDiagnosticLocation(exceptionType, context.CancellationToken);

					if (location != null)
						ReportMissingGetObjectDataOverride(context, pxContext, location);
				}
			}
		}

		private static bool IsException(INamedTypeSymbol type, PXContext pxContext) => type.InheritsFrom(pxContext.Exceptions.Exception);

		private static bool HasSerializationConstructor(PXContext pxContext, INamedTypeSymbol exceptionType) =>
			exceptionType.InstanceConstructors
						 .Any(constructor => IsSerializationConstructor(constructor, pxContext));

		private static bool IsSerializationConstructor(IMethodSymbol constructor, PXContext pxContext) =>
			constructor.Parameters.Length == 2 && 
			constructor.Parameters[0].Type == pxContext.Serialization.SerializationInfo &&
			constructor.Parameters[1].Type == pxContext.Serialization.StreamingContext;

		private static bool HasGetObjectDataOverride(PXContext pxContext, INamedTypeSymbol exceptionType) =>
			exceptionType.GetMembers()
						 .OfType<IMethodSymbol>()	
						 .Any(method => IsGetObjectDataOverride(method, pxContext));

		private static bool IsGetObjectDataOverride(IMethodSymbol method, PXContext pxContext) =>
			method.MethodKind == MethodKind.Ordinary && method.IsOverride && 
			method.DeclaredAccessibility == Accessibility.Public &&
			method.Name == DelegateNames.Serialization.GetObjectData &&
			method.Parameters.Length == 2 &&
			method.Parameters[0].Type == pxContext.Serialization.SerializationInfo &&
			method.Parameters[1].Type == pxContext.Serialization.StreamingContext;

		private static IEnumerable<ISymbol> GetSerializableFieldsAndProperties(INamedTypeSymbol exceptionType, PXContext pxContext)
		{
			var members = exceptionType.GetMembers();
			
			if (members.IsDefaultOrEmpty)
				return Enumerable.Empty<ISymbol>();

			var allBackingFieldsAssociatedSymbols = members.OfType<IFieldSymbol>()
														   .Where(field => !field.IsStatic && field.AssociatedSymbol != null)
														   .Select(field => field.AssociatedSymbol)
														   .ToHashSet();
			return from member in members
				   where member.IsExplicitlyDeclared() && exceptionType.Equals(member.ContainingType) &&
						 IsSerializableFieldOrProperty(member, pxContext, allBackingFieldsAssociatedSymbols)
				   select member;
		}

		private static bool IsSerializableFieldOrProperty(ISymbol exceptionMember, PXContext pxContext, HashSet<ISymbol> allBackingFieldsAssociatedSymbols)
		{
			if (exceptionMember.IsStatic)
				return false;

			switch (exceptionMember)
			{
				case IFieldSymbol exceptionField when !exceptionField.IsConst:
				case IPropertySymbol exceptionProperty when IsAutoProperty(exceptionProperty):
					break;
				default:
					return false;
			}

			var attributes = exceptionMember.GetAttributes();
			return attributes.IsDefaultOrEmpty 
				? true
				: attributes.All(a => a.AttributeClass != pxContext.Serialization.NonSerializedAttribute);

			//---------------------------------------------------Local Function-------------------------------------------------------------------
			bool IsAutoProperty(IPropertySymbol property) => 
				allBackingFieldsAssociatedSymbols.Contains(property);
		}

		private static void ReportMissingSerializationConstructor(SymbolAnalysisContext context, PXContext pxContext, Location location,
																  bool hasNewSerializableData)
		{
			var diagnosticPropertes =
				ImmutableDictionary<string, string>.Empty
												   .Add(ExceptionSerializationDiagnosticProperties.HasNewDataForSerialization, 
														hasNewSerializableData.ToString());

			var diagnostic = Diagnostic.Create(Descriptors.PX1063_NoSerializationConstructorInException, location, diagnosticPropertes);
			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static void ReportMissingGetObjectDataOverride(SymbolAnalysisContext context, PXContext pxContext, Location location)
		{
			var diagnostic = Diagnostic.Create(Descriptors.PX1064_NoGetObjectDataOverrideInExceptionWithNewFields, location);
			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static Location? GetDiagnosticLocation(INamedTypeSymbol exceptionType, CancellationToken cancellation) =>
			exceptionType.GetSyntax(cancellation) is ClassDeclarationSyntax exceptionDeclaration
				? exceptionDeclaration.Identifier.GetLocation()
				: null;
	}
}
