#Troubleshooting

## Resolving Issues with Installing Updates

Sometimes, when you install an update of Acuminator, the code analysis and, therefore, diagnostics may stop working. 
To resolve this issue, you can try the following approaches:
 - Check and remove remaining Acuminator files from the Visual Studio folder
 - Clear MEF cache using an extension
 
Before you try any of the approaches, you need to remove the Acuminator extension.
 
### Removing Acuminator Files
In some cases, the Acuminator files from the previous version may remain in the Local folder of Visual Studio.
To remove these Acuminator files, do the following:
1. Locate the Visual Studio folder. The location of the folder may depend on the installation settings. An example location is the following where <User_Name> is the name of your user in Windows:
   `C:\Users\<User_Name>\AppData\Local\Microsoft\VisualStudio`
   If you do not see the AppData folder in the File Explorer, you need to enable showing of hidden folders in the Control Panel.
2. In the VisualStudio folder, locate the folder for your version of Visual Studio. The folder's name contains the number of the Visual Studio version and the hashed string that contains the update number. For example, `17.0_b0a1b8a3`.
3. In the folder for your version of Visual Studio, open the `Extensions` folder.
4. In the Extensions folder, find the folder with name is a unique identifier for the Acuminator extension. You can find these folder by locating the Acumintor.dll file in it.
   If there is no such folder, all Acuminator files were deleted by the system and you need to try the other approach.
   If the folder exists, remove it. 
   
### Clearing the MEF Cache
To clear the MEF cache, do the following:
1. Install the [Clear MEF Component Cache]("https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ClearMEFComponentCache") extension.
   After the extension is installed, the new command **Clear MEF Component Cache** appears in the Tools menu of Visual Studio.
2. In Visual Studio, go to **Tools > Clear MEF Component Cache**.
3. In the dialog box which appear, click **Yes**.
The cache is cleared. 
After that, you can try installing Acuminator.
