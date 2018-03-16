Acuminator is the static code analysis and colorizer tool for Visual Studio that simplifies development with Acumatica Framework. 
Acuminator provides diagnostics and code fixes for common developer pitfalls related to Acumatica Framework, can colorize and format BQL statements, and can collapse attributes and parts of BQL queries.

## Diagnostics and Code Fixes
In the code based on Acumatica Framework, Acuminator finds common mistakes and typos, which are usually not so easy to find, such as the following:
* Incorrect signatures of `PXAction` handlers
* Typos in view delegate names
* `PXStringList` declarations without `PXDBString`
* C#-style inheritance from `PXCahcheExtension`

For the found errors, Acuminator suggests code fixes. For the full list of supported diagnostics and code fixes, see the Release Notes. 

## Code Coloring, Formatting, and Outlining
Acuminator colorizes and formats BQL statements and therefore improves readability of long BQL queries. You can adjust the colors in the Visual Studio settings.

Acuminator can collapse attributes and parts of BQL queries to small tags, which makes it easier for you to focus on the parts of code related to the current task.

## Building of the Solution
To build the solution, do the following:
1. Create the _lib_ folder in the root folder. 
2. Add _PX.Data.dll_, _PX.Common.dll_, _PX.BulkInsert.dll_, _PX.DbServices.dll_ (from Acumatica ERP 6.1 or higher) to the _lib_ folder.
3. Add your strong-name key file as _src/key.snk_. If you don't have one, you can generate it by using the _sn.exe_ tool from Windows SDK (_sn.exe -k "src\key.snk"_).
4. Build PX.Analyzers.sln.
