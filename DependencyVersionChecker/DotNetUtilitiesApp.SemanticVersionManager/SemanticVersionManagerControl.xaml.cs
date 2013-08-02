using System.Windows;
using System.Windows.Forms;

namespace DotNetUtilitiesApp.SemanticVersionManager
{
    /// <summary>
    /// Interaction logic for SemanticVersionManagerControl.xaml
    /// </summary>
    public partial class SemanticVersionManagerControl : System.Windows.Controls.UserControl
    {
        private SemanticVersionManagerViewModel _viewModel;

        public SemanticVersionManagerControl()
        {
            _viewModel = new SemanticVersionManagerViewModel();
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        public void LoadFromSolution( string slnPath )
        {
            _viewModel.LoadFromSolution( slnPath );
        }

        private void OpenSolution_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog d = new OpenFileDialog();
            d.CheckFileExists = true;
            d.Filter = "Visual Studio solutions (*.sln)|*.sln";
            DialogResult result = d.ShowDialog();
            if( result == System.Windows.Forms.DialogResult.OK )
            {
                LoadFromSolution( d.FileName );
            }
        }
    }
}