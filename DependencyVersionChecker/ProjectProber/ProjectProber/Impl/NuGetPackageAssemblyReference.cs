using System.Diagnostics;
using System.IO;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    [DebuggerDisplay( "{PackageIdVersion} ({TargetFramework}) => {AssemblyFileShortName}" )]
    internal class NuGetPackageAssemblyReference : INuGetPackageAssemblyReference
    {
        #region IPackageLibraryReference Members

        public string PackageIdVersion { get; private set; }

        public string TargetFramework { get; private set; }

        public string AssemblyFileName { get; private set; }

        public string AssemblyFileShortName { get { return Path.GetFileName( AssemblyFileName ); } }

        public string FullPath { get; private set; }

        #endregion IPackageLibraryReference Members

        internal NuGetPackageAssemblyReference( string packageIdVersion, string targetFramework, string assemblyFilename, string fullPath )
        {
            PackageIdVersion = packageIdVersion;
            TargetFramework = targetFramework;
            AssemblyFileName = assemblyFilename;
            FullPath = fullPath;
        }
    }
}