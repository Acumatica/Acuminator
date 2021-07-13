# Acuminator

Acuminator is a Visual Studio extension that simplifies development with Acumatica Framework. 
Acuminator provides the following functionality to boost developer productivity:
* Static code analysis diagnostics, code fixes, and refactorings
* Syntax highlighting of Acumatica-specific code elements
* BQL formatting and outlining
* Navigation between related code elements
* The Code Map tool which displays the structure of graphs, DACs, and their extensions

## Diagnostics and Code Fixes
Acuminator provides diagnostics and code fixes for common developer challenges related to Acumatica Framework.
Acuminator finds common mistakes and typos that are usually not so easy to find, for example:
* Incorrect signatures of the `PXAction` delegates
* Typos in the names of view delegates
* `PXStringList` declarations without the `PXDBString` attribute
* C#-style inheritance from `PXCacheExtension`
* Incompatible types of a DAC property and a DB field attribute declared on it
* Improper localization of a string

For the detected errors, Acuminator suggests code fixes. For the full list of supported diagnostics and code fixes, see [Diagnostics](docs/Summary.md#diagnostics). 

Acuminator supports two approaches for the suppression of unwanted diagnostic alerts:
* Suppress diagnostic with a special comment placed a line above the code
* Suppress diagnostic with a specific suppression file. With this mechanism, a specific project file will store a list of diagnostics suppressed in the project. This approach is supported only if Acuminator is installed as a VSIX plugin. 

## Code Coloring
Acuminator adds code coloring to the following Acumatica-specific code elements:
* Graphs and graph extensions
* DACs and DAC extensions
* DAC fields
* BQL queries: operators and angle braces
* BQL constants
* Actions

You can adjust the color schema in the "Fonts and Colors" section of Visual Studio settings.
 
## BQL Formatting, and Outlining
Acuminator allows you to format BQL statements, which improves the readability of complex BQL queries. The command to enable formatting of BQL queries is located in the context menu of the Visual Studio code editor.

Also, Acuminator provides an outlining functionality. It can collapse parts of BQL queries and the code inside attributes to small tags, which makes it easier for you to focus on the parts of code related to the current task.

## Navigation
Acuminator adds a command to the context menu of the Visual Studio code editor. The command allows you to quickly navigate between the following objects:
* A graph view and its view delegate
* An action and its delegate

## Code Map
Acuminator provides the Code Map tool which displays to the user a structure of the following Acumatica-specific code elements:
* Graphs and graph extensions. For these elements, the Code Map displays the following:
   - Views and corresponding view delegates
   - Actions and corresponding action delegates
   - Cache attached events with attributes declared on them. The events are grouped
   by the DAC type and the DAC field
   - Row events grouped by the DAC type
   - Field events grouped by the DAC type and the DAC field
   - Members overridden using the `PXOverride` attribute
* DACs and DAC extensions. For these elements, the Code Map displays:
   - Key DAC fields with attributes declared on them
   - All DAC fields with attributes declared on them
   
   For each DAC field, the Code Map displays the following additional information:
   - The field data type
   - Indicator of whether the field is bound or unbound
   - Indicator of whether the field has identity functionality
   - Indicator of whether the field has auto-numbering functionality
   
The Code Map shows the elements in a tree view. You can collapse a tree node to hide all its descendants. The Code Map also provides an ability to sort nodes children and descendants alphabetically or by the declaration order.

You can navigate to every code element displayed in the Code Map by double clicking on the corresponding tree node. Some category nodes support cycling navigation. You can double click them sequentially and navigate through the list of its children code elements.

## The Process of Building the Solution
To build the solution, do the following:
1. Create the _lib_ folder in the root folder. 
2. Add _PX.Data.dll_, _PX.Data.BQL.Fluent.dll_, _PX.Common.dll_, _PX.BulkInsert.dll_, _PX.Objects.dll_, and _PX.DbServices.dll_ (from Acumatica ERP 2019 R1 or higher) to the _lib_ folder.
   Starting from Acumatica ERP 2020 R2, you also need to add _PX.Common.Std.dll_ to the _lib_ folder.
3. Configure assembly signing in one of the following ways:
    * Add your strong-name key file as _src/key.snk_. If you don't have one, run Developer Command Prompt and generate the key by using the following command: _sn.exe -k "src\key.snk"_.
    * Turn off assembly signing for all projects in the solution. (To turn off assembly signing for a project, open the project properties and, in the **Signing** pane, clear the **Sign the assembly** check box.)
4. Build _Acuminator.sln_.

## Documentation
* [Diagnostics](docs/Summary.md#diagnostics)
* [Code Refactoring](docs/Summary.md#code-refactoring)

## Developer Documentation
* [Coding Guidelines](docs/dev/CodingGuidelines/CodingGuidelines.md)
* [Recursive Code Analysis](docs/dev/RecursiveCodeAnalysis/RecursiveCodeAnalysis.md)
* [Documentation Guidelines](docs/dev/DocumentationGuidelines/DiagnosticDescription.md)

## Release Notes
[Release Notes](docs/ReleaseNotes.md)
