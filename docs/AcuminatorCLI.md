# Acuminator Console Runner

**Acuminator console runner** is a standalone command-line tool that performs Acuminator static code analysis of .NET projects and solutions based on Acumatica Framework.
Acuminator console runner serves as a command-line interface (CLI) for Acuminator code analysis that allows to run it outside of IDE. Such tool is useful for CI/CD pipelines and other automated scenarios.
The name of the executable file is `Acuminator.Runner.NetFramework.exe`.

Acuminator console runner supports analysis of .NET solutions (*.sln*) and projects (*.csproj*). The tool requires .NET Framework 4.8 runtime.

## Analysis

Acuminator enforces Acumatica-specific development rules and best practices, helping developers to ensure that their code adheres to the standards required for customization and extension of Acumatica ERP.
Acuminator diagnostics are designed to help developers avoid common mistakes, enforce architectural guidelines, and ensure compatibility with Acumatica extensibility model. 

Among the things checked by Acuminator are:
- **Graphs and DACs:** Acuminator checks that Acumatica Graphs and DACs follow Acumatica's required best practices and conventions.
- **DAC Field Attributes:** Acuminator checks for proper declaration of Acumatica Framework attributes declared on DAC field properties.
- **Event Handlers:** Acuminator verifies correct usage of graph event handler methods such as ``RowSelected`` or ``FieldUpdated``).
- **Prohibited API Usage:** Acuminator detects the use of Acumatica internal APIs or APIs that are discouraged or unsupported in customizations of Acumatica ERP.

You can find the list of all Acuminator diagnostics in this [summary](./Summary.md).

Acuminator also provides its own mechanism for diagnostics suppression, which allows developers to selectively suppress specific diagnostics in their code. This is useful when a particular diagnostic is not applicable or when a developer has a valid reason
to ignore it. The suppression mechanism can be applied via comments in the code or through special Acuminator suppression files.

## Exit Codes

Acuminator console runner was designed with the goal of being integrated into continuous integration processes and automated testing. The exit codes returned by the app follow the common conventions 
of the shell scripting. Here is the list of exit codes returned by the Acuminator console runner:

| Exit Code | Description                                                                                                                                                                              |
|---------- |------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|    `0`    | Acuminator Console Runner finished the execution successfully and the analyzed code passed the Acuminator code analysis validation                                                       |
|    `1`    | Acuminator Console Runner finished the execution successfully but the analyzed code did not pass the Acuminator code analysis validation.                                                |
|    `2`    | The execution of Acuminator Console Runner was cancelled. This can happen if you started the validation in the interactive mode (which is the default mode) and pressed "Ctrl + C" keys. |
|    `4`    | The execution of Acuminator Console Runner was interrupted by a runtime error.                                                                                                           |

When you integrate Acuminator console runner into your CI/CD pipeline, you can use these exit codes to determine the outcome of the code analysis and take appropriate actions based on the results.
The exit codes `0` and `1` indicate that the analysis completed successfully, so you can use them ot judge whether the code passed or failed the validation. 
The rest of the exit codes indicate that the analysis was interrupted or failed due to an unexpected runtime error. You can use them to detect incidents in your automated testing.


## Command Line Arguments

The Acuminator console runner provides several command line arguments to configure its code analysis and the format of the generated report. Run the tool with `--help` argument to see the documentation for all supported command line arguments in the console.

All command line arguments can be divided into the following three groups:
- Code analysis arguments: These arguments control how the code analysis is performed.
- Output arguments: These arguments control how the results of the code analysis are reported.
- Other arguments: These additional arguments control different aspects of the Acuminator console runner.

### Analysis Command Line Arguments

| Argument                               | Description                                                                                                                                                                                                                                                                                |
|--------------------------------------- |--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `codeSource` (position 0)              | Required. This argument should be specified first. The argument contains a path to the *code source* which will be validated.<br>The term *code source* is a general term that describes something that can provide source code to the tool. Currently, the supported code sources are C# projects and C# solutions. |
| `--disable-suppression`                | Optional. A flag that indicates that the code analysis will report Acuminator errors suppressed with Acuminator suppression mechanisms.                                                                                                                                                   |
| `--isv-mode`                           | Optional. A flag that enables ISV-specific analysis mode. In this mode, Acuminator performs stricter code analysis with extra diagnostics. The severity of some diagnostics is also increased from *Warning* to *Error*. The name of this mode is inspired by the Acumatica certification process for customizations developed by Independent Software Vendors (ISV). The certification uses stricter Acuminator analysis in the ISV mode to validate such customizations. |
| `--enable-PX1007`                      | Optional. A flag that enables the [**PX1007**](./diagnostics/PX1007.md) diagnostic. This diagnostic checks the presence of XML documentation comments on DACs and DAC fields. It is used internally in Acumatica development processes but disabled by default.                               |
| `--disable-PX1099`                     | Optional. A flag that disables the [**PX1099**](./diagnostics/PX1099.md) diagnostic. The **PX1099** diagnostic detects APIs that should not be used with Acumatica Framework.                                                                                                                                |
| `--enable-info-diagnostics`            | Optional. A flag that enables Acuminator informational diagnostics with severity `Info`.                                                                                                                                                                                                      |
| `--banned-APIs-path`                   | Optional. A path to a file with a custom list of banned APIs for the **PX1099** diagnostic. For more details about custom files with banned APIs and the format of banned APIs, see [**PX1099** documentation](./diagnostics/PX1099.md).                                                                        |
| `--allowed-APIs-path`                  | Optional. A path to a file with a custom list of allowed APIs for the **PX1099** diagnostic. For more details about allowed APIs, custom files with allowed APIs, and the format of allowed APIs, see [**PX1099** documentation](./diagnostics/PX1099.md).                                                      |
| `-w`, `--work-mode`                    | Optional. The mode in which Acuminator should work.<br><br>The following work modes are available:<br> - `report-errors` outputs errors if they are not suppressed with Acuminator suppression mechanisms. It is used by default.<br> - `generate-suppressions` generates suppression records in Acuminator suppression files for all errors it finds in the code.<br> - `report-and-generate` combines the first and the second modes. |


### Output Command Line Arguments

| Argument                               | Description                                                                                                                                                                                                                                     |
|--------------------------------------- |-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-f`, `--file`                         | Optional. A path to the output file. If not specified, the report with analysis results will be outputted to the console window.                                                                                                                    |
| `--output-absolute-paths-for-errors`   | Optional. A flag that regulates how the locations of errors are outputted. By default, the report contains file paths relative to the containing project directory. However, if this flag is set, the absolute file paths are used.        |
| `--format`                             | Optional. The output format of the report. The following values are supported: `text` (default) or `json`.                                                                                                                                                           |
| `-g`, `--grouping`                     | Optional. The way the diagnostics are grouped in the report. The expected value is a combination of symbols `f`/`F` (files) and `d`/`D` (diagnostic IDs). For example: `-g fd` groups found errors by the file and diagnostic ID.                                                 |

### Other Command Line Arguments

| Argument                               | Description                                                                                                                                                                                                                                                                                            |
|--------------------------------------- |--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-v`, `--verbosity`                    | Optional. Specifies logger verbosity. The available values are taken from the `Serilog.Events.LogEventLevel` enum. The following value are available: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`. By default, the logger uses the `Information` verbosity. |
| `--msBuild-path`                       | Optional. Provides an explicit path to the MSBuild tool that will be used for analysis. By default, MSBuild installations are detected automatically on the current machine and the latest found version will be used.                                                 |
| `--non-interactive`                    | Optional. A flag that forces the Acuminator Console Runner to run in non-interactive mode. In this mode, some interactive features such as interactive cancellation of the analysis with "Ctrl + C" are disabled. This mode is useful when Acuminator is executed in automated environments like CI/CD pipelines where no user interaction is possible. Some automated environments such as Azure Pipelines may throw an error if the application attempts to use one of the interactive features. |
| `--help`                               | Optional. A flag that allows you to see the description of all available command line arguments in the console. Use this flag without any other arguments.                                                                                                                                                                        |
| `--version`                            | Optional. A flag that allows you to see the Acuminator console runner's version. Use this flag without any other arguments.                                                                                                                                                                                                              |

## Usage Examples

Below are examples of how you can run Acuminator console runner from the command line.
```console
Acuminator.Runner.NetFramework.exe <path to solution/project> --verbosity Debug --format json -f <path to output file> -g <grouping> --enable-PX1007 --disable-PX1099
```
The example with real values will look as follows.
```console
Acuminator.Runner.NetFramework.exe "..\..\..\..\..\Samples\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo.csproj" --verbosity Debug --format json -f report.json -g FD --enable-PX1007 --disable-PX1099
```
The command above will analyze the `PX.Objects.HackathonDemo` project, output the report in the JSON format to the `report.json` file, group diagnostics by file and diagnostic ID, enable the **PX1007** diagnostic, and disable the **PX1099** diagnostic.
The verbosity of the logger will be set to `Debug`.

To run Acuminator analysis in a plain text format with default code analysis settings and output it to console, you can use the following command.
```console
Acuminator.Runner.NetFramework.exe "..\..\..\..\..\Samples\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo.csproj"
```
This command does not specify any grouping for diagnostics, so they will be outputted in a flat ordered list for each analyzed project. Note, that if you run the analysis for the C# solution, diagnostics will always be grouped by project. 

## Acuminator Console Runner and Different .Net Runtimes

Currently, there is only one version of Acuminator console runner based on .NET Framework runtime. The runner is based on the older .NET Framework runtime due to differences in Roslyn and MSBuild behavior for console applications 
based on different .NET runtimes. These differences can be observed on large complex code bases such as Acumatica ERP code. 

In case of modern .NET runtimes, Roslyn (and MSBuild used by the tool to load solutions for analysis) fails to correctly load Acumatica  ERP solution for analysis. This results in the inability to correctly analyze Acumatica ERP projects. 
On the other hand, when Roslyn and MSBuild are used with .Net Framework, they can correctly load Acumatica ERP solution and projects for analysis.

It seems, the issue is related to the fact that Acumatica ERP solution is still based on .NET Framework, which causes compatibility problems with Roslyn and MSBuild DLLs that target different .NET runtimes. In the future, after Acumatica ERP 
is fully migrated to .NET Core, a port of Acuminator console runner to .NET Core will probably appear.