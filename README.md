# Acuminator
Acumatica-specific Visual Studio extension

# How To Build
1. Add _PX.Data.dll_, _PX.Common.dll_, _PX.BulkInsert.dll_, _PX.DbServices.dll_ (6.1 or higher) to _lib_ folder
2. Add your strong name key file as _src/key.snk_. If you don't have one, you can generate it using _sn.exe_ tool from Windows SDK (_sn.exe -k "src\key.snk"_)
3. Build PX.Analyzers.sln
