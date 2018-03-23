# Acuminator Release Notes
This document provides information about fixes, enhancements, and key features that are available in Acuminator 1.2.

## Acuminator 1.2
Acuminator 1.2 includes the new features described in this section as well as the features for the previous versions, which are described in the Acuminator 1.1 and Acuminator 1.0 sections.
### New Settings for Roslyn Coloring
You can adjust the color settings of Acuminator for Roslyn coloring as follows:
* Colorize PXAction declarations.<br/> In Visual Studio, specify the value of **Tools > Options > Acuminator > Enable PXAction coloring**.      
* Colorize PXGraph declarations.<br/> In Visual Studio, specify the value of **Tools > Options > Acuminator > Enable PXGraph coloring**.                                                                                                                                       
* Colorize code only inside BQL commands.<br/> In Visual Studio, specify the value of **Tools > Options > Acuminator > Colorize code only inside BQL commands**. 

### Support for Visual Themes in Visual Studio
Acuminator can use different color sets for different visual themes (such as light and dark themes) in Visual Studio.

### Code Outlining
Acuminator can collapse parts of BQL statements and the code inside attributes to small tags. To use code outlining, in Visual Studio, set **Tools > Options > Acuminator > Use BQL outlining** to true.

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
| PX1000 | An invalid signature of the `PXAction` handler is used.                                                                                                | Error   | Available   | Available     |                     
| PX1001 | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method.                                                        | Error   | Available   | Available     |                     
| PX1002 | The field must have the type attribute corresponding to the list attribute.                                                                     | Error   | Available   | Available     | 
| PX1003 | A `PXGraph` instance must be created with the `PXGraph.CreateInstance()` factory method. Consider using a specific implementation of `PXGraph`. | Error   | Available   | Available     | 
| PX1004 | The order of view declarations will cause the creation of two cache instances.                                                                      | Warning | Available   | Unavailable   | 
| PX1005 | There is probably a typo in the view delegate name.                                                                                             | Warning | Available   | Available     | 
| PX1006 | The order of view declarations will cause the creation of one cache instance for multiple DACs.                                                     | Warning | Available   | Unavailable   | 
| PX1008 | The reference of `@this` graph in the delegate will cause synchronous delegate execution.                                                       | Warning | Available   | Unavailable   | 
| PX1009 | Multiple levels of inheritance are not supported for PXCacheExtension.                                                                          | Error   | Available   | Available     | 
| PX1010 | If a delegate applies paging in an inner select, StartRow must be reset. (If StartRow is not reset, paging will be applied twice.)              | Warning | Available   | Available     | 
| PX1011 | Because multiple levels of inheritance are not supported for PXCacheExtension, the derived type can be marked as sealed.                        | Error   | Available   | Available     | 

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



