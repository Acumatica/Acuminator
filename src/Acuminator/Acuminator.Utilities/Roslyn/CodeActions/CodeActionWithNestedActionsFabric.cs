#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.Roslyn.CodeActions
{
	/// <summary>
	/// A code action with nested actions fabric. Obtains internal CodeAction with nested actions. 
	/// In Roslyn v3 the API was made public, so we need to remove this hack after switching to it. 
	/// </summary>
	public static class CodeActionWithNestedActionsFabric
	{
		private const string CodeActionWithNestedActionsTypeName = "CodeActionWithNestedActions";
		private const string CodeActionPriorityTypeFullName = "Microsoft.CodeAnalysis.CodeActions.CodeActionPriority";

		private static object _locker = new object();

		private static volatile Type? _codeActionWithNestedActionsType;
		private static object? _lowCodeActionPriorityInstance;

		private static bool? _isApiWithoutPriorityUsed;
		private static bool? _isVs2022Roslyn;

		private const bool IsCodeActionCreatedFromFactoryMethod = true;

		/// <summary>
		/// Creates a <see cref="CodeAction"/> representing a group of code actions. 
		/// </summary>
		/// <param name="groupActionTitle">Title of the <see cref="CodeAction"/> group.</param>
		/// <param name="nestedCodeActions">The code actions within the group.</param>
		/// <param name="isInlinable"><see langword="true"/> to allow inlining the members of the group into the parent;
		/// otherwise, <see langword="false"/> to require that this group appear as a group with nested actions.</param>
		/// <returns>
		/// The new code action with nested actions.
		/// </returns>
		public static CodeAction? CreateCodeActionWithNestedActions(string groupActionTitle, ImmutableArray<CodeAction> nestedCodeActions,
																    bool isInlinable = false)
		{
			groupActionTitle.ThrowOnNullOrWhiteSpace(nameof(groupActionTitle));

			if (nestedCodeActions.IsDefaultOrEmpty)
				return null;

			InitializeSharedStaticData();

			if (_codeActionWithNestedActionsType == null || _isApiWithoutPriorityUsed == null || _isVs2022Roslyn == null)
				return null;

			try
			{
				if (_isApiWithoutPriorityUsed.Value)
				{
					return Activator.CreateInstance(_codeActionWithNestedActionsType, groupActionTitle, nestedCodeActions, isInlinable) as CodeAction;
				}
				else if (_lowCodeActionPriorityInstance != null)
				{
					BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					var args = _isVs2022Roslyn.Value
						? new[] { groupActionTitle, nestedCodeActions, isInlinable, _lowCodeActionPriorityInstance, IsCodeActionCreatedFromFactoryMethod }
						: new[] { groupActionTitle, nestedCodeActions, isInlinable, _lowCodeActionPriorityInstance };

					return Activator.CreateInstance(_codeActionWithNestedActionsType, bindingFlags, binder: null, args, culture: null) as CodeAction;
				}
				else
					return null;
			}
			catch (Exception e) when (e is MissingMemberException || e is KeyNotFoundException || e is InvalidCastException)
			{
				return null;
			}
		}

		private static void InitializeSharedStaticData()
		{
			if (_codeActionWithNestedActionsType == null)
			{
				lock (_locker)
				{
					if (_codeActionWithNestedActionsType == null)
					{
						InitializeCodeActionWithNestedActionsDataTypeThreadUnsafe();
					}
				}
			}
		}

		private static void InitializeCodeActionWithNestedActionsDataTypeThreadUnsafe()
		{
			System.Reflection.TypeInfo codeActionTypeInfo = typeof(CodeAction).GetTypeInfo();
			var _codeActionWithNestedActionsDataTypeInfo = codeActionTypeInfo.GetDeclaredNestedType(CodeActionWithNestedActionsTypeName);
			_codeActionWithNestedActionsType = _codeActionWithNestedActionsDataTypeInfo?.AsType();

			if (_codeActionWithNestedActionsType == null)
				return;

			var declaredConstructors = _codeActionWithNestedActionsDataTypeInfo!.DeclaredConstructors.ToList();

			if (declaredConstructors.Count == 0)
			{
				_codeActionWithNestedActionsType = null;
				return;
			}

			_isVs2022Roslyn = declaredConstructors.Count > 1;
			var codeActionWithNestedActionsConstructor = _isVs2022Roslyn.Value
				? declaredConstructors.FirstOrDefault(constructor => constructor.GetParameters().Length == 5)
				: declaredConstructors[0];

			if (codeActionWithNestedActionsConstructor == null)
			{
				_codeActionWithNestedActionsType = null;
				_isVs2022Roslyn = null;
				return;
			}

			var parameters = codeActionWithNestedActionsConstructor.GetParameters();

			switch (parameters?.Length)
			{
				case 3:
					_isApiWithoutPriorityUsed = true;
					return;
				case 4:
				case 5:
					_isApiWithoutPriorityUsed = false;
					InitializeCodeActionPriorityInstance(codeActionTypeInfo);
					return;
				default:
					_codeActionWithNestedActionsType = null;
					_isVs2022Roslyn = null;
					return;
			}
		}

		private static void InitializeCodeActionPriorityInstance(System.Reflection.TypeInfo codeActionTypeInfo)
		{
			Type? codeActionPriorityType = codeActionTypeInfo.Assembly.GetType(CodeActionPriorityTypeFullName);

			if (codeActionPriorityType == null)
			{
				_codeActionWithNestedActionsType = null;
				_isApiWithoutPriorityUsed = null;
				_isVs2022Roslyn = null;
				return;
			}

			const int lowCodeActionPriorityValue = 1;

			try
			{
				_lowCodeActionPriorityInstance = Enum.ToObject(codeActionPriorityType, lowCodeActionPriorityValue);
			}
			catch (Exception e) when (e is MissingMemberException || e is KeyNotFoundException || e is InvalidCastException)
			{
				_codeActionWithNestedActionsType = null;
				_lowCodeActionPriorityInstance = null;
				_isApiWithoutPriorityUsed = null;
				_isVs2022Roslyn = null;
			}
		}
	}
}
