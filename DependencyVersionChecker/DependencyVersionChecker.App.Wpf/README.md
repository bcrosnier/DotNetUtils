# .NET Assembly reference viewer

Utility describing references of a .NET assembly, or a set of assemblies.

## Usage

Execute program. This will open the main window.

The software defaults to examine assemblies from the working directory.

### Examine a single assembly

Open the `File` menu, and select `Open file`.

From the file dialog, open the assembly you wish to examine.

### Examine an assembly directory

Open the `File` menu, and select `Open directory`.

From the folder dialog, select the folder containing the assemblies you wish to examine.

This will **recursively** open them, and display all found assemblies on the tree view.


## User interface description

### Tree view

The **tree view** is displayed on the left side of the window.

It contains the assembly opened, or, if you selected a directory, all assemblies recursively found inside it.

Each entry contains, if applicable, other assemblies it references.

### Graph view

The **graph view** is displayed on the right side of the window.

It displays an entry for each assembly found, either from a directory, or as a reference from another assembly.

If the assembly does not reference any other assembly which could be found inside its directory, its graph entry will have a **blue** border.

If the assembly has references, the border will be **red**.

The entry background will be **orange** if multiple assemblies with the same name, but a different version, are found in the same graph.

### Log view

The **log view** is displayed on the bottom of the window.

Log messages are displayed inside, for development purposes.

## Notes

- When discovering references, the software shall only consider and display assemblies:
  - That could be found, and are in the software directory
  - That could not be found
- Assemblies which were found, but are outside the searched directory are considered **system assemblies** and shall not be displayed.
- The software will not check references of assemblies it cannot find. Duh.
- Assemblies which couldn't be opened are silently ignored. Hence, the Windows directory may appear somewhat empty if you check it.