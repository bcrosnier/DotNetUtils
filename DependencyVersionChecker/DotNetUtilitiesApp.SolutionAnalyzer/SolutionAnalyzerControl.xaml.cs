using System.Windows.Controls;

namespace DotNetUtilitiesApp.SolutionAnalyzer
{
    /// <summary>
    /// Interaction logic for SolutionAnalyzerControl.xaml
    /// </summary>
    public partial class SolutionAnalyzerControl : UserControl
    {
        private SolutionAnalyzerViewModel _viewModel;

        public SolutionAnalyzerControl()
        {
            _viewModel = new SolutionAnalyzerViewModel();

            this.DataContext = _viewModel;
            InitializeComponent();
        }

        public void LoadAndCheckSolution( string slnPath )
        {
            _viewModel.AnalyzeSolutionFile( slnPath );
        }

        public void CleanUp()
        {
            _viewModel.CleanUp();
        }
    }
}