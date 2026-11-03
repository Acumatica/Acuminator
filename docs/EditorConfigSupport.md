# Support of EditorConfig Files in Acuminator

Acuminator diagnostics are standard Roslyn analyzer diagnostics. You can therefore configure them in `.editorconfig` files in the same way as any other .NET analyzer. Currently, Acuminator does not define its own `.editorconfig` keys.

EditorConfig is a technology not specific to .NET. It helps maintain consistent coding styles for multiple developers working on the same project in different IDEs and editors. The EditorConfig project consists of two parts - a file format for defining coding styles and a 
collection of IDE and editor plugins that enable editors to read the file format and apply styles defined in them. EditorConfig files are easy to read and understand. They also work nicely with version control systems.

You can find the details of the `.editorconfig` feature in the Microsoft documentation: 
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files
- https://learn.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options

## Summary

| Capability																												| Supported in `.editorconfig` |
| ------------------------------------------------------------------------------------------------------------------------- | ---------------------------- |
| Change the severity of a single diagnostic (`error`, `warning`, `suggestion`, `silent`, `none`)							| Yes						   |
| Disable a diagnostic (`none`)																								| Yes						   |
| Configure all Acuminator diagnostics at once by category																	| Yes						   |
| Scope the configuration to folders or files with `.editorconfig` sections													| Yes						   |
| Acuminator-specific code analysis options (recursive analysis, ISV mode, suppression mechanism, PX1007, info diagnostics) | Currently, no.			   |

## Configure the Severity of Diagnostics

Every Acuminator diagnostic has the `PXnnnn` ID and the `Acuminator` category. To configure a single diagnostic, use the `dotnet_diagnostic.PXnnnn.severity` key. To configure all Acuminator diagnostics, use the `dotnet_analyzer_diagnostic.category-Acuminator.severity` key. 
The available severity values are `error`, `warning`, `suggestion`, `silent`, `none`, and `default`. The `default` value restores the severity that Acuminator assigns to the diagnostic by default.

EditorConfig files are used to provide options that apply to specific source files or folders. Options are placed under **section headers** to identify the applicable files and folders. Add an entry for each rule you want to configure, 
and place it under the corresponding file extension section, for example, `[*.cs]`.

### Example `.editorconfig` configuration

```ini
[*.cs]

# Change the severity of a single diagnostic
dotnet_diagnostic.PX1000.severity = error
dotnet_diagnostic.PX1016.severity = suggestion

# Disable a diagnostic
dotnet_diagnostic.PX1099.severity = none

# Configure all Acuminator diagnostics
dotnet_analyzer_diagnostic.category-Acuminator.severity = warning
```

### Precedence

Roslyn applies the following precedence rules, from the highest to the lowest priority:

1. The configuration of a specific diagnostic ID (`dotnet_diagnostic.PXnnnn.severity`).
2. The configuration of the category (`dotnet_analyzer_diagnostic.category-Acuminator.severity`).
3. The configuration of all analyzers (`dotnet_analyzer_diagnostic.severity`).
4. The default severity of the diagnostic.

EditorConfig files specify a key-value pair for each option. Conflicts arise when there are multiple entries with the same key but different values. The following precedence rules are used to resolve conflicts:

1. The entry that appears later in the file wins. This is true for conflicting entries within a single EditorConfig file.
2. A file in a subfolder overrides settings from the files in parent folders. The search stops at the file that contains `root = true`. The `root = true` entry must be placed at the top of the file, before the first section header.

For severity options, the following additional rules apply:
- The `-nowarn` and `-warnaserror` compiler options always override severity configured in `.editorconfig` files.
- Precedence between a ruleset file and an `.editorconfig` file for the same diagnostic is undefined. Ruleset files are deprecated in favor of `.editorconfig`.

More information about the `.editorconfig` precedence rules is available in the Microsoft documentation:
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options#precedence
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files#precedence

### Interaction with Other Acuminator Settings

Acuminator code analysis settings may interact with the diagnostic settings specified in `.editorconfig` files. Keep the following in mind:

- If **Enable code analysis** is `False`, Acuminator does not report anything, regardless of `.editorconfig`.
- Some diagnostics are controlled by Acuminator options. Examples are **PX1007** (disabled by default), informational diagnostics, and diagnostics that run only with **Enable additional diagnostics for ISV Solution Certification**. 
  Setting a higher severity for them in `.editorconfig` does not turn them on. Enable the corresponding option first.
- The ISV mode changes the default severity of some diagnostics from `Warning` to `Error`. An explicit `dotnet_diagnostic.PXnnnn.severity` entry in `.editorconfig` overrides this default.
- The [Acuminator suppression mechanism](./diagnostics/DiagnosticSuppression.md) (suppression comments and `.acuminator` files) is independent of `.editorconfig`. A diagnostic can be hidden by either mechanism.

### Apply Different Settings to Different Code

Use sections to scope the configuration to specific folders or files.

```ini
[*.cs]
dotnet_diagnostic.PX1043.severity = warning

# Stricter rules for the production code
[src/**/*.cs]
dotnet_diagnostic.PX1043.severity = error

# Disable rules for generated and legacy code
[{**/Legacy/**,**/*.Designer.cs}]
dotnet_analyzer_diagnostic.category-Acuminator.severity = none

# Need to explicitly disable a diagnostic for generated code, because the previous entry does not apply to it according to the precedence rules.
dotnet_diagnostic.PX1043.severity = none
```
In the above example, `[*.cs]`, `[src/**/*.cs]`, `[{**/Legacy/**,*.Designer.cs}]` are editorconfig section headers. For example, `[*.cs]` is applied to all C# files with `.cs` file extension within the current folder, including subfolders. 
The subsequent entries look like `<option_name> = <option_value>`. These are analyzer options that will be applied to all files matching the pattern of the section.

You can apply EditorConfig file conventions to a folder, a project, or an entire repo by placing the file in the corresponding directory. These options are applied when executing the analysis at build time and while you edit code in Visual Studio.
You can read more about available section headers and glob patterns in the specification: https://spec.editorconfig.org/index.html#glob-expressions.

## Troubleshooting

| Problem                                                       | Possible cause and solution                                                                                                                                           |
| ------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| The configured severity is not applied                        | Check the section glob, because the file must match the section (for example, `[*.cs]`). Also check that a closer `.editorconfig` file does not override the setting. |
| The diagnostic is not displayed after you raised its severity | The diagnostic may be disabled by an Acuminator option. See [Interaction with Other Acuminator Settings](#interaction-with-other-acuminator-settings).                |
| The change is not visible in Visual Studio                    | Reopen the file, or reload the solution.                                                                                                                              |

## See Also
- Microsoft documentation on `.editorconfig` files:
	- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files
	- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options
	- https://learn.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options
- EditorConfig general specification: https://spec.editorconfig.org/