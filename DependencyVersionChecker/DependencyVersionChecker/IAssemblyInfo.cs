using System;
using System.Collections.Generic;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Information about a particular .NET assembly.
    /// </summary>
    public interface IAssemblyInfo
    {
        /// <summary>
        /// Assembly's FileVersion
        /// </summary>
        Version FileVersion { get; }

        /// <summary>
        /// Assembly version
        /// </summary>
        Version AssemblyVersion { get; }

        /// <summary>
        /// Assembly short name
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Assembly full name
        /// </summary>
        string AssemblyFullName { get; }

        /// <summary>
        /// Read-only information on depended assemblies
        /// </summary>
        IEnumerable<IAssemblyInfo> Dependencies { get; }
    }
}