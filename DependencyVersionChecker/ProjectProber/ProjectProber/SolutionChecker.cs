using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet;
using ProjectProber.Interfaces;
using ProjectProber;

namespace ProjectProber
{
    /// <summary>
    /// Solution checking utility.
    /// </summary>
    /// <seealso cref="SolutionChecker.CheckSolutionFile"/>
    public static class SolutionChecker
    {
        /// <summary>
        /// Checks a solution file.
        /// </summary>
        /// <param name="filePath">Solution file path</param>
        /// <returns>SolutionCheckResult</returns>
        public static SolutionCheckResult CheckSolutionFile( string filePath )
        {
            if( String.IsNullOrEmpty( filePath ) )
                throw new ArgumentNullException( "filePath" );
            if( !File.Exists( filePath ) )
                throw new ArgumentException( "File must exist", "filePath" );

            string solutionDirectory = Path.GetDirectoryName( filePath );
            string packageRoot = Path.Combine( solutionDirectory, "packages" );

            LocalPackageRepository localRepository = new LocalPackageRepository( packageRoot );

            // Key: package identifier (Name.Version)
            Dictionary<string, IPackage> solutionPackages = new Dictionary<string, IPackage>();

            Dictionary<ISolutionProjectItem, List<INuGetPackageAssemblyReference>> assemblyReferences =
                new Dictionary<ISolutionProjectItem, List<INuGetPackageAssemblyReference>>();

            Dictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>> projectPackageReferences =
                new Dictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>>();

            ISolution solution = SolutionFactory.ReadFromSolutionFile( filePath );

            IEnumerable<ISolutionProjectItem> supportedProjectItems =
                solution.Projects
                .Where( project => project.GetItemType() == SolutionProjectType.VISUAL_C_SHARP );

            // Loop in projects
            foreach( ISolutionProjectItem projectItem in supportedProjectItems )
            {
                string projectFilePath = Path.Combine( solutionDirectory, projectItem.ProjectPath );
                string projectDirectory = Path.GetDirectoryName( projectFilePath );
                string packagesConfigPath = Path.Combine( projectDirectory, "packages.config" );

                // Check assembly references from NuGet packages
                IEnumerable<string> nugetAssembliesPaths = ProjectUtils.GetPackageAssemblyReferencePaths( projectFilePath );

                List<INuGetPackageAssemblyReference> projectAssemblyReferences = new List<INuGetPackageAssemblyReference>();
                foreach( string assemblyPath in nugetAssembliesPaths )
                {
                    // Note: NuGet.Core might have the same one; have to check.
                    INuGetPackageAssemblyReference reference = ProjectUtils.ParseReferenceFromPath( assemblyPath );

                    // reference is nulled if the assembly isn't a NuGet package assembly (ie. a third party DLL)
                    if( reference != null )
                        projectAssemblyReferences.Add( reference );
                }

                assemblyReferences.Add( projectItem, projectAssemblyReferences );

                IEnumerable<INuGetPackageReference> packageReferences = null;
                // Check NuGet packages from package configuration
                if( File.Exists( packagesConfigPath ) ) // No packages.config : No NuGet package for this project
                {
                    packageReferences =
                    ProjectUtils.GetReferencesFromPackageConfig( packagesConfigPath );


                    foreach( INuGetPackageReference packageRef in packageReferences )
                    {
                        string packageIdentifier = packageRef.Id + '.' + packageRef.Version;

                        IPackage package;
                        if( !solutionPackages.TryGetValue( packageIdentifier, out package ) )
                        {
                            package = packageRef.GetPackageFromRepository( localRepository );
                            if( package != null )
                                solutionPackages.Add( packageIdentifier, package );
                        }

                        // Might want to link project to package directly here later
                    }
                }
                projectPackageReferences.Add( projectItem, packageReferences );
            }


            SolutionCheckResult result =
                new SolutionCheckResult(
                    filePath,
                    solutionPackages.Values,
                    supportedProjectItems,
                    assemblyReferences.ToDictionary( x => x.Key, x => (IEnumerable<INuGetPackageAssemblyReference>)x.Value ),
                    projectPackageReferences.ToDictionary( x => x.Key, x => (IEnumerable<INuGetPackageReference>)x.Value )
                    );

            return result;
        }
    }
}