#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal class MappedDacPropertyFinder
	{
		private readonly PXContext _pxContext;

		public MappedDacPropertyFinder(PXContext pxContext)
		{
			_pxContext = pxContext;
		}

		private IPropertySymbol? GetDacPropertyCorrespondingToTheMappedBqlField(ITypeSymbol mappedBqlField)
		{
			if (mappedBqlField.ContainingType == null)
				return null;

			DacType? dacType = mappedBqlField.ContainingType.GetDacType(_pxContext);

			if (dacType == null)
				return null;
			else if (dacType == DacType.DacExtension)
				return GetDacPropertySymbol(mappedBqlField.ContainingType, mappedBqlField.Name);

			INamedTypeSymbol? currentDac = mappedBqlField.ContainingType;

			while (currentDac != null)
			{
				var property = GetDacPropertySymbol(currentDac, mappedBqlField.Name);

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
						  .FirstOrDefault(property => caseInsensitivePropertyName.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
		}
	}
}