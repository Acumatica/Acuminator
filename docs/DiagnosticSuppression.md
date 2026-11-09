# Acuminator Diagnostic Suppression

## Table of Contents

- [Overview](#overview)
- [Local Suppression with Acuminator Suppression Comment](#local-suppression-with-acuminator-suppression-comment)
  - [Writing a Suppression Comment Manually](#writing-a-suppression-comment-manually)
  - [Generating Suppression Comment In Visual Studio](#generating-suppression-comment-in-visual-studio)
    - [Visual Studio 2022 and Older Versions](#visual-studio-2022-and-older-versions)
    - [Visual Studio 2026](#visual-studio-2026)
- [Global Suppression in Acuminator Suppression File](#global-suppression-in-acuminator-suppression-file)
  - [Suppress a Diagnostic](#suppress-a-diagnostic)
  - [Remove a Suppression](#remove-a-suppression)
  - [Suppression Record Format](#suppression-record-format)

## Overview

Sometimes you know better than an automatic code analyzer. You may have a good reason to ignore a diagnostic and want to let the analyzer know.

Acuminator supports two external ways to suppress diagnostics:

 - Use of the Visual Studio suppression mechanisms, such as the compiler pragma directives and the
[`System.Diagnostics.CodeAnalysis.SuppressMessage`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.suppressmessageattribute?view=netframework-4.8) attribute. 
 - Configuration of code analysis in the .editorconfig files. For details, see [Support for EditorConfig Files in Acuminator](./EditorConfigSupport.md).

However, due to various reasons, the existing VS suppression mechanisms are not very convenient to work with. Therefore, Acuminator provides two additional mechanisms to suppress its diagnostics:

* *Local suppression* with Acuminator suppression comments
* *Global suppression* in an Acuminator suppression file

Each of the mechanisms is used for different scenarios. Local suppression provides a notice to the reader that there is an Acuminator alert suppressed due to the specified reasons. 
Global suppression does not provide any information regarding alerts suppressed in the code or the reason for the suppression of the alert to the reader. It is used to suppress errors in Acumatica legacy code 
in an automated way to avoid huge rewrites of the legacy code. Acuminator provides an easy way to suppress a diagnostic globally in the global suppression file.

**We recommend to use local suppression with comments in the code wherever possible.**

The Acuminator suppression mechanism differs from the standard C# and .NET diagnostic suppression mechanisms. The standard suppression mechanisms either suppress all diagnostics with this ID in the type or its member or force you to use pragma directives which degrade code readability and are disliked by many developers.  With Acuminator suppression mechanisms, you can suppress a particular diagnostic granularly in a specific place inside the code. 

The article provides the detailed description of both Acuminator suppression mechanisms.

## Local Suppression with Acuminator Suppression Comment

To use Acuminator local suppression with a suppression comment, add a C# comment above the line with alert in the following format.

```cs
// Acuminator disable once <diagnostic ID> <short diagnostic name> <optional justification>
```

You can add a comment in one of the following ways:
 - Type the comment manually
   >Note: If you don't use Visual Studio, specifying suppression comments manually is the only option.
 - Generate the comment automatically in Visual Studio

Here is an example of a suppression comment for the [PX1003](./diagnostics/PX1003.md) diagnostic.

```cs
// Acuminator disable once PX1003 NonSpecificPXGraphCreateInstance [Justification]
```

Notice the `NonSpecificPXGraphCreateInstance` part. It is a *short diagnostic name* which quickly tells the reader of the code what error is suppressed there. 

The suppression comment can also contain an optional `[Justification]` placeholder. It does not affect Acuminator behavior. 
You should replace the `Justification` placeholder with a reason for the suppression of the alert, as the following example shows.

```cs
// Acuminator disable once PX1003 NonSpecificPXGraphCreateInstance Created instance of base graph to just read some data from the database
var graph = new PXGraph();
```

>Note: It is not necessary to surround the justification with square braces.

To suppress multiple diagnostics displayed for a single line, add multiple suppression comment for this line, one comment after another.

To stop the suppression of a diagnostic in the code fragment, remove the comment that corresponds to the diagnostic from the code.

### Writing a Suppression Comment Manually

You can write Acuminator suppression comments manually: Type the comment on the line above the reported Acuminator error. 
In the suppression comment, you need to specify the diagnostic short name. For the list of diagnostic short names, see [Diagnostic Short Name](https://github.com/Acumatica/Acuminator/blob/dev/src/Acuminator/Acuminator.Analyzers/DiagnosticsShortName.resx).

You can also find the suppression comment for the diagnostic in documentation for each diagnostic. The URL for each diagnostic description has the following format:
    ```text
    http://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/<diagnostic ID>.md
    ```
    For example, for the PX1007 diagnostic, the documentation page is [http://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/**PX1007**.md](http://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1007.md). 
    At the end of each diagnostic documentation page, locate the "**Acuminator Suppression Comment**" section with the suppression comment for the corresponding diagnostic.

### Generating Suppression Comment In Visual Studio

Within Visual Studio, Acuminator provides a code fix that generates the comment automatically. The access to the code fix in the UI depends on the version of Visual Studio you are using.
In all versions of Visual Studio, Acuminator will generate and place the suppression comment on the line above the error. Here is an example of a suppression comment generated by the code fix in Visual Studio.

```cs
...
else
{
	if (!Attribute.IsDefined(method, typeof(PXSuppressEventValidationAttribute)))
		// Acuminator disable once PX1053 ConcatenationPriorLocalization [Justification]
		throw new PXException(string.Format(MsgNotLocalizable.InvalidArgumentTypeInEventHandler, method.DeclaringType.FullName, method.Name));
	else
		return null;
}
...
```

The suppression comment in the code above includes the following parts:
 - The ID of the [PX1053](./diagnostics/PX1053.md) diagnostic.
 - The short diagnostic name `ConcatenationPriorLocalization`, which can give readers an idea of what the suppressed diagnostic is about.
 - The `[Justification]` placeholder. You should replace it with the explanation of why the Acuminator alert is suppressed in this place.

#### Visual Studio 2022 and Older Versions

For Visual Studio 2022 and older versions, the path to the code fix option is shown on the following screenshot.

![Suppression with a Comment in VS 2022](./images/SuppressDiagnosticInCommentVS2022.png)

#### Visual Studio 2026

The path to the code fix in Visual Studio 2026 is shown in the screenshot below. To use classic non-AI code fixes in Visual Studio, open the **Other Fixes** menu and select the suppression option. The rest of configuration is the same as in the older versions of Visual Studio.

![Suppression with a Comment in VS 2026](./images/SuppressDiagnosticInCommentVS2026.png)


## Global Suppression in Acuminator Suppression File

Acuminator global suppression is a mechanism that allows you to keep information about suppressed Acuminator diagnostics in your C# project in a separate suppression file. 
Each C# project can contain only one suppression file. The file should be located into the project's root folder. The file name should have the following structure.

```text
<Project name>.acuminator
```

Include the suppression file as an additional file in the project's `.csproj` file:

```xml
<ItemGroup>
  <AdditionalFiles Include="<Project name>.acuminator" />
</ItemGroup>
```

### Suppress a Diagnostic

Because suppression entries are difficult to create manually, we recommend adding them by using the Acuminator code fix in Visual Studio.

To suppress a diagnostic, open the available code fixes for the diagnostic and select **Suppress the PXnnnn diagnostic with Acuminator > in the Acuminator suppression file**, where `PXnnnn` is the diagnostic ID.

The following screenshot shows this code fix in Visual Studio 2022.

![Suppression in the Acuminator Suppression File in Visual Studio 2022](./images/SuppressDiagnosticInSuppressionFileVS2022.png)

### Remove a Suppression

To stop suppressing a diagnostic at a specific location in your code, remove the corresponding entry from the Acuminator suppression file.

### Suppression Record Format

Global suppression files may contains thousands of records. Each record contains information about the corresponding suppressed error which includes
 - The ID of suppressed diagnostic
 - A type or type member containing the error
 - Information on a syntax node with the error 
 
 Here is an example of a record in the suppression file.

```xml
<?xml version="1.0" encoding="utf-8"?>
<suppressions>
  <suppressMessage id="PX1001">
    <target>PX.Objects.AR.ARStatementMaint.recreateLast(PX.Data.PXAdapter)</target>
    <syntaxNode>new StatementCycleProcessBO()</syntaxNode>
  </suppressMessage>
  ...
</suppressions>
```

In practice, global suppression files are usually generated automatically. You can generate the suppression file for the whole project automatically by using the [Acuminator Console Runner](./AcuminatorCLI.md).