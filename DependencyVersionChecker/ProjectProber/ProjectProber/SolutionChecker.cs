using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyProber;
using CK.Core;
using NuGet;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Solution checking utility.
    /// </summary>
    /// <seealso cref="SolutionChecker.CheckSolutionFile(string, IActivityLogger)"/>
    public static class SolutionChecker
    {
        /// <summary>
        /// Checks a solution file.
        /// </summary>
        /// <param name="filePath">Solution file path</param>
        /// <returns>SolutionCheckResult</returns>
        public static SolutionCheckResult CheckSolutionFile( string filePath )
        {
            return CheckSolutionFile( filePath, DefaultActivityLogger.Empty );
        }

        /// <summary>
        /// Checks a solution file.
        /// </summary>
        /// <param name="filePath">Solution file path</param>
        /// <param name="logger">Logger to use</param>
        /// <returns>SolutionCheckResult</returns>
        public static SolutionCheckResult CheckSolutionFile( string filePath, IActivityLogger logger )
        {
            if( String.IsNullOrEmpty( filePath ) )
                throw new ArgumentNullException( "filePath" );
            if( !File.Exists( filePath ) )
                throw new ArgumentException( "File must exist", "filePath" );

            filePath = Path.GetFullPath( filePath );
            string solutionDirectory = Path.GetDirectoryName( filePath );
            string packageRoot = Path.Combine( solutionDirectory, "packages" );

            IAssemblyLoader assemblyLoader = new AssemblyLoader( logger  );

            // Init NuGet repository
            LocalPackageRepository localRepository = new LocalPackageRepository( packageRoot );

            // Result items:

            // Solution packages
            // Associates Package reference to NuGet package object, if found in the repository (null otherwise)
            Dictionary<INuGetPackageReference, IPackage> solutionPackages = new Dictionary<INuGetPackageReference, IPackage>();

            // Project assembly references/dependencies
            // Assembly references of each project, including system.
            Dictionary<ISolutionProjectItem, List<IProjectAssemblyReference>> projectsAssemblyReferences =
                new Dictionary<ISolutionProjectItem, List<IProjectAssemblyReference>>();

            // Project NuGet package references
            // NuGet package references of each project
            Dictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>> projectsPackageReferences =
                new Dictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>>();

            // Assembly information of every solution NuGet package found in solutionPackages.
            // Does not consider multiple frameworks!
            Dictionary<IPackage, IEnumerable<IAssemblyInfo>> packageAssemblies =
                new Dictionary<IPackage, IEnumerable<IAssemblyInfo>>();

            // Loaded solution
            ISolution solution = SolutionFactory.ReadFromSolutionFile( filePath );

            using( logger.OpenGroup( LogLevel.Info, "Solution: {0} ({1})", solution.Name, solution.FilePath ) )
            {
                // Parsed projects from solution
                IEnumerable<ISolutionProjectItem> supportedProjectItems = solution.Projects
                    .Where( project => project.GetItemType() == SolutionProjectType.VISUAL_C_SHARP ); // Keep only C# projects

                logger.Info( "Loading {0} projects.", supportedProjectItems.Count() );

                // Loop in solution projects
                foreach( ISolutionProjectItem projectItem in supportedProjectItems )
                {
                    // Init paths
                    string projectFilePath = Path.Combine( solutionDirectory, projectItem.ProjectPath );
                    string projectDirectory = Path.GetDirectoryName( projectFilePath ) + Path.DirectorySeparatorChar;
                    string packagesConfigPath = Path.Combine( projectDirectory, "packages.config" );

                    using( logger.OpenGroup( LogLevel.Info, "Project: {0} ({1})", projectItem.ProjectName, projectItem.ProjectPath ) )
                    {
                        // Get project references
                        List<IProjectAssemblyReference> projectAssemblyReferences = ProjectUtils.GetReferencesFromProjectFile( projectFilePath ).ToList();

                        projectsAssemblyReferences.Add( projectItem, projectAssemblyReferences );

                        IEnumerable<INuGetPackageReference> packageReferences = null;
                        // Check NuGet packages from package configuration
                        if( File.Exists( packagesConfigPath ) ) // No packages.config = No NuGet packages for this project
                        {
                            logger.Trace( "Project has NuGet package configuration." );
                            packageReferences = ProjectUtils.GetReferencesFromPackageConfig( packagesConfigPath );

                            List<string> projectNuGetPackageNames = new List<string>();

                            // Package loop
                            foreach( INuGetPackageReference packageRef in packageReferences )
                            {
                                logger.Trace( "Found NuGet package reference: {0}", packageRef.Id, packageRef.ToString() );

                                IPackage package;
                                if( !solutionPackages.TryGetValue( packageRef, out package ) )
                                {
                                    logger.Trace( "Adding package to solution packages." );
                                    package = packageRef.GetPackageFromRepository( localRepository );
                                    if( package == null )
                                    {
                                        solutionPackages.Add( packageRef, null );
                                        logger.Error( "Couldn't load package from repository: {0}. Was the NuGet package downloaded?", packageRef.ToString() );
                                    }
                                    else
                                    {
                                        solutionPackages.Add( packageRef, package );

                                        IEnumerable<IAssemblyInfo> packageInfo = package.GetAssemblyNames( assemblyLoader );

                                        packageAssemblies.Add( package, packageInfo );
                                    }
                                }

                                // Check that: Project contains only one of each package Id.
                                if( projectNuGetPackageNames.Contains( packageRef.Id ) )
                                {
                                    logger.Error( "Package Id {0} is referenced more than once in this project", packageRef.Id );
                                }
                                else
                                {
                                    projectNuGetPackageNames.Add( packageRef.Id );
                                }
                            }

                            // Check that: Referenced assemblies, if they are detected inside a NuGet package folder, match the one in the package file
                            foreach( IProjectAssemblyReference assemblyRef in projectAssemblyReferences.Where( x => !String.IsNullOrEmpty( x.HintPath ) ) )
                            {
                                string absoluteHintPath = PathUtility.GetAbsolutePath( projectDirectory, assemblyRef.HintPath );

                                if( PathUtility.IsSubdirectory( packageRoot, absoluteHintPath ) )
                                {
                                    logger.Trace( "Assembly found in NuGet package directory: {0}", assemblyRef.AssemblyName );
                                    if( !File.Exists( absoluteHintPath ) )
                                    {
                                        logger.Error( "Couldn't find this assembly from its HintPath: {0}", assemblyRef.HintPath );
                                    }
                                    else
                                    {
                                        IAssemblyInfo referencedAssembly = assemblyLoader.LoadFromFile( absoluteHintPath );

                                        var matchedPackages = packageAssemblies
                                            .Where( x => x.Value.Any( y => y.FullName == referencedAssembly.FullName ) )
                                            .Select( x => x.Key );

                                        if( matchedPackages.Count() < 1 )
                                        {
                                            logger.Error( "Assembly is in NuGet directory, but doesn't match any assembly from NuGet packages: {0} ({1})",
                                                referencedAssembly.FullName, assemblyRef.HintPath );
                                        }
                                        else if( matchedPackages.Count() > 2 )
                                        {
                                            using( logger.OpenGroup( LogLevel.Error, "Assembly was found in multiple NuGet packages: {0}", referencedAssembly.FullName ) )
                                            {
                                                foreach( IPackage matchedPackage in matchedPackages )
                                                {
                                                    logger.Error( "Found in package: {0}, version {1}", matchedPackage.Id, matchedPackage.Version.ToString() );
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    logger.Warn( "Stray assembly found: {0}", assemblyRef.AssemblyName, assemblyRef.HintPath );
                                    if( !File.Exists( absoluteHintPath ) )
                                    {
                                        logger.Error( "Couldn't load assembly from its HintPath: {0}", assemblyRef.HintPath );
                                    }
                                }
                            }
                        }
                        if( packageReferences != null )
                            projectsPackageReferences.Add( projectItem, packageReferences );
                    }
                }

                // Check that: In the solution, each project references the same version of a NuGet package
                var packagesWithMultipleVersions = solutionPackages.Keys
                    .GroupBy( x => x.Id )
                    .Where( x => x.Count() > 1 );

                foreach( var packageGroup in packagesWithMultipleVersions.OrderBy( x => x.Key ) )
                {
                    string packageName = packageGroup.Key;
                    using( logger.OpenGroup( LogLevel.Error, "Multiple versions referenced for package Id: {0}", packageName ) )
                    {
                        foreach( INuGetPackageReference p in packageGroup.OrderBy( x => x.Version ) )
                        {
                            string version = p.Version.ToString();

                            var referencingProjects = projectsPackageReferences
                                .Where( x => x.Value.Any( y => y.Id == p.Id && y.Version == version ) )
                                .Select( x => x.Key );

                            foreach( ISolutionProjectItem project in referencingProjects )
                            {
                                logger.Error( "Project {0} is referencing package {1}, version {2}", project.ProjectName, p.Id, version );
                            }
                        }
                    }
                }

                SolutionCheckResult result =
                new SolutionCheckResult(
                        filePath,
                        solutionPackages,
                        supportedProjectItems,
                        projectsAssemblyReferences.ToDictionary( x => x.Key, x => (IEnumerable<IProjectAssemblyReference>)x.Value ),
                        projectsPackageReferences.ToDictionary( x => x.Key, x => (IEnumerable<INuGetPackageReference>)x.Value )
                        );

                return result;
            }
        }
    }
}