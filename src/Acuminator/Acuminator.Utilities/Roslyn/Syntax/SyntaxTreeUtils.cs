using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public static class SyntaxTreeUtils
	{
		/// <summary>
		/// Get the syntax tree root from the given node.
		/// </summary>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SyntaxNode Root(this SyntaxNode node)
		{
			if (node == null)
				return null;

			SyntaxNode curNode = node;

			while (curNode.Parent != null)
			{
				curNode = curNode.Parent;
			}

			return curNode;
		}

		/// <summary>
		/// Get a parent with a specified type <typeparamref name="TParent"/>
		/// </summary>
		/// <typeparam name="TParent">Parent node type.</typeparam>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TParent Parent<TParent>(this SyntaxNode node)
		where TParent : SyntaxNode
		{
			if (node == null)
				return null;

			SyntaxNode curNode = node.Parent;

			while (curNode != null)
			{
				if (curNode is TParent parent)
					return parent;

				curNode = curNode.Parent;
			}

			return null;
		}

		/// <summary>
		/// Get depths of the given syntax node measuring from the root.
		/// </summary>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Depth(this SyntaxNode node)
		{
			node.ThrowOnNull(nameof(node));

			int depth = 0;
			SyntaxNode curNode = node.Parent;

			while (curNode != null)
			{
				depth++;
				curNode = curNode.Parent;
			}

			return depth;
		}

		/// <summary>
		/// Get depths of the given syntax node measuring from the first ancestor of type <typeparamref name="TRoot"/>. 
		/// If node doesn't have the ancestor of this type then depth from the root is returned.
		/// </summary>
		/// <typeparam name="TRoot">Type of the root.</typeparam>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Depth<TRoot>(this SyntaxNode node)
		where TRoot : SyntaxNode
		{
			node.ThrowOnNull(nameof(node));

			if (node is TRoot)
				return 0;

			int depth = 0;
			SyntaxNode curNode = node.Parent;

			while (curNode != null && !(curNode is TRoot))
			{
				depth++;
				curNode = curNode.Parent;
			}

			return depth;
		}

		/// <summary>
		/// Get depths of the given syntax node measuring from the first ancestor of type <typeparamref name="TRoot"/> and taking into account only nodes
		/// of type <typeparamref name="TNode"/>. If node doesn't have the ancestor of this type then the depth (taking into account only nodes of type
		/// <typeparamref name="TNode"/>) from the root is returned.
		/// </summary>
		/// <typeparam name="TRoot">Type of the root.</typeparam>
		/// <typeparam name="TNode">Type of the node.</typeparam>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Depth<TRoot, TNode>(this TNode node)
		where TRoot : SyntaxNode
		where TNode : SyntaxNode
		{
			node.ThrowOnNull(nameof(node));

			if (node is TRoot)
				return 0;

			int depth = 0;
			TNode curNode = node.Parent<TNode>();

			while (curNode != null && !(curNode is TRoot))
			{
				depth++;
				curNode = curNode.Parent<TNode>();
			}

			return depth;
		}

		public static SyntaxNode LowestCommonAncestor(SyntaxNode nodeX, SyntaxNode nodeY)
		{
			nodeX.ThrowOnNull(nameof(nodeX));
			nodeY.ThrowOnNull(nameof(nodeY));

			int depthX = nodeX.Depth();            //Depth is average O(log n) operation, worst case is O(n) but it isn't the case for the syntax tree which is wide but not very deep
			int depthY = nodeY.Depth();

			SyntaxNode currentX = nodeX;
			SyntaxNode currentY = nodeY;

			while (depthX != depthY)                //First get nodes on the equal levels of depth
			{
				if (depthX > depthY)
				{
					currentX = currentX.Parent;
					depthX--;
				}
				else
				{
					currentY = currentY.Parent;
					depthY--;
				}
			}

			while (currentX != currentY)          //Then move up the branches until nodes coincide
			{
				currentX = currentX.Parent;
				currentY = currentY.Parent;
			}

			return currentX;
		}
	}
}
