A couple notes I took related to VSIX (Visual Studio extensions) development. -- With love, BC.

### The Debug/Run button doesn't work.
Happens because you just pulled the project, and project build settings are *user* settings (and thus not pulled). It *is* a class library, after all.

Go to the **Project properties**, open the **Build** tab, and check **Open with an external program**, the program being `devenv.exe` (?:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE).

Also, ensure the **Command line arguments** are set to `/rootSuffix Exp`. This adds the `Exp` suffix to the root location the VS instance uses, effectively creating a new environment.

If that interests you, you can check the new tab in **Project properties**, named **VSIX**, notably to configure VSIX deployment in the experimental VS instance.

See also:

- [(MSDN) Devenv Command-Line Switches for VSPackage Development](http://msdn.microsoft.com/en-us/library/bb166507.aspx)
- [(MSDN) Experimental Instance of Visual Studio](http://msdn.microsoft.com/en-us/library/bb166560.aspx)


### First loading: Is it seriously loading the whole .NET development symbol stack?
The first time the experimental VS is loaded may take a bit of time, since it's not only launching VS (a pretty daunting task on its own), but also creating a completely new environment. If will even ask you for the general environment settings and what help files to store.

If you don't want to be interrupted and having to hit Continue every time, reset your Exception breaks (`Debug` menu, `Exceptions` item, uncheck CLR/Win32 Exceptions). You can reenable them later.

Future runs should be much faster.