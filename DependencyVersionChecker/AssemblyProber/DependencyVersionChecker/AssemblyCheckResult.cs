using System.Collections.Generic;

namespace DependencyVersionChecker
{
    public class AssemblyCheckResult
    {
        public IEnumerable<IAssemblyInfo> Assemblies
        {
            get;
            private set;
        }

        public IEnumerable<DependencyAssembly> Dependencies
        {
            get;
            private set;
        }

        public IEnumerable<DependencyAssembly> VersionConflicts
        {
            get;
            private set;
        }

        public AssemblyCheckResult( IEnumerable<IAssemblyInfo> assemblies, IEnumerable<DependencyAssembly> dependencies, IEnumerable<DependencyAssembly> versionConflicts )
        {
            Assemblies = assemblies;
            Dependencies = dependencies;
            VersionConflicts = versionConflicts;
        }
    }
}