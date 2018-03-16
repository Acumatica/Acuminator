# Release Notes
This document provides information about fixes, enhancements, and key features that are available in Acuminator 1.2.

## Diagnostics and Code Fixes
In the code based on Acumatica Framework, Acuminator finds common mistakes and typos and suggests code fixes. The full list of the issues that Acuminator diagnostics is described in the following table.
Code   | Description                                                                                                                                     | Type    | Diagnostics | Code Fix      | Available Since Version
------ | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------- | ----------- | ------------- | -----------------------
PX1000 | Invalid signature of `PXAction` handler is used.                                                                                                | Error   | Available   | Available     | 1.0                    
PX1001 | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method.                                                        | Error   | Available   | Available     | 1.0                    
PX1002 | The field must have the type attribute corresponding to the list attribute.                                                                     | Error   | Available   | Available     | 1.0
PX1003 | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method. Consider using a specific implementation of `PXGraph`. | Error   | Available   | Available     | 1.0
PX1004 | The order of view declarations will cause creation of two cache instances.                                                                      | Warning | Available   | Unavailable   | 1.0
PX1005 | Probably there is a typo in the view delegate name.                                                                                             | Warning | Available   | Available     | 1.0
PX1006 | The order of view declarations will cause creation of one cache instance for multiple DACs.                                                     | Warning | Available   | Unavailable   | 1.0
PX1008 | The reference of `@this` graph in the delegate will cause synchronous delegate execution.                                                       | Warning | Available   | Unavailable   | 1.0
PX1009 | Multiple levels of inheritance are not supported for PXCacheExtension.                                                                          | Error   | Available   | Available     | 1.0
PX1010 | If a delegate applies paging in an inner select, StartRow must be reset. (If StartRow is not reset, paging will be applied twice.)              | Warning | Available   | Available     | 1.0
PX1011 | Because multiple levels of inheritance are not supported for PXCacheExtension, the derived type can be marked as sealed.                        | Error   | Available   | Available     | 1.0
PX1014 | A DAC field must have a nullable type.                                                                                                          | Error   | Available   | Available     | 1.1

## Code Coloring
Acuminator colorizes BQL statements and therefore improves readability of long BQL queries. 

### Colorized Code Elements
Acuminator supports coloring for the following code elements.
Description                              | Available Since Version
---------------------------------------- | -----------------------
DAC name                                 | 1.0
DAC extension name                       | 1.0
DAC field name                           | 1.0
BQL parameters                           | 1.0
BQL operators                            | 1.0
BQL constant (prefix)                    | 1.0
BQL constant (ending)                    | 1.0
PXGraph                                  | 1.0
PXAction                                 | 1.0
BQL angle brackets (levels from 1 to 14) | 1.0

### Color Settings
You can adjust the color settings of Acuminator, as follows.
Description                                                                                                                                                                                                                                                                                            | Available Since Version
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -----------------------
*Change the default colors*<br/> In Visual Studio, open **Tools > Options > Environment > Fonts and Colors**, select the needed **Acuminator** option in **Display items**, adjust colors, and click **OK**.                                                                                           | 1.0
*Turn on or turn off coloring*<br/> In Visual Studio, set the value of **Tools > Options > Acuminator > Enable coloring**.                                                                                                                                                                             | 1.0
*Colorize code based on the regular expressions or using Roslyn*<br/> In Visual Studio, set the value of **Tools > Options > Acuminator > Use RegEx colorizer**. Coloring based on regular expressions works faster but coloring using Roslyn (which is used by default) provides more color options.  | 1.0
*Colorize PXAction declarations (only for Roslyn coloring)*<br/> In Visual Studio, set the value of **Tools > Options > Acuminator > Enable PXAction coloring**.                                                                                                                                       | 1.2
*Colorize PXGraph declarations (only for Roslyn coloring)*<br/> In Visual Studio, set the value of **Tools > Options > Acuminator > Enable PXGraph coloring**.                                                                                                                                         | 1.2
*Colorize code only inside BQL commands (only for Roslyn coloring)*<br/> In Visual Studio, set the value of **Tools > Options > Acuminator > Colorize code only inside BQL commands**.                                                                                                                 | 1.2

## BQL Formatting
Acuminator can format BQL statements, as follows.
 Description                                                                                                                                                                                                                                         | Available Since Version 
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------- 
 *Format all BQL statements in the currently open file*<br/> In the Visual Studio, click **Edit > Format BQL Statements** in the main menu. You can also add the command to the context menu of the code editor in **Tools > Customize > Commands**. | 1.0 

## Code Outlining
Acuminator can collapse code fragments to small tags, as follows.
Description                                                                                                                                                | Available Since Version
---------------------------------------------------------------------------------------------------------------------------------------------------------- | -----------------------
*Collapse parts of BQL statements and the code inside attributes*<br/> In Visual Studio, set **Tools > Options > Acuminator > Use BQL outlining** to true. | 1.2


