using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace DotNetUtilitiesApp.GithubDownloader
{
    /// <summary>
    /// Interaction logic for GithubDownloader.xaml
    /// </summary>
    public partial class GithubDownloader : Window
    {
        GithubDownloaderViewModel _viewModel;

        public GithubDownloader()
        {
            _viewModel = new GithubDownloaderViewModel();
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
            e.Handled = true;
        }
    }
}