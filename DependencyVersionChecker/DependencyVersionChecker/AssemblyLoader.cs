using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Utility class for describing an assembly, and its dependencies.
    /// </summary>
    public class AssemblyLoader
        : IAssemblyLoader
    {
        public event EventHandler<AssemblyLoadingCompleteEventArgs> AsyncAssemblyLoaded;

        /// <summary>
        /// Already-loaded assemblies. Used as cache.
        /// </summary>
        private ConcurrentDictionary<string, AssemblyInfo> _assemblyIndex;
        private readonly object _indexLock = new object();

        /// <summary>
        /// Assemblies that failed to resolve.
        /// </summary>
        public IDictionary<IAssemblyInfo, Exception> UnresolvedAssemblies { get; private set; }

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies.
        /// </summary>
        public AssemblyLoader()
        {
            _assemblyIndex = new ConcurrentDictionary<string, AssemblyInfo>();

            UnresolvedAssemblies = new Dictionary<IAssemblyInfo, Exception>();
        }

        /// <summary>
        /// Describe an assembly, and its dependencies recursively.
        /// </summary>
        /// <param name="assemblyFile">Assembly file to load</param>
        /// <returns>Assembly information</returns>
        public IAssemblyInfo LoadFromFile( FileInfo assemblyFile )
        {
            return LoadFromAssemblyFile( assemblyFile );
        }

        /// <summary>
        /// Describe an assembly, and its dependencies recursively.
        /// </summary>
        /// <param name="assemblyFile">Assembly file to load</param>
        /// <returns>Assembly information</returns>
        public AssemblyInfo LoadFromAssemblyFile( FileInfo assemblyFile )
        {
            // Load from DLL using Mono.Cecil
            ModuleDefinition moduleInfo;
            try
            {
                moduleInfo = ModuleDefinition.ReadModule( assemblyFile.OpenRead() );
            }
            catch( BadImageFormatException ex )
            {
                // Pass invalid DLLs
                throw ex;
            }
            
            if( _assemblyIndex.Keys.Contains( moduleInfo.Assembly.Name.FullName ) )
            {
                return _assemblyIndex[moduleInfo.Assembly.Name.FullName];
            }


            AssemblyInfo outputInfo = AssemblyInfoFromAssemblyNameReference( moduleInfo.Assembly.Name );

            // File version is somewhat tricky to get (It's the constructor parameter of a custom attribute).
            // Might want to do some error-checking later.
            var fileVersionCustomAttribute =
                moduleInfo.Assembly.CustomAttributes
                    .Where( attribute => attribute.AttributeType.FullName == @"System.Reflection.AssemblyFileVersionAttribute" )
                    .Select( attribute => attribute.ConstructorArguments )
                    .FirstOrDefault();

            if( fileVersionCustomAttribute != null )
            {
                string fileVersionString = fileVersionCustomAttribute
                    .FirstOrDefault()
                    .Value
                    .ToString();

                outputInfo.FileVersion = Version.Parse( fileVersionString );
            }

            AssemblyInfo referenceAssemblyInfo;
            AssemblyDefinition resolvedAssembly;

            // Recursively load references.
            foreach( var referenceAssemblyName in moduleInfo.AssemblyReferences )
            {
                referenceAssemblyInfo = null;
                resolvedAssembly = null;

                // Only resolve assemblies if they're not already resolved.
                if( !_assemblyIndex.ContainsKey( referenceAssemblyName.FullName ) )
                {
                    // Resolve assembly.
                    try
                    {
                        resolvedAssembly = moduleInfo.AssemblyResolver.Resolve( referenceAssemblyName.FullName );
                    }
                    catch( Exception ex )
                    {
                        // Can't resolve assembly, but we can still have data from its name reference.
                        referenceAssemblyInfo = AssemblyInfoFromAssemblyNameReference( referenceAssemblyName );
                        UnresolvedAssemblies.Add( referenceAssemblyInfo, ex );
                        Console.WriteLine( "Failed to resolve: {0} ({1})", referenceAssemblyName.FullName, ex.Message );
                    }

                    if( resolvedAssembly != null )
                    {
                        // Take only assemblies in the working folder.
                        if( Path.GetDirectoryName( resolvedAssembly.MainModule.FullyQualifiedName ) ==
                            assemblyFile.DirectoryName )
                        {
                            referenceAssemblyInfo = LoadFromAssemblyFile( new FileInfo( resolvedAssembly.MainModule.FullyQualifiedName ) );
                        }
                    }
                }
                else
                {
                    referenceAssemblyInfo = _assemblyIndex[referenceAssemblyName.FullName];
                }

                if( referenceAssemblyInfo != null )
                {
                    outputInfo.InternalDependencies.Add( referenceAssemblyInfo );
                }
            }

            if( !_assemblyIndex.Keys.Contains( moduleInfo.Assembly.Name.FullName ) )
            {
                _assemblyIndex.TryAdd( moduleInfo.Assembly.Name.FullName, outputInfo );
            }

            return outputInfo;
        }

        public void Reset()
        {
            _assemblyIndex = new ConcurrentDictionary<string, AssemblyInfo>();
            UnresolvedAssemblies = new Dictionary<IAssemblyInfo, Exception>();
        }

        /// <summary>
        /// Describe an assembly from its name reference (using Mono.Cecil).
        /// </summary>
        /// <remarks>
        /// This does not resolve or load the assembly.
        /// Hence, this doesn't get any custom attributes, or list/recurse into dependencies.
        /// </remarks>
        /// <param name="assemblyNameRef">Assembly name reference</param>
        /// <returns>Generated description</returns>
        private static AssemblyInfo AssemblyInfoFromAssemblyNameReference( AssemblyNameReference assemblyNameRef )
        {
            AssemblyInfo outputInfo = new AssemblyInfo();

            outputInfo.AssemblyName = assemblyNameRef.Name;

            outputInfo.AssemblyFullName = assemblyNameRef.FullName;

            outputInfo.AssemblyVersion = assemblyNameRef.Version;

            return outputInfo;
        }

        public void LoadFromFileAsync( FileInfo f )
        {
            Task.Factory.StartNew(() => DoLoadFromFileAsyncTask(f));
        }

        private void DoLoadFromFileAsyncTask(FileInfo f)
        {
            IAssemblyInfo resultAssembly = null;
            Exception exception = null;

            try
            {
                resultAssembly = LoadFromAssemblyFile( f );
            }
            catch( Exception ex )
            {
                exception = ex;
            }

            AssemblyLoadingCompleteEventArgs args = new AssemblyLoadingCompleteEventArgs( f, resultAssembly, exception );

            RaiseAsyncAssemblyLoaded( args );
        }

        private void RaiseAsyncAssemblyLoaded( AssemblyLoadingCompleteEventArgs args )
        {
            if( AsyncAssemblyLoaded != null )
            {
                AsyncAssemblyLoaded( this, args );
            }
        }

        /// <summary>
        /// Load Assembly information from a directory
        /// </summary>
        /// <param name="assemblyDirectory">Directory containing assemblies</param>
        /// <returns>Assembly information</returns>
        //public IEnumerable<IAssemblyInfo> LoadAssembliesFromFolder( DirectoryInfo assemblyDirectory )
        //{
        //    return LoadAssembliesFromFolder( assemblyDirectory, false );
        //}

        /// <summary>
        /// Load Assembly information from a directory
        /// </summary>
        /// <param name="assemblyDirectory">Directory containing assemblies</param>
        /// <param name="recurse">Recurse into subdirectories</param>
        /// <returns>Assembly information</returns>
        //public IEnumerable<IAssemblyInfo> LoadAssembliesFromFolder( DirectoryInfo assemblyDirectory, bool recurse )
        //{
        //    List<AssemblyInfo> foundAssemblies = new List<AssemblyInfo>();

        //    List<FileInfo> fileList = assemblyDirectory.GetFiles( "*.dll" ).ToList();
        //    fileList.AddRange( assemblyDirectory.GetFiles( "*.exe" ) );

        //    AssemblyInfo assembly;

        //    foreach( FileInfo f in fileList )
        //    {
        //        try
        //        {
        //            assembly = LoadFromAssemblyFile( f );
        //        }
        //        catch( BadImageFormatException ex )
        //        {
        //            Console.WriteLine( "Bad image: {0}", f );
        //            continue;
        //        }

        //        foundAssemblies.Add( assembly );
        //    }

        //    if( recurse )
        //        foreach( DirectoryInfo d in assemblyDirectory.GetDirectories() ) LoadAssembliesFromFolder( d, recurse );

        //    return foundAssemblies;
        //}
    }
}