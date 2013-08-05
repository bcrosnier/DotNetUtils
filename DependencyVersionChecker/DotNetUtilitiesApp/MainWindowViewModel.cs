using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using DotNetUtilitiesApp.AssemblyProber;
using DotNetUtilitiesApp.SemanticVersionManager;
using DotNetUtilitiesApp.SolutionAnalyzer;
using DotNetUtilitiesApp.WpfUtils;
using DotNetUtilitiesApp.VersionAnalyzer;

namespace DotNetUtilitiesApp
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Fields

        private string _windowTitle;
        private string _solutionPath;

        private  AssemblyProberUserControl _assemblyProberControl;
        private SemanticVersionManagerControl _semanticVersionManagerControl;
        private SolutionAnalyzerControl _solutionAnalyzerControl;
        private VersionAnalyzerControl _versionAnalyzerControl;

        public ICommand LoadSolutionFileCommand { get; private set; }

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

        #endregion Observed properties

        #region Constructor

        internal MainWindowViewModel()
        {
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

        private void InitControls()
        {
            _assemblyProberControl.SetActiveSolution( _solutionPath );
            _semanticVersionManagerControl.LoadFromSolution( _solutionPath );
            _solutionAnalyzerControl.LoadSolutionFile( _solutionPath );
            _versionAnalyzerControl.LoadFromSolution(_solutionPath);
        }

        #endregion Constructor

        #region Internal methods

        internal void LoadSolutionFile( string slnPath )
        {
            _solutionPath = null;

            if( !string.IsNullOrEmpty( slnPath ) && File.Exists( slnPath ) )
            {
                _solutionPath = slnPath;
                string solutionName = Path.GetFileNameWithoutExtension( slnPath );
                WindowTitle = String.Format( ".NET utilities - {0}", solutionName );

                InitControls();
            }
        }

        #endregion Internal methods

        #region Private methods

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