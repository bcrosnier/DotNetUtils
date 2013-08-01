using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotNetUtilitiesApp.WpfUtils;
using ProjectProber;

namespace DotNetUtilitiesApp.SemanticVersionManager
{
    internal class SemanticVersionManagerViewModel : ViewModel
    {
        #region Fields

        private string _currentVersion;
        private bool _hasBugFixes;
        private bool _hasNewFeatures;
        private bool _hasBreakingChanges;
        private bool _isNotStable;
        private string _versionTag;
        private string _newVersion;

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

        public string NewVersion
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
            CurrentVersion = "1.0.0";
        }

        #endregion Constructor

        #region Public methods

        public void LoadFromSolution( string slnPath )
        {
            Warnings.Clear();
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( slnPath );
            ShowResultWarnings( result );

            var versions = result.Versions.Where( x => x != null );
            if( versions.Count() > 0 )
            {
                CurrentVersion = versions.First().ToString();
            }
        }

        #endregion Public methods

        #region Private methods

        private void ShowResultWarnings( AssemblyVersionInfoCheckResult result )
        {
            if( !result.HaveSharedAssemblyInfo )
            {
                Warnings.Add( "No SharedAssemblyInfo file was found in solution directory." );
            }

            if( result.MultipleRelativeLinkInCSProj )
            {
                Warnings.Add( "More than one SharedAssemblyInfo link was found in a project file." );
            }

            if( result.MultipleSharedAssemblyInfo )
            {
                Warnings.Add( "More than one SharedAssemblyInfo file was found in the solution." );
            }

            if( result.MultipleSharedAssemblyInfoDifferenteVersion )
            {
                Warnings.Add( "More than one SharedAssemblyInfo file was found in the solution, with different versions." );
            }

            if( result.MultipleVersionInPropretiesAssemblyInfo )
            {
                Warnings.Add( "More than one version was found in a project's Properties/AssemblyInfo.cs." );
            }

            if( result.Versions.Count == 0 )
            {
                Warnings.Add( "Couldn't find any version to use." );
            }
        }

        private void UpdateNewVersion()
        {
        }

        #endregion Private methods
    }
}