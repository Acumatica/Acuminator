﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	/// <summary>
	/// The class holds information about suppression of reported Acuminator's diagnostic 
	/// </summary>
	public readonly struct SuppressMessage : IEquatable<SuppressMessage>
    {
		private const string IdAttribute = "id";
		private const string TargetElement = "target";
		private const string SyntaxNodeElement = "syntaxNode";

		private static HashSet<SyntaxKind> _targetKinds = 
		[
			SyntaxKind.ClassDeclaration,
			SyntaxKind.StructDeclaration,
			SyntaxKind.EnumDeclaration,
			SyntaxKind.InterfaceDeclaration,
			SyntaxKind.DelegateDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.FieldDeclaration
		];

		/// <summary>
		/// Suppressed diagnostic Id
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Description of a member declaration where suppressed diagnostic is located
		/// </summary>
		public string Target { get; }

		/// <summary>
		/// Syntax node of a suppressed diagnostic
		/// </summary>
		public string SyntaxNode { get; }

		/// <summary>
		/// True if this structure is correctly initialized, false if not.
		/// </summary>
		
		public bool IsValid => !Id.IsNullOrWhiteSpace() && !Target.IsNullOrWhiteSpace() && !SyntaxNode.IsNullOrWhiteSpace();

		private readonly int _hashCode;

		public SuppressMessage(string id, string target, string syntaxNode)
		{
			Id = id.CheckIfNull();
			Target = target.CheckIfNull();
			SyntaxNode = syntaxNode.CheckIfNull();

			var hash = 17;

			unchecked
			{
				hash = 23 * hash + Id.GetHashCode();
				hash = 23 * hash + Target.GetHashCode();
				hash = 23 * hash + SyntaxNode.GetHashCode();
			}

			_hashCode = hash;
		}

		public static SuppressMessage? MessageFromElement(XElement element)
		{
			string? id = element?.Attribute(IdAttribute)?.Value;
			if (id.IsNullOrWhiteSpace())
				return null;

			string? target = element!.Element(TargetElement)?.Value;
			if (target.IsNullOrWhiteSpace())
				return null;

			string? syntaxNode = element.Element(SyntaxNodeElement)?.Value;
			if (syntaxNode.IsNullOrWhiteSpace())
				return null;

			return new SuppressMessage(id, target, syntaxNode);
		}

		public XElement? ToXml()
		{
			if (!IsValid)
				return null;

			return new XElement(SuppressionFile.SuppressMessageElement,
				new XAttribute(IdAttribute, Id),
				new XElement(TargetElement, Target),
				new XElement(SyntaxNodeElement, SyntaxNode));
		}

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object obj) => obj is SuppressMessage message && Equals(message);

		public bool Equals(SuppressMessage other) =>
			other.Id.Equals(Id, StringComparison.Ordinal) &&
			other.Target.Equals(Target, StringComparison.Ordinal) &&
			other.SyntaxNode.Equals(SyntaxNode, StringComparison.Ordinal);

		public override string ToString() => $"ID={Id}, Target={Target}";

		internal static (string? Assembly, SuppressMessage Message) GetSuppressionInfo(SemanticModel semanticModel, Diagnostic diagnostic,
																					 CancellationToken cancellation = default)
		{
			return diagnostic?.Location != null
				? GetSuppressionInfo(semanticModel, diagnostic.Id, diagnostic.Location.SourceSpan, cancellation)
				: (null, default);
		}

		internal static (string? Assembly, SuppressMessage Message) GetSuppressionInfo(SemanticModel semanticModel, string diagnosticID, 
																					  TextSpan diagnosticSpan, CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			if (semanticModel == null || diagnosticID.IsNullOrWhiteSpace())
				return (null, default);

			var rootNode = semanticModel.SyntaxTree.GetRoot(cancellation);
			if (rootNode == null)
				return (null, default);

			var diagnosticNode = rootNode.FindNode(diagnosticSpan);
			if (diagnosticNode == null)
				return (null, default);

			var targetNode = FindTargetNode(diagnosticNode);
			if (targetNode == null)
				return (null, default);

			var targetSymbol = semanticModel.GetDeclaredSymbol(targetNode, cancellation);
			if (targetSymbol == null)
				return (null, default);

			var assemblyName = targetSymbol.ContainingAssembly?.Name;
			if (string.IsNullOrEmpty(assemblyName))
				return (null, default);

			var target = targetSymbol.ToDisplayString();
			string syntaxNodeString = GetSyntaxNodeStringForSuppressionMessage(diagnosticNode,
																			   diagnosticPosition: diagnosticSpan.Start);
			var message = new SuppressMessage(diagnosticID, target, syntaxNodeString);
			return (assemblyName, message);
		}

		private static SyntaxNode? FindTargetNode(SyntaxNode node)
		{
			var targetNode = node?.AncestorsAndSelf().FirstOrDefault(a => _targetKinds.Contains(a.Kind()));

			if (targetNode is not FieldDeclarationSyntax fieldDeclaration)
				return targetNode;
			else if (fieldDeclaration.Declaration == null)
				return null;

			SeparatedSyntaxList<VariableDeclaratorSyntax> declaredVariables = fieldDeclaration.Declaration.Variables;

			return declaredVariables.Count switch
			{
				0 => null,
				1 => declaredVariables[0],
				_ => declaredVariables.FirstOrDefault(variableDeclarator => variableDeclarator.Contains(node))
			};
		}

		private static string GetSyntaxNodeStringForSuppressionMessage(SyntaxNode diagnosticNode, int diagnosticPosition)
		{
			// Try to obtain token in case of member declaration syntax as we do not want to store the text of the entire declaration node
			SyntaxToken? token = null;

			if (diagnosticNode is MemberDeclarationSyntax memberDeclaration)
			{
				try
				{
					token = memberDeclaration.FindToken(diagnosticPosition);
				}
				catch (ArgumentOutOfRangeException)
				{
					token = null;
				}
			}

			return token != null
				? token.ToString()
				: diagnosticNode.ToString().Replace("\r", "");  // Replace \r symbol as XDocument does not preserve it in suppression file
		}
	}
}
