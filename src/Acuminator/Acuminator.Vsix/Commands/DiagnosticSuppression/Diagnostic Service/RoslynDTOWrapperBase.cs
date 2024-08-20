#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A base class for wrappers for Roslyn internal DTO with static shared data which should be initialized only once.
	/// </summary>
	/// <typeparam name="TDerivedWrapper">Type of the derived wrapper. 
	/// The generic is required to have separate _areStaticMembersInitialized flags for different derived wrappers</typeparam>
	internal abstract class RoslynDTOWrapperBase<TDerivedWrapper>
	where TDerivedWrapper : RoslynDTOWrapperBase<TDerivedWrapper>
	{
		private const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _areStaticMembersInitialized = NOT_INITIALIZED;

		public static Type? RoslynDTOType
		{
			get;
			private set;
		}

		protected static Dictionary<string, FieldInfo>? DtoFields
		{
			get;
			private set;
		}

		protected static Dictionary<string, PropertyInfo>? DtoProperties
		{
			get;
			private set;
		}

		protected static void InitializeSharedStaticData(object roslynDTO)
		{
			roslynDTO.ThrowOnNull();

			if (Interlocked.CompareExchange(ref _areStaticMembersInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				RoslynDTOType = roslynDTO.GetType();
				var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

				DtoFields = RoslynDTOType.GetFields(bindingFlags).ToDictionary(field => field.Name);
				DtoProperties = RoslynDTOType.GetProperties(bindingFlags).ToDictionary(property => property.Name);
			}
		}
	}
}
