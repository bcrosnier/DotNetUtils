using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Information about a particular .NET assembly.
    /// </summary>
    [DebuggerDisplay( "AssemblyInfo = {AssemblyFullName}" )]
    public class AssemblyInfo
        : IAssemblyInfo
    {
        /// <summary>
        /// Read/write dependencies (as Assemblies from the assembly's references).
        /// </summary>
        private List<AssemblyInfo> _internalDependencies;

        /// <summary>
        /// Gets read-write list of dependencies (as Assemblies from the assembly's references).
        /// </summary>
        internal List<AssemblyInfo> InternalDependencies
        {
            get { return _internalDependencies; }
        }

        /// <summary>
        /// Gets assembly's FileVersion
        /// </summary>
        public Version FileVersion { get; internal set; }

        /// <summary>
        /// Gets assembly version
        /// </summary>
        public Version AssemblyVersion { get; internal set; }

        /// <summary>
        /// Gets assembly short name
        /// </summary>
        public string AssemblyName { get; internal set; }

        /// <summary>
        /// Gets assembly full name
        /// </summary>
        public string AssemblyFullName { get; internal set; }

        /// <summary>
        /// Gets read-only dependencies
        /// </summary>
        public IEnumerable<IAssemblyInfo> Dependencies
        {
            get
            {
                return _internalDependencies.AsReadOnly();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo" /> class.
        /// </summary>
        internal AssemblyInfo()
        {
            _internalDependencies = new List<AssemblyInfo>();
        }
    }
}