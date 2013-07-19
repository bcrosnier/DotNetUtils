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
    [XmlRoot("AssemblyInfo" )]
    public class AssemblyInfo
        : IAssemblyInfo
    {
        #region Fields

        /// <summary>
        /// This assembly's dependencies (as Assemblies from the assembly's references).
        /// </summary>
        [XmlElement( ElementName = "References" )]
        private List<AssemblyInfo> _internalDependencies;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Assembly display name.
        /// The display name typically consists of the simple name, version number, supported culture, and public key.
        /// See: <see cref="System.Reflection.AssemblyName.FullName"/>
        /// </summary>
        [XmlElement( ElementName = "AssemblyFullName" )]
        public string AssemblyFullName { get; internal set; }

        /// <summary>
        /// Assembly simple name, from its unique identity.
        /// Equivalent of: <see cref="System.Reflection.AssemblyName.Name"/>
        /// </summary>
        [XmlElement( ElementName = "SimpleName" )]
        public string SimpleName { get; internal set; }

        /// <summary>
        /// Assembly version, as compiled in System.Reflection.AssemblyName, from VersionString.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "Version" )]
        public Version Version { get; internal set; }

        /// <summary>
        /// Supported culture, as compiled in System.Reflection.AssemblyName.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "Culture" )]
        public string Culture { get; internal set; }

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
        public string FileVersion { get; internal set; }

        /// <summary>
        /// Assembly version string, as usually declared in Properties/AssemblyInfo.cs,
        /// or through Project Properties.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "InformationalVersion" )]
        public string InformationalVersion { get; internal set; }

        /// <summary>
        /// Description of the assembly.
        /// See: <see cref="System.Reflection.AssemblyDescriptionAttribute"/>
        /// </summary>
        [XmlElement( ElementName = "Description" )]
        public string Description { get; internal set; }

        /// <summary>
        /// Assemblies this one has references to.
        /// </summary>
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
        internal List<AssemblyInfo> InternalDependencies
        {
            get { return _internalDependencies; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo" /> class.
        /// </summary>
        internal AssemblyInfo()
        {
            _internalDependencies = new List<AssemblyInfo>();
        }

        #endregion Constructor
    }
}