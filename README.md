# Acuminator

Acuminator is a Visual Studio extension that simplifies development with Acumatica Framework. 
It provides following functionality in order to boost developer productivity:
* Static code analysis diagnostics, code fixes and refactorings
* Syntax highlighting of Acumatica-specific code elements
* BQL formatting and outlining
* Navigation between related code elements
* Code Map tool window which displays the structure of graphs, DACs and their extensions

## Diagnostics and Code Fixes
Acuminator provides diagnostics and code fixes for common developer challenges related to Acumatica Framework.
It finds common mistakes and typos that are usually not so easy to find, such as the following:
* Incorrect signatures of `PXAction` delegates
* Typos in the names of view delegates
* `PXStringList` declarations without `PXDBString`
* C#-style inheritance from `PXCacheExtension`
* Incompatible types of DAC property and DB field attribute declared on it
* Localization diagnostics

For the errors it finds, Acuminator suggests code fixes. For the full list of supported diagnostics and code fixes, see [Diagnostics](docs/Summary.md#diagnostics). 

Acuminator supports two approaches for suppression of unwanted diagnostic alerts:
* Suppress diagnostic with a special comment placed a line above the code
* Suppress diagnostic with a specific suppression file. With this mechanism a specific one per project file will store a list of  diagnostics suppressed in the project. This approach is supported only if Acuminator is installed as a VSIX plugin. 

## Code Coloring
Acuminator adds code coloring to Acumatica-specific code elements:
* graphs and graph extensions
* DACs and DAC extensions
* DAC fields
* BQL queries - operators and angle braces
* BQL constants
* actions

You can adjust the colors in the Visual Studio settings in the "Fonts and Colors" section.
 
## BQL Formatting, and Outlining
Acuminator allows you to format BQL statements, which improves the readability of long BQL queries. The command is located in the context menu of the Visual Studio code editor.

Also Acuminator provides an outlining functionality. It can collapse parts of BQL queries and the code inside attributes to small tags, which makes it easier for you to focus on the parts of code related to the current task.

## Navigation
Acuminator adds a command to code editor's context menu which allows you to quickly navigate between:
* graph view and its view delegate
* action and its action delegate

## Code Map
Acuminator provides a "Code Map" tool window which display to the user a structure of Acumatica-specific code elements:
* graph and graph extensions. For these elements Code Map displays:
   - views and their corresponding view delegates
   - actions and their corresponding action delegates
   - cache attached events with attributes declared on them. The events are grouped by DAC type and DAC field
   - row events grouped by DAC type
   - field events grouped by DAC type and DAC field
   - `PXOverride`s
* DAC and DAC extensions. For these elements Code Map displays:
   - Key DAC fields with attributes declared on them
   - All DAC fields with attributes declared on them
 For each DAC field Code Map displays some extra information:
   - Data type
   - Is field bound/unbound
   - Indicator if field has identity functionality
   - Indicator if field has auto-numbering functionality
   
Code Map shows the elements in a tree view. It allows to collapse a tree node hiding all its descendants. Code Map also provides an ability to sort nodes children and descendants alphabetically or by declaration order.

You can navigate to every code element displayed in Code Map by double clicking on the corresponding tree node. Some category nodes support cycling navigation. You can double click them sequentially and navigate through the list of its children code elements.

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
