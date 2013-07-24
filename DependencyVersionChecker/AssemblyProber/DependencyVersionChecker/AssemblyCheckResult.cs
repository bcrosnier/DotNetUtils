using System.Collections.Generic;

namespace AssemblyProber
{
    /// <summary>
    /// Results of the AssemblyChecker.
    /// </summary>
    public class AssemblyCheckResult
    {
        /// <summary>
        /// Assemblies that were read.
        /// </summary>
        public IEnumerable<IAssemblyInfo> Assemblies
        {
            get;
            private set;
        }

        /// <summary>
        /// Dependencies found between assemblies.
        /// </summary>
        public IEnumerable<DependencyAssembly> Dependencies
        {
            get;
            private set;
        }

        /// <summary>
        /// Found version discrepancies.
        /// </summary>
        public IEnumerable<DependencyAssembly> VersionConflicts
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new instance of AssemblyCheckResult.
        /// </summary>
        /// <param name="assemblies">Assemblies that were read</param>
        /// <param name="dependencies">Dependencies found between assemblies</param>
        /// <param name="versionConflicts">Found version discrepancies</param>
        internal AssemblyCheckResult( IEnumerable<IAssemblyInfo> assemblies, IEnumerable<DependencyAssembly> dependencies, IEnumerable<DependencyAssembly> versionConflicts )
        {
            Assemblies = assemblies;
            Dependencies = dependencies;
            VersionConflicts = versionConflicts;
        }
    }
}