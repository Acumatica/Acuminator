# Support of the EditorConfig Files

EditorConfig is an open-source project that helps teams maintain consistent coding styles across different IDEs and editors. It provides a file format for defining coding conventions and plugins that enable editors to apply them. These conventions are stored in `.editorconfig` files, which you can include in version control to share settings with everyone working on the project.

In .NET projects, `.editorconfig` files also support configuration of code analysis rules. Acuminator diagnostics are standard Roslyn analyzer diagnostics, so you can configure their severity in these files by using the standard .NET analyzer settings. Currently, Acuminator does not define any custom `.editorconfig` keys.

For more information, see the following Microsoft documentation:
- [Configuration files for code analysis rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files)
- [Define consistent coding styles with EditorConfig](https://learn.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options)

## Capabilities of Acuminator with EditorConfig Files

| Capability																												| Supported in `.editorconfig` |
| ------------------------------------------------------------------------------------------------------------------------- | ---------------------------- |
| Change the severity of a single diagnostic (`error`, `warning`, `suggestion`, `silent`, `none`)							| Yes						   |
| Disable a diagnostic (`none`)																								| Yes						   |
| Configure all Acuminator diagnostics at once by category																	| Yes						   |
| Scope the configuration to folders or files with `.editorconfig` sections													| Yes						   |
| Manage Acuminator-specific code analysis options (recursive analysis, ISV mode, suppression mechanism, PX1007, info diagnostics) | Currently, no.			   |

## Configure the Severity of Diagnostics

You can configure the severity of an individual diagnostic or all diagnostics in this category.

To configure the severity:
1. In an `.editorconfig` file, declare a section header that identifies the files to which the settings apply. For example, use `[*.cs]` to apply the settings to C# files in the current folder and its subfolders.
2. Place severity settings under the section header.
   Every setting contains the key and the severity value.

The following keys are available:

- **An individual diagnostic:** `dotnet_diagnostic.PXnnnn.severity`, where `PXnnnn` is the diagnostic ID.
- **All Acuminator diagnostics:** `dotnet_analyzer_diagnostic.category-Acuminator.severity`.

The available severity values are: 
 - `error`
 - `warning`
 - `suggestion`
 - `silent`
 - `none`
 - `default` 
 
 The `default` value restores the severity that Acuminator assigns to the diagnostic by default.

### Examples of the `.editorconfig` Configuration

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

1. Configure a specific diagnostic ID (`dotnet_diagnostic.PXnnnn.severity`).
2. Configure the category (`dotnet_analyzer_diagnostic.category-Acuminator.severity`).
3. Configure all analyzers (`dotnet_analyzer_diagnostic.severity`).
4. Apply the default severity of the diagnostic.

The EditorConfig files specify a key-value pair for each option. Conflicts arise when there are multiple entries with the same key that have different values. The following precedence rules are used to resolve conflicts:

1. The entry that appears later in the file wins. This rule applies to conflicting entries within a single EditorConfig file.
2. A file in a subfolder overrides settings from the files in parent folders. The search stops at the file that contains `root = true`. The `root = true` entry must be placed at the top of the file, before the first section header.

For severity options, the following additional rules apply:

- The `-nowarn` and `-warnaserror` compiler options always override the severity configured in `.editorconfig` files.
- Precedence between a ruleset file and an `.editorconfig` file for the same diagnostic is undefined. Ruleset files are deprecated in favor of `.editorconfig`.

For more details about the `.editorconfig` precedence rules, see Microsoft documentation:

- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options#precedence
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files#precedence

### Interaction with Other Acuminator Settings

Acuminator code analysis settings may interact with the diagnostic settings specified in the `.editorconfig` files. Keep the following in mind:

- If **Enable code analysis** is `False`, Acuminator does not report anything, regardless of the `.editorconfig` files content.
- Some diagnostics are controlled by Acuminator options. Examples are **PX1007** (disabled by default), informational diagnostics, and diagnostics that run only with **Enable additional diagnostics for ISV Solution Certification**. 
  If you set a higher severity for them in `.editorconfig`, it would not turn them on. To turn on the diagnostic, first enable the corresponding option and then configure the severity if needed.
- The ISV mode changes the default severity of some diagnostics from `Warning` to `Error`. An explicit `dotnet_diagnostic.PXnnnn.severity` entry in `.editorconfig` overrides this default.
- The [Acuminator suppression mechanism](./DiagnosticSuppression.md) (suppression comments and `.acuminator` files) is independent of `.editorconfig`. A diagnostic can be hidden by either mechanism.

### Apply Different Settings to Different Code

To scope the configuration to specific folders or files, declare sections in the .editorconfig file, as shown below.

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

In the example above, `[*.cs]`, `[src/**/*.cs]`, `[{**/Legacy/**,*.Designer.cs}]` are `editorconfig` section headers. For example, `[*.cs]` is applied to all C# files with the `.cs` file extension within the current folder, including subfolders. 
The subsequent entries look like `<key_name> = <key_value>`. These are analyzer options that will be applied to all files matching the pattern of the section.

You can apply EditorConfig file conventions to a folder, a project, or an entire repo by placing the file in the corresponding directory. These options are applied when executing the analysis at build time and while you edit code in Visual Studio.
For more details about available section headers and glob patterns, see the EditorConfig specification: https://spec.editorconfig.org/index.html#glob-expressions.

## Troubleshooting

| Problem                                                 | Possible cause and solution                                                                                                                                 |
| ------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| The configured severity is not applied                  | Check the section [glob pattern](https://en.wikipedia.org/wiki/Glob_(programming): The file extension must match the section title (for example, `[*.cs]`). Also check that a closer `.editorconfig` file does not override the setting. |
| The diagnostic is not displayed after you raised its severity. | The diagnostic may be disabled by an Acuminator option. See [Interaction with Other Acuminator Settings](#interaction-with-other-acuminator-settings).|
| The change is not visible in Visual Studio.             | Reopen the file, or reload the solution.                                                                                                                   |

## See Also
- Microsoft documentation on `.editorconfig` files:
	- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files
	- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options
	- https://learn.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options
- EditorConfig general specification: https://spec.editorconfig.org/