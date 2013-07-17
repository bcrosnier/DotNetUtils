using System;
using System.Collections.Generic;
using System.IO;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Utility for describing an assembly, and its assembly dependencies.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Fires on completion of a LoadFromFileAsync().
        /// </summary>
        event EventHandler<AssemblyLoadingCompleteEventArgs> AsyncAssemblyLoaded;

        /// <summary>
        /// Assemblies that failed to resolve.
        /// </summary>
        IDictionary<IAssemblyInfo, Exception> UnresolvedAssemblies { get; }

        /// <summary>
        /// Load an assembly from a single file.
        /// </summary>
        IAssemblyInfo LoadFromFile( FileInfo assemblyFile );

        /// <summary>
        /// Load an assembly from a single file. Fires AsyncAssemblyLoaded on completion (whether it ends in error or success).
        /// </summary>
        void LoadFromFileAsync( FileInfo assemblyFile );

        /// <summary>
        /// Clear assembly cache.
        /// </summary>
        void Reset();
    }
}