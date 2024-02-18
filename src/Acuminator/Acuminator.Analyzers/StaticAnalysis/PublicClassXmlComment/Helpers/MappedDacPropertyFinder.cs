#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal class MappedDacPropertyFinder
	{
		private enum MappingType : byte
		{
			/// <summary>
			/// Mapping is specified via BqlField property.
			/// </summary>
			BqlField,

			/// <summary>
			/// Mapping is specified via BqlTable property.
			/// </summary>
			BqlTable,

			/// <summary>
			/// Mapping is specified via inheritance of a projection DAC from the original DAC.
			/// </summary>
			Inheritance
		}

		private readonly PXContext _pxContext;
		private readonly SemanticModel _semanticModel;
		private readonly CancellationToken _cancellation;

		public MappedDacPropertyFinder(PXContext pxContext, SemanticModel semanticModel, CancellationToken cancellation)
		{
			_pxContext = pxContext;
			_semanticModel = semanticModel;
			_cancellation = cancellation;
		}

		public IPropertySymbol? GetMappedPropertyFromTheOriginalDac(TypeInfo typeInfo, PropertyDeclarationSyntax projectionDacPropertyDeclaration)
		{
			if (typeInfo.ContainingType == null || !typeInfo.IsProjectionDacOrExtensionToProjectionDac ||
				projectionDacPropertyDeclaration.AttributeLists.Count == 0)
			{
				return null;
			}

			if (_semanticModel.GetDeclaredSymbol(projectionDacPropertyDeclaration, _cancellation) is not IPropertySymbol projectionDacProperty)
				return null;

			var mappedDacProperty = GetMappedDacProperty(typeInfo, projectionDacProperty);

			_cancellation.ThrowIfCancellationRequested();
			return mappedDacProperty;
		}

		private IPropertySymbol? GetMappedDacProperty(TypeInfo typeInfo, IPropertySymbol projectionDacProperty)
		{
			MappingType mappingType;
			INamedTypeSymbol? originalDacOrDacBqlField		 = null;
			IPropertySymbol? mappedPropertyFromBaseDacTypes  = null;
			var propertyMappingInfoFromAttribute = GetMappingInfoFromAttribute(projectionDacProperty);

			if (propertyMappingInfoFromAttribute != null)
			{
				(mappingType, TypedConstant mappingInfo) = propertyMappingInfoFromAttribute.Value;
				originalDacOrDacBqlField				 = mappingInfo.Value as INamedTypeSymbol;
			}
			else
			{
				// The projection DAC property needs to be overridden to support binding to a property of a base DAC
				if (!projectionDacProperty.IsOverride ||
					!typeInfo.PropertiesFromBaseDacs.TryGetValue(projectionDacProperty.Name, out mappedPropertyFromBaseDacTypes))
				{
					return null;
				}

				mappingType = MappingType.Inheritance;
			}

			_cancellation.ThrowIfCancellationRequested();

			switch (mappingType)
			{
				case MappingType.BqlField:
					return originalDacOrDacBqlField?.ContainingType == null
						? null
						: GetMappedDacPropertyFromMappedDacOrDacExtension(originalDacOrDacBqlField.ContainingType, originalDacOrDacBqlField.Name);

				case MappingType.BqlTable:
					if (originalDacOrDacBqlField == null) 
						return null;

					string mappedPropertyName = projectionDacProperty.Name;
					return GetMappedDacPropertyFromMappedDacOrDacExtension(originalDacOrDacBqlField, mappedPropertyName);

				case MappingType.Inheritance:
					return mappedPropertyFromBaseDacTypes;

				default:
					return null;
			}
		}

		private (MappingType MappingType, TypedConstant MappingInfoFromAttribute)? GetMappingInfoFromAttribute(IPropertySymbol projectionDacProperty)
		{
			var attributes = projectionDacProperty.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return null;

			foreach (AttributeData attributeApplication in attributes)
			{
				var namedArguments = attributeApplication.NamedArguments;

				if (namedArguments.IsDefaultOrEmpty)
					continue;

				foreach (var (argumentName, argumentValue) in namedArguments)
				{
					if (argumentValue.Kind != TypedConstantKind.Type)
						continue;

					if (argumentName == PropertyNames.Attributes.BqlField)
						return (MappingType.BqlField, argumentValue);
					else if (argumentName == PropertyNames.Attributes.BqlTable)
						return (MappingType.BqlTable, argumentValue);
				}
			}

			return null;
		}

		private IPropertySymbol? GetMappedDacPropertyFromMappedDacOrDacExtension(INamedTypeSymbol mappedDacOrDacExtension,
																				 string caseInsensitivePropertyName)
		{
			DacType? dacType = mappedDacOrDacExtension.GetDacType(_pxContext);

			if (dacType == null)
				return null;
			else if (dacType == DacType.DacExtension)
				return GetDacPropertySymbol(mappedDacOrDacExtension, caseInsensitivePropertyName);

			INamedTypeSymbol? currentDac = mappedDacOrDacExtension;

			while (currentDac != null)
			{
				var property = GetDacPropertySymbol(currentDac, caseInsensitivePropertyName);

				if (property != null)
					return property;

				currentDac = currentDac.BaseType;
			}

			return null;
		}

		private IPropertySymbol? GetDacPropertySymbol(INamedTypeSymbol type, string caseInsensitivePropertyName)
		{
			var members = type.GetMembers();

			if (members.IsDefaultOrEmpty)
				return null;

			return members.OfType<IPropertySymbol>()
						  .FirstOrDefault(property => caseInsensitivePropertyName.Equals(property.Name, StringComparison.OrdinalIgnoreCase) && 
													  !property.IsStatic && !property.IsIndexer && !property.IsAbstract);
		}
	}
}