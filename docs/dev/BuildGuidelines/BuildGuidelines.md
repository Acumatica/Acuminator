# Build Guidelines

## Table of Contents

* [Build preparation steps](#build-preparation-steps)  
* [Acuminator Version Policy](#acuminator-version-policy)
* [Preparing a New Release](#preparing-a-new-release)
   

## Build Preparation Steps

You need to perform the following preparatory steps to build Acuminator:

1. Create the _lib_ folder in the root folder. 
2. Add _PX.Data.dll_, _PX.Data.BQL.Fluent.dll_, _PX.Common.dll_, _PX.Common.Std.dll_, _PX.BulkInsert.dll_, _PX.Objects.dll_, and _PX.DbServices.dll_ (from Acumatica ERP 2022 R2 or higher) to the _lib_ folder. Thses DLLs are used by Acuminator unit tests.
   You can use older versions of Acumatica ERP DLLs but there is no guarantee that Acuminator will work correctly. The current recommended version is Acumatica ERP 2022 R2 GA.
3. Configure assembly signing in one of the following ways:
    * Add your strong-name key file as _src/key.snk_. If you don't have one, run Developer Command Prompt and generate the key by using the following command: _sn.exe -k "src\key.snk"_.
    * Turn off assembly signing for all projects in the solution. (To turn off assembly signing for a project, open the project properties and, in the **Signing** pane, clear the **Sign the assembly** check box.)
4. Build _Acuminator.sln_.

## Acuminator Version Policy

Acuminator version should be a three digits separated by dots like `3.1.2`. The first digit is a major version which is updated when Acuminator starts supporting a new Visual Studio version or drops the support for one of the previous Visual Studio versions.
The second digits is incremented for major updates with a lot of new features and the third one is usually incremented for hotfixes. The distinction between the second and third digits is a bit subjective and overall it's not very important which one you will increment.

If a digit in the new version number is incremented then all digits after it should be 0. For example, if the current latest released version is `3.1.2` and for the next version you are going to increment the second digit, then the version should be `3.2.0`.

Acuminator releases are not frequent. Therefore, we do not use any automation to update the Acuminator version and do it manually in the following places:

* In `Acuminator.Analyzers` and `Acuminator.Utilities` project files, the `<Version>` property
* In `AssemblyInfo.cs` of `Acuminator.Tests` and `Acuminator.Vsix` projects, in the assembly attributes.
* In the VSIX manifest file `source.extension.vsixmanifest` of the `Acuminator.Vsix` project, the `Identity` tag has `Version` attribute.
* In the `AcuminatorVSPackage.cs` file of the `Acuminator.Vsix` project. The class `AcuminatorVSPackage` has a constant `PackageVersion`.

You can do the update quickly in one step if you use Visual Studio "Find and Replace" functionality. 

## Preparing a New Release

To make a new Acuminator release you need to do the following:

1. Update the Acuminator version according to the [Acuminator Version Policy](#acuminator-version-policy)
2. Cover all changes in the static code analysis in the documentation and merge the updated documentation.
3. Write Release Notes for the new release.
4. Make sure that Acuminator can be built successfully. Perform all steps from the [Build preparation steps](#build-preparation-steps) section.
5. Run all unit tests and make sure that they all pass.

After that Acuminator can be built and released using its CI pipeline.