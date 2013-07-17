using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using DependencyVersionChecker;

namespace DependencyVersionCheckerApp.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            AssemblyVersionChecker checker = new AssemblyVersionChecker( new AssemblyLoader() ); ;
            _viewModel = new MainWindowViewModel( checker );
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void LoadAssemblyDirectory_Click( object sender, RoutedEventArgs e )
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = Environment.CurrentDirectory;
            var result = d.ShowDialog();
            if( result == System.Windows.Forms.DialogResult.OK )
            {
                DirectoryInfo dir = new DirectoryInfo( d.SelectedPath );
                _viewModel.ChangeAssemblyFolderCommand.Execute( dir );
            }
        }
    }
}