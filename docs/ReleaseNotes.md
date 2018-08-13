# Acuminator Release Notes
This document provides information about fixes, enhancements, and key features that are available in Acuminator 1.3.

## Acuminator 1.3
Acuminator 1.3 includes the diagnostics and code fixes, enhancements, and bug fixes described in this section, as well as the features that have been implemented in the previous versions.
### New Diagnostics and Code Fixes
In this version, diagnostics and code fixes for the following issues have been added.

| Code   | Issue Description                                               | Type    | Diagnostics | Code Fix  | 
| ------ | --------------------------------------------------------------- | ------- | ----------- | --------- | 
| PX1012 | `PXAction` is declared on a non-primary view.                     | Warning | Available   | Available |
| PX1015 | For a BQL statement that contains only `Required` parameters, the number of arguments of the `Select` method must be equal to the number of `Required` parameters. | Warning | Available | Unavailable |
| PX1018 | The graph with the specified primary view type parameter doesn't contain the primary view of the specified type. | Error | Available | Unavailable |
| PX1021 | The DAC property field has a type that is not compatible with the field attribute assigned to this property. | Error   | Available   | Available |
| PX1023 | The DAC property is marked with multiple field attributes.      | Error   | Available   | Available |
| PX1024 | The DAC nested class is not declared as an abstract class.      | Error   | Available   | Available |
| PX1026 | Underscores cannot be used in the names of DACs and DAC fields. | Error   | Available   | Available |
| PX1040 | Constructors cannot be used in BLC extensions.                  | Error   | Available   | Available | 

### New Code Navigation
Acuminator now can navigate between an action and its handler, and between a data view declaration and its delegate. To navigate between these items, do the following:
1. Click on an action, an action handler, a data view declaration, or a data view delegate.
2. Click **Go To Action/View Declaration/Handler** in the context menu or on the **Acuminator** main menu.

### BQL Formatting Enhancements
In previous versions of Acuminator, you had to manually add the **Format BQL Statements** command to the context menu of the code editor in Visual Studio. In Acuminator 1.3, this command is available in the context menu by default. Also this command is now available on the **Acuminator** main menu. 

### Code Outlining Enhancements
* Now you can configure Acuminator to outline entire BQL statements. To do this in Visual Studio, set to `False` the value of **Tools > Options > Acuminator > BQL Outlining > Outline parts of BQL commands**.
* The **Use BQL outlining** has been moved under **Tools > Options > Acuminator > BQL Outlining**.

### Code Coloring Enhancements
* All code coloring options have been grouped under **Tools > Options > Acuminator > BQL Coloring**.
* Acuminator now colors angle brackets of any level. For coloring, Acuminator cycles through 14 colors.
* Acuminator now colors type parameters if they represent DACs, graphs, or other types that are colored by Acuminator.

### Other Enhancements
* Static code analyzers from Acuminator are now available as a [standalone NuGet package](https://www.nuget.org/packages/Acuminator.Analyzers/).
* The PX.Objects.HackathonDemo demo solution has been refactored: The files of the solution have been grouped by the diagnostics they illustrate and placed in folders with sensible names.

### Bug Fixes
In this version, the following bugs have been fixed.

| Bug | Fix Description |
| --- | --------------- |
| The PX1000 diagnostic worked only on graphs. | The PX1000 diagnostic now also works on graph extensions. |
| The PX1004 and PX1006 diagnostics threw the `InvalidCastException` exception in some cases. | The cast expression in the diagnostic now takes into account the type parameters in the graph views. |
| The PX1008 diagnostic threw the `NullReferenceException` exception in some cases. | The PX1008 diagnostic now supports the case when a method group from a helper class is passed to `SetProcessingDelegate`. Null checks have been added. |
| The code fix of the PX1010 diagnostic didn't work for delegates with iterator methods, multiple return statements, and `goto` statements. | The PX1010 diagnostic now supports iterator methods, multiple return statements, and `goto` statements. |
| The PX1010 diagnostic threw an exception for abstract BQL delegate declarations. | The PX1010 diagnostic now checks whether the method body exists. |
| Simple BQL statements, such as `PXSelect<Table>`, were formatted. | Simple BQL statements now are not formatted. |
| The `PXUpdate` BQL statements were not colorized. | Code coloring is now supported for the `PXUpdate` classes. |
| The unbound generic BQL types, such as `Select5<,>`, used in BQL compose scenarios were not colorized. | Code coloring now works for the unbound generic BQL types. |
| Some DACs were not colorized. | The Roslyn SemanticModel has been refreshed. Now all DACs are colorized. |
| Visual Studio showed errors and warnings about a failed resource load in the **Error List**. | The resource file for the Acuminator diagnostics is now generated correctly. |

## Acuminator 1.2.1
Acuminator 1.2.1 contains the hotfix for the critical bug found in Version 1.2 that broke the execution of static code analysis.

## Acuminator 1.2
Acuminator 1.2 includes the new features and the bug fix described in this section, as well as the features for the previous versions, which are described in the Acuminator 1.1 and Acuminator 1.0 sections.
### New Settings for Roslyn Coloring
You can adjust the color settings of Acuminator for Roslyn coloring as follows:
* Colorize PXAction declarations.<br/> In Visual Studio, specify the value of **Tools > Options > Acuminator > Enable PXAction coloring**.      
* Colorize PXGraph declarations.<br/> In Visual Studio, specify the value of **Tools > Options > Acuminator > Enable PXGraph coloring**.                                                      
* Colorize code only inside BQL commands.<br/> In Visual Studio, specify the value of **Tools > Options > Acuminator > Colorize code only inside BQL commands**. 

### Support for Visual Themes in Visual Studio
Acuminator can use different color sets for different visual themes (such as light and dark themes) in Visual Studio.

### Code Outlining
Acuminator can collapse parts of BQL statements and the code inside attributes to small tags. To use code outlining, in Visual Studio, set **Tools > Options > Acuminator > Use BQL outlining** to True.

### Bug Fix
In this version, the following bug has been fixed.

| Bug    | Fix Description                         | 
| ------ | --------------------------------------- | 
| Acuminator 1.1 always used tabs to format BQL statements, regardless of the Visual Studio settings. | Acuminator 1.2 formats BQL statements with spaces or tabs depending on the settings in **Tools > Options > Text Editor > C# > Tabs**.  |
 
## Acuminator 1.1
Acuminator 1.1 provides the new features described in this section as well as the features described below for Acuminator 1.0.
### New Diagnostics and Code Fixes
In this version, diagnostics and a code fix for the following issue have been added.

| Code   | Issue Description                       | Type    | Diagnostics | Code Fix  | 
| ------ | --------------------------------------- | ------- | ----------- | --------- | 
| PX1014 | A DAC field must have a nullable type.  | Error   | Available   | Available | 

### Coloring Based on Regular Expressions
Acuminator can colorize code based on regular expressions or by using Roslyn.  Coloring based on regular expressions works faster but coloring that uses Roslyn (which is used by default) provides more color options. To change the way the code is colored, in Visual Studio, set the value of **Tools > Options > Acuminator > Use RegEx colorizer**.

### BQL Formatting
Acuminator can format all BQL statements in the file that is currently open. To apply formatting, in Visual Studio, click **Edit > Format BQL Statements** on the main menu. You can also add the command to the context menu of the code editor in **Tools > Customize > Commands**. 

## Acuminator 1.0
Acuminator 1.0 introduces the following features.

### Diagnostics and Code Fixes
In the code based on Acumatica Framework, Acuminator finds common mistakes and typos and suggests code fixes. The list of the issues that Acuminator diagnoses is described in the following table.

| Code   | Issue Description                                                                                                                               | Type    | Diagnostics | Code Fix      | 
| ------ | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------- | ----------- | ------------- | 
| PX1000 | An invalid signature of the `PXAction` handler is used.                                                                                         | Error   | Available   | Available     |
| PX1001 | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method.                                                        | Error   | Available   | Available     | 
| PX1002 | The field must have the type attribute corresponding to the list attribute.                                                                     | Error   | Available   | Available     | 
| PX1003 | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method. Consider using a specific implementation of `PXGraph`. | Error   | Available   | Available     | 
| PX1004 | The order of view declarations will cause the creation of two cache instances.                                                                  | Warning | Available   | Unavailable   | 
| PX1005 | There is probably a typo in the view delegate name.                                                                                             | Warning | Available   | Available     | 
| PX1006 | The order of view declarations will cause the creation of one cache instance for multiple DACs.                                                 | Warning | Available   | Unavailable   | 
| PX1008 | The reference of `@this` graph in the delegate will cause synchronous delegate execution.                                                       | Warning | Available   | Unavailable   | 
| PX1009 | Multiple levels of inheritance are not supported for `PXCacheExtension`.                                                                        | Error   | Available   | Available     | 
| PX1010 | If a delegate applies paging in an inner select, `StartRow` must be reset. (If `StartRow` is not reset, paging will be applied twice.)          | Warning | Available   | Available     | 
| PX1011 | Because multiple levels of inheritance are not supported for `PXCacheExtension`, the derived type can be marked as sealed.                      | Error   | Available   | Available     | 

### Code Coloring
Acuminator colorizes BQL statements, thus improving the readability of long BQL queries. 

#### Colorized Code Elements
Acuminator supports coloring for the following code elements:
* DAC name                                 
* DAC extension name                      
* DAC field name                           
* BQL parameters                           
* BQL operators                           
* BQL constant (prefix)                    
* BQL constant (ending)                    
* PXGraph                                  
* PXAction                                 
* BQL angle brackets (levels from 1 to 14) 

#### Color Settings
You can adjust the color settings of Acuminator, as follows.
* Change the default colors.<br/> In Visual Studio, open **Tools > Options > Environment > Fonts and Colors**, select the needed **Acuminator** option in **Display items**, adjust colors, and click **OK**.
* Turn on or turn off coloring.<br/> In Visual Studio, set the value of **Tools > Options > Acuminator > Enable coloring**.                                                                                                                                                                             



