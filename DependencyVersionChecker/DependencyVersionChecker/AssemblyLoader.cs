using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Utility class for describing an assembly, and its dependencies.
    /// </summary>
    public class AssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Already-loaded assemblies. Used as cache.
        /// </summary>
        readonly Dictionary<string, AssemblyInfo> _assemblyIndex;

        readonly BorderChecker _borderChecker;

        public delegate string BorderChecker( AssemblyDefinition newAssembly );

        public static BorderChecker DefaultBorderChecker = ( newReference ) =>
        {
            string company = AssemblyLoader.GetCustomAttributeString( newReference, @"System.Reflection.AssemblyCompanyAttribute " );

            /** Microsoft tokens:
             * "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
             * "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
             * "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
             * "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
             * */

            string[] microsoftTokens = new string[] { "b77a5c561934e089", "31bf3856ad364e35", "b03f5f7f11d50a3a", "7cec85d7bea7798e" };

            string token = BitConverter.ToString( newReference.Name.PublicKeyToken ).Replace( "-", string.Empty ).ToLowerInvariant();

            if( microsoftTokens.Contains( token ) || company == "Microsoft" )
            {
                return "Microsoft";
            }
            if( newReference.MainModule.FullyQualifiedName.StartsWith( Environment.SystemDirectory ) )
            {
                return "System";
            }
            return null;
        };

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies.
        /// </summary>
        public AssemblyLoader( BorderChecker borderChecker )
        {
            _borderChecker = borderChecker;
            _assemblyIndex = new Dictionary<string, AssemblyInfo>();
        }

        public IEnumerable<IAssemblyInfo> Assemblies
        {
            get { return _assemblyIndex.Values.Distinct(); }
        }

        public IEnumerable<IAssemblyInfo> LoadFromDirectory( DirectoryInfo assemblyDirectory, bool recurse )
        {
            List<IAssemblyInfo> foundAssemblies = new List<IAssemblyInfo>();

            List<FileInfo> fileList = assemblyDirectory.GetFiles( "*.dll" ).ToList();
            fileList.AddRange( assemblyDirectory.GetFiles( "*.exe" ) );

            IAssemblyInfo assembly;

            foreach( FileInfo f in fileList )
            {
                assembly = LoadFromFile( f );

                foundAssemblies.Add( assembly );
            }

            if( recurse )
                foreach( DirectoryInfo d in assemblyDirectory.GetDirectories() ) LoadFromDirectory( d, recurse );

            return foundAssemblies;
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
        public IAssemblyInfo LoadFromAssemblyFile( FileInfo assemblyFile )
        {
            AssemblyInfo outputInfo;
            if( _assemblyIndex.TryGetValue( assemblyFile.FullName, out outputInfo ) ) return outputInfo;

            try
            {
                // Load from DLL using Mono.Cecil
                ModuleDefinition moduleInfo;
                using( var s = assemblyFile.OpenRead() )
                {
                    moduleInfo = ModuleDefinition.ReadModule( s );
                }
                
                if( !_assemblyIndex.TryGetValue( moduleInfo.Assembly.Name.FullName, out outputInfo ) )
                {
                    outputInfo = CreateAssemblyInfoFromAssemblyNameReference( moduleInfo.Assembly.Name );
                    Debug.Assert( assemblyFile.FullName == moduleInfo.FullyQualifiedName );
                }
                outputInfo.Paths.Add( assemblyFile.FullName );

                LoadAssembly( moduleInfo, outputInfo );
            }
            catch( Exception ex )
            {
                if( outputInfo == null )
                {
                    outputInfo = new AssemblyInfo() { AssemblyFullName = assemblyFile.FullName };
                    _assemblyIndex.Add( assemblyFile.FullName, outputInfo );
                }
                outputInfo.Error = ex;
            }
            return outputInfo;
        }

        private void LoadAssembly( ModuleDefinition moduleInfo, AssemblyInfo outputInfo )
        {
            if( _assemblyIndex.Keys.Contains( moduleInfo.FullyQualifiedName ) )
                return; // TODO: May load twice

            try
            {
                // Most custom attributes are optional.
                outputInfo.FileVersion = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyFileVersionAttribute" );
                outputInfo.InformationalVersion = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyInformationalVersionAttribute" );
                outputInfo.Description = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyDescriptionAttribute" );
                outputInfo.BorderName = _borderChecker != null ? _borderChecker( moduleInfo.Assembly ) : null;
                if( outputInfo.BorderName == null )
                {
                    ResolveReferences( moduleInfo, outputInfo );
                }
            }
            catch( Exception ex )
            {
                outputInfo.Error = ex;
            }
        }

        private void ResolveReferences( ModuleDefinition moduleInfo, AssemblyInfo outputInfo )
        {
            AssemblyInfo referenceAssemblyInfo;
            AssemblyDefinition resolvedAssembly;

            // Recursively load references.
            foreach( var referenceAssemblyName in moduleInfo.AssemblyReferences )
            {
                referenceAssemblyInfo = null;
                resolvedAssembly = null;

                if( !_assemblyIndex.TryGetValue( referenceAssemblyName.FullName, out referenceAssemblyInfo ) )
                {
                    // Resolve assembly.
                    try
                    {
                        resolvedAssembly = moduleInfo.AssemblyResolver.Resolve( referenceAssemblyName.FullName );
                    }
                    catch( Exception ex )
                    {
                        // Can't resolve assembly, but we can still have data from its name reference.
                        referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( referenceAssemblyName );
                        referenceAssemblyInfo.Error = ex;
                    }
                    if( referenceAssemblyInfo == null )
                    {
                        referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( referenceAssemblyName );
                        referenceAssemblyInfo.Paths.Add( resolvedAssembly.MainModule.FullyQualifiedName );
                        LoadAssembly( resolvedAssembly.MainModule, referenceAssemblyInfo );
                    }
                }
                Debug.Assert( referenceAssemblyInfo != null );
                outputInfo.InternalDependencies.Add( referenceAssemblyInfo );
            }
        }

        public void Reset()
        {
            _assemblyIndex.Clear();
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
        AssemblyInfo CreateAssemblyInfoFromAssemblyNameReference( AssemblyNameReference assemblyNameRef )
        {
            AssemblyInfo outputInfo = new AssemblyInfo()
            {
                AssemblyFullName = assemblyNameRef.FullName,
                SimpleName = assemblyNameRef.Name,
                Version = assemblyNameRef.Version,
                Culture = assemblyNameRef.Culture
            };
            _assemblyIndex.Add( assemblyNameRef.FullName, outputInfo );
            return outputInfo;
        }

        /// <summary>
        /// Gets the value of a custom attribute type (using Mono.Cecil).
        /// </summary>
        /// <param name="assembly">Assembly to examine</param>
        /// <param name="attributeTypeName">Attribute type to get value from</param>
        /// <returns>Value of attribute, or String.Empty if not found.</returns>
        public static string GetCustomAttributeString( AssemblyDefinition assembly, string attributeTypeName )
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

    }
}