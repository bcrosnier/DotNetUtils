using ProjectProber.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Package;

namespace ProjectProber
{
    /// <summary>
    /// Generate a version analysis result.
    /// </summary>
    public class AssemblyVersionInfoCheckResult
    {
        /// <summary>
        /// It's a path's *.sln directory.
        /// </summary>
        public string SolutionDirectoryPath { get { return _solutionDirectoryPath; } }

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
        public IReadOnlyList<AssemblyVersionInfo> AssemblyVersions { get { return _assemblyVersions; } }

        /// <summary>
        /// True if one or more SharedAssemblyInfo in solution has been found.
        /// </summary>
        /// <remarks>
        /// The path of SharedAssemblyInfo are in SharedAssemblyInfoVersions.
        /// </remarks>
        public bool HasNotSharedAssemblyInfo
        { 
            get { return _hasNotSharedAssemblyInfo; }
            private set
            {
                if( _hasNotSharedAssemblyInfo )
                {
                    _hasNotSharedAssemblyInfo = value;
                }
            }
        }

        /// <summary>
        /// True if multiple SharedAssemblyInfo in solution has been found.
        /// </summary>
        /// <remarks>
        /// The path of SharedAssemblyInfos are in SharedAssemblyInfoVersions.
        /// </remarks>
        public bool HasMultipleSharedAssemblyInfo
        { 
            get { return _hasMultipleSharedAssemblyInfo; }
            private set
            {
                if( !_hasMultipleSharedAssemblyInfo )
                {
                    _hasMultipleSharedAssemblyInfo = value;
                }
            }
        }

        /// <summary>
        /// True if multiple AssemblyVersion has been found.
        /// </summary>
        public bool HasMultipleAssemblyVersion
        { 
            get { return _hasMultipleAssemblyVersion; }
            private set
            {
                if( !_hasMultipleAssemblyVersion )
                {
                    _hasMultipleAssemblyVersion = value;
                }
            }
        }

        /// <summary>
        /// True if multiple RelativePath has been found in *.csproj.
        /// </summary>
        public bool HasMultipleRelativeLinkInCSProj
        { 
            get { return _hasMultipleRelativeLinkInCSProj; }
            private set
            {
                if( !_hasMultipleRelativeLinkInCSProj )
                {
                    _hasMultipleRelativeLinkInCSProj = value;
                }
            }
        }

        /// <summary>
        /// True if multiple AssemblyFileVersion has been found.
        /// </summary>
        public bool HasMultipleAssemblyFileVersion
        { 
            get { return _hasMultipleAssemblyFileVersion; }
            private set
            {
                if( !_hasMultipleAssemblyFileVersion )
                {
                    _hasMultipleAssemblyFileVersion = value;
                }
            }
        }

        /// <summary>
        /// True if multiple AssemblyInformationVersion has been found.
        /// </summary>
        public bool HasMultipleAssemblyInformationVersion
        { 
            get { return _hasMultipleAssemblyInformationVersion; }
            private set
            {
                if( !_hasMultipleAssemblyInformationVersion )
                {
                    _hasMultipleAssemblyInformationVersion = value;
                }
            }
        }

        /// <summary>
        /// True if the version found isn't semantic version compliante.
        /// </summary>
        public bool HasOneVersionNotSemanticVersionCompliant
        { 
            get { return _hasOneVersionNotSemanticVersionCompliant; }
            private set
            {
                if( !_hasOneVersionNotSemanticVersionCompliant )
                {
                    _hasOneVersionNotSemanticVersionCompliant = value;
                }
            }
        }

        /// <summary>
        /// True if multiple version has been found in a single file.
        /// </summary>
        public bool HasMultipleVersionInOneAssemblyInfo
        { 
            get { return _hasMultipleVersionInOneAssemblyInfo; }
            private set
            {
                if( !_hasMultipleVersionInOneAssemblyInfo )
                {
                    _hasMultipleVersionInOneAssemblyInfo = value;
                }
            }
        }

        /// <summary>
        /// True if multiple AssemblyVersion has been found.
        /// </summary>
        public bool HasRelativeLinkInCSProjNotFound
        { 
            get { return _hasRelativeLinkInCSProjNotFound; }
            private set
            {
                if( !_hasRelativeLinkInCSProjNotFound )
                {
                    _hasRelativeLinkInCSProjNotFound = value;
                }
            }
        }

        /// <summary>
        /// True if versions cannot be found un SharedAssemblyInfo.cs or AssemblyInfo.cs.
        /// </summary>
        public bool HasFileWithoutVersion
        { 
            get { return _hasFileWithoutVersion; }
            private set
            {
                if( !_hasFileWithoutVersion )
                {
                    _hasFileWithoutVersion = value;
                }
            }
        }
        /// <summary>
        /// Is True, if an AssemblyInfo.cs has a version despite the solution having a SharedAssemblyInfo.cs.
        /// </summary>
        public bool HasAssemblyInfoWithVersion 
        { 
            get { return _hasAssemblyInfoWithVersion; }
            private set
            {
                if( !_hasAssemblyInfoWithVersion )
                {
                    _hasAssemblyInfoWithVersion = value;
                }
            }
        }

        private bool _hasNotSharedAssemblyInfo = true;
        private bool _hasMultipleSharedAssemblyInfo = false;
        private bool _hasMultipleAssemblyVersion = false;
        private bool _hasMultipleRelativeLinkInCSProj = false;
        private bool _hasRelativeLinkInCSProjNotFound = false;
        private bool _hasMultipleAssemblyFileVersion = false;
        private bool _hasMultipleAssemblyInformationVersion = false;
        private bool _hasOneVersionNotSemanticVersionCompliant = false;
        private bool _hasMultipleVersionInOneAssemblyInfo = false;
        private bool _hasFileWithoutVersion = false;
        private bool _hasAssemblyInfoWithVersion = false;
        private List<CSProjCompileLinkInfo> _csProjs;
        private List<AssemblyVersionInfo> _assemblyVersions;
        private List<AssemblyVersionInfo> _sharedAssemblyInfoVersions;
        private string _solutionDirectoryPath;

        /// <summary>
        /// It's different versions has been found.
        /// </summary>
        public IReadOnlyList<Version> Versions { get { return _versions.AsReadOnly(); } }

        private List<Version> _versions;

        /// <summary>
        /// It's different versions has been found.
        /// </summary>
        public IReadOnlyList<Version> FileVersions { get { return _fileVersions.AsReadOnly(); } }

        private List<Version> _fileVersions;

        /// <summary>
        /// It's different versions has been found.
        /// </summary>
        public IReadOnlyList<string> InformationVersions { get { return _informationVersions.AsReadOnly(); } }

        private List<string> _informationVersions;


        internal AssemblyVersionInfoCheckResult( string solutionDirectoryPath,
            List<AssemblyVersionInfo> sharedAssemblyInfoVersions,
            List<CSProjCompileLinkInfo> csProjs,
            List<AssemblyVersionInfo> assemblyVersions )
        {
            _solutionDirectoryPath = solutionDirectoryPath;
            _sharedAssemblyInfoVersions = sharedAssemblyInfoVersions;
            _csProjs = csProjs;
            _assemblyVersions = assemblyVersions;
            _versions = new List<Version>();
            _fileVersions = new List<Version>();
            _informationVersions = new List<string>();

            RunAnalyse();
        }

        private void RunAnalyse()
        {
            HasNotSharedAssemblyInfo = CheckForSharedAssemblyInfo();
            if( !HasNotSharedAssemblyInfo )
            {
                HasAssemblyInfoWithVersion = CheckForAssemblyWithVersion();
                if( HasAssemblyInfoWithVersion )
                {
                    CheckAssemblyVersionInfo( _assemblyVersions );
                }

                CheckSharedAssemblyInfo();

                CheckAssemblyVersionInfo( _sharedAssemblyInfoVersions );
            }
            else
            {
                CheckAssemblyVersionInfo( _assemblyVersions );
            }
        }

        //test bizarre car une double négation est utilisé.
        private bool CheckForSharedAssemblyInfo()
        {
            return _sharedAssemblyInfoVersions.Count == 0;
        }

        private bool CheckForMultipleSharedAssemblyInfo()
        {
            return _sharedAssemblyInfoVersions.Count > 1;
        }

        private bool CheckForRelativeLink()
        {
            return _csProjs.Any( x => string.IsNullOrEmpty( x.AssociateLink ) 
                                    && string.IsNullOrEmpty( x.SharedAssemblyInfoRelativePath ) );
        }

        private bool CheckForMultipleRelativeLink()
        {
            if( _csProjs.Select( x => x.AssociateLink ).Distinct().Count() > 1 ) return true;
            if( _csProjs.Select( x => x.SharedAssemblyInfoRelativePath ).Distinct().Count() > 1 ) return true;
            return false;
        }

        private bool CheckForAssemblyWithVersion()
        {
            return _assemblyVersions.Any( x => x.AssemblyVersion != null
                                            || !string.IsNullOrEmpty( x.AssemblyInformationalVersion )
                                            || x.AssemblyFileVersion != null );
        }

        //virer le paramètre ?
        private bool CheckForSemanticVersionCompliant( List<AssemblyVersionInfo> listVersions )
        {
            return listVersions.Any( x => ( x.AssemblyVersion != null
                                        && IsSemanticVersionCompliant( x.AssemblyVersion.ToString() ) ) 
                                        || ( x.AssemblyFileVersion != null 
                                        && IsSemanticVersionCompliant( x.AssemblyFileVersion.ToString() ) )
                                        || ( !string.IsNullOrEmpty( x.AssemblyInformationalVersion ) 
                                        &&IsSemanticVersionCompliant( x.AssemblyInformationalVersion ) ) );
        }

        private bool CheckForFileWithoutVersion( List<AssemblyVersionInfo> listVersions )
        {
            return listVersions.Any( x => x.AssemblyVersion == null 
                                        && x.AssemblyFileVersion == null 
                                        && (!string.IsNullOrEmpty( x.AssemblyInformationalVersion ) ) );
        }

        private bool CheckForMultipleVersionInOneAssemblyInfo( List<AssemblyVersionInfo> listVersions )
        {
            return listVersions.Any( x => x.AssemblyVersion != x.AssemblyFileVersion
                                        || (!string.IsNullOrEmpty( x.AssemblyInformationalVersion ) 
                                        && x.AssemblyVersion.ToString() != x.AssemblyInformationalVersion) );
        }

        private void CheckForMultipleVersion( List<AssemblyVersionInfo> listVersions )
        {
            HasMultipleAssemblyVersion = CheckForMultipleAssemblyVersion( listVersions );
            HasMultipleAssemblyFileVersion = CheckForMultipleAssemblyFileVersion( listVersions );
            HasMultipleAssemblyInformationVersion = CheckForMultipleAssemblyInformationVersion( listVersions );
        }

        private bool CheckForMultipleAssemblyVersion( List<AssemblyVersionInfo> listVersions )
        {
            _versions = listVersions.Where(x => x.AssemblyVersion != null ).Select( x => x.AssemblyVersion ).Distinct().ToList();
            return _versions.Count > 1;
        }

        private bool CheckForMultipleAssemblyFileVersion( List<AssemblyVersionInfo> listVersions )
        {
            _fileVersions = listVersions.Where( x => x.AssemblyFileVersion != null ).Select( x => x.AssemblyFileVersion ).Distinct().ToList();
            return _fileVersions.Count > 1;
        }

        private bool CheckForMultipleAssemblyInformationVersion( List<AssemblyVersionInfo> listVersions )
        {
            _informationVersions = listVersions.Where( x => x.AssemblyInformationalVersion != null ).Select( x => x.AssemblyInformationalVersion ).Distinct().ToList();
            return _informationVersions.Count > 1;
        }

        private void CheckSharedAssemblyInfo()
        {
            HasRelativeLinkInCSProjNotFound = CheckForRelativeLink();
            HasMultipleRelativeLinkInCSProj = CheckForMultipleRelativeLink();
            HasAssemblyInfoWithVersion = CheckForAssemblyWithVersion();
            HasMultipleSharedAssemblyInfo = CheckForMultipleSharedAssemblyInfo();
        }


        private bool IsSemanticVersionCompliant( string version )
        {
            SemanticVersion tempToTest;
            return !SemanticVersion.TryParseStrict( version, out tempToTest );
        }

        private void CheckAssemblyVersionInfo( List<AssemblyVersionInfo> listVersions )
        {
            HasFileWithoutVersion = CheckForFileWithoutVersion( listVersions );
            HasOneVersionNotSemanticVersionCompliant = CheckForSemanticVersionCompliant( listVersions );
            HasMultipleVersionInOneAssemblyInfo = CheckForMultipleVersionInOneAssemblyInfo( listVersions );
            CheckForMultipleVersion( listVersions );
        }
    }
}
