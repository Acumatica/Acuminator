using Acuminator.Utilities.Common;
using System;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public readonly struct SuppressMessage
	{
		public string Id { get; }

		public string Target { get; }

		public string SyntaxNode { get; }

		private readonly int _hashCode;

		public SuppressMessage(string id, string target, string syntaxNode)
		{
			id.ThrowOnNull(nameof(id));
			target.ThrowOnNull(nameof(target));
			syntaxNode.ThrowOnNull(nameof(syntaxNode));

			Id = id;
			Target = target;
			SyntaxNode = syntaxNode;

			_hashCode = $"{Id}_{Target}_{SyntaxNode}".GetHashCode();
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SuppressMessage message))
			{
				return false;
			}

			if (!message.Id.Equals(Id, StringComparison.Ordinal))
			{
				return false;
			}

			if (!message.Target.Equals(Target, StringComparison.Ordinal))
			{
				return false;
			}

			if (!message.SyntaxNode.Equals(SyntaxNode, StringComparison.Ordinal))
			{
				return false;
			}

			return true;
		}
	}
}
