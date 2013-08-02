using System;
using System.Windows;

namespace DotNetUtilitiesApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private string _runningSlnPath;

        public MainWindow()
        {
            // _runningSlnPath is filled in ProcessArgs(), or null'd

            _viewModel = new MainWindowViewModel();
            this.DataContext = _viewModel;

            InitializeComponent();
            _viewModel.SetControls( this.AssemblyProberUserControl, this.SemanticVersionManagerControl, this.SolutionAnalyzerControl );

            ProcessArgs();

            _viewModel.LoadSolutionFile( _runningSlnPath );
        }

        private void ProcessArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            if( args.Length >= 2 )
            {
                _runningSlnPath = args[1];
            }

            if( args.Length >= 3 )
            {
                string command = args[2];

                if( command.ToLowerInvariant() == "-assemblyanalysis" )
                {
                    AnalyzeAssemblyFolder();
                }
                else if( command.ToLowerInvariant() == "-packageanalysis" )
                {
                    AnalyzeSolution();
                }
                else if( command.ToLowerInvariant() == "-versionanalysis" )
                {
                    PrepareSemanticVersion();
                }
            }
        }

        public void AnalyzeAssemblyFolder()
        {
            //this.AssemblyProberUserControl.LoadFolder( folderPath );
            this.TabControl.SelectedIndex = 0;
        }

        public void AnalyzeSolution()
        {
            //this.SolutionAnalyzerControl.LoadSolutionFile( slnPath );
            this.TabControl.SelectedIndex = 1;
        }

        public void PrepareSemanticVersion()
        {
            //this.SemanticVersionManagerControl.LoadFromSolution( slnPath );
            this.TabControl.SelectedIndex = 2;
        }
    }
}