using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	/// <summary>
	/// The class holds information about suppression of reported Acuminator's diagnostic 
	/// </summary>
	public readonly struct SuppressMessage : IEquatable<SuppressMessage>, IComparable<SuppressMessage>
    {
		private static HashSet<SyntaxKind> _targetKinds = new HashSet<SyntaxKind>(new[] {
			SyntaxKind.ClassDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.FieldDeclaration
		});

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
			id.ThrowOnNull(nameof(id));
			target.ThrowOnNull(nameof(target));
			syntaxNode.ThrowOnNull(nameof(syntaxNode));

			Id = id;
			Target = target;
			SyntaxNode = syntaxNode;

			var hash = 17;

			unchecked
			{
				hash = 23 * hash + Id.GetHashCode();
				hash = 23 * hash + Target.GetHashCode();
				hash = 23 * hash + SyntaxNode.GetHashCode();
			}

			_hashCode = hash;
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public bool Equals(SuppressMessage other)
		{
			if (!other.Id.Equals(Id, StringComparison.Ordinal))
			{
				return false;
			}

			if (!other.Target.Equals(Target, StringComparison.Ordinal))
			{
				return false;
			}

			if (!other.SyntaxNode.Equals(SyntaxNode, StringComparison.Ordinal))
			{
				return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SuppressMessage message))
			{
				return false;
			}

			return Equals(message);
		}

        public int CompareTo(SuppressMessage other)
        {
            if (Equals(other))
            {
                return 0;
            }

            var idComparison = string.CompareOrdinal(Id, other.Id);
            if (idComparison != 0)
            {
                return idComparison;
            }

            var targetComparison = string.CompareOrdinal(Target, other.Target);
            if (targetComparison != 0)
            {
                return targetComparison;
            }

            var syntaxNodeComparison = string.CompareOrdinal(SyntaxNode, other.SyntaxNode);
            if (syntaxNodeComparison != 0)
            {
                return syntaxNodeComparison;
            }

            return 0;
        }

		public static (string Assembly, SuppressMessage Message) GetSuppressionInfo(SemanticModel semanticModel, Diagnostic diagnostic,
																					 CancellationToken cancellation = default)
		{
			return diagnostic?.Location != null
				? GetSuppressionInfo(semanticModel, diagnostic?.Id, diagnostic.Location.SourceSpan, cancellation)
				: (null, default);
		}

		public static (string Assembly, SuppressMessage Message) GetSuppressionInfo(SemanticModel semanticModel, string diagnosticID, 
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

		private static SyntaxNode FindTargetNode(SyntaxNode node)
		{
			if (node == null)
				return null;

			var targetNode = node
				.AncestorsAndSelf()
				.Where(a => _targetKinds.Contains(a.Kind()))
				.FirstOrDefault();

			if (!(targetNode is FieldDeclarationSyntax fieldDeclaration))
				return targetNode;
			else if (fieldDeclaration.Declaration == null)
				return null;

			SeparatedSyntaxList<VariableDeclaratorSyntax> declaredVariables = fieldDeclaration.Declaration.Variables;

			switch (declaredVariables.Count)
			{
				case 0:
					return null;
				case 1:
					return declaredVariables[0];
				default:
					return declaredVariables.FirstOrDefault(variableDeclarator => variableDeclarator.Contains(node));
			}
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
