using System.Collections.Generic;
using System.IO;

namespace AssemblyProber
{
    /// <summary>
    /// Utility for describing an assembly, and its assembly dependencies.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Load an assembly from a single file.
        /// </summary>
        IAssemblyInfo LoadFromFile(FileInfo assemblyFile);

        /// <summary>
        /// Load an assembly from a single file.
        /// </summary>
        IAssemblyInfo LoadFromFile(string assemblyFilePath);

        /// <summary>
        /// Load an assembly from a single file.
        /// </summary>
        IAssemblyInfo LoadFromFile(Stream fileStream);

        /// <summary>
        /// Load an assembly from an entire directory.
        /// </summary>
        IEnumerable<IAssemblyInfo> LoadFromDirectory(DirectoryInfo assemblyDirectory, bool recurse);

        /// <summary>
        /// Assemblies loaded previously.
        /// </summary>
        IEnumerable<IAssemblyInfo> Assemblies { get; }

        /// <summary>
        /// Clear assembly cache.
        /// </summary>
        void Reset();
    }
}