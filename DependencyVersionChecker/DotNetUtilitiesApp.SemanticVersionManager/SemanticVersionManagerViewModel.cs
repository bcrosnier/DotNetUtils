using System.Collections.ObjectModel;
using System.Linq;
using CK.Package;
using DotNetUtilitiesApp.WpfUtils;
using ProjectProber;
using ProjectProber.Impl;

namespace DotNetUtilitiesApp.SemanticVersionManager
{
    internal class SemanticVersionManagerViewModel : ViewModel
    {
        #region Fields

        private string _activeSolutionPath;

        private string _currentVersion;
        private bool _hasBugFixes;
        private bool _hasNewFeatures;
        private bool _hasBreakingChanges;
        private bool _isNotStable;
        private bool _noChanges;
        private string _versionTag;
        private SemanticVersion _newVersion;

        #endregion Fields

        #region Observable properties

        public string CurrentVersion
        {
            get { return _currentVersion; }
            set
            {
                if( value != _currentVersion )
                {
                    _currentVersion = value;
                    UpdateNewVersion();
                    RaisePropertyChanged();
                }
            }
        }

        public string VersionTag
        {
            get { return _versionTag; }
            set
            {
                if( value != _versionTag )
                {
                    _versionTag = value;
                    UpdateNewVersion();
                    RaisePropertyChanged();
                }
            }
        }

        public SemanticVersion NewVersion
        {
            get { return _newVersion; }
            set
            {
                if( value != _newVersion )
                {
                    _newVersion = value;
                    UpdateNewVersion();
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasBugFixes
        {
            get { return _hasBugFixes; }
            set
            {
                if( value != _hasBugFixes )
                {
                    _hasBugFixes = value;
                    UpdateNewVersion();
                    RaisePropertyChanged( "IsNotReleaseCheckBoxEnabled" );
                    RaisePropertyChanged( "IsNotStable" );
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasNewFeatures
        {
            get { return _hasNewFeatures; }
            set
            {
                if( value != _hasNewFeatures )
                {
                    _hasNewFeatures = value;
                    UpdateNewVersion();
                    RaisePropertyChanged( "IsNotReleaseCheckBoxEnabled" );
                    RaisePropertyChanged( "IsNotStable" );
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasBreakingChanges
        {
            get { return _hasBreakingChanges; }
            set
            {
                if( value != _hasBreakingChanges )
                {
                    _hasBreakingChanges = value;
                    UpdateNewVersion();
                    RaisePropertyChanged( "IsNotReleaseCheckBoxEnabled" );
                    RaisePropertyChanged( "IsNotStable" );
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsNotStable
        {
            get { return _isNotStable; }
            set
            {
                if( value != _isNotStable )
                {
                    _isNotStable = value;
                    UpdateNewVersion();
                    RaisePropertyChanged();
                }
            }
        }

        public bool NoChanges
        {
            get { return _noChanges; }
            set
            {
                if( value != _noChanges )
                {
                    _noChanges = value;
                    UpdateNewVersion();
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<string> Warnings
        {
            get;
            private set;
        }

        #endregion Observable properties

        #region Constructor

        public SemanticVersionManagerViewModel()
        {
            Warnings = new ObservableCollection<string>();
            NoChanges = true;
            CurrentVersion = "0.0.0";
        }

        #endregion Constructor

        #region Public methods

        public void LoadFromSolution( string slnPath )
        {
            CleanUp();

            _activeSolutionPath = slnPath;
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( slnPath );
            ShowResultWarnings( result );

            CurrentVersion = GetResultVersion( result );
        }

        public void CleanUp()
        {
            Warnings.Clear();
            IsNotStable = false;
            HasBreakingChanges = false;
            HasNewFeatures = false;
            HasBugFixes = false;
            NewVersion = new SemanticVersion("0.0.0");
            VersionTag = string.Empty;
            CurrentVersion = "0.0.0";
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Gets a version from a version check result to display as the Current Version in the version update UI.
        /// </summary>
        /// <param name="result">AssemblyVersionInfoCheckResult to use</param>
        /// <returns>Returned version; null if none found</returns>
        private string GetResultVersion( AssemblyVersionInfoCheckResult result )
        {
            var versions = result.Versions.Where( x => x != null );
            var informationVersions = result.InformationVersions.Where( x => !string.IsNullOrEmpty( x ) );
            if( versions.Count() == 1 )
            {
                if( !string.IsNullOrEmpty( informationVersions.First() ) )
                {
                    return informationVersions.First();
                }
                else if( versions.First() != null )
                {
                    return versions.First().ToString();
                }
            }
            return "0.0.0";
        }

        private void ShowResultWarnings( AssemblyVersionInfoCheckResult result )
        {
            if( result.Versions.Count > 1 )
            {
                Warnings.Add( "Couldn't find any version to use." );
            }
        }

        private void UpdateNewVersion()
        {
            SemanticVersion version;
            if( SemanticVersion.TryParse( CurrentVersion, out version, true ) )
            {
                SemanticVersion newVersion;
                if( IsNotStable )
                {
                    string message = "New prerelease must be superior to current one";
                    Warnings.Remove(message);
                    newVersion = SemanticVersionGenerator.GenerateSemanticVersion(version, HasBreakingChanges, HasNewFeatures, HasBugFixes, VersionTag);
                    if (newVersion.ToString() == "0.0.0")
                    {
                        Warnings.Add(message);
                    }
                }
                else
                {
                    newVersion = SemanticVersionGenerator.GenerateSemanticVersion( version, HasBreakingChanges, HasNewFeatures, HasBugFixes, null );
                }

                NewVersion = newVersion;
            }
        }

        #endregion Private methods
    }
}