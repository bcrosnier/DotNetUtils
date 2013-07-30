using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    /// <summary>
    /// Project reference to an assembly, through its full or simple name.
    /// </summary>
    [DebuggerDisplay( "{AssemblyName}" )]
    internal class ProjectReference : IProjectReference
    {
        /// <summary>
        /// Required target framework of the reference. Can be null.
        /// </summary>
        public string RequiredTargetFramework { get; internal set; }

        /// <summary>
        /// Requested assembly name. Either a simple name, or an assembly full name.
        /// </summary>
        public string AssemblyName { get; internal set; }

        /// <summary>
        /// Assembly hint path, where the builder will try to look. Can be null.
        /// </summary>
        public string HintPath { get; internal set; }

        /// <summary>
        /// Private flag of the reference.
        /// </summary>
        public bool IsPrivate { get; internal set; }

        /// <summary>
        /// EmbedInteropTypes flag of the reference.
        /// </summary>
        public bool EmbedInteropTypes { get; internal set; }

        /// <summary>
        /// SpecificVersion flag of the reference.
        /// </summary>
        public bool SpecificVersion { get; internal set; }

        internal ProjectReference() { }
    }
}
