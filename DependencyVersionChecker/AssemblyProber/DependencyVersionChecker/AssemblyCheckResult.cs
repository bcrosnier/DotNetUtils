using System.Collections.Generic;

namespace AssemblyProber
{
    /// <summary>
    /// Results of the AssemblyChecker.
    /// </summary>
    public class AssemblyCheckResult
    {
        /// <summary>
        /// All assemblies that were listed.
        /// </summary>
        public IEnumerable<IAssemblyInfo> Assemblies
        {
            get;
            private set;
        }

        /// <summary>
        /// All assembly references
        /// </summary>
        public IEnumerable<AssemblyReferenceName> Dependencies
        {
            get;
            private set;
        }

        /// <summary>
        /// Found version discrepancies for a single assembly name.
        /// </summary>
        public IEnumerable<AssemblyReferenceName> VersionConflicts
        {
            get;
            private set;
        }

        /// <summary>
        /// Found version discrepancies for a single assembly reference.
        /// </summary>
        public IEnumerable<AssemblyReference> ReferenceVersionMismatches
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
        /// <param name="referenceVersionMismatches">Found reference version mismatches</param>
        internal AssemblyCheckResult( IEnumerable<IAssemblyInfo> assemblies,
            IEnumerable<AssemblyReferenceName> dependencies,
            IEnumerable<AssemblyReferenceName> versionConflicts,
            IEnumerable<AssemblyReference> referenceVersionMismatches
            )
        {
            Assemblies = assemblies;
            Dependencies = dependencies;
            VersionConflicts = versionConflicts;
            ReferenceVersionMismatches = referenceVersionMismatches;
        }
    }
}