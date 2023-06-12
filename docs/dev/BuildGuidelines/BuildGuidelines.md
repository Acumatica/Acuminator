# Build Guidelines

## Table of Contents

* [Build preparation steps](#build-preparation-steps)  
* [Acuminator Version Policy](#acuminator-version-policy)
* [Preparing a New Release](#preparing-a-new-release)
   

## Build Preparation Steps

To build Acuminator, you need to perform the following preparatory steps:

1. In the root folder of the Acuminator solution, create the _lib_ folder. 
2. Add the following DLL files from Acumatica ERP 2022 R2 or later versions to the _lib_ folder: 
   * _PX.Data.dll_
   * _PX.Data.BQL.Fluent.dll_
   * _PX.Common.dll_
   * _PX.Common.Std.dll_
   * _PX.BulkInsert.dll_
   * _PX.Objects.dll_
   * _PX.DbServices.dll_ ()
   These DLLs are used by Acuminator unit tests.
   You can use older versions of Acumatica ERP DLLs but there is no guarantee that Acuminator will work correctly. The current recommended version is Acumatica ERP 2022 R2 GA.
3. Configure assembly signing in one of the following ways:
    * Add your strong-name key file as _src/key.snk_. If you don't have it, run Developer Command Prompt and generate the key by using the following command: 
      ``` 
      _sn.exe -k "src\key.snk"_
      ```
    * Turn off assembly signing for all projects in the solution. 
      (To turn off assembly signing for a project, open the project properties and, in the **Signing** pane, clear the **Sign the assembly** check box.)
4. Build _Acuminator.sln_.

## Acuminator Version Policy

The Acuminator version number should consist of three numbers separated by dots such as `3.1.2`. The first number indicates a major version which is updated when Acuminator starts supporting a new version of Visual Studio or drops the support of one of the previous Visual Studio versions.
The second number is incremented for major updates with a lot of new features. The third number is usually incremented for hotfixes. The distinction between the second and the third number can be subjective and overall it's not very important which one you increment.

If one number of the new version number is incremented, then all numbers after it should be 0. For example, if the current latest released version is `3.1.2` and for the next version, you are going to increment the second number, then the version should be `3.2.0`.

Acuminator releases are not frequent. Therefore, we do not use any automation to update the Acuminator version and do it manually in the following places:

* In the `Acuminator.Analyzers` and `Acuminator.Utilities` project files, the `<Version>` property
* In the `AssemblyInfo.cs` file of the `Acuminator.Tests` and `Acuminator.Vsix` projects, in the assembly attributes
* In the `source.extension.vsixmanifest` VSIX manifest file of the `Acuminator.Vsix` project, the `Version` attribute of the `Identity` tag
* In the `AcuminatorVSPackage.cs` file of the `Acuminator.Vsix` project, the `PackageVersion` constant of the `AcuminatorVSPackage` class

> **Note:** You can update the version number quickly in one step if you use Visual Studio "Find and Replace" functionality. 

## Preparing a New Release

To prepare a new Acuminator release, you need to do the following:

1. Update the Acuminator version according to the [Acuminator Version Policy](#acuminator-version-policy).
2. Cover all changes of the static code analysis in documentation, and merge the updated documentation.
3. Write release notes for the new release.
4. Make sure that Acuminator can be built successfully. Perform all steps from the [Build preparation steps](#build-preparation-steps) section.
5. Run all unit tests and make sure that they all pass.

After you complete all these steps, Acuminator can be built and released using its CI pipeline.
