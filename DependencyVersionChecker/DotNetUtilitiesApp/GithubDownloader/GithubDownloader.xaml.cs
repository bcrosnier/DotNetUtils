﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using DotNetUtilitiesApp.WpfUtils;

namespace DotNetUtilitiesApp.GithubDownloader
{
    /// <summary>
    /// Interaction logic for GithubDownloader.xaml
    /// </summary>
    public partial class GithubDownloader : Window
    {
        public event EventHandler<GithubRepositorySolutionEventArgs> SolutionFileReady;

        private GithubDownloaderViewModel _viewModel;

        public GithubDownloader( DirectoryInfo cacheDir )
        {
            _viewModel = new GithubDownloaderViewModel( cacheDir );
            _viewModel.RaisedWarning += _viewModel_RaisedWarning;

            _viewModel.SolutionPathAvailable += _viewModel_SolutionPathAvailable;

            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void _viewModel_SolutionPathAvailable( object sender, GithubRepositorySolutionEventArgs e )
        {
            if ( SolutionFileReady != null )
                SolutionFileReady( this, e );
        }

        private void _viewModel_RaisedWarning( object sender, StringEventArgs e )
        {
            MessageBox.Show( e.Content );
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
            e.Handled = true;
        }
    }
}