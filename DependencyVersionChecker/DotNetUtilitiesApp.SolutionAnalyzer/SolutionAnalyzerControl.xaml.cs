using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public void LoadSolutionFile( string slnPath )
        {
            _viewModel.AnalyzeSolutionFile( slnPath );
        }
    }
}
