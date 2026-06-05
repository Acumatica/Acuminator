#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.VisualStudio.Text;

namespace Acuminator.Vsix.Coloriser;

internal class TextDocumentDisposedNotification
{
	private readonly ITextDocumentFactoryService _textDocumentFactory;
	private readonly ITextBuffer _textBuffer;
	private ITextDocument? _textDocument;

	public event EventHandler? CurrentTextDocumentDisposed;

	public TextDocumentDisposedNotification(ITextDocumentFactoryService textDocumentFactory, ITextBuffer textBuffer)
	{
		_textDocumentFactory = textDocumentFactory.CheckIfNull();
		_textBuffer = textBuffer.CheckIfNull();

		SubscribeToDocumentLifetimeEvents();
	}

	private void SubscribeToDocumentLifetimeEvents()
	{
		// If a document already exists for this buffer just attach the disposed handler.
		if (_textDocumentFactory.TryGetTextDocument(_textBuffer, out ITextDocument existingTextDocument))
		{
			SubscribeOnTextDocumentDisposedEvent(existingTextDocument);
		}
		else
		{
			// If the document is created later, attach to the created event and listen for the right document to be created, then attach the disposed handler.
			_textDocumentFactory.TextDocumentCreated += OnTextDocumentCreated;
		}
	}

	private void OnTextDocumentCreated(object sender, TextDocumentEventArgs e)
	{
		// Filter out doc creation events for other documents
		if (e.TextDocument == null || !ReferenceEquals(_textBuffer, e.TextDocument.TextBuffer))
			return;

		//Dispose of the subscription immediately to always run the handler only once
		_textDocumentFactory.TextDocumentCreated -= OnTextDocumentCreated;
		SubscribeOnTextDocumentDisposedEvent(e.TextDocument);
	}

	private void SubscribeOnTextDocumentDisposedEvent(ITextDocument textDocument)
	{
		_textDocument = textDocument;
		_textDocumentFactory.TextDocumentDisposed += OnTextDocumentDisposed;
	}

	private void OnTextDocumentDisposed(object sender, TextDocumentEventArgs e)
	{
		// Filter out dispose events for other documents
		if (!ReferenceEquals(_textDocument, e.TextDocument))
			return;

		//Dispose of the subscription immediately to always run the handler only once
		_textDocumentFactory.TextDocumentDisposed -= OnTextDocumentDisposed;
		CurrentTextDocumentDisposed?.Invoke(this, EventArgs.Empty);
	}
}
