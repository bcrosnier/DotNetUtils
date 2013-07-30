A couple notes I took related to VSIX (Visual Studio extensions) development. -- With love, BC.

### The Debug/Run button doesn't work
Happens because you just pulled the project, the way the project runs is stored in the project build settings, and project build settings are *user* settings (and thus not pulled). It *is* a class library, after all, so it won't run by itself unless you tell it what to run in.

Go to this project's **Project properties**, open the **Build** tab, and check **Open with an external program**, the program being `devenv.exe` (?:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE).

Also, ensure the **Command line arguments** are set to `/rootSuffix Exp`. This adds the `Exp` suffix to the root location the VS instance uses, effectively creating a new environment.

If that interests you, you can check the new tab in **Project properties**, named **VSIX**, notably to configure VSIX deployment in the experimental VS instance.

See also:

- [(MSDN) Devenv Command-Line Switches for VSPackage Development](http://msdn.microsoft.com/en-us/library/bb166507.aspx)
- [(MSDN) Experimental Instance of Visual Studio](http://msdn.microsoft.com/en-us/library/bb166560.aspx)


### First loading takes ages
The first time the experimental VS is loaded may take a bit of time, since it's not only launching VS (a pretty daunting task on its own), but also creating a completely new environment. It will even ask you for the general environment settings and what help files to store.

If you don't want to be interrupted and having to hit Continue every other second, reset your Exception breaks (`Debug` menu, `Exceptions` item, uncheck CLR/Win32 Exceptions). You can re-enable them later.

Future runs should be much faster; note however that it the experimental environment is inside the user's temporary files, and if you clean them, you'll have to redo this again.