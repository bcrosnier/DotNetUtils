using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet;
using ProjectProber.Interfaces;
using ProjectProber;
using CK.Core;
using AssemblyProber;

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
            string packageRoot =  Path.Combine( solutionDirectory, "packages" );

            //IAssemblyLoader assemblyLoader = new AssemblyLoader( logger );
            IAssemblyLoader assemblyLoader = new AssemblyLoader();

            // Init NuGet repository
            LocalPackageRepository localRepository = new LocalPackageRepository( packageRoot );

            // Key: package identifier (Name.Version)
            Dictionary<string, IPackage> solutionPackages = new Dictionary<string, IPackage>();

            // All assembly references, including system, per project
            Dictionary<ISolutionProjectItem, List<IProjectReference>> assemblyReferencesPerProject =
                new Dictionary<ISolutionProjectItem, List<IProjectReference>>();

            // NuGet package references, per project
            Dictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>> nuGetPackageReferencesPerProject =
                new Dictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>>();

            // Does not consider multiple frameworks!
            Dictionary<IPackage, IEnumerable<IAssemblyInfo>> packageAssemblies =
                new Dictionary<IPackage, IEnumerable<IAssemblyInfo>>();

            // Loaded solution
            ISolution solution = SolutionFactory.ReadFromSolutionFile( filePath );

            using( logger.OpenGroup( LogLevel.Info, "Solution: {0} ({1})", solution.Name, solution.FilePath ) )
            {
                // Parsed projects from solution
                IEnumerable<ISolutionProjectItem> supportedProjectItems =
                solution.Projects
                    .Where( project => project.GetItemType() == SolutionProjectType.VISUAL_C_SHARP ); // Keep only C# projects


                logger.Info( "Loading {0} projects.", supportedProjectItems.Count() );

                // Loop in projects
                foreach( ISolutionProjectItem projectItem in supportedProjectItems )
                {
                    // Init paths
                    string projectFilePath = Path.Combine( solutionDirectory, projectItem.ProjectPath );
                    string projectDirectory = Path.GetDirectoryName( projectFilePath ) + Path.DirectorySeparatorChar;
                    string packagesConfigPath = Path.Combine( projectDirectory, "packages.config" );

                    using( logger.OpenGroup( LogLevel.Info, "Project: {0} ({1})", projectItem.ProjectName, projectItem.ProjectPath ) )
                    {
                        // Get project references
                        List<IProjectReference> projectAssemblyReferences = ProjectUtils.GetReferencesFromProjectFile( projectFilePath ).ToList();

                        assemblyReferencesPerProject.Add( projectItem, projectAssemblyReferences );

                        IEnumerable<INuGetPackageReference> packageReferences = null;
                        // Check NuGet packages from package configuration
                        if( File.Exists( packagesConfigPath ) ) // No packages.config : No NuGet packages for this project
                        {
                            logger.Trace( "Project has NuGet package configuration." );
                            packageReferences = ProjectUtils.GetReferencesFromPackageConfig( packagesConfigPath );

                            List<string> projectNuGetPackageNames = new List<string>();

                            // Package loop
                            foreach( INuGetPackageReference packageRef in packageReferences )
                            {
                                logger.Trace( "Found NuGet package reference: {0}", packageRef.Id, packageRef.ToString() );

                                string packageIdentifier = packageRef.Id + '.' + packageRef.Version;

                                IPackage package;
                                if( !solutionPackages.TryGetValue( packageIdentifier, out package ) )
                                {
                                    logger.Trace( "Adding package to solution packages." );
                                    package = packageRef.GetPackageFromRepository( localRepository );
                                    if( package == null )
                                        logger.Error( "Couldn't load package from repository: {0}. Was the NuGet package downloaded?", packageRef.ToString() );
                                    else
                                    {
                                        solutionPackages.Add( packageIdentifier, package );

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
                            foreach( IProjectReference assemblyRef in projectAssemblyReferences.Where( x => !String.IsNullOrEmpty( x.HintPath ) ) )
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
                            nuGetPackageReferencesPerProject.Add( projectItem, packageReferences );
                    }
                }

                // Check that: In the solution, each project references the same version of a NuGet package
                var packagesWithMultipleVersions = solutionPackages.Values
                    .GroupBy( x => x.Id )
                    .Where( x => x.Count() > 1 );

                foreach( var packageGroup in packagesWithMultipleVersions.OrderBy( x => x.Key ) )
                {
                    string packageName = packageGroup.Key;
                    using( logger.OpenGroup( LogLevel.Error, "Multiple versions referenced for package Id: {0}", packageName ) )
                    {
                        foreach( IPackage p in packageGroup.OrderBy( x => x.Version ) )
                        {
                            string version = p.Version.ToString();

                            var referencingProjects = nuGetPackageReferencesPerProject
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
                        solutionPackages.Values,
                        supportedProjectItems,
                        assemblyReferencesPerProject.ToDictionary( x => x.Key, x => (IEnumerable<IProjectReference>)x.Value ),
                        nuGetPackageReferencesPerProject.ToDictionary( x => x.Key, x => (IEnumerable<INuGetPackageReference>)x.Value )
                        );

                return result;
            }
        }
    }
}