using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    class PackageLibraryReference : IPackageLibraryReference
    {
        #region IPackageLibraryReference Members

        public string PackageIdVersion { get; private set; }

        public string TargetFramework { get; private set; }

        public string AssemblyFileName { get; private set; }

        public string FullPath { get; private set; }

        #endregion

        internal PackageLibraryReference( string packageIdVersion, string targetFramework, string assemblyFilename, string fullPath )
        {
            PackageIdVersion = packageIdVersion;
            TargetFramework = targetFramework;
            AssemblyFileName = assemblyFilename;
            FullPath = fullPath;
        }
    }
}
