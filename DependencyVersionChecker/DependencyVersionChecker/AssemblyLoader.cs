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

            AssemblyInfo outputInfo;

            // Cache lock
            if( _assemblyIndex.Keys.Contains( moduleInfo.Assembly.Name.FullName ) )
            {
                return _assemblyIndex[moduleInfo.Assembly.Name.FullName];
            }
            else
            {
                outputInfo = AssemblyInfoFromAssemblyNameReference( moduleInfo.Assembly.Name );
                Console.WriteLine( "Adding {0}.", moduleInfo.Assembly.Name );
                _assemblyIndex.TryAdd( moduleInfo.Assembly.Name.FullName, outputInfo );
            }

            // Most custom attributes are optional.
            outputInfo.FileVersion = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyFileVersionAttribute" );
            outputInfo.InformationalVersion = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyInformationalVersionAttribute" );

            AssemblyInfo referenceAssemblyInfo;
            AssemblyDefinition resolvedAssembly;
            bool cached = false;

            // Recursively load references.
            foreach( var referenceAssemblyName in moduleInfo.AssemblyReferences )
            {
                referenceAssemblyInfo = null;
                resolvedAssembly = null;
                cached = false;

                if( _assemblyIndex.ContainsKey( referenceAssemblyName.FullName ) )
                {
                    referenceAssemblyInfo = _assemblyIndex[referenceAssemblyName.FullName];
                    cached = true;
                }
                else
                {
                    cached = false;
                }

                // Only resolve assemblies if they're not already resolved.
                if( !cached )
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
                        // Not going through LoadFromAssemblyFile, so adding to index here.
                        if( !_assemblyIndex.Keys.Contains( referenceAssemblyName.FullName ) )
                        {
                            _assemblyIndex.TryAdd( referenceAssemblyName.FullName, referenceAssemblyInfo );
                        }
                    }

                    if( resolvedAssembly != null )
                    {
                        // Take only assemblies in the working folder.
                        if( Path.GetDirectoryName( resolvedAssembly.MainModule.FullyQualifiedName ) ==
                            assemblyFile.DirectoryName )
                        {
                            Console.WriteLine( "{0} wasn't cached.", referenceAssemblyName.Name );
                            referenceAssemblyInfo = LoadFromAssemblyFile( new FileInfo( resolvedAssembly.MainModule.FullyQualifiedName ) );
                        }
                    }
                }
                else
                {
                    Console.WriteLine( "{0} was cached.", referenceAssemblyName.Name );
                    referenceAssemblyInfo = _assemblyIndex[referenceAssemblyName.FullName];
                }

                if( referenceAssemblyInfo != null )
                {
                    outputInfo.InternalDependencies.Add( referenceAssemblyInfo );
                }
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

            outputInfo.AssemblyFullName = assemblyNameRef.FullName;

            outputInfo.SimpleName = assemblyNameRef.Name;

            outputInfo.Version = assemblyNameRef.Version;

            outputInfo.Culture = assemblyNameRef.Culture;

            return outputInfo;
        }

        /// <summary>
        /// Gets the value of a custom attribute type (using Mono.Cecil).
        /// </summary>
        /// <param name="assembly">Assembly to examine</param>
        /// <param name="attributeTypeName">Attribute type to get value from</param>
        /// <returns>Value of attribute, or String.Empty if not found.</returns>
        private static string GetCustomAttributeString( AssemblyDefinition assembly, string attributeTypeName )
        {
            var customAttributeConstructorArguments =
                assembly.CustomAttributes
                .Where( attribute => attribute.AttributeType.FullName == attributeTypeName )
                .Select( attribute => attribute.ConstructorArguments )
                .FirstOrDefault();

            if( customAttributeConstructorArguments != null )
            {
                string attributeValue = customAttributeConstructorArguments
                    .FirstOrDefault()
                    .Value as string;

                return attributeValue;
            }
            else
            {
                return String.Empty;
            }
        }

        public void LoadFromFileAsync( FileInfo f )
        {
            Task.Factory.StartNew( () => DoLoadFromFileAsyncTask( f ) );
        }

        private void DoLoadFromFileAsyncTask( FileInfo f )
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