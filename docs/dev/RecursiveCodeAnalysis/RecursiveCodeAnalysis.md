# Recursive Code Analysis
Acuminator can analyze code recursively (that is, it can analyze the whole tree of method invocations in a recursive manner).
Acuminator performs this recursive code analysis by default. In Visual Studio, you can turn off this behavior by setting the value of **Tools > Options > Acuminator > Code Analysis > Enable recursive code analysis** to `False`. (This option is shown in the following screenshot.) 

![Options Page](Options.png)

Recursive code analysis cannot be turned off if Acuminator is used as a NuGet package.

## Development of Recursive Code Analysis
For many diagnostics, Acuminator should check not only the original syntax node, but also all the code that is invoked within that node.

For example, suppose that you want to show a diagnostic message if a long-running operation is started from an event handler. In the following code example, a long-running operation is invoked directly from the event handler.

```C#
protected virtual void _(Events.RowSelected<SOOrder> e)
{
    PXLongOperation.Start(() => Release(e.Row)); // Show a diagnostic message here.
}
```

However, a part of the logic of the event handler can be moved to a separate method or class, as shown in the following example. If you want to show the diagnostic message in the original syntax node, you need to recursively analyze the code of the event handler and of the methods invoked within the handler.

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

Because the analysis of the whole tree of invocations is quite complex, making it inconvenient to write this analysis for every diagnostic you create, the code of Acuminator includes a special class, `NestedInvocationWalker`, that incapsulates this logic. With this class, you can reuse the recursive code analysis in every new diagnostic.

## `NestedInvocationWalker` Class
`NestedInvocationWalker` is a C# syntax walker that follows any code invocations found in the original syntax node being analyzed. `NestedInvocationWalker` directly inherits from `CSharpSyntaxWalker`.

For each invocation, `NestedInvocationWalker` obtains a linked symbol, finds the declaring syntax reference (if source code for the linked symbol is available), and analyzes the referenced code recursively.

`NestedInvocationWalker` supports the following invocations:

 - Constructors
 - Method invocations (including expression-bodied methods)
 - Property getters and setters (both from assignment and from object initializers)
 - Local lambda functions

## The Use of the `NestedInvocationWalker` Class
If you need to use the recursive code analysis in a new diagnostic, you derive a syntax walker class from `NestedInvocationWalker`. You must use the following special methods from the base `NestedInvocationWalker` class, which are described below:

 - `SemanticModel GetSemanticModel(SyntaxTree syntaxTree)`
 - `void ReportDiagnostic(Action<Diagnostic> reportDiagnostic, DiagnosticDescriptor diagnosticDescriptor, SyntaxNode node, params object[] messageArgs)`
 - `T GetSymbol<T>(ExpressionSyntax node) where T : class, ISymbol`
 - `void ThrowIfCancellationRequested()`

### `SemanticModel GetSemanticModel(SyntaxTree syntaxTree)`
This method obtains a `SemanticModel` for a node. `NestedInvocationWalker` walks across different syntax trees and different documents; the returned `SemanticModel` is tied to the current `SyntaxTree`. 
You should use this method each time you need the `SemanticModel` for a node.

### `void ReportDiagnostic(Action<Diagnostic> reportDiagnostic, DiagnosticDescriptor diagnosticDescriptor, SyntaxNode node, params object[] messageArgs)`
This method reports a diagnostic for the provided descriptor on the syntax node from which the recursive analysis has been started. This method excludes diagnostic duplication and determines the correct syntax node to report the diagnostic. For the following code example, the method would display a diagnostic message in the event handler.

```C#
private void PerformRelease(SOOrder row)
{
    // An invalid call is found here, but we don't want to report it
    // in this method because the object under analysis
    // is an event handler method declaration.
    PXLongOperation.Start(() => Release(e.Row));
}

protected virtual void _(Events.RowSelected<SOOrder> e)
{
    PerformRelease(e.Row); // A diagnostic should be reported for this node.
}
```

You should use this method for each diagnostic report within the walker.

### `T GetSymbol<T>(ExpressionSyntax node) where T : class, ISymbol`
This method returns a precise symbol or the most appropriate candidate and tries to cast it to the requested symbol type.

### `void ThrowIfCancellationRequested()`
This method verifies a cancellation token that has been passed to the constructor. You should use this method instead of manual verification of a cancellation token.

## Overriding of the Methods of the `Visit` Family
If you override any of the `Visit` methods, you have to do the following:

 - If the diagnostic has not been reported for the current node yet, call the `base` implementation. 
 - If the diagnostic has already been reported for the current node, don't call `base` implementation to avoid unnecessary visits.

The following code example shows a typical pattern of overriding of a `Visit` method.

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
