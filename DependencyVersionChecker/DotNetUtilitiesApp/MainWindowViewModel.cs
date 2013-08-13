using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using DotNetUtilitiesApp.AssemblyProber;
using DotNetUtilitiesApp.SemanticVersionManager;
using DotNetUtilitiesApp.SolutionAnalyzer;
using DotNetUtilitiesApp.WpfUtils;
using DotNetUtilitiesApp.VersionAnalyzer;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CK.Core;
using System.Collections.Specialized;

namespace DotNetUtilitiesApp
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Fields

        private string _windowTitle;
        private string _solutionPath;
        private int _tabIndex;
        private ObservableCollection<RecentSolutionItem> _recentSolutionItems;

        private GithubRepositorySolution _activeGithubSolution;

        private AssemblyProberUserControl _assemblyProberControl;
        private SemanticVersionManagerControl _semanticVersionManagerControl;
        private SolutionAnalyzerControl _solutionAnalyzerControl;
        private VersionAnalyzerControl _versionAnalyzerControl;

        public ICommand LoadSolutionFileCommand { get; private set; }
        public ICommand CheckAllCommand { get; private set; }
        public ICommand CheckCurrentCommand { get; private set; }
        public ICommand ChangeGithubSolutionCommand { get; private set; }

        #endregion Fields

        #region Observed properties

        public string SolutionPath
        {
            get { return _solutionPath; }
            set
            {
                if( value != _solutionPath )
                {
                    _solutionPath = value;
                    UpdateRecentSolutionItems( value );
                    RaisePropertyChanged();
                }
            }
        }

        public int TabIndex
        {
            get { return _tabIndex; }
            set
            {
                if( value != _tabIndex )
                {
                    _tabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                if( value != _windowTitle )
                {
                    _windowTitle = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<RecentSolutionItem> RecentSolutionItems
        {
            get { return _recentSolutionItems; }
        }

        #endregion Observed properties

        #region Constructor

        internal MainWindowViewModel()
        {
            _recentSolutionItems = new ObservableCollection<RecentSolutionItem>();
            PrepareCommands();
        }

        internal void SetControls(
            AssemblyProberUserControl assemblyProberControl,
            SemanticVersionManagerControl semanticVersionManagerControl,
            SolutionAnalyzerControl solutionAnalyzerControl,
            VersionAnalyzerControl versionAnalyzerControl
            )
        {
            _assemblyProberControl = assemblyProberControl;
            _semanticVersionManagerControl = semanticVersionManagerControl;
            _solutionAnalyzerControl = solutionAnalyzerControl;
            _versionAnalyzerControl = versionAnalyzerControl;
        }

        private void PrepareCommands()
        {
            LoadSolutionFileCommand = new RelayCommand( ExecuteLoadSolutionFileCommand );
            CheckAllCommand = new RelayCommand( ExecuteCheckAllCommand, CanExecuteCheck );
            CheckCurrentCommand = new RelayCommand( ExecuteCheckCurrentCommand, CanExecuteCheck );
            ChangeGithubSolutionCommand = new RelayCommand( ExecuteChangeGithubSolution, CanExecuteChangeGithubSolution );
        }

        private bool CanExecuteChangeGithubSolution( object param )
        {
            return _activeGithubSolution != null && _activeGithubSolution.AvailableSolutions.Count() > 1;
        }

        private void ExecuteChangeGithubSolution( object param )
        {
            if ( _activeGithubSolution == null )
                return;

            ChoiceWindowResult<string> newSolutionPathResult =
                ChoiceWindow.ShowSelectWindow<string>( "Select solution", "More than one solution was found in this repository.\nPlease choose a solution file:", _activeGithubSolution.AvailableSolutions );
            
            if ( newSolutionPathResult.Result == System.Windows.MessageBoxResult.OK )
            {
                string solutionPath = newSolutionPathResult.Selected;

                _activeGithubSolution.SolutionPath = solutionPath;
                SetGithubSolutionFile( _activeGithubSolution );
            }
        }

        private bool CanExecuteCheck( object obj )
        {
            return !string.IsNullOrEmpty( _solutionPath );
        }

        private void ExecuteCheckCurrentCommand( object obj )
        {
            LoadSolutionInActiveControl();
        }

        private void ExecuteCheckAllCommand( object obj )
        {
            LoadSolutionInAllControls();
        }

        private void ExecuteLoadSolutionFileCommand( object obj )
        {
            OpenFileDialog d = new OpenFileDialog();

            if( _solutionPath != null )
            {
                d.InitialDirectory = Path.GetDirectoryName( _solutionPath );
                d.FileName = _solutionPath;
            }

            d.CheckFileExists = true;
            d.Filter = "Visual Studio solution (*.sln)|*.sln";

            DialogResult result = d.ShowDialog();

            if( result == System.Windows.Forms.DialogResult.OK )
            {
                FileInfo file = new FileInfo( d.FileName );
                Environment.CurrentDirectory = file.DirectoryName;
                LoadSolutionFile( d.FileName );
            }
        }

        private void LoadSolutionInAllControls()
        {
            _assemblyProberControl.SetActiveSolution( _solutionPath );
            _semanticVersionManagerControl.LoadAndCheckSolution( _solutionPath );
            _solutionAnalyzerControl.LoadAndCheckSolution( _solutionPath );
            _versionAnalyzerControl.LoadAndCheckSolution( _solutionPath );
        }

        private void LoadSolutionInActiveControl()
        {
            if( _tabIndex == 0 )
            {
                _assemblyProberControl.SetActiveSolution( _solutionPath );
            }
            else if( _tabIndex == 1 )
            {
                _solutionAnalyzerControl.LoadAndCheckSolution( _solutionPath );
            }
            else if( _tabIndex == 2 )
            {
                _semanticVersionManagerControl.LoadAndCheckSolution( _solutionPath );
            }
            else if( _tabIndex == 3 )
            {
                _versionAnalyzerControl.LoadAndCheckSolution( _solutionPath );
            }
        }

        #endregion Constructor

        #region Internal methods

        internal void SetSolutionFile( string slnPath )
        {
            _activeGithubSolution = null;

            LoadSolutionFile( slnPath );
        }

        internal void SetGithubSolutionFile( GithubRepositorySolution githubSolution )
        {
            _activeGithubSolution = githubSolution;

            string solutionPath = Path.GetFullPath( Path.Combine( githubSolution.RepositoryDirectoryPath, githubSolution.SolutionPath ) );

            LoadSolutionFile( solutionPath );
            CheckAllCommand.Execute( null );
        }

        private void LoadSolutionFile( string slnPath )
        {
            CleanUp();

            SolutionPath = null;

            if ( !string.IsNullOrEmpty( slnPath ) && File.Exists( slnPath ) )
            {
                SolutionPath = slnPath;
                string solutionName = Path.GetFileNameWithoutExtension( slnPath );
                WindowTitle = String.Format( ".NET utilities - {0}", solutionName );
            }
        }

        #endregion Internal methods

        #region Private methods

        private void CleanUp()
        {
            _assemblyProberControl.CleanUp();
            _semanticVersionManagerControl.CleanUp();
            _solutionAnalyzerControl.CleanUp();
            _versionAnalyzerControl.CleanUp();
        }

        private void UpdateRecentSolutionItems( string value )
        {
            if( !string.IsNullOrEmpty( value ) )
            {
                if( _recentSolutionItems.Count == 5 )
                    _recentSolutionItems.RemoveAt( 0 );
                _recentSolutionItems.Add( new RecentSolutionItem( value ) );
            }
        }

        private void WarnUser( string title, string message )
        {
            System.Windows.MessageBox.Show(
                message, title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Exclamation,
                System.Windows.MessageBoxResult.OK
                );
        }

        #endregion Private methods
    }
}