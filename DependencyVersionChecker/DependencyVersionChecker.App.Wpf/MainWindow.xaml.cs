using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CK.Core;
using DependencyVersionChecker;
using Mono.Cecil;

namespace DependencyVersionCheckerApp.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private string _lastLoadedFolder = Environment.CurrentDirectory;
        private DefaultActivityLogger _logger;

        public MainWindow()
        {
            _logger = new DefaultActivityLogger();
            _logger.Filter = LogLevelFilter.None;

            IAssemblyLoader l = new AssemblyLoader( _logger );

            AssemblyVersionChecker checker = new AssemblyVersionChecker( l );
            _viewModel = new MainWindowViewModel( _logger, checker );
            this.DataContext = _viewModel;
            InitializeComponent();

            ( (INotifyCollectionChanged)LogListBox.Items ).CollectionChanged += LogListBox_CollectionChanged;

            LogListBox.ScrollIntoView( LogListBox.Items.GetItemAt( LogListBox.Items.Count - 1 ) );
        }

        private void LogListBox_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( e.Action == NotifyCollectionChangedAction.Add )
            {
                if ( e.NewItems.Count > 0 )
                {
                    object lastItem = e.NewItems[e.NewItems.Count - 1];
                    LogListBox.ScrollIntoView( lastItem );
                }
            }
        }

        private void LoadAssemblyDirectory_Click( object sender, RoutedEventArgs e )
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = _lastLoadedFolder;
            var result = d.ShowDialog();
            if ( result == System.Windows.Forms.DialogResult.OK )
            {
                DirectoryInfo dir = new DirectoryInfo( d.SelectedPath );
                _lastLoadedFolder = d.SelectedPath;
                Environment.CurrentDirectory = d.SelectedPath;
                _viewModel.ChangeAssemblyFolderCommand.Execute( dir );
            }
        }

        private void LoadAssemblyFile_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog d = new OpenFileDialog();
            d.CheckFileExists = true;
            d.Filter = "Binary assemblies (*.dll;*.exe)|*.dll;*.exe";
            DialogResult result = d.ShowDialog();
            if ( result == System.Windows.Forms.DialogResult.OK )
            {
                FileInfo file = new FileInfo( d.FileName );
                Environment.CurrentDirectory = file.DirectoryName;
                _viewModel.ChangeAssemblyFile( file );
            }
        }

        private void LoadAssemblyXml_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog d = new OpenFileDialog();
            d.CheckFileExists = true;
            d.Filter = "XML file (*.xml)|*.xml";
            DialogResult result = d.ShowDialog();
            if ( result == System.Windows.Forms.DialogResult.OK )
            {
                FileInfo file = new FileInfo( d.FileName );
                _viewModel.LoadXmlFile( file );
            }
        }

        private void SaveAssemblyXml_Click( object sender, RoutedEventArgs e )
        {
            SaveFileDialog d = new SaveFileDialog();
            d.OverwritePrompt = true;
            d.Filter = "XML file (*.xml)|*.xml";
            DialogResult result = d.ShowDialog();
            if ( result == System.Windows.Forms.DialogResult.OK )
            {
                FileInfo file = new FileInfo( d.FileName );
                _viewModel.SaveXmlFile( file );
            }
        }

        private void GraphTypeMenuItem_Click( object sender, RoutedEventArgs e )
        {
            System.Windows.Controls.MenuItem clickedItem = (System.Windows.Controls.MenuItem)e.OriginalSource;
            _viewModel.LayoutAlgorithmType = clickedItem.Header.ToString();
        }
    }
}