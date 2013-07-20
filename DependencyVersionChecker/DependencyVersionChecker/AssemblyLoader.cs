using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CK.Core;
using Mono.Cecil;

namespace DependencyVersionChecker
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

        private readonly DefaultActivityLogger _logger;

        public delegate string BorderChecker( AssemblyDefinition newAssembly );

        public static BorderChecker DefaultBorderChecker = ( newReference ) =>
        {
            /** Microsoft tokens:
             * "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
             * "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
             * "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
             * "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
             * */

            string[] microsoftTokens = new string[] { "b77a5c561934e089", "31bf3856ad364e35", "b03f5f7f11d50a3a", "7cec85d7bea7798e" };

            string token = BitConverter.ToString( newReference.Name.PublicKeyToken ).Replace( "-", string.Empty ).ToLowerInvariant();

            if ( microsoftTokens.Contains( token ) )
            {
                return "Microsoft";
            }
            if ( newReference.MainModule.FullyQualifiedName.ToLowerInvariant().StartsWith( Environment.GetFolderPath( Environment.SpecialFolder.Windows ).ToLowerInvariant() ) )
            {
                return "Windows/GAC";
            }
            return null;
        };

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies, using the specified borderChecker to determine whether it should recurse on given references.
        /// <param name="borderChecker">Delegate which determines whether it should recurse on given references. Will recurse it it returns null.</param>
        /// <param name="parentLogger">Parent IActivityLogger to hook on.</param>
        /// </summary>
        public AssemblyLoader( IActivityLogger parentLogger, BorderChecker borderChecker )
        {
            _borderChecker = borderChecker;
            _assemblyIndex = new Dictionary<string, AssemblyInfo>();

            _logger = new DefaultActivityLogger( true );
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "AssemblyLoader" );
            if ( parentLogger != null )
            {
                _logger.Output.BridgeTo( parentLogger );
            }
            else
            {
                _logger.Tap.Register( new ActivityLoggerConsoleSink() );
            }
        }

        /// <summary>
        /// Utility class for describing an assembly, and its dependencies, using the specified borderChecker to determine whether it should recurse on given references.
        /// <param name="borderChecker">Delegate which determines whether it should recurse on given references. Will recurse it it returns null.</param>
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
        /// <param name="parentLogger">Parent IActivityLogger to hook on.</param>
        /// </summary>
        public AssemblyLoader( IActivityLogger parentLogger )
            : this( parentLogger, DefaultBorderChecker ) { }

        #endregion Constructors

        #region Properties

        public IEnumerable<IAssemblyInfo> Assemblies
        {
            get { return _assemblyIndex.Values.Distinct(); }
        }

        #endregion Properties

        #region Public methods

        public IEnumerable<IAssemblyInfo> LoadFromDirectory( DirectoryInfo assemblyDirectory, bool recurse )
        {
            _logger.Info( "Loading directory: {0}", assemblyDirectory.FullName );
            List<IAssemblyInfo> foundAssemblies = new List<IAssemblyInfo>();

            List<FileInfo> fileList = assemblyDirectory.GetFiles( "*.dll" ).ToList();
            fileList.AddRange( assemblyDirectory.GetFiles( "*.exe" ) );

            IAssemblyInfo assembly;

            foreach ( FileInfo f in fileList )
            {
                assembly = LoadFromFile( f );

                foundAssemblies.Add( assembly );
            }

            if ( recurse )
                foreach ( DirectoryInfo d in assemblyDirectory.GetDirectories() ) LoadFromDirectory( d, recurse );

            return foundAssemblies;
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
            if ( _assemblyIndex.TryGetValue( assemblyFile.FullName, out outputInfo ) )
            {
                _logger.Trace( "File was already cached" );
                return outputInfo;
            }

            try
            {
                // Load from DLL using Mono.Cecil
                ModuleDefinition moduleInfo;
                using ( var s = assemblyFile.OpenRead() )
                {
                    moduleInfo = ModuleDefinition.ReadModule( s );
                }

                if ( !_assemblyIndex.TryGetValue( moduleInfo.Assembly.Name.FullName, out outputInfo ) )
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
            catch ( Exception ex )
            {
                _logger.Error( ex, "Failed to read module" );
                if ( outputInfo == null )
                {
                    outputInfo = new AssemblyInfo() { FullName = assemblyFile.FullName };
                    _assemblyIndex.Add( assemblyFile.FullName, outputInfo );
                }
                outputInfo.Error = ex;
            }
            return outputInfo;
        }

        public void Reset()
        {
            _assemblyIndex.Clear();
        }

        #endregion Public methods

        #region Private methods

        private void LoadAssembly( ModuleDefinition moduleInfo, AssemblyInfo outputInfo )
        {
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
                if ( outputInfo.BorderName == null )
                {
                    ResolveReferences( moduleInfo, outputInfo );
                }
                else
                {
                    _logger.Trace( "Border: {0} - Skipping reference resolution.", outputInfo.BorderName );
                }
            }
            catch ( Exception ex )
            {
                _logger.Error( ex, "Failed to resolve references" );
                outputInfo.Error = ex;
            }
        }

        private void ResolveReferences( ModuleDefinition moduleInfo, AssemblyInfo outputInfo )
        {
            AssemblyInfo referenceAssemblyInfo;
            AssemblyDefinition resolvedAssembly;

            _logger.OpenGroup( LogLevel.Trace, "References of: {0}", outputInfo.FullName );

            // Recursively load references.
            foreach ( var referenceAssemblyName in moduleInfo.AssemblyReferences )
            {
                _logger.Trace( "Resolving reference: {0}", referenceAssemblyName.FullName );
                referenceAssemblyInfo = null;
                resolvedAssembly = null;

                if ( !_assemblyIndex.TryGetValue( referenceAssemblyName.FullName, out referenceAssemblyInfo ) )
                {
                    // Resolve assembly.
                    try
                    {
                        resolvedAssembly = moduleInfo.AssemblyResolver.Resolve( referenceAssemblyName.FullName );
                    }
                    catch ( Exception ex )
                    {
                        _logger.Error( ex, "Failed to resolve assembly" );
                        // Can't resolve assembly, but we can still have data from its name reference.
                        referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( referenceAssemblyName );
                        referenceAssemblyInfo.Error = ex;
                    }
                    if ( referenceAssemblyInfo == null )
                    {
                        if ( !_assemblyIndex.TryGetValue( resolvedAssembly.MainModule.FullyQualifiedName, out referenceAssemblyInfo ) )
                        {
                            referenceAssemblyInfo = CreateAssemblyInfoFromAssemblyNameReference( referenceAssemblyName );
                            _assemblyIndex.Add( resolvedAssembly.MainModule.FullyQualifiedName, referenceAssemblyInfo );
                            referenceAssemblyInfo.Paths.Add( resolvedAssembly.MainModule.FullyQualifiedName );
                            _logger.Trace( "Resolved in: {0}", resolvedAssembly.MainModule.FullyQualifiedName );
                            LoadAssembly( resolvedAssembly.MainModule, referenceAssemblyInfo );
                        }
                        else
                        {
                            _logger.Warn( "Uncached assembly name: {0} tried to load cached file: {1}", referenceAssemblyName.FullName, resolvedAssembly.MainModule.FullyQualifiedName );
                        }
                    }
                }
                else
                {
                    _logger.Trace( "Reference was already cached." );
                }
                Debug.Assert( referenceAssemblyInfo != null );
                outputInfo.InternalDependencies.Add( referenceAssemblyInfo );
            }

            _logger.CloseGroup();
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
            _assemblyIndex.Add( assemblyNameRef.FullName, outputInfo );
            return outputInfo;
        }

        #endregion Private methods

        #region Private static methods

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

            if ( customAttributeConstructorArguments != null )
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
    }
}