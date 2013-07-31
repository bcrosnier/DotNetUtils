using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace AssemblyProber
{
    /// <summary>
    /// Information about a particular .NET assembly.
    /// </summary>
    [DebuggerDisplay("{FullName}")]
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
        [XmlElement(ElementName = "References")]
        private readonly Dictionary<string, AssemblyInfo> _internalDependencies;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Assembly display name.
        /// The display name typically consists of the simple name, version number, supported culture, and public key.
        /// See: <see cref="System.Reflection.AssemblyName.FullName"/>
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Assembly simple name, from its unique identity.
        /// Equivalent of: <see cref="System.Reflection.AssemblyName.Name"/>
        /// </summary>
        public string SimpleName { get; set; }

        /// <summary>
        /// Assembly version, as compiled in System.Reflection.AssemblyName, from VersionString.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Supported culture, as compiled in System.Reflection.AssemblyName.
        /// See: <see cref="System.Reflection.AssemblyCultureAttribute"/>
        /// </summary>
        public string Culture { get; set; }

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

        /// <summary>
        /// Public key token.
        /// See: <see cref="System.Reflection.AssemblyName.GetPublicKeyToken"/>
        /// </summary>
        public byte[] PublicKeyToken { get; set; }

        // Properties above can be inferred from assembly reference.
        // Properties below require assembly resolution.

        /// <summary>
        /// File version string, as usually declared in Properties/AssemblyInfo.cs,
        /// or through Project Properties.
        /// See: <see cref="System.Reflection.AssemblyFileVersionAttribute"/>
        /// </summary>
        public string FileVersion { get; set; }

        /// <summary>
        /// Assembly version string, as usually declared in Properties/AssemblyInfo.cs,
        /// or through Project Properties.
        /// See: <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/>
        /// </summary>
        public string InformationalVersion { get; set; }

        /// <summary>
        /// Description of the assembly.
        /// See: <see cref="System.Reflection.AssemblyDescriptionAttribute"/>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Company.
        /// See: <see cref="System.Reflection.AssemblyCompanyAttribute"/>
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Product.
        /// See: <see cref="System.Reflection.AssemblyProductAttribute"/>
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Copyright.
        /// See: <see cref="System.Reflection.AssemblyCopyrightAttribute"/>
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Trademark.
        /// See: <see cref="System.Reflection.AssemblyTrademarkAttribute"/>
        /// </summary>
        public string Trademark { get; set; }

        /// <summary>
        /// Assemblies this one has references to.
        /// </summary>
        [XmlIgnore()]
        public IReadOnlyDictionary<string, IAssemblyInfo> Dependencies
        {
            get
            {
                return _internalDependencies.ToDictionary(x => x.Key, x => (IAssemblyInfo)x.Value);
            }
        }

        /// <summary>
        /// Dependencies (as Assemblies from the assembly's references).
        /// </summary>
        [XmlIgnore()]
        internal Dictionary<string, AssemblyInfo> InternalDependencies
        {
            get { return _internalDependencies; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo" /> class, with an exception set.
        /// </summary>
        internal AssemblyInfo(Exception ex)
            : this()
        {
            Error = ex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo" /> class.
        /// </summary>
        internal AssemblyInfo()
        {
            _internalDependencies = new Dictionary<string, AssemblyInfo>();
            _paths = new List<string>();
        }

        #endregion Constructor
    }
}