using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Solution check helper. Use CheckSolutionFile() to use.
    /// </summary>
    /// <seealso cref="SolutionChecker.CheckSolutionFile"/>
    public class SolutionChecker
    {
        private ISolution _solution;
        private string _packageRoot;

        private Dictionary<string, IPackage> _solutionPackages;
        private Dictionary<string, List<string>> _packageVersions;

        private Dictionary<ISolutionProjectItem, List<IPackageLibraryReference>> _projectReferences;

        /// <summary>
        /// Get a list of project referencing the given full package identifier (PackageId.PackageVersion)
        /// </summary>
        /// <param name="packageIdVersion">Package identifier to search, in the format "PackageId.PackageVersion", eg. CK.Core.2.8.14</param>
        /// <returns></returns>
        public IEnumerable<ISolutionProjectItem> GetProjectsReferencing( string packageIdVersion )
        {
            return
                _projectReferences
                .Where( x => x.Value.Any( y => y.PackageIdVersion == packageIdVersion ) )
                .Select( x => x.Key );
        }

        /// <summary>
        /// Gets a dictionary associating the full package identifiers (PackageId.PackageVersion) to their found NuGet IPackages.
        /// </summary>
        public IReadOnlyDictionary<string, IPackage> Packages
        {
            get
            {
                return _solutionPackages;
            }
        }

        /// <summary>
        /// Gets a dictionary associating solution projects to all their NuGet package references.
        /// </summary>
        public IReadOnlyDictionary<ISolutionProjectItem, List<IPackageLibraryReference>> ProjectReferences
        {
            get
            {
                return _projectReferences;
            }
        }

        /// <summary>
        /// Gets a dictionary associating a simple package id (eg. CK.Core) to all matching full package identifiers detected during the check, when there are multiple.
        /// </summary>
        /// <example>
        /// There are two CK.Core with two distinct versions referenced during the check.
        /// Entry will be:
        /// Key: CK.Core
        /// Value: { "CK.Core.2.1.0", "CK.Core.2.8.5" }
        /// </example>
        public IDictionary<string, List<string>> PackageNamesWithMultipleVersions
        {
            get
            {
                return _packageVersions.Where( x => x.Value.Count > 1 ).ToDictionary( x => x.Key, x => x.Value );
            }
        }

        private SolutionChecker( ISolution s, string packageRoot )
        {
            _solution = s;
            _packageRoot = packageRoot;

            _solutionPackages = new Dictionary<string, IPackage>();
            _packageVersions = new Dictionary<string, List<string>>();
            _projectReferences = new Dictionary<ISolutionProjectItem, List<IPackageLibraryReference>>();
        }

        private void Check()
        {
            foreach ( ISolutionProjectItem projectItem in _solution.Projects.Where( i => SolutionUtils.GetProjectType( i ) == SolutionProjectType.VISUAL_C_SHARP ) )
            {
                List<IPackageLibraryReference> references = new List<IPackageLibraryReference>();

                string projectPath = Path.Combine( _solution.DirectoryPath, projectItem.ProjectPath );

                IEnumerable<string> packageAssemblyPaths = ProjectUtils.GetPackageLibraryReferences( projectPath, _solution.DirectoryPath );

                foreach ( string path in packageAssemblyPaths )
                {
                    IPackageLibraryReference reference = ProjectUtils.ParseReferenceFromPath( path );
                    if ( reference == null )
                        continue;

                    references.Add( reference );

                    IPackage package = null;
                    if ( !_solutionPackages.TryGetValue( reference.PackageIdVersion, out package ) )
                    {
                        package = ProjectUtils.GetPackageFromReference( reference, _packageRoot );

                        _solutionPackages.Add( reference.PackageIdVersion, package );

                        List<string> packageIdVersions;
                        if ( !_packageVersions.TryGetValue( package.Id, out packageIdVersions ) )
                        {
                            packageIdVersions = new List<string>();

                            packageIdVersions.Add( reference.PackageIdVersion );

                            _packageVersions.Add( package.Id, packageIdVersions );
                        }
                        else if ( !packageIdVersions.Contains( reference.PackageIdVersion ) )
                        {
                            packageIdVersions.Add( reference.PackageIdVersion );
                        }
                    }
                }

                _projectReferences.Add( projectItem, references );
            }
        }

        /// <summary>
        /// Checks given solution file for any discrepancies.
        /// </summary>
        /// <param name="slnFilePath">Solution file to check. Must exist.</param>
        /// <returns>SolutionChecker object, containing properties with solution references, NuGet package reference, and other discrepancies.</returns>
        public static SolutionChecker CheckSolutionFile( string slnFilePath )
        {
            if ( String.IsNullOrEmpty( slnFilePath ) )
                throw new ArgumentNullException( "slnFilePath" );
            if ( !File.Exists( slnFilePath ) )
                throw new ArgumentException( "File must exist", "slnFilePath" );

            string packageRoot = Path.Combine( Path.GetDirectoryName( slnFilePath ), "packages" );

            Debug.Assert( Directory.Exists( packageRoot ), "Package root exists" );

            ISolution s = SolutionFactory.ReadFromSolutionFile( slnFilePath );

            SolutionChecker checker = new SolutionChecker( s, packageRoot );

            checker.Check();

            return checker;
        }
    }
}