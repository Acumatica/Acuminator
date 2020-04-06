# Acuminator

Acuminator is a Visual Studio extension that simplifies development with Acumatica Framework. 
It provides following functionality in order to boost developer productivity:
* Static code analysis diagnostics, code fixes and refactorings
* Syntax highlighting of Acumatica-specific code elements
* BQL formatting and outlining
* Navigation between related code elements
* Code Map tool window which display the structure of graphs, DACs and their extensions

Acuminator provides diagnostics and code fixes for common developer challenges related to Acumatica Framework. Also, Acuminator can colorize and format BQL statements, and can collapse attributes and parts of BQL queries.

## Diagnostics and Code Fixes
In the code based on Acumatica Framework, Acuminator finds common mistakes and typos that are usually not so easy to find, such as the following:
* Incorrect signatures of `PXAction` handlers
* Typos in the names of view delegates
* `PXStringList` declarations without `PXDBString`
* C#-style inheritance from `PXCacheExtension`

For the errors it finds, Acuminator suggests code fixes. For the full list of supported diagnostics and code fixes, see [Diagnostics](docs/Summary.md#diagnostics). 

## Code Coloring, Formatting, and Outlining
Acuminator colorizes and formats BQL statements, which improves the readability of long BQL queries. You can adjust the colors in the Visual Studio settings.

Acuminator can collapse parts of BQL queries and the code inside attributes to small tags, which makes it easier for you to focus on the parts of code related to the current task.

## The Process of Building the Solution
To build the solution, do the following:
1. Create the _lib_ folder in the root folder. 
2. Add _PX.Data.dll_, _PX.Data.BQL.Fluent.dll_, _PX.Common.dll_, _PX.BulkInsert.dll_, _PX.Objects.dll_, and _PX.DbServices.dll_ (from Acumatica ERP 2019 R1 or higher) to the _lib_ folder.
3. Configure assembly signing in one of the following ways:
    * Add your strong-name key file as _src/key.snk_. If you don't have one, run Developer Command Prompt and generate the key by using the following command: _sn.exe -k "src\key.snk"_.
    * Turn off assembly signing for all projects in the solution. (To turn off assembly signing for a project, open the project properties and, in the **Signing** pane, clear the **Sign the assembly** check box.)
4. Build _Acuminator.sln_.

## Documentation
* [Diagnostics](docs/Summary.md#diagnostics)
* [Code Refactoring](docs/Summary.md#refactorings)

## Developer Documentation
* [Coding Guidelines](docs/dev/CodingGuidelines/CodingGuidelines.md)
* [Recursive Code Analysis](docs/dev/RecursiveCodeAnalysis/RecursiveCodeAnalysis.md)
* [Documentation Guidelines](docs/dev/DocumentationGuidelines/DiagnosticDescription.md)

## Release Notes
[Release Notes](docs/ReleaseNotes.md)
