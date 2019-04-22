# Acuminator

Acuminator is a static code analysis and colorizer tool for Visual Studio that simplifies development with Acumatica Framework. 
Acuminator provides diagnostics and code fixes for common developer challenges related to Acumatica Framework. Also, Acuminator can colorize and format BQL statements, and can collapse attributes and parts of BQL queries.

## Diagnostics and Code Fixes
In the code based on Acumatica Framework, Acuminator finds common mistakes and typos that are usually not so easy to find, such as the following:
* Incorrect signatures of `PXAction` handlers
* Typos in the names of view delegates
* `PXStringList` declarations without `PXDBString`
* C#-style inheritance from `PXCacheExtension`

For the errors it finds, Acuminator suggests code fixes. For the full list of supported diagnostics and code fixes, see Acuminator Release Notes. 

## Code Coloring, Formatting, and Outlining
Acuminator colorizes and formats BQL statements, which improves the readability of long BQL queries. You can adjust the colors in the Visual Studio settings.

Acuminator can collapse parts of BQL queries and the code inside attributes to small tags, which makes it easier for you to focus on the parts of code related to the current task.

## The Process of Building the Solution
To build the solution, do the following:
1. Create the _lib_ folder in the root folder. 
2. Add _PX.Data.dll_, _PX.Common.dll_, _PX.BulkInsert.dll_, and _PX.DbServices.dll_ (from Acumatica ERP 2017R2 or higher) to the _lib_ folder.
3. Add your strong-name key file as _src/key.snk_. If you don't have one, run Developer Command Prompt and generate the key by using the following command: _sn.exe -k "src\key.snk"_.
3. Open _src/Acuminator/Acuminator.Analyzers/Properties/AssemblyInfo.cs_ and change the public key in the _InternalsVisibleTo_ attribute to your own. To extract the key from the strong-name key file, do the following: 
   1. In Developer Command Prompt, extract the public key from the _snk_ file to a _txt_ file as follows: _sn.exe -p "src\key.snk" "src\publickey.txt"_. 
   2. Display the public key to the console by using the following command: _sn.exe -tp "src\publickey.txt"_.
4. Build _Acuminator.sln_.

## Documentation
* [Diagnostic Descriptions](docs/summary.md5#diagnostics)
* [Refactoring Descriptions](docs/summary.md5#refactorings)

## Developer Documentation
* [Coding Guidelines](docs/CodingGuidelines/CodingGuidelines.md)
* [Recursive Code Analysis](docs/RecursiveCodeAnalysis/RecursiveCodeAnalysis.md)
* [Documentation Guidelines](docs/DocumentationGuidelines/DiagnosticDescription.md)

## Release Notes
[Release Notes](docs/ReleaseNotes.md)
