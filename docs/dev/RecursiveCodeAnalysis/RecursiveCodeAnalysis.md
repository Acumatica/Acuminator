# Recursive Code Analysis
## Problem Statement
For many diagnostics, it is very useful to check not only the original syntax node, but also all the code that is invoked within that node.

A quick example. Let's assume that we want to show a diagnostic if a user starts a long operation from an event handler:

```C#
protected virtual void _(Events.RowSelected<SOOrder> e)
{
    PXLongOperation.Start(() => Release(e.Row)); // show a diagnostic here
}
```

It is very typical to extract some parts of the logic to a separate method / class / etc. like in this example:

```C#
private void PerformRelease(SOOrder row)
{
    PXLongOperation.Start(() => Release(e.Row));
}

protected virtual void _(Events.RowSelected<SOOrder> e)
{
    PerformRelease(e.Row); // a diagnostic should be here
}
```

But such analysis is quite complex, and it is not convenient to write it for every diagnostic you create.

So, there is a special class that incapsulates this logic and allows you to re-use it in every new diagnostic. It is called `NestedInvocationWalker`.

## NestedInvocationWalker
`NestedInvocationWalker` is a C# syntax walker that follows any code invocations found in the original syntax node being analyzed. It directly inherits from `CSharpSyntaxWalker`.

For each invocation, it retrieves a linked symbol, founds declaring syntax reference (if a source code for the symbol is available) and analyzes it recursively.

It supports the following invocations:

 - Constructors
 - Method invocations (including expression-bodied methods)
 - Property getters and setters (both from assignment and from object initializers)
 - Local lambda functions

This behavior can be enabled or disabled depending on the corresponding setting on the Options page. It is enabled by default, and it is always enabled if the analyzers are used as a NuGet package.

![Options Page](Options.png)


### How to Use
When writing a new diagnostic, you can derive from `NestedInvocationWalker` to obtain recursive analysis behavior.
In this case, special methods from the base class must be used instead of default ones:

#### `SemanticModel GetSemanticModel(SyntaxTree syntaxTree)`
`NestedInvocationWalker` walks across different syntax trees and different documents, and `SemanticModel` is tied to the current `SyntaxTree`. This method must be used every time when you need a `SemanticModel` for a node.

#### `void ReportDiagnostic(Action<Diagnostic> reportDiagnostic, DiagnosticDescriptor diagnosticDescriptor, SyntaxNode node, params object[] messageArgs)`
Reports a diagnostic for the provided descriptor on the original syntax node from which recursive analysis started.
This method must be used for all diagnostic reporting within the walker because it does diagnostic deduplication and determines the right syntax node to perform diagnostic reporting.

```C#
private void PerformRelease(SOOrder row)
{
    // Invalid call is found here, but we don't want to report it
    // in this method because the object under analysis
    // is an event handler method declaration
    PXLongOperation.Start(() => Release(e.Row));
}

protected virtual void _(Events.RowSelected<SOOrder> e)
{
    PerformRelease(e.Row); // a diagnostic should be reported for this node
}
```

#### `T GetSymbol<T>(ExpressionSyntax node) where T : class, ISymbol`
Helper method that returns either a precise symbol or the most appropriate candidate, and tries to cast it to the requested symbol type.

#### `void ThrowIfCancellationRequested()`
Helper method that checks a cancellation token that has been passed to the constructor. Should be preferred over the manual checks on a cancellation token.

#### Overriding `VisitXXX` Methods
When you override some of the `Visit` methods, don't forget to call the `base` implementation in case if diagnostic is not reported. But if a diagnostic has already been reported for the current node, don't call `base` implementation to avoid unneccessary visits.

A typical pattern looks as follows:

```C#
public override void VisitInvocationExpression(InvocationExpressionSyntax node)
{
	ThrowIfCancellationRequested();

	var methodSymbol = GetSymbol<IMethodSymbol>(node);

	if (methodSymbol != null && ShouldReportDiagnostic(methodSymbol))
	{
		ReportDiagnostic(_context.ReportDiagnostic, Descriptors.DiagnosticDescriptor, node);
	}
	else
	{
		base.VisitInvocationExpression(node);
	}
}
```
