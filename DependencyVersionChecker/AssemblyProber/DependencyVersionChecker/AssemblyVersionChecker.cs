using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssemblyProber
{
    /// <summary>
    /// Utility to get dependency links from a set of assemblies.
    /// Usage: Instanciate, add files and folders, and run Check().
    /// </summary>
    public class AssemblyVersionChecker
    {
        private List<FileInfo> _filesToCheck;
        private IAssemblyLoader _assemblyLoader;

        /// <summary>
        /// Creates a new instance of AssemblyVersionChecker.
        /// </summary>
        /// <param name="loader">IAssemblyLoader used for loading assemblies.</param>
        public AssemblyVersionChecker( IAssemblyLoader loader )
        {
            if ( loader == null )
            {
                throw new ArgumentNullException( "loader" );
            }
            _assemblyLoader = loader;
            Initialize();
        }

        private void Initialize()
        {
            _filesToCheck = new List<FileInfo>();
        }

        /// <summary>
        /// Resets this AssemblyVersionChecker, clearing any data it kept.
        /// </summary>
        public void Reset()
        {
            _assemblyLoader.Reset();
            Initialize();
        }

        /// <summary>
        /// Adds a new file to be checked by Check().
        /// </summary>
        /// <param name="file">File to add. Must exist.</param>
        public void AddFile( FileInfo file )
        {
            if ( !file.Exists )
            {
                throw new FileNotFoundException( "Assembly file does not exist", file.FullName );
            }

            if ( !_filesToCheck.Contains( file ) )
                _filesToCheck.Add( file );
        }

        /// <summary>
        /// Adds all assemblies from a directory to be checked by Check().
        /// </summary>
        /// <param name="dir">Directory to search files in. Must exist.</param>
        /// <param name="recurse">If true, will recurse into subdirectories.</param>
        public void AddDirectory( DirectoryInfo dir, bool recurse )
        {
            IEnumerable<FileInfo> fileList = ListAssembliesFromDirectory( dir, recurse );

            foreach ( FileInfo f in fileList )
                AddFile( f );
        }

        /// <summary>
        /// Get a list of potential assembly files (.dll, .exe) from a directory. Static utility.
        /// </summary>
        /// <param name="dir">Directory to search files in. Must exist.</param>
        /// <param name="recurse">If true, will recurse into subdirectories.</param>
        /// <returns>List of potential assembly files</returns>
        public static IEnumerable<FileInfo> ListAssembliesFromDirectory( DirectoryInfo dir, bool recurse )
        {
            List<FileInfo> fileList;
            try
            {
                fileList = dir.GetFiles( "*.dll" ).ToList();
                fileList.AddRange( dir.GetFiles( "*.exe" ) );
            }
            catch ( UnauthorizedAccessException ex )
            {
                Console.WriteLine( ex );
                return new List<FileInfo>();
            }

            if ( recurse )
                foreach ( DirectoryInfo d in dir.GetDirectories() ) fileList.AddRange( ListAssembliesFromDirectory( d, recurse ) );

            return fileList;
        }

        /// <summary>
        /// Begins checking the assemblies registered with AddFile/AddDirectory.
        /// </summary>
        /// <returns>AssemblyCheckResult object.</returns>
        public AssemblyCheckResult Check()
        {
            List<IAssemblyInfo> assemblies;
            List<DependencyAssembly> dependencies = new List<DependencyAssembly>();

            foreach ( var f in _filesToCheck )
            {
                _assemblyLoader.LoadFromFile( f );
            }

            assemblies = _assemblyLoader.Assemblies.Where( x => x.BorderName == null ).ToList();

            var conflicts = GetConflictsFromAssemblyList( assemblies );

            AssemblyCheckResult result = new AssemblyCheckResult( assemblies, dependencies, conflicts );

            return result;
        }

        /// <summary>
        /// Find reference assembly version discrepancies within a given list of assemblies.
        /// </summary>
        /// <param name="assemblies">Collection of assemblies to check</param>
        /// <returns>Collection of dependencies with discrepancies</returns>
        public static IEnumerable<DependencyAssembly> GetConflictsFromAssemblyList( IEnumerable<IAssemblyInfo> assemblies )
        {
            List<DependencyAssembly> dependencies = new List<DependencyAssembly>();
            foreach ( var assembly in assemblies )
            {
                if ( assembly != null )
                    dependencies = GetAssemblyDependencies( assembly, dependencies );
            }

            // Only get dependencies with multiple links
            var conflicts =
                dependencies
                .Where( x => x.DependencyLinks.Count >= 2 )
                .Where( x => x.HasConflict )
                .Select( x => x )
                .ToList();

            return conflicts;
        }

        private static List<DependencyAssembly> GetAssemblyDependencies( IAssemblyInfo info )
        {
            return GetAssemblyDependencies( info, new List<DependencyAssembly>() );
        }

        private static List<DependencyAssembly> GetAssemblyDependencies( IAssemblyInfo info, List<DependencyAssembly> existingDependencies )
        {
            foreach ( IAssemblyInfo dep in info.Dependencies )
            {
                if ( dep.BorderName != null )
                    continue; // Ignore bordering dependencies

                DependencyAssembly dependencyItem = existingDependencies
                    .Where( x => x.AssemblyName == dep.SimpleName )
                    .FirstOrDefault();

                if ( dependencyItem == null )
                {
                    dependencyItem = new DependencyAssembly( dep.SimpleName );
                    existingDependencies.Add( dependencyItem );
                }
                if ( !dependencyItem.DependencyLinks.Keys.Contains( info ) )
                    dependencyItem.Add( info, dep );

                GetAssemblyDependencies( dep, existingDependencies );
            }

            return existingDependencies;
        }
    }
}