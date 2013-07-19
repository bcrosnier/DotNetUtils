using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Information about a particular .NET assembly.
    /// </summary>
    [DebuggerDisplay( "AssemblyInfo = {AssemblyFullName}" )]
    [XmlRoot( "AssemblyInfo" )]
    public class AssemblyInfo : IAssemblyInfo
    {
        #region Fields

        /// <summary>
        /// This assembly's paths.
        /// </summary>
        private readonly List<string> _paths;

        /// <summary>
        /// This assembly's dependencies (as Assemblies from the assembly's references).
        /// </summary>
        [XmlElement( ElementName = "References" )]
        private readonly List<AssemblyInfo> _internalDependencies;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Assembly display name.
        /// The display name typically consists of the simple name, version number, supported culture, and public key.
        /// See: <see cref="System.Reflection.AssemblyName.FullName"/>
        /// </summary>
        [XmlElement( ElementName = "AssemblyFullName" )]
        public string AssemblyFullName { get; set; }

        /// <summary>
        /// Assembly simple name, from its unique identity.
        /// Equivalent of: <see cref="System.Reflection.AssemblyName.Name"/>
        /// </summary>
        [XmlElement( ElementName = "SimpleName" )]
        public string SimpleName { get; set; }

        /// <summary>
        /// Assembly version, as compiled in System.Reflection.AssemblyName, from VersionString.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        [XmlIgnore()] // XML adapter below
        public Version Version { get; set; }

        /// <summary>
        /// Supported culture, as compiled in System.Reflection.AssemblyName.
        /// See: <see cref="System.Reflection.AssemblyCultureAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "Culture" )]
        public string Culture { get; set; }

        /// <summary>
        /// Version string adapter. Used for XML serialization.
        /// </summary>
        [XmlElement( ElementName = "Version" )]
        public string VersionString
        {
            get
            {
                return Version.ToString();
            }
            set
            {
                Version v = null;
                Version.TryParse( value, out v );
                if( v != null )
                    Version = v;
            }
        }

        /// <summary>
        /// Exception, which happened during the resolution or opening of the assembly.
        /// Can be null.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// The paths at which this assembly was found.
        /// </summary>
        public IList<string> Paths
        {
            get
            {
                return _paths;
            }
        }

        /// <summary>
        /// Name of the border, if this assembly acted as one.
        /// </summary>
        public string BorderName { get; set; }

        /**
         * Properties above can be inferred from assembly reference.
         * Properties below require assembly resolution.
         */

        /// <summary>
        /// File version string, as usually declared in Properties/AssemblyInfo.cs,
        /// or through Project Properties.
        /// See: <see cref="System.Reflection.AssemblyFileVersionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "FileVersion" )]
        public string FileVersion { get; set; }

        /// <summary>
        /// Assembly version string, as usually declared in Properties/AssemblyInfo.cs,
        /// or through Project Properties.
        /// See: <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "InformationalVersion" )]
        public string InformationalVersion { get; set; }

        /// <summary>
        /// Description of the assembly.
        /// See: <see cref="System.Reflection.AssemblyDescriptionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "Description" )]
        public string Description { get; set; }

        /// <summary>
        /// Assemblies this one has references to.
        /// </summary>
        [XmlIgnore()]
        public IEnumerable<IAssemblyInfo> Dependencies
        {
            get
            {
                return _internalDependencies.AsReadOnly();
            }
        }

        /// <summary>
        /// Dependencies (as Assemblies from the assembly's references).
        /// </summary>
        [XmlIgnore()]
        internal List<AssemblyInfo> InternalDependencies
        {
            get { return _internalDependencies; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo" /> class, with an exception set.
        /// </summary>
        internal AssemblyInfo(Exception ex) : this()
        {
            Error = ex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo" /> class.
        /// </summary>
        internal AssemblyInfo()
        {
            _internalDependencies = new List<AssemblyInfo>();
            _paths = new List<string>();
        }

        #endregion Constructor
    }
}