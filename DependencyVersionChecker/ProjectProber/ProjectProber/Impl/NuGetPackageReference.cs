using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;
using NuGet;

namespace ProjectProber.Impl
{
    /// <summary>
    /// NuGet package reference, as seen in projects' package.config
    /// </summary>
    [DebuggerDisplay( "{Id} {Version} ({TargetFramework})" )]
    public class NuGetPackageReference : INuGetPackageReference
    {
        /// <summary>
        /// Package ID
        /// </summary>
        /// <example>CK.Context</example>
        public string Id { get; private set; }

        /// <summary>
        /// Package descriptive version
        /// </summary>
        /// <example>2.9.1-develop</example>
        public string Version { get; private set; }

        /// <summary>
        /// Target framework object
        /// </summary>
        /// <example>net40-Client</example>
        public FrameworkName TargetFramework { get; private set; }

        internal NuGetPackageReference( string id, string version, string targetFramework )
            : this(id, version, VersionUtility.ParseFrameworkName(targetFramework))
        {
        }

        internal NuGetPackageReference( string id, string version, FrameworkName targetFramework )
        {
            Id = id;
            Version = version;
            TargetFramework = targetFramework;
        }
    }
}
