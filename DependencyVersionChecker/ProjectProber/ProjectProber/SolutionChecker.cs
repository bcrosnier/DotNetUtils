using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    public class SolutionChecker
    {
        ISolution _solution;
        string _packageRoot;

        Dictionary <string, IPackage> _solutionPackages;
        Dictionary <string, List<string>> _packageVersions;

        Dictionary <ISolutionProjectItem, List<IPackageLibraryReference>> _projectReferences;

        public IEnumerable<ISolutionProjectItem> GetProjectsReferencing( string packageIdVersion )
        {
            return
                _projectReferences
                .Where( x => x.Value.Any( y => y.PackageIdVersion == packageIdVersion ) )
                .Select( x => x.Key);

        }

        public IReadOnlyDictionary<string, IPackage> Packages
        {
            get
            {
                return _solutionPackages;
            }
        }

        public IReadOnlyDictionary<ISolutionProjectItem, List<IPackageLibraryReference>> ProjectReferences
        {
            get
            {
                return _projectReferences;
            }
        }

        public IDictionary<string, List<string>> PackageNamesWithMultipleVersions
        {
            get
            {
                return _packageVersions.Where( x => x.Value.Count > 1 ).ToDictionary( x => x.Key, x => x.Value );
            }
        }

        private SolutionChecker(ISolution s, string packageRoot)
        {
            _solution = s;
            _packageRoot = packageRoot;

            _solutionPackages = new Dictionary<string, IPackage>();
            _packageVersions = new Dictionary<string, List<string>>();
            _projectReferences = new Dictionary<ISolutionProjectItem, List<IPackageLibraryReference>>();
        }

        private void Check()
        {
            foreach( ISolutionProjectItem projectItem in _solution.Projects.Where( i => SolutionUtils.GetProjectType(i) == SolutionProjectType.VISUAL_C_SHARP ) )
            {
                List<IPackageLibraryReference> references = new List<IPackageLibraryReference>();

                string projectPath = Path.Combine( _solution.DirectoryPath, projectItem.ProjectPath );

                IEnumerable<string> packageAssemblyPaths = ProjectUtils.GetPackageLibraryReferences( projectPath, _solution.DirectoryPath );

                foreach( string path in packageAssemblyPaths )
                {
                    IPackageLibraryReference reference = ProjectUtils.ParseReferenceFromPath( path );
                    if( reference == null )
                        continue;

                    references.Add( reference );

                    IPackage package = null;
                    if( !_solutionPackages.TryGetValue( reference.PackageIdVersion, out package ) )
                    {
                        package = ProjectUtils.GetPackageFromReference( reference, _packageRoot );

                        _solutionPackages.Add( reference.PackageIdVersion, package );

                        List<string> packageIdVersions;
                        if( !_packageVersions.TryGetValue( package.Id, out packageIdVersions ) )
                        {
                            packageIdVersions = new List<string>();

                            packageIdVersions.Add( reference.PackageIdVersion );

                            _packageVersions.Add( package.Id, packageIdVersions );
                        } else if( !packageIdVersions.Contains( reference.PackageIdVersion ) )
                        {
                            packageIdVersions.Add( reference.PackageIdVersion );
                        }
                    }
                }

                _projectReferences.Add( projectItem, references );
            }
        }

        public static SolutionChecker CheckSolutionFile( string slnFilePath )
        {
            if( String.IsNullOrEmpty( slnFilePath ) )
                throw new ArgumentNullException( "slnFilePath" );
            if( !File.Exists( slnFilePath ) )
                throw new ArgumentException( "File must exist", "slnFilePath" );

            string packageRoot = Path.Combine( Path.GetDirectoryName( slnFilePath ), "packages" );

            Debug.Assert( Directory.Exists( packageRoot ), "Package root exists" );

            ISolution s = SolutionFactory.ReadFromSolutionFile( slnFilePath );

            SolutionChecker checker = new SolutionChecker(s, packageRoot);

            checker.Check();

            return checker;
        }
    }
}
