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
            if( loader == null )
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
            if( !file.Exists )
            {
                throw new FileNotFoundException( "Assembly file does not exist", file.FullName );
            }

            if( !_filesToCheck.Contains( file ) )
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

            foreach( FileInfo f in fileList )
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
            catch( UnauthorizedAccessException ex )
            {
                Console.WriteLine( ex );
                return new List<FileInfo>();
            }

            if( recurse )
                foreach( DirectoryInfo d in dir.GetDirectories() ) fileList.AddRange( ListAssembliesFromDirectory( d, recurse ) );

            return fileList;
        }

        /// <summary>
        /// Begins checking the assemblies registered with AddFile/AddDirectory.
        /// </summary>
        /// <returns>AssemblyCheckResult object.</returns>
        public AssemblyCheckResult Check()
        {
            List<IAssemblyInfo> assemblies;
            List<AssemblyReferenceName> dependencies = new List<AssemblyReferenceName>();

            foreach( var f in _filesToCheck )
            {
                _assemblyLoader.LoadFromFile( f );
            }

            assemblies = _assemblyLoader.Assemblies.Where( x => x.BorderName == null ).ToList();

            var conflicts = GetConflictsFromAssemblyList( assemblies );

            var refMismatches = GetReferenceMismatches( assemblies );

            AssemblyCheckResult result = new AssemblyCheckResult( assemblies, dependencies, conflicts, refMismatches );

            return result;
        }

        /// <summary>
        /// Find assembly name references where multiple versions are found.
        /// </summary>
        /// <param name="assemblies">Collection of assemblies to check</param>
        /// <returns>Collection of dependencies with discrepancies</returns>
        public static IEnumerable<AssemblyReferenceName> GetConflictsFromAssemblyList( IEnumerable<IAssemblyInfo> assemblies )
        {
            List<AssemblyReferenceName> dependencies = new List<AssemblyReferenceName>();
            foreach( var assembly in assemblies )
            {
                if( assembly != null )
                    dependencies = GetAssemblyDependencies( assembly, dependencies, 0 );
            }

            // Only get dependencies with multiple links
            var conflicts =
                dependencies
                .Where( x => x.ReferenceLinks.Count >= 2 )
                .Where( x => x.HasConflict )
                .Select( x => x )
                .ToList();

            return conflicts;
        }

        /// <summary>
        /// Find references where resolution worked, but for a different version.
        /// </summary>
        /// <param name="assemblies">Collection of assemblies to check</param>
        /// <returns>Collection of references</returns>
        public static IEnumerable<AssemblyReference> GetReferenceMismatches( IEnumerable<IAssemblyInfo> assemblies )
        {
            List<AssemblyReference> references = new List<AssemblyReference>();
            List<IAssemblyInfo> parsedAssemblies = new List<IAssemblyInfo>();
            foreach( var assembly in assemblies )
            {
                if( assembly != null )
                    references = GetAssemblyReferences( assembly, references, parsedAssemblies );
            }

            var conflicts = references.Where( x => x.HasVersionMismatch );

            return conflicts;
        }

        private static List<AssemblyReferenceName> GetAssemblyDependencies( IAssemblyInfo info )
        {
            return GetAssemblyDependencies( info, new List<AssemblyReferenceName>(), 0 );
        }

        private static List<AssemblyReferenceName> GetAssemblyDependencies( IAssemblyInfo info, List<AssemblyReferenceName> existingDependencies, int depth )
        {
            if( existingDependencies.Any( x => x.AssemblyName == info.SimpleName ) )
                return existingDependencies;

            depth++;
            if( depth > 100 )
                throw new Exception( "Possible assembly recursion" ); // Here to prevent stack overflows
            foreach( var pair in info.Dependencies )
            {
                IAssemblyInfo dep = pair.Value;
                if( dep.BorderName != null )
                    continue; // Ignore bordering dependencies

                AssemblyReferenceName dependencyItem = existingDependencies
                    .Where( x => x.AssemblyName == dep.SimpleName )
                    .FirstOrDefault();

                if( dependencyItem == null )
                {
                    dependencyItem = new AssemblyReferenceName( dep.SimpleName );
                    existingDependencies.Add( dependencyItem );
                }
                if( !dependencyItem.ReferenceLinks.Keys.Contains( info ) )
                    dependencyItem.Add( info, dep );

                GetAssemblyDependencies( dep, existingDependencies, depth );
            }

            return existingDependencies;
        }

        private static List<AssemblyReference> GetAssemblyReferences( IAssemblyInfo info )
        {
            return GetAssemblyReferences( info, new List<AssemblyReference>(), new List<IAssemblyInfo>() );
        }

        private static List<AssemblyReference> GetAssemblyReferences( IAssemblyInfo info, List<AssemblyReference> existingReferences, List<IAssemblyInfo> parsedAssemblies )
        {
            if( parsedAssemblies.Contains( info ) )
                return existingReferences;
            else
                parsedAssemblies.Add( info );

            foreach( var pair in info.Dependencies )
            {
                IAssemblyInfo dep = pair.Value;
                string referenceName = pair.Key;

                if( dep.BorderName != null )
                    continue; // Ignore bordering dependencies

                AssemblyReference referenceItem = new AssemblyReference( info, referenceName, dep );
                existingReferences.Add( referenceItem );

                GetAssemblyReferences( dep, existingReferences, parsedAssemblies );
            }
            return existingReferences;
        }
    }

    /// <summary>
    /// Represents a reference from an assembly to another by assembly full name, and the resolved assemblies.
    /// </summary>
    public class AssemblyReference
    {
        /// <summary>
        /// The assembly which has the reference.
        /// </summary>
        public IAssemblyInfo Parent { get; private set; }

        /// <summary>
        /// The reference which the parent assembly uses.
        /// </summary>
        public string ReferenceName { get; private set; }

        /// <summary>
        /// An incomplete IAssemblyInfo, derived from the ReferenceName.
        /// </summary>
        /// <remarks>
        /// If you need the actual references assembly, use ReferencedAssembly.
        /// </remarks>
        public IAssemblyInfo ReferenceNameAssemblyObject { get; private set; }

        /// <summary>
        /// The resolved assembly which was referenced by the ReferenceName.
        /// </summary>
        public IAssemblyInfo ReferencedAssembly { get; private set; }

        /// <summary>
        /// Whether the ReferenceName version does not match the actual, resolved assembly.
        /// </summary>
        public bool HasVersionMismatch { get; private set; }

        internal AssemblyReference( IAssemblyInfo parent, string referenceName, IAssemblyInfo child )
        {
            Parent = parent;
            ReferenceName = referenceName;
            ReferencedAssembly = child;

            ReferenceNameAssemblyObject = AssemblyLoader.ParseAssemblyInfoFromString( ReferenceName );

            if( ReferenceNameAssemblyObject.Version != ReferencedAssembly.Version )
                HasVersionMismatch = true;
            else
                HasVersionMismatch = false;
        }
    }
}