# Guidelines for Acuminator Diagnostic Desciptions
Acuminator documentation contains descriptions of each diagnostic. The diagnostic descriptions are located in the `md` (markdown) files of the `/docs/diagnostics` folder of the [Acuminator](https://github.com/Acumatica/Acuminator) repository on gitHub. 

## Template

You can use as a template a description of an existing diagnostic.

## File Name

The file name is the ID of the diagnostic, such as `PX1000.md`.

## Topic Title

The topic title is the ID of the diagnostic, such as PX1000.

## Topic Sections

The diagnostic description contains the following sections:

-   Introduction without a title 
-   Summary 
-   Diagnostic Description 
-   Code Examples (one or multiple sections with code examples)

### Introduction

-   Required
-   Has no title
-   Contains the following phrase with the needed ID of the diagnostic:  
    _This document describes the PX1000 diagnostic._

### Summary

-   Required
-   Has the following title: _Summary_
-   Contains a table in the following format:

    `| Code   | Short Description                                      | Type  | Code Fix  |`
    `| ------ | ------------------------------------------------------ | ----- | --------- |`
    `| PX1000 | An invalid signature of the `PXAction` handler is used | Error | Available |`

The values of the table columns are described below.

| Column            | Description                                                                                          |
| ----------------- | -----------------------------------------------------------------------------------------------------|
| Code              | The ID of the diagnostic.                                                                            |
| Short Description | A short description of the issue. You can use the message displayed by Visual Studio in this column. |
| Type              | The type of the issue, which can have one of the following values:<ul><li>Error</li><li>Warning (Level 1: Significant)</li><li>Warning (Level 2: Production quality)</li><li>Warning (Level 3: Informational)</li></ul> |
| Code Fix          | A value that specifies whether the code fix for the issue is implemented. The value can be one of the following:<ul><li>Available</li><li>Unavailable</li></ul> |

### Diagnostic Description

-   Required
-   Has the following title: _Diagnostic Description_
-   Contains the following paragraphs:
    -   Description of the correct behavior (Required)
    -   Explanation of why the incorrect behavior is not good (Optional)
    -   Description of the code fix or an explanation how the issue can be fixed if no code fix is provided by Acuminator (Required)
    -   Any limitations (Optional)

### Code Examples 

#### Example of the code that produces the error or warning

-   Required
-   Has one of the following titles:
    -   _Incorrect Code Example_ (for errors)
    -   _Example of the Code that Produces the Warning_ (for warnings)
-   Contains an example of the code in C# with a comment in the line where the diagnostic is displayed

#### Example of the code fix (if the code fix is available)

-   Required if the code fix is available
-   Has the following title: _Code Fix Example_
-   Shows how the issue shown in the code example of the issue is fixed by Acuminator

#### Example of the possible code fix (if the code fix is unavailable)

-   Optional
-   Has the following title: _Possible Code Fix_ 
-   Shows how the issue shown in the code example of the issue can be fixed
