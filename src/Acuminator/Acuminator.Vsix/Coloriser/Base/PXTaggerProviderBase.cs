#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

using Path = System.IO.Path;

namespace Acuminator.Vsix.Coloriser
{
	public abstract class PXTaggerProviderBase
	{
		public Workspace? Workspace { get; private set; }

		/// <summary>
		/// Initializes the base <see cref="Workspace"/>.
		/// </summary>
		protected virtual void Initialize(ITextBuffer buffer)
		{
			Workspace = buffer?.GetWorkspace();
		}
	}
}
