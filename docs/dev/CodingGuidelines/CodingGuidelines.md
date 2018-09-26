# Coding Guidelines

## Table of Contents

* [Code Style](#code-style)
    * [Private and Protected Fields Naming](#private-and-protected-fields-naming)
    * [Constants Naming](#constants-naming)
    * [Asynchronous Methods Naming](#asynchronous-methods-naming)
    * [Value Tuples Naming](#value-tuples-naming)
    * [Test Methods Naming](#test-methods-naming)
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

## Code Style

General [.NET Framework Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines) must be used beside our own.

### Private and Protected Fields Naming

Use underscore prefix for private and protected fields in classes.

```C#
public class MyClass
{
  private object wronglyNamedField;       //Bad naming
 
  protected object _correctlyNamedField;  //Correct naming
}
```

### Constants Naming

Names of constants must begin with a capital letter.

```C#
private const string SetValueMethodName = "SetValue"; // right
private const string _setValueMethodName = "SetValue"; // wrong
```

### Asynchronous Methods Naming

Asynchronous methods must have `Async` postfix.

```C#
public Task<int> GetCountAsync() { } // correct
public Task<int> GetCount() { } // incorrect
public async Task ExecuteAsync() { } // correct
public async Task Execute() { } // incorrect
```

### Value Tuples Naming

Names of properties in `ValueTuple` must use *PascalCase*, except the case when the tuple is a local variable.

```C#
public (int Line, int Column) GetPosition() { } // right
public (int line, int column) GetPosition() { } // wrong
```

### Test Methods Naming

Test methods should be named in *PascalCase*, with underscore character as a separator between logical statements.

```C#
public Task EventHandlersWithExternalMethod(string actual) { } // valid
public Task EventHandlers_ShouldNotShowDiagnostic(string actual) { } // valid
public Task TestCodeFix_RowSelected(string actual, string expected) { } // valid
public Task TestDiagnostic_RowSelected(string actual) { } // valid in case if there are also some tests for the code fix in the same class

public Task No_Connection_Scope_In_Row_Selected(string actual) { } // invalid
public Task Test_CodeFix_For_RowSelecting(string actual) { } // invalid
```

### Indentation Depth

Indentation depth (brace block levels) should be no more than 3.

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

Separate control flow statements (if / while / for / foreach / switch / do-while) with empty lines.

```C#
public bool Foo(bool flag)
{
    DoSomething();

    if (flag)
        return true;

    return false;
}
```

There are few possible exceptions to this rule when there is a logical grouping in the code which makes more sense:

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

There are three possible use cases:

* Implementing a generator method with the argument check made immediately:

```C#
public IEnumerable<int> Generator(string parameter)
{
    if (parameter == null)
       throw new ArgumentNullException(nameof(parameter));   //Check is performed immediately
    
    return GeneratorInternal();
 
     IEnumerable<int> GeneratorInternal()
     {
         int i = parameter == "Y" ? 1 : 0;
         yield return i; 
     }
}
```

* Implementing an async method with argument check. Similar to the previous case.
* Better grouping of the public method with the private methods which only this method use. General .NET convention recommends putting all public methods above the private ones. However, you can improve readability a little bit by grouping private methods related to only one public method as its local functions. The number of such local methods shouldn't be too big - no more than 3 local functions.

## Best Practices

General [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) should be considered besides our own.

### Unit Tests

Each analyzer, code fix or refactoring should be covered with unit tests.

### Cancellation Support

The cancellation support should be added to Acuminator diagnostics and code fixes.

This means that the cancellation token which is passed in some way to every Roslyn diagnostic and code fix should be checked between big calculation steps. Also, the token should be passed to every Roslyn framework method which supports cancellation, for example, `Document.GetSyntaxRootAsync()`, etc.

The check for cancellation via cancellation token must be done using the following construction:

```C#
cancellationToken.ThrowIfCancellationRequested();
```

### Demo Solution

If you add a new diagnostic or other functionality to the Acuminator, you also should add corresponding examples to the demo solution located under */src/Samples* folder.

We use the demo solution for two purposes.

First, we can use it as a lightweight VS solution for debugging.

Secondly, we use it to show the demos of the Acuminator. Therefore, we need to maintain it in a consistent state — it should compile without errors, and it should contain examples similar to the real business cases.

### Code Reuse

Try to re-use existing Acuminator's helper methods. There are already existing utilities for many common tasks related to Acumatica specific objects:

* [Recursive Code Analysis](../RecursiveCodeAnalysis/RecursiveCodeAnalysis.md)
* Check if a type is a DAC, DAC extension, graph, graph extension, View, BQL, PXAction and etc
* Get views/actions from graph/graph extension
* Get field attributes from DAC property and resolve if it is bound or unbound
* Syntax manipulation helpers, like get next statement node in a method
* Semantic helpers

When you write a piece of functionality for a diagnostic (e.g., a generic check), please check existing helpers inside Acuminator.Utilities namespace. Make sure it is added to the usings list. If there are no helpers appropriate for your task, please add a new helper to the Acuminator.Utilities project.

### Task Blocking

Avoid using `Task.Result` and `Task.Wait()` because it can cause deadlocks and thread pool exhaustion. Consider using `ThreadHelper.JoinableTaskFactory.Run()` instead.

See [this article](https://docs.microsoft.com/en-us/visualstudio/extensibility/managing-multiple-threads-in-managed-code) for more details. Additional information can be found [here](https://github.com/Microsoft/vs-threading/blob/master/doc/cookbook_vs.md) and [here](https://github.com/Microsoft/vs-threading/blob/master/doc/threading_rules.md).

### Task Awaiting

Use `Task.ConfigureAwait(false)` within analyzers, code fixes and refactorings to improve the performance and avoid potential deadlocks.

Be aware that using `ConfigureAwait(false)` in other places (such as VSIX) without any additional statements like `await TaskScheduler.Default` should be avoided; see [this article](https://github.com/Microsoft/vs-threading/blob/master/doc/cookbook_vs.md) for more details.

### Parametrized Diagnostic Messages

If a message for a diagnostic must be parameterized (e.g. "The {0} field cannot be declared within a DAC declaration"), this parametrized message should be passed to the diagnostic descriptor in `MessageFormat` property, not in `Title`. The `Title` property must contain a brief description without any parameters.

This rule is described inside the `DiagnosticDescriptor` class:

```C#
/// <summary>A short localizable title describing the diagnostic.</summary>
public LocalizableString Title { get; }
 /// <summary>
/// A localizable format message string, which can be passed as the first argument to <see cref="M:System.String.Format(System.String,System.Object[])" /> when creating the diagnostic message with this descriptor.
/// </summary>
/// <returns></returns>
public LocalizableString MessageFormat { get; }
```

`MessageFormat` can be set using an optional parameter when creating a new `DiagnosticDescriptor` instance:

```C#
public static DiagnosticDescriptor PX1027_ForbiddenFieldsInDacDeclaration { get; } =
    Rule("PX1027", nameof(Resources.PX1027Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error,
    nameof(Resources.PX1027MessageFormat).GetLocalized()); // MessageFormat
```

### Test Methods

Test methods should be asynchronous.

It improves overall test performance and avoids potential deadlocks for some test runners.

```C#
public Task TestDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);
```

Use `async` / `await` pattern to avoid wrapping exceptions in `AggregateException` and make the test output more readable.

```C#
public async Task TestDiagnostic(string actual) => await VerifyCSharpDiagnosticAsync(actual);
```
