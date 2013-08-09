using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;

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

        public void LoadAndCheckSolution( string slnPath )
        {
            _viewModel.LoadFromSolution( slnPath );
        }

        public void CleanUp()
        {
            _viewModel.CleanUp();
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
            e.Handled = true;
        }
    }
}