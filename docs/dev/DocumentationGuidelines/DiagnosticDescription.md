# Guidelines for Acuminator Diagnostic Desciptions
The Acuminator documentation contains descriptions of each diagnostic. The diagnostic descriptions are located in the `md` (markdown) files of the `/docs/diagnostics` folder of the [Acuminator](https://github.com/Acumatica/Acuminator) repository on gitHub. 

## Template

You can use a description of an existing diagnostic as a template.

## File Name

The file name is the ID of the diagnostic, such as `PX1000.md`.

## Topic Title

The topic title is the ID of the diagnostic, such as `PX1000`.

## Topic Sections

The diagnostic description contains the following sections:

-   An introduction without a title 
-   A summary 
-   The description of the diagnostic 
-   Code examples (one section or multiple sections with code examples)
-   Related articles

### Introduction

-   Is required
-   Has no title
-   Contains the following phrase with the needed ID of the diagnostic:  
    _This document describes the PX1000 diagnostic._

### Summary

-   Is required
-   Has the following title: _Summary_
-   Contains a table in the following format:

    | Code   | Short Description                                       | Type  | Code Fix  |
    | ------ | ------------------------------------------------------- | ----- | --------- |
    | PX1000 | An invalid signature of the `PXAction` handler is used. | Error | Available |

    The values of the table columns are described below.

    | Column            | Description                                                                                          |
    | ----------------- | -----------------------------------------------------------------------------------------------------|
    | Code              | The ID of the diagnostic.                                                                            |
    | Short Description | A short description of the issue. You can use the message displayed by Visual Studio in this column. |
    | Type              | The type of the issue, which can have one of the following values:<ul><li>_Error_</li><li>_Warning (ISV Level 1: Significant)_</li><li>_Warning (ISV Level 2: Production Quality)_</li><li>_Warning (ISV Level 3: Informational)_</li><li>_Message_</li></ul> |
    | Code Fix          | A value that specifies whether the code fix for the issue is available. The value can be one of the following:<ul><li>_Available_</li><li>_Unavailable_</li></ul> |

### Diagnostic Description

-   Is required
-   Has the following title: _Diagnostic Description_
-   Contains the following information:
    -   A description of the correct behavior (required)
    -   An explanation of why the incorrect behavior is problematic (optional)
    -   A description of the code fix or an explanation of how the issue can be fixed if no code fix is provided by Acuminator (required)
    -   Any limitations (optional)

### Code Examples 

#### Example of code that results in the error or warning

-   Is required
-   Has one of the following titles:
    -   _Example of Incorrect Code_ (for errors)
    -   _Example of Code that Results in the Warning_ (for warnings)
-   Contains an example of the code in C# with a comment in the line where the diagnostic is displayed (_The PX1047 error is displayed for this line._)

#### Example of the code fix (if a code fix is available)

-   Is required if the code fix is available
-   Has the following title: _Example of Code Fix_
-   Shows how the issue shown in the code example of the issue is fixed by Acuminator

#### Example of the possible code fix (if the code fix is unavailable)

-   Is optional
-   Has the following title: _Example of Possible Code Fix_ 
-   Shows how the issue shown in the code example of the issue can be fixed

### Related Articles

-   Is optional
-   Has the following title: _Related Articles_
-   Contains links to related articles on help.acumatica.com or other resources