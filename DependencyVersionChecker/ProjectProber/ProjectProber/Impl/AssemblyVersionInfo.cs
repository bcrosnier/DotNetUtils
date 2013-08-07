using System;
using CK.Core;
using CK.Package;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    public class AssemblyVersionInfo
    {
        public Version AssemblyVersion { get { return _assemblyVersion; } }

        public Version AssemblyFileVersion { get { return _assemblyFileVersion; } }

        public SemanticVersion AssemblyInformationSemanticVersion { get { return _assemblyInformationSemanticVersion; } }

        public Version AssemblyInformationVersion { get { return _assemblyInformationVersion; } }

        public bool IsSharedAssemblyInformation { get { return _isSharedAssemblyInformation; } }

        public string AssemblyInfoFilePath { get { return _assemblyInfoFilePath; } }

        public ISolutionProjectItem SolutionProjectItem { get { return _solutionProjectItem; } }

        private Version _assemblyVersion;
        private Version _assemblyFileVersion;
        private Version _assemblyInformationVersion;
        private SemanticVersion _assemblyInformationSemanticVersion;
        private bool _isSharedAssemblyInformation;
        private string _assemblyInfoFilePath;
        private ISolutionProjectItem _solutionProjectItem;

        public AssemblyVersionInfo( string assemblyInfoFilePath, ISolutionProjectItem solutionProjectItem, Version assemblyVersion, Version assemblyFileVersion, SemanticVersion assemblyInformationSemanticVersion )
        {
            _assemblyInfoFilePath = assemblyInfoFilePath;
            _solutionProjectItem = solutionProjectItem;
            _assemblyVersion = assemblyVersion;
            _assemblyFileVersion = assemblyFileVersion;
            _assemblyInformationSemanticVersion = assemblyInformationSemanticVersion;

            _isSharedAssemblyInformation = solutionProjectItem == null;
        }

        public AssemblyVersionInfo(string assemblyInfoFilePath, ISolutionProjectItem solutionProjectItem, Version assemblyVersion, Version assemblyFileVersion, Version assemblyInformationVersion)
        {
            _assemblyInfoFilePath = assemblyInfoFilePath;
            _solutionProjectItem = solutionProjectItem;
            _assemblyVersion = assemblyVersion;
            _assemblyFileVersion = assemblyFileVersion;
            _assemblyInformationVersion = assemblyInformationVersion;

            _isSharedAssemblyInformation = solutionProjectItem == null;
        }

        public override bool Equals( Object obj )
        {
            AssemblyVersionInfo other = obj as AssemblyVersionInfo;
            return other != null && _assemblyVersion == other._assemblyVersion
                && _assemblyFileVersion == other._assemblyFileVersion
                && _assemblyInformationVersion == other._assemblyInformationVersion;
        }

        public override int GetHashCode()
        {
            return Util.Hash.Combine( Util.Hash.Combine( Util.Hash.Combine( Util.Hash.StartValue, _assemblyVersion ), _assemblyFileVersion ), _assemblyInformationVersion ).GetHashCode();
        }
    }
}