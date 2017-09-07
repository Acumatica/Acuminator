using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;


namespace PX.Analyzers.Coloriser
{
	public class ParsedDocument
	{
		//public Workspace Workspace { get; private set; }
		//public Document Document { get; private set; }
		//public SemanticModel SemanticModel { get; private set; }
		//public SyntaxNode SyntaxRoot { get; private set; }

		//public ITextSnapshot Snapshot { get; private set; }

		//private ParsedDocument()
		//{
		//}

		//public static async Task<ParsedDocument> Resolve(ITextBuffer buffer, ITextSnapshot snapshot)
		//{
		//	Workspace workspace = buffer.GetWorkspace();
		//	Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
		
		//	var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);  // the ConfigureAwait() calls are important, otherwise we'll deadlock VS
		//	var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);

		//	return new ParsedDocument
		//	{
		//		Workspace = workspace,
		//		Document = document,
		//		SemanticModel = semanticModel,
		//		SyntaxRoot = syntaxRoot,
		//		Snapshot = snapshot
		//	};
		//}
	}
}
