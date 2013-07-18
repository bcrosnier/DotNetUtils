using System;
using System.Collections.Generic;

namespace DependencyVersionChecker
{
    public class AssemblyCheckCompleteEventArgs : EventArgs
    {
        public IEnumerable<AssemblyLoadingCompleteEventArgs> AssemblyCompleteEventArgs
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

        public AssemblyCheckCompleteEventArgs( IEnumerable<AssemblyLoadingCompleteEventArgs> assemblyEventArgs, IEnumerable<DependencyAssembly> dependencies, IEnumerable<DependencyAssembly> versionConflicts )
        {
            AssemblyCompleteEventArgs = assemblyEventArgs;
            Dependencies = dependencies;
            VersionConflicts = versionConflicts;
        }
    }
}