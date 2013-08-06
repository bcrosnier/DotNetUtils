using System.Collections.Generic;
using System.Linq;
using CK.Package;
using ProjectProber.Impl;

namespace ProjectProber
{
    public class AssemblyVersionInfoCheckResult
    {
        /// <summary>
        /// It's a path's *.sln directory.
        /// </summary>
        public string SolutionDirectoryPath { get; private set; }

        /// <summary>
        /// Represent different SharedAssemblyInfo.cs files in solution.
        /// </summary>
        public IReadOnlyList<AssemblyVersionInfo> SharedAssemblyInfoVersions { get { return _sharedAssemblyInfoVersions; } }

        /// <summary>
        /// Represent different relatives links *.csproj files in solution.
        /// </summary>
        public IReadOnlyList<CSProjCompileLinkInfo> CsProjs { get { return _csProjs; } }

        /// <summary>
        /// Represent different AssemblyInfo.cs files in solution.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public IReadOnlyList<AssemblyVersionInfo> AssemblyVersions { get { return _assemblyVersions; } }

        /// <summary>
        /// True if one or more SharedAssemblyInfo in solution has been found.
        /// </summary>
        /// <remarks>
        /// The path of SharedAssemblyInfo are in SharedAssemblyInfoVersions.
        /// </remarks>
        public bool HasNotSharedAssemblyInfo { get { return _hasNotSharedAssemblyInfo; } }

        /// <summary>
        /// True if multiple SharedAssemblyInfo in solution has been found.
        /// </summary>
        /// <remarks>
        /// The path of SharedAssemblyInfos are in SharedAssemblyInfoVersions.
        /// </remarks>
        public bool HasMultipleSharedAssemblyInfo { get { return _hasMultipleSharedAssemblyInfo; } }

        /// <summary>
        /// True if multiple AssemblyVersion has been found.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasMultipleAssemblyVersion { get { return _hasMultipleAssemblyVersion; } }

        /// <summary>
        /// True if multiple RelativePath has been found in *.csproj.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasMultipleRelativeLinkInCSProj { get { return _hasMultipleRelativeLinkInCSProj; } }

        /// <summary>
        /// True if multiple AssemblyFileVersion has been found.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasMultipleAssemblyFileVersion { get { return _hasMultipleAssemblyFileVersion; } }

        /// <summary>
        /// True if multiple AssemblyInformationVersion has been found.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasMultipleAssemblyInformationVersion { get { return _hasMultipleAssemblyInformationVersion; } }

        /// <summary>
        /// True if the version found isn't semantic version compliante.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasOneVersionNotSemanticVersionCompliant { get { return _hasOneVersionNotSemanticVersionCompliant; } }

        /// <summary>
        /// True if multiple version has been found in a single file.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasMultipleVersionInOneAssemblyInfoFile { get { return _hasMultipleVersionInOneAssemblyInfoFile; } }

        /// <summary>
        /// True if multiple AssemblyVersion has been found.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasRelativeLinkInCSProjNotFound { get { return _hasRelativeLinkInCSProjNotFound; } }

        /// <summary>
        /// True if versions cannot be found un SharedAssemblyInfo.cs or AssemblyInfo.cs.
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        public bool HasFileWithoutVersion { get { return _hasFileWithoutVersion; } }

        private bool _hasNotSharedAssemblyInfo = true;
        private bool _hasMultipleSharedAssemblyInfo = false;
        private bool _hasMultipleAssemblyVersion = false;
        private bool _hasMultipleRelativeLinkInCSProj = false;
        private bool _hasRelativeLinkInCSProjNotFound = false;
        private bool _hasMultipleAssemblyFileVersion = false;
        private bool _hasMultipleAssemblyInformationVersion = false;
        private bool _hasOneVersionNotSemanticVersionCompliant = false;
        private bool _hasMultipleVersionInOneAssemblyInfoFile = false;
        private bool _hasFileWithoutVersion = false;
        private List<CSProjCompileLinkInfo> _csProjs;
        private List<AssemblyVersionInfo> _assemblyVersions;
        private List<AssemblyVersionInfo> _sharedAssemblyInfoVersions;

        /// <summary>
        /// It's different versions has been found.
        /// </summary>
        public IReadOnlyList<AssemblyVersionInfo> Versions { get { return _versions.AsReadOnly(); } }

        private List<AssemblyVersionInfo> _versions;

        internal AssemblyVersionInfoCheckResult( string solutionDirectoryPath,
            List<AssemblyVersionInfo> sharedAssemblyInfoVersions,
            List<CSProjCompileLinkInfo> csProjs,
            List<AssemblyVersionInfo> assemblyVersions )
        {
            SolutionDirectoryPath = solutionDirectoryPath;
            _sharedAssemblyInfoVersions = sharedAssemblyInfoVersions;
            _csProjs = csProjs;
            _assemblyVersions = assemblyVersions;
            _versions = new List<AssemblyVersionInfo>();

            RunAnalyse();
        }

        private void RunAnalyse()
        {
            if( _sharedAssemblyInfoVersions.Count > 1 )
            {
                _hasNotSharedAssemblyInfo = false;
                _hasMultipleSharedAssemblyInfo = true;

                CheckAssemblyVersionInfo( _sharedAssemblyInfoVersions );
            }
            else if( _sharedAssemblyInfoVersions.Count == 1 )
            {
                CheckCSProjCompileLinkInfo( _csProjs );
            }
            else
            {
                CheckAssemblyVersionInfo( _assemblyVersions );
            }
            if( _versions.Count > 1 )
            {
                for( int i = 1; i < _versions.Count; i++ )
                    CheckAssemblyInfo( _versions.First(), _versions[i] );
            }
        }

        private void CheckAssemblyInfo( AssemblyVersionInfo a1, AssemblyVersionInfo a2 )
        {
            if( a1.AssemblyVersion != null && a2.AssemblyVersion != null && a1.AssemblyVersion != a2.AssemblyVersion ) _hasMultipleAssemblyVersion = true;
            if( a1.AssemblyFileVersion != null && a2.AssemblyFileVersion != null && a1.AssemblyFileVersion != a2.AssemblyFileVersion ) _hasMultipleAssemblyFileVersion = true;
            if( a1.AssemblyInformationVersion != null && a2.AssemblyInformationVersion != null && a1.AssemblyInformationVersion != a2.AssemblyInformationVersion ) _hasMultipleAssemblyInformationVersion = true;
            SemanticVersion tempToTest;
            if( a1.AssemblyFileVersion != null && !SemanticVersion.TryParseStrict( a1.AssemblyFileVersion.ToString(), out tempToTest ) ) _hasOneVersionNotSemanticVersionCompliant = true;
            if( a1.AssemblyVersion != null && !SemanticVersion.TryParseStrict( a1.AssemblyVersion.ToString(), out tempToTest ) ) _hasOneVersionNotSemanticVersionCompliant = true;
            if( a1.AssemblyInformationVersion != null && !SemanticVersion.TryParseStrict( a1.AssemblyInformationVersion.ToString(), out tempToTest ) ) _hasOneVersionNotSemanticVersionCompliant = true;
            if( a2.AssemblyFileVersion != null && !SemanticVersion.TryParseStrict( a2.AssemblyFileVersion.ToString(), out tempToTest ) ) _hasOneVersionNotSemanticVersionCompliant = true;
            if( a2.AssemblyVersion != null && !SemanticVersion.TryParseStrict( a2.AssemblyVersion.ToString(), out tempToTest ) ) _hasOneVersionNotSemanticVersionCompliant = true;
            if( a2.AssemblyInformationVersion != null && !SemanticVersion.TryParseStrict( a2.AssemblyInformationVersion.ToString(), out tempToTest ) ) _hasOneVersionNotSemanticVersionCompliant = true;
        }

        private void CheckAssemblyVersionInfo( List<AssemblyVersionInfo> assemblyVersionInfo )
        {
            foreach( AssemblyVersionInfo version in assemblyVersionInfo )
            {
                if( version.AssemblyVersion == null && version.AssemblyFileVersion == null && version.AssemblyInformationVersion == null )
                {
                    _hasFileWithoutVersion = true;
                    continue;
                }
                if( version.AssemblyVersion != null && _versions.Count == 0 )
                {
                    _versions.Add( version );
                    if( CheckMultipleVersionInOneAssemblyInfoFile( version ) ) _hasMultipleVersionInOneAssemblyInfoFile = true;
                }
                bool temp = false;
                foreach( AssemblyVersionInfo versionCompare in _versions )
                {
                    if( versionCompare.Equals( version ) )
                    {
                        temp = true;
                        break;
                    }
                }
                if( !temp )
                {
                    _versions.Add( version );
                    if( CheckMultipleVersionInOneAssemblyInfoFile( version ) ) _hasMultipleVersionInOneAssemblyInfoFile = true;
                }
            }
        }

        private void CheckCSProjCompileLinkInfo( List<CSProjCompileLinkInfo> csProjs )
        {
            _hasNotSharedAssemblyInfo = false;
            _versions.Add( SharedAssemblyInfoVersions.First() );
            _hasMultipleVersionInOneAssemblyInfoFile = CheckMultipleVersionInOneAssemblyInfoFile( SharedAssemblyInfoVersions.First() );
            IList<CSProjCompileLinkInfo> csProjCompileLinkInfoToCompare = new List<CSProjCompileLinkInfo>();
            foreach( CSProjCompileLinkInfo csProjCompileLinkInfo in csProjs )
            {
                if( csProjCompileLinkInfo == null )
                {
                    _hasRelativeLinkInCSProjNotFound = true;
                    continue;
                }
                if( csProjCompileLinkInfoToCompare.Count == 0 )
                {
                    csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
                }
                bool temp = false;
                foreach( CSProjCompileLinkInfo csProjCompileLinkInfoCompare in csProjCompileLinkInfoToCompare )
                {
                    if( csProjCompileLinkInfoCompare == csProjCompileLinkInfo )
                    {
                        temp = true;
                        break;
                    }
                }
                if( !temp )
                {
                    csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
                    _hasMultipleRelativeLinkInCSProj = true;
                }
            }
        }

        private bool CheckMultipleVersionInOneAssemblyInfoFile( AssemblyVersionInfo assembly )
        {
            return assembly.AssemblyVersion != assembly.AssemblyFileVersion
                    || (assembly.AssemblyInformationVersion != null
                    && assembly.AssemblyVersion != assembly.AssemblyInformationVersion.Version);
        }
    }
}