﻿using System;
using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the graph events related to the DAC fields.
	/// </summary>
	public class GraphFieldEventInfo : GraphEventInfoBase<GraphFieldEventInfo>
	{
		/// <summary>
		/// The DAC field name.
		/// </summary>
		public string DacFieldName { get; }

		public GraphFieldEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
								   EventHandlerSignatureType signatureType, EventType eventType) :
							  base(node, symbol, declarationOrder, signatureType, eventType)
		{
			ValidateEventType(eventType);
			DacFieldName = GetDacFieldName();
		}

		public GraphFieldEventInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
								   EventHandlerSignatureType signatureType, EventType eventType, GraphFieldEventInfo baseInfo)
							: base(node, symbol, declarationOrder, signatureType, eventType, baseInfo)
		{
			ValidateEventType(eventType);
			DacFieldName = GetDacFieldName();
		}

		private void ValidateEventType(EventType eventType)
		{
			if (!IsDacFieldEvent())
				throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType.ToString()} is not a field event type.");
		}

		private string GetDacFieldName()
		{
			switch (SignatureType)
			{
				case EventHandlerSignatureType.Default:
					return GetDacFieldNameForDefaultFieldEvent();
				case EventHandlerSignatureType.Generic when !Symbol.Parameters.IsDefaultOrEmpty:
					return GetDacFieldNameForGenericFieldEvent();
				default:
					return string.Empty;
			}
		}

		private string GetDacFieldNameForDefaultFieldEvent()
		{
			string eventName = Symbol.Name;

			if (eventName.Length < 5 || eventName[0] == '_' || eventName[eventName.Length - 1] == '_')
			{
				return string.Empty;
			}

			int underscoreCount = 0;
			int firstUnderscoreIndex = -1, lastUnderscoreIndex = -1;

			for (int i = 0; i < eventName.Length; i++)
			{
				if (eventName[i] != '_')
					continue;

				if (underscoreCount == 0)
				{
					firstUnderscoreIndex = i;
				}
				else
				{
					lastUnderscoreIndex = i;
				}

				underscoreCount++;
			}

			if (underscoreCount != 2 || firstUnderscoreIndex < 0 || lastUnderscoreIndex < 0)
				return string.Empty;

			int length = lastUnderscoreIndex - firstUnderscoreIndex - 1;

			if (length == 0)
				return string.Empty;

			return eventName.Substring(firstUnderscoreIndex + 1, length);
		}

		private string GetDacFieldNameForGenericFieldEvent()
		{
			if (!(Symbol.Parameters[0]?.Type is INamedTypeSymbol firstParameter) || firstParameter.TypeArguments.Length < 2)
			{
				return string.Empty;
			}

			ITypeSymbol dacField = firstParameter.TypeArguments[1];
			return dacField.IsDacField()
				? dacField.Name.ToPascalCase()
				: string.Empty;
		}
	}
}
