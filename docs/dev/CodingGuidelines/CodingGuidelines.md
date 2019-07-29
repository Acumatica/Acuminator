# Coding Guidelines

## Table of Contents

* [Code Style](#code-style)
    * [Naming of Private and Protected Fields](#naming-of-private-and-protected-fields)
    * [Naming of Constants](#naming-of-constants)
    * [Naming of Asynchronous Methods](#naming-of-asynchronous-methods)
    * [Naming of Value Tuples](#naming-of-value-tuples)
    * [Naming of Test Methods](#naming-of-test-methods)
    * [Indentation Depth](#indentation-depth)
    * [Control Flow Statements](#control-flow-statements)
    * [Local Functions](#local-functions)
* [Best Practices](#best-practices)
    * [Unit Tests](#unit-tests)
    * [Cancellation Support](#cancellation-support)
    * [Demo Solution](#demo-solution)
    * [Code Reuse](#code-reuse)
    * [Task Blocking](#task-blocking)
    * [Task Awaiting](#task-awaiting)
    * [Parametrized Diagnostic Messages](#parametrized-diagnostic-messages)
    * [Test Methods](#test-methods)
    * [Async Anonymous Delegates](#async-anonymous-delegates)

## Code Style

The [.NET Framework Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines) must be used in addition to our own.

### Naming of Private and Protected Fields

An underscore must be used as a prefix for private and protected fields in classes.

```C#
public class MyClass
{
  private object wronglyNamedField;       //Incorrect naming
 
  protected object _correctlyNamedField;  //Correct naming
}
```

### Naming of Constants

Names of constants must begin with a capital letter.

```C#
private const string SetValueMethodName = "SetValue"; // Correct
private const string _setValueMethodName = "SetValue"; // Incorrect
```

### Naming of Asynchronous Methods

Asynchronous methods must have the `Async` postfix.

```C#
public Task<int> GetCountAsync() { } // Correct
public Task<int> GetCount() { } // Incorrect
public async Task ExecuteAsync() { } // Correct
public async Task Execute() { } // Incorrect
```

### Naming of Value Tuples

The names of the properties in `ValueTuple` must use *PascalCase*, except the case when the tuple is a local variable.

```C#
public (int Line, int Column) GetPosition() { } // Correct
public (int line, int column) GetPosition() { } // Incorrect
```

### Naming of Test Methods

Test methods should be named in *PascalCase* with the underscore character as a separator between logical statements.

```C#
public Task EventHandlersWithExternalMethod(string actual) { } // Valid
public Task EventHandlers_ShouldNotShowDiagnostic(string actual) { } // Valid
public Task TestCodeFix_RowSelected(string actual, string expected) { } // Valid
public Task TestDiagnostic_RowSelected(string actual) { } // Valid if the same class also contains the tests for the code fix

public Task No_Connection_Scope_In_Row_Selected(string actual) { } // Invalid
public Task Test_CodeFix_For_RowSelecting(string actual) { } // Invalid
```

### Indentation Depth

Indentation depth (brace block levels) should be no more than three.

```C#
public void Foo(bool flag, IEnumerable collection)
{
  if (collection != null)
  {
      // some code
 
      if (flag)
      {
        // some code
       
        foreach(object item in collection)
        {
           //Maximum allowed level of indentation
           if (item != null)
           {
              //identation level > 3, invalid
           }
        }
      }
   }
}
```

### Control Flow Statements

Control flow statements (such as `if `, `while`, `for`, `foreach`, `switch`, `do`-`while`) should be separated with empty lines.

```C#
public bool Foo(bool flag)
{
    DoSomething();

    if (flag)
        return true;

    return false;
}
```

A logical grouping of the code can be an exception to this rule.

```C#
public bool Foo(bool flag)
{
    DoSomething();

    bool condition = flag & GetStatus();
    if (condition)
        return true;

    return false;
}
```

### Local Functions

The local functions from C# 7 should be used with caution. A general recommendation is to not overuse this feature.

The local functions can be used in the following three cases:

* If you implement a generator method with the argument check that is made immediately.

  ```C#
  public IEnumerable<int> Generator(string parameter)
  {
      if (parameter == null)
         throw new ArgumentNullException(nameof(parameter));   //The check is performed immediately.
    
      return GeneratorInternal();
 
       IEnumerable<int> GeneratorInternal()
       {
           int i = parameter == "Y" ? 1 : 0;
          yield return i; 
       }
  }
  ```

* If you implement an async method with the argument check.
* If you need better grouping of the public method with the private methods that only this public method uses. The general .NET convention recommends that you put all public methods above the private ones. However, you can improve readability by grouping private methods that are related to only one public method as its local functions. The number of these local functions should be no more than three.

## Best Practices

The [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) should be considered in addition to our own.

### Unit Tests

Each analyzer, code fix, or refactoring should be covered with unit tests. Each unit test must specify explicitly all code analysis settings that could affect its result. The tests should be independent of default code analysis settings values.
Specifying settings that don't affect the test is not required. For example:
  ```C#
  public class ThrowingExceptionsInEventHandlersTests : DiagnosticVerifier
  {
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
										.WithStaticAnalysisEnabled()
										.WithSuppressionMechanismDisabled(),
				new ThrowingExceptionsInEventHandlersAnalyzer());

	//The rest of the code
  }
  ```

### Cancellation Support

The cancellation support should be added to Acuminator diagnostics and code fixes.

This means that the cancellation token, which is passed to every Roslyn diagnostic and code fix, should be checked between big calculation steps. Also, the token should be passed to every Roslyn framework method that supports cancellation, such as `Document.GetSyntaxRootAsync()`.

The check for cancellation via cancellation token must be done using the following construction.

```C#
cancellationToken.ThrowIfCancellationRequested();
```

There is one exception to this rule. Do not pass cancellation token to the `ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync()` in the VSIX project. This method has overload that accepts cancellation.
However, it doesn't cancel the operation. It just cancels the transition to the UI thread if it weren't already done scheduling the continuation on a threadpool. This is not expected by our code and should be avoided.

### Demo Solution

If you add a new diagnostic or other functionality to the Acuminator, you also should add corresponding examples to the demo solution, which is located under the */src/Samples* folder.

We use the demo solution for the following two purposes:

1. As a lightweight Visual Studio solution for debugging.
2. As a demo of Acuminator. 

Because the solution is used for demo purposes, we need to maintain it in the consistent state — that is, it should compile without errors, and it should contain examples that are similar to the real business cases.

### Code Reuse

You should try to reuse existing Acuminator's helper methods. The Acuminator code already includes the utilities for many common tasks related to the Acumatica-specific objects, such as the following:

* The utility for the [recursive code analysis](../RecursiveCodeAnalysis/RecursiveCodeAnalysis.md)
* The utility that checks whether the type is an Acumatica-specific type, such as a DAC, DAC extension, graph, graph extension, view, BQL statement, or PXAction
* The utility that obtains the views or actions from a graph or graph extension
* The utility that obtains the field attributes from a DAC property and finds out whether the field is bound or unbound
* Syntax manipulation helpers, such as a helper that gets the next statement node in a method
* Semantic helpers

When you write a piece of functionality for a diagnostic (such as a generic check), you should add the `Acuminator.Utilities` namespace to the list of `using` directives and review the existing helpers in the `Acuminator.Utilities` namespace. If there are no helpers appropriate for your task, you should add a new helper to the `Acuminator.Utilities` project.

### Task Blocking

You should avoid the use of `Task.Result` and `Task.Wait()` because this can cause deadlocks and thread pool exhaustion. You should consider using `ThreadHelper.JoinableTaskFactory.Run()` instead.

For details, see the following articles: 

* [How to: Manage multiple threads in managed code](https://docs.microsoft.com/en-us/visualstudio/extensibility/managing-multiple-threads-in-managed-code) 
* [Asynchronous and multithreaded programming within VS using the JoinableTaskFactory](https://blogs.msdn.microsoft.com/andrewarnottms/2014/05/07/asynchronous-and-multithreaded-programming-within-vs-using-the-joinabletaskfactory/)
* [Cookbook for Visual Studio](https://github.com/Microsoft/vs-threading/blob/master/doc/cookbook_vs.md)
* [Three Threading Rules](https://github.com/Microsoft/vs-threading/blob/master/doc/threading_rules.md)

### Task Awaiting

You should use `Task.ConfigureAwait(false)` within analyzers, code fixes, and refactorings to improve the performance and avoid potential deadlocks.

The use of `ConfigureAwait(false)` in other places (such as VSIX) without additional statements, such as `await TaskScheduler.Default`, should be avoided. For more information, see [Cookbook for Visual Studio](https://github.com/Microsoft/vs-threading/blob/master/doc/cookbook_vs.md).

### Parametrized Diagnostic Messages

If a message for a diagnostic must be parameterized (such as "The {0} field cannot be declared within a DAC declaration"), this parametrized message should be passed to the diagnostic descriptor in the `MessageFormat` property instead of the `Title` property. The `Title` property must contain a brief description without parameters.

This rule is described inside the `DiagnosticDescriptor` class, which fragment is shown in the following code.

```C#
/// <summary>A short localizable title describing the diagnostic.</summary>
public LocalizableString Title { get; }
 /// <summary>
/// A localizable format message string, which can be passed as the first argument to <see cref="M:System.String.Format(System.String,System.Object[])" /> when creating the diagnostic message with this descriptor.
/// </summary>
/// <returns></returns>
public LocalizableString MessageFormat { get; }
```

You can set the `MessageFormat` property by using an optional parameter during creation of a new `DiagnosticDescriptor` instance, as shown in the following code.

```C#
public static DiagnosticDescriptor PX1027_ForbiddenFieldsInDacDeclaration { get; } =
    Rule("PX1027", nameof(Resources.PX1027Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error,
    nameof(Resources.PX1027MessageFormat).GetLocalized()); // MessageFormat
```

### Test Methods

Test methods should be asynchronous. This improves overall test performance and avoids potential deadlocks for test runners.

```C#
public Task TestDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);
```

You should use the `async` / `await` pattern to avoid wrapping exceptions in `AggregateException` and make the test output more readable.

```C#
public async Task TestDiagnostic(string actual) => await VerifyCSharpDiagnosticAsync(actual);
```

### Async Anonymous Delegates

You must never pass the asynchronous methods that have the `void` return type to the `analyzer.RegisterXXX` API methods. 

Internally Roslyn diagnostic engine catches `OperationCancelledException` exceptions. However, in .NET, the asynchronous methods that return `void` handle exceptions differently than the asynchronous methods that return a `Task`. The latter methods attach the aggregated exception to the `Task` while the methods that return `void` report the exception to `SynchronizationContext`. In `SynchronizationContext`, the exception becomes unhandled and silently crashes Visual Studio. 

These bugs have concurrent nature because they appear only during the cancellation of the wrongly-written analyzer and are very hard to reproduce and debug.
Therefore, such methods prevent Visual Studio from catching `OperationCancelledException` and must never be passed to the analyzer API. 
The latest version of Roslyn even has a runtime check for async methods.

The following code produces this bug.

```C#
//WRONG
compilationStartContext.RegisterSymbolAction(Analyze_Something_Async, SymbolKind.NamedType);

private static async void Analyze_Something_Async(SymbolAnalysisContext symbolContext) 
{
    //Analysis
}
```

It is especially easy to introduce this bug with async delegates because `analyzer.RegisterXXX` API methods accept `Action<T>` delegates that return `void`, but do not accept `Func<T, Task>`. For example, the following code produces the bug.

```C#
//WRONG
compilationStartContext.RegisterSymbolAction(async symbolContext => await Analyze_Something_Async(symbolContext, pxContext),
                                             SymbolKind.NamedType);
```

So, how to write analyzers correctly? Do not pass asynchronous methods to the `analyzer.RegisterXXX` methods at all. 

Roslyn has both synchronous and asynchronous API methods, such as `symbol.GetSyntax(CancellationToken)` and `symbol.GetSyntaxAsync(CancellationToken)`. The analysis is executed in a background thread, therefore the 
use of the synchronous API shouldn't be a big deal.

```C#
//CORRECT (Pay no attention to the VS warning about a not awaited task in this case)
compilationStartContext.RegisterSymbolAction(symbolContext => Analyze_Something_Sync(symbolContext, pxContext),
                                             SymbolKind.NamedType);
```

There is also an option to pass the asynchronous methods that return a `Task`. However we do not recommend to do like this. 
The Roslyn diagnostic engine does not expect any return objects, so the returned `Task` won't be ever awaited. 
You also cannot await it because this will lead to the creation of the async delegate as described above. And if the cancellation happens the returned `Task`
will become 'unobserved' and may throw an exception in the finalization thread (`Task` is `IDisposable`). Such unobserved `Task`s can be swallowed 
but this depends on the configuration of the user runtime. For more details, see
https://docs.microsoft.com/ru-ru/dotnet/framework/configure-apps/file-schema/runtime/throwunobservedtaskexceptions-element.

Overall, the passing of asynchronous methods to `analyzer.RegisterXXX `is an unwanted scenario that should be avoided at all costs.