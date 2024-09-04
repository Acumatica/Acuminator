@ECHO off
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION
SET me=%~n0

ECHO %me%: .........................................................
ECHO %me%: Current folder %CD%

SET TargetDir=%1
SET SolutionDir=%2
SET AssemblyName=%3

IF NOT DEFINED TargetDir (
	ECHO %me%: TargetDir parameter is not specified
	GOTO ERROR_EXIT
)
IF NOT DEFINED SolutionDir (
	ECHO %me%: SolutionDir parameter is not specified
	GOTO ERROR_EXIT
)
IF NOT DEFINED AssemblyName (
	ECHO %me%: AssemblyName parameter is not specified
	GOTO ERROR_EXIT
)

ECHO %me%: Script parameters: TargetDir=%TargetDir%, SolutionDir=%SolutionDir%, AssemblyName=%AssemblyName%

IF NOT EXIST "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs\%AssemblyName%.dll" (
	ECHO %me%: .........................................................
	ECHO %me%: Copying %AssemblyName% assembly and PDB file to the "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs" folder
	XCOPY "%TargetDir%%AssemblyName%.dll*" "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs\%AssemblyName%.dll*" /H /R /Y /f 
	XCOPY "%TargetDir%%AssemblyName%.pdb*" "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs\%AssemblyName%.pdb*" /H /R /Y /f
)

IF NOT EXIST "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs\Acuminator.Utilities.dll" (
	ECHO %me%: .........................................................
	ECHO %me%: Copying Acuminator.Utilities assembly and PDB file to the "Acuminator.Tests\ExternalRunner\src\Acuminator Libs" folder
	XCOPY "%TargetDir%Acuminator.Utilities.dll*" "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs\Acuminator.Utilities.dll*" /H /R /Y /f
	XCOPY "%TargetDir%Acuminator.Utilities.pdb*" "%SolutionDir%Acuminator\Acuminator.Tests\ExternalRunner\src\Acuminator Libs\Acuminator.Utilities.pdb*" /H /R /Y /f
)

EXIT /B 0

REM ---------------------------------------------------------------------------------------------------------------------------------------

:ERROR_EXIT
ECHO %me%: .........................................................
ECHO %me%: Error during execution of after-build script. Error code is %ERRORLEVEL% 

EXIT /B 1