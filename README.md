DotNetUtilities
===============

Outil/série d'outils pour analyser la conformité de certains points de **projets C# dans une même solution Visual Studio**, notamment :

- Les différentes *versions des projets* (ie. AssemblyInfo.cs),
- Les versions des *packages NuGet* référencés par les différents projets,
- Les références entre les bibliothèques d'un projet, et de leurs dépendances

Présenté sous la forme d'un exécutable, lancé normalement, via une extension Visual Studio 2012, ou en passant le chemin d'un fichier solution (.sln) en paramètre.


Analyse des références de bibliothèques .NET
--------------------------------------------
Cette partie permet d'analyser les références d'une bibliothèque ou d'un dossier de bibliothèques .NET (.dll, .exe) vers d'autres bibliothèques.

Cet outil permet de voir le détail des bibliothèques ouvertes et analysées dans un arbre, et de leur dépendances dans un graphe.

Il indique si plusieurs bibliothèques d'un même nom mais d'une version différente sont présentes ou référencées dans l'arbre, ou si une bibliothèque est référencée sous une version qui n'est pas exacte (par exemple grâce à l'assembly rebinding).


### Utilisation ###

Le projet qui se charge de l'analyse des bibliothèques est `AssemblyProber`.
La classe `AssemblyLoader` est utilisée pour charger une bibliothèque ou un ensemble de bibliothèques, et obtenir un certain nombre de détails pour finalement rendre un ou plusieurs `IAssemblyInfo`. Ces détails comprennent notamment les références (dépendances) vers d'autres bibliothèques.

> `IAssemblyInfo LoadFromFile( string assemblyFilePath )`<br>
> `IEnumerable<IAssemblyInfo> LoadFromDirectory( DirectoryInfo assemblyDirectory, bool recurse )`


#### `BorderChecker` ####

L'instanciation d'`AssemblyLoader` se fait optionnellement avec un `CK.Core.IActivityLogger` et un delegate local appelé `BorderChecker`.

Ce delegate, appelé chaque fois qu'une nouvelle assembly est chargée, déterminera si l'`AssemblyLoader` doit rechercher et résoudre les dépendances : si le delegate renvoie un string, alors l’assembly sera considérée comme une "limite" : elle sera chargée, mais l'`AssemblyLoader` ne tentera pas de rechercher et de résoudre ses dépendances. Si le delegate renvoie `null`, alors ses dépendances seront chargées.

Le `BorderChecker` par défaut marquera comme "limite" toutes les assemblies contenues dans le dossier %SYSTEMROOT%, ce qui comprend la plupart des bibliothèques de Microsoft Windows et du Global Assembly Cache.


Analyse des packages NuGet de la solution
-----------------------------------------

Cette partie permet de lister tous les packages NuGet utilisés par les projets de la solution, et de vérifier que tous utilisent bien la même version.

Le projet `ProjectProber` se charge de l'analyse des projets d'une solution, y compris l'analyse des packages NuGet. Une analyse des packages se fait dans `SolutionChecker.CheckSolutionFile( string slnFilePath )`. Le résultat renvoyé inclut les projets trouvés dans la solution, leur références vers des packages NuGet, et peut être utilisé pour voir si il y a des conflits de version.


Analyse des versions de la solution
-----------------------------------

Cette partie permet d'identifier les versions des différents projets d'une solution, et de déterminer si plusieurs versions sont différentes ou non-existantes.

Le projet `ProjectProber` se charge de l'analyse des projets d'une solution, y compris l'analyse des versions des projets. Une analyse des versions se fait dans `AssemblyVersionInfoChecker.AssemblyVersionInfoCheckResult( string slnFilePath )`. Le résultat renvoyé contient le détails des versions détectées, et un set de booléens avec les différents problèmes qu'il a rencontré.


Générateur de Semantic Version
------------------------------

Cette partie permet de générer une nouvelle Semantic Version à partir d'une version existante et détectée dans la solution, ou de n'importe quelle version.