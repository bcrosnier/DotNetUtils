using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using ProjectProber.Impl;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    public static class ProjectUtils
    {
        private static readonly string PACKAGE_PATH_PATTERN = @"\.\.\\packages\\([^\\]+)\\lib\\(?:([^\\]+)\\)?([^\\]+)";

        public static IEnumerable<ProjectItem> LoadProjectReferencesFromFile( string projectPath )
        {
            return LoadProjectReferencesFromFile( projectPath, null );
        }

        public static IEnumerable<ProjectItem> LoadProjectReferencesFromFile( string projectPath, string solutionDir )
        {
            Dictionary<string, string> globalProperties = new Dictionary<string, string>();
            if( solutionDir != null )
            {
                globalProperties.Add( "SolutionDir", solutionDir );
            }
            Project p = new Project( projectPath, globalProperties, null );

            IEnumerable<ProjectItem> items = p.AllEvaluatedItems
                .Where( pi => pi.ItemType == "Reference" || pi.ItemType == "ProjectReference" );

            p.ProjectCollection.UnloadAllProjects();

            return items;
        }

        public static IEnumerable<string> GetPackageLibraryReferences( string projectPath)
        {
            return GetPackageLibraryReferences( projectPath, null );
        }
        public static IEnumerable<string> GetPackageLibraryReferences( string projectPath, string solutionDir )
        {
            var items = LoadProjectReferencesFromFile( projectPath, solutionDir );

            var itemsWithMetadata = items
                .Where( i => i.MetadataCount > 0 );

            var hintPaths = itemsWithMetadata
                .Select(
                    i => i.Metadata
                        .Where( m => m.Name == "HintPath" )
                        .FirstOrDefault()
                )
                .Where( a => a != null );

            return hintPaths.Select( a => a.EvaluatedValue );
        }

        public static IPackageLibraryReference ParseReferenceFromPath( string path )
        {
            Match m = Regex.Match( path, PACKAGE_PATH_PATTERN );
            if( m.Success )
            {
                string packageIdVersion = m.Groups[1].Value;
                string targetFramework = m.Groups[2].Value;
                string assemblyFilename = m.Groups[3].Value;

                PackageLibraryReference libRef = new PackageLibraryReference( packageIdVersion, targetFramework, assemblyFilename, path );

                return libRef;
            }
            else
            {
                return null;
            }
        }

        public static NuGet.IPackage GetPackageFromReference( IPackageLibraryReference libReference, string packageRoot )
        {
            string packageFile = Path.Combine( packageRoot, libReference.PackageIdVersion, libReference.PackageIdVersion + ".nupkg" );

            Debug.Assert( File.Exists( packageFile ), "Package file was found" );

            NuGet.IPackage package = new NuGet.ZipPackage( packageFile );

            return package;
        }
    }
}
