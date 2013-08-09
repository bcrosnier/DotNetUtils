using System;
using CK.Core;
using CK.Package;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    /// <summary>
    /// Represent an AssemblyInfo.cs or SharedAssemblyInfo.cs
    /// </summary>
    public class AssemblyVersionInfo
    {
        /// <summary>
        /// Version of [assembly: AssemblyVersion("1.0.0")]. 
        /// </summary>
        public Version AssemblyVersion { get { return _assemblyVersion; } }
        /// <summary>
        /// Version of [assembly: AssemblyFileVersion("1.0.0.0")]. 
        /// </summary>
        public Version AssemblyFileVersion { get { return _assemblyFileVersion; } }
        /// <summary>
        /// Version of [assembly: AssemblyInformationalVersion( "2.8.14" )]. 
        /// </summary>
        /// <remarks>
        /// The representation's AssemblyInformationalVersion for VisualStudio is a string.
        /// This version can be 1.0.0.0 (not semantic compliant) and 1.0.0-develop.
        /// </remarks>
        public string AssemblyInformationalVersion { get { return _assemblyInformationalVersion; } }
        /// <summary>
        /// True, if the file is a SharedAssemblyInfo.cs
        /// </summary>
        public bool IsSharedAssemblyInfo { get { return _isSharedAssemblyInfo; } }
        /// <summary>
        /// Path of AssemblyInfo.cs or SharedAssemblyInfo.cs.
        /// </summary>
        public string AssemblyInfoFilePath { get { return _assemblyInfoFilePath; } }

        private Version _assemblyVersion;
        private Version _assemblyFileVersion;
        private string _assemblyInformationalVersion;
        private bool _isSharedAssemblyInfo;
        private string _assemblyInfoFilePath;

        public AssemblyVersionInfo(string assemblyInfoFilePath, Version assemblyVersion, Version assemblyFileVersion, string assemblyInformationVersion)
        {
            _assemblyInfoFilePath = assemblyInfoFilePath;
            _assemblyVersion = assemblyVersion;
            _assemblyFileVersion = assemblyFileVersion;
            _assemblyInformationalVersion = assemblyInformationVersion;

            _isSharedAssemblyInfo = assemblyInfoFilePath.Contains("SharedAssemblyInfo.cs");
        }

        public override bool Equals( Object obj )
        {
            AssemblyVersionInfo other = obj as AssemblyVersionInfo;
            return other != null && _assemblyVersion == other._assemblyVersion
                && _assemblyFileVersion == other._assemblyFileVersion
                && _assemblyInformationalVersion == other._assemblyInformationalVersion;
        }

        public override int GetHashCode()
        {
            return Util.Hash.Combine( Util.Hash.Combine( Util.Hash.Combine( Util.Hash.StartValue, _assemblyVersion ), _assemblyFileVersion ), _assemblyInformationalVersion ).GetHashCode();
        }
    }
}