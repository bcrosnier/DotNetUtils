using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CK.Core;
using Mono.Cecil;

namespace AssemblyProber
{
    /// <summary>
    /// Utility class for describing an assembly, and its dependencies.
    /// </summary>
    public class AssemblyLoader : IAssemblyLoader
    {
        #region Fields

        /// <summary>
        /// Already-loaded assemblies. Used as cache.
        /// </summary>
        private readonly Dictionary<string, AssemblyInfo> _assemblyIndex;

        private readonly BorderChecker _borderChecker;

        private readonly IActivityLogger _logger;

        /// <summary>
        /// Delegate to use when checking whether to recurse into a new assembly reference.
        /// </summary>
        /// <param name="newAssembly">New assembly reference</param>
        /// <returns>null if the loader should recurse. Otherwise, a string with a tag clarifying why it shouldn't (eg. "Microsoft", "GAC", etc.)</returns>
        public delegate string BorderChecker( AssemblyDefinition newAssembly );

        /// <summary>
        /// Default border checking delegate. It will mark as border:
        /// - All assemblies with a known Microsoft PublicKeyToken with "Microsoft"
        /// - All assemblies found within the system Windows directory with "Windows/GAC"
        /// </summary>
        public static BorderChecker DefaultBorderChecker = ( newReference ) =>
        {
            // Microsoft tokens:
            // "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
            // "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            // "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            // "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"

            //string[] microsoftTokens = new string[] { "b77a5c561934e089", "31bf3856ad364e35", "b03f5f7f11d50a3a", "7cec85d7bea7798e" };

            //string token = BitConverter.ToString(newReference.Name.PublicKeyToken).Replace("-", string.Empty).ToLowerInvariant();

            //if( microsoftTokens.Contains( token ) )
            //{
            //    return "Microsoft";
            //}
            if( newReference.MainModule.FullyQualifiedName.ToLowerInvariant().StartsWith( Environment.GetFolderPath( Environment.SpecialFolder.Windows ).ToLowerInvariant() ) )
            {
                return "Windows/GAC";
            }
            return null;
        };

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies, using the specified borderChecker to determine whether it should recurse on given references.
        /// <param name="borderChecker">Delegate which determines whether it should recurse on given references. Will recurse if it returns null.</param>
        /// <param name="logger">Parent IActivityLogger to hook on.</param>
        /// </summary>
        public AssemblyLoader( IActivityLogger logger, BorderChecker borderChecker )
        {
            if( logger == null )
                logger = DefaultActivityLogger.Empty;

            _borderChecker = borderChecker;
            _assemblyIndex = new Dictionary<string, AssemblyInfo>();

            _logger = logger;
        }

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies, using the specified borderChecker to determine whether it should recurse on given references.
        /// <param name="borderChecker">Delegate which determines whether it should recurse on given references. Will recurse if it returns null.</param>
        /// </summary>
        public AssemblyLoader( BorderChecker borderChecker )
            : this( null, borderChecker ) { }

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies, without recursing on system and/or Microsoft DLLs.
        /// </summary>
        public AssemblyLoader()
            : this( null, DefaultBorderChecker ) { }

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies, without recursing on system and/or Microsoft DLLs.
        /// <param name="logger">Parent IActivityLogger to hook on.</param>
        /// </summary>
        public AssemblyLoader( IActivityLogger logger )
            : this( logger, DefaultBorderChecker ) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Loaded assemblies.
        /// </summary>
        public IEnumerable<IAssemblyInfo> Assemblies
        {
            get { return _assemblyIndex.Values.Distinct(); }
        }

        #endregion Properties

        #region Public methods

        /// <summary>
        /// Finds and loads assemblies from an entire directory.
        /// </summary>
        /// <param name="assemblyDirectory">Directory to search in</param>
        /// <param name="recurse">If true, will recurse into subdirectories</param>
        /// <returns></returns>
        public IEnumerable<IAssemblyInfo> LoadFromDirectory( DirectoryInfo assemblyDirectory, bool recurse )
        {
            _logger.Info( "Loading directory: {0}", assemblyDirectory.FullName );
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
        /// <param name="assemblyFilePath">Assembly file path to load</param>
        /// <returns>Assembly information</returns>
        public IAssemblyInfo LoadFromFile( string assemblyFilePath )
        {
            return LoadFromFile( new FileInfo( assemblyFilePath ) );
        }

        /// <summary>
        /// Describe an assembly, and its dependencies recursively.
        /// </summary>
        /// <param name="fileStream">Assembly file to load</param>
        /// <returns>Assembly information</returns>
        public IAssemblyInfo LoadFromFile( Stream fileStream )
        {
            AssemblyInfo outputInfo;
            try
            {
                // Load from stream using Mono.Cecil
                ModuleDefinition moduleInfo;

                moduleInfo = ModuleDefinition.ReadModule( fileStream );

                if( !_assemblyIndex.TryGetValue( moduleInfo.Assembly.Name.FullName, out outputInfo ) )
                {
                    _logger.Trace( "Found new assembly: {0}", moduleInfo.Assembly.Name.FullName );
                    outputInfo = CreateAssemblyInfoFromAssemblyNameReference( moduleInfo.Assembly.Name );
                }
                else
                {
                    _logger.Trace( "Found known assembly: {0}", moduleInfo.Assembly.Name.FullName );
                }
                LoadAssembly( moduleInfo, outputInfo );
            }
            catch( Exception ex )
            {
                _logger.Error( ex, "Failed to read module" );
                outputInfo = new AssemblyInfo( ex );
            }
            return outputInfo;
        }

        /// <summary>
        /// Describe an assembly, and its dependencies recursively.
        /// </summary>
        /// <param name="assemblyFile">Assembly file to load</param>
        /// <returns>Assembly information</returns>
        public IAssemblyInfo LoadFromFile( FileInfo assemblyFile )
        {
            _logger.Info( "Loading file: {0}", assemblyFile.FullName );
            AssemblyInfo outputInfo;
            if( _assemblyIndex.TryGetValue( assemblyFile.FullName, out outputInfo ) )
            {
                _logger.Trace( "File was already cached" );
                return outputInfo;
            }

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
                    _logger.Trace( "Found new assembly: {0}", moduleInfo.Assembly.Name.FullName );
                    outputInfo = CreateAssemblyInfoFromAssemblyNameReference( moduleInfo.Assembly.Name );
                    Debug.Assert( assemblyFile.FullName == moduleInfo.FullyQualifiedName );
                }
                else
                {
                    _logger.Trace( "Found known assembly: {0}", moduleInfo.Assembly.Name.FullName );
                }
                outputInfo.Paths.Add( assemblyFile.FullName );
                _assemblyIndex.Add( assemblyFile.FullName, outputInfo );
                LoadAssembly( moduleInfo, outputInfo );
            }
            catch( Exception ex )
            {
                _logger.Error( ex, "Failed to read module" );
                if( outputInfo == null )
                {
                    outputInfo = new AssemblyInfo() { FullName = assemblyFile.FullName };
                    _assemblyIndex.Add( assemblyFile.FullName, outputInfo );
                }
                outputInfo.Error = ex;
            }
            return outputInfo;
        }

        /// <summary>
        /// Resets this AssemblyLoader and clears its cache
        /// </summary>
        public void Reset()
        {
            _assemblyIndex.Clear();
        }

        #endregion Public methods

        #region Private methods

        private void LoadAssembly( ModuleDefinition moduleInfo, AssemblyInfo outputInfo )
        {
            _logger.Trace( "Loading assembly: {0}", moduleInfo.Assembly.FullName );
            try
            {
                // Most custom attributes are optional.
                outputInfo.FileVersion = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyFileVersionAttribute" );
                outputInfo.InformationalVersion = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyInformationalVersionAttribute" );
                outputInfo.Description = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyDescriptionAttribute" );
                outputInfo.Company = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyCompanyAttribute" );
                outputInfo.Product = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyProductAttribute" );
                outputInfo.Copyright = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyCopyrightAttribute" );
                outputInfo.Trademark = GetCustomAttributeString( moduleInfo.Assembly, @"System.Reflection.AssemblyTrademarkAttribute" );
                outputInfo.BorderName = _borderChecker != null ? _borderChecker( moduleInfo.Assembly ) : null;
                if( outputInfo.BorderName == null )
                {
                    ResolveReferences( moduleInfo, outputInfo );
                }
                else
                {
                    _logger.Trace( "Border: {0} - Skipping reference resolution.", outputInfo.BorderName );
                }
            }
            catch( Exception ex )
            {
                _logger.Error( ex, "Failed to resolve references" );
                outputInfo.Error = ex;
            }
        }

        private void ResolveReferences( ModuleDefinition moduleInfo, AssemblyInfo outputInfo )
        {
            AssemblyInfo referenceAssemblyInfo;
            AssemblyDefinition resolvedAssembly;

            using( _logger.OpenGroup( LogLevel.Trace, "References of: {0}", outputInfo.FullName ) )
            {
                // Recursively load references.
                foreach( var referenceAssemblyName in moduleInfo.AssemblyReferences )
                {
                    _logger.Trace( "Resolving reference: {0}", referenceAssemblyName.FullName );
                    referenceAssemblyInfo = null;
                    resolvedAssembly = null;

                    // Check for REFERENCE NAME (the name referenced by the parent asssembly)
                    if( !_assemblyIndex.TryGetValue( referenceAssemblyName.FullName, out referenceAssemblyInfo ) )
                    {
                        // Resolve assembly.
                        try
                        {
                            resolvedAssembly = moduleInfo.AssemblyResolver.Resolve( referenceAssemblyName.FullName );
                        }
                        catch( Exception ex )
                        {
                            _logger.Error( ex, "Failed to resolve assembly" );
                            // Can't resolve assembly, but we can still have data from its name reference.
                            referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( referenceAssemblyName );
                            referenceAssemblyInfo.Error = ex;
                        }
                        if( referenceAssemblyInfo == null )
                        {
                            // Check for FILE NAME (the file name of the resolved assembly)
                            if( !_assemblyIndex.TryGetValue( resolvedAssembly.MainModule.FullyQualifiedName, out referenceAssemblyInfo ) )
                            {
                                _logger.Trace( "Resolved in: {0}", resolvedAssembly.MainModule.FullyQualifiedName );

                                // Check for RESOLVED ASSEMBLY NAME (the actual name of the assembly,
                                // which may or may not be the same as the reference because of rebinding
                                if( !_assemblyIndex.TryGetValue( resolvedAssembly.Name.FullName, out referenceAssemblyInfo ) )
                                {
                                    referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( resolvedAssembly.Name );
                                }
                                //referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( resolvedAssembly.Name );

                                _logger.Trace( "Adding to cache: {0} as {1}", resolvedAssembly.MainModule.FullyQualifiedName, referenceAssemblyInfo.FullName );
                                _assemblyIndex.Add( resolvedAssembly.MainModule.FullyQualifiedName, referenceAssemblyInfo );

                                referenceAssemblyInfo.Paths.Add( resolvedAssembly.MainModule.FullyQualifiedName );

                                LoadAssembly( resolvedAssembly.MainModule, referenceAssemblyInfo );
                            }
                            else
                            {
                                _logger.Warn( "Uncached assembly name: {0} tried to load cached file: {1}", referenceAssemblyName.FullName, resolvedAssembly.MainModule.FullyQualifiedName );
                                _assemblyIndex.Add( referenceAssemblyName.FullName, referenceAssemblyInfo );
                            }
                        }
                    }
                    else
                    {
                        _logger.Trace( "{0} was already cached.", referenceAssemblyName.FullName );
                    }
                    Debug.Assert( referenceAssemblyInfo != null );
                    //if( outputInfo.InternalDependencies.TryGetValue( referenceAssemblyName.FullName, out referenceAssemblyInfo ) )
                    //{
                    //    _logger.Warn( "Reference {0} was already added to {1}", referenceAssemblyName.FullName, outputInfo.SimpleName );
                    //}
                    //else
                    //{
                    outputInfo.InternalDependencies.Add( referenceAssemblyName.FullName, referenceAssemblyInfo );
                    //}
                }
            }
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
        private AssemblyInfo CreateAssemblyInfoFromAssemblyNameReference( AssemblyNameReference assemblyNameRef )
        {
            AssemblyInfo outputInfo = new AssemblyInfo()
            {
                FullName = assemblyNameRef.FullName,
                SimpleName = assemblyNameRef.Name,
                Version = assemblyNameRef.Version,
                Culture = assemblyNameRef.Culture,
                PublicKeyToken = assemblyNameRef.PublicKeyToken
            };
            _logger.Trace( "Adding to cache: {0}", assemblyNameRef.FullName );
            _assemblyIndex.Add( assemblyNameRef.FullName, outputInfo );
            return outputInfo;
        }

        #endregion Private methods

        #region Private static methods

        /// <summary>
        /// Gets the value of a custom attribute type (using Mono.Cecil). Static utility.
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

        #endregion Private static methods

        #region Public static methods

        /// <summary>
        /// Generate an AssemblyInfo object from a simple qualified name.
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        public static IAssemblyInfo ParseAssemblyInfoFromString( string assemblyQualifiedName )
        {
            // Example: nunit.framework, Version=2.6.2.12296, Culture=neutral, PublicKeyToken=96d09a1eb7f44a77, processorArchitecture=MSIL
            string pattern = @"^(.+), Version=([0-9.]+), Culture=([a-zA-Z0-9-_.]+), PublicKeyToken=(null|[a-fA-F0-9]{16})";

            Match m = Regex.Match( assemblyQualifiedName, pattern );

            if( m.Success )
            {
                string name = m.Groups[1].Value;
                Version version = Version.Parse( m.Groups[2].Value );
                string culture = m.Groups[3].Value;
                byte[] publicKeyToken;

                if( m.Groups[4].Value != "null" )
                    publicKeyToken = StringUtils.HexStringToByteArray( m.Groups[4].Value );
                else
                    publicKeyToken = new byte[0];

                string fullName = GetFullNameOf( name, version, culture, publicKeyToken );

                if( culture.ToLowerInvariant() == "neutral" )
                    culture = String.Empty;

                IAssemblyInfo info = new AssemblyInfo()
                {
                    SimpleName = name,
                    Version = version,
                    Culture = culture,
                    PublicKeyToken = publicKeyToken,
                    FullName = fullName
                };

                return info;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create an Assembly full name from its data.
        /// </summary>
        /// <param name="name">Assembly simple name</param>
        /// <param name="version">Assembly version</param>
        /// <param name="culture">Assembly culture</param>
        /// <param name="publicKeyToken">Assembly public key</param>
        /// <returns>Assembly full name</returns>
        public static string GetFullNameOf( string name, Version version, string culture, byte[] publicKeyToken )
        {
            string versionString = version.ToString();

            if( culture.ToLowerInvariant() == "" )
                culture = "neutral";

            string publicKeyTokenString = publicKeyToken.Length == 0 ? "null" : StringUtils.ByteArrayToHexString( publicKeyToken );

            return String.Format( "{0}, Version={1}, Culture={2}, PublicKeyToken={3}", name, versionString, culture, publicKeyTokenString );
        }

        #endregion Public static methods
    }
}