using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AssemblyProber;
using AssemblyProberApp.Wpf.Graphing;
using CK.Core;

namespace AssemblyProberApp.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private string _lastLoadedFolder = Environment.CurrentDirectory;

        private FileStream _logFileStream;
        private TextWriter _logTextWriter;
        private IActivityLogger _logger;

        private Graphing.AssemblyVertex highlightedVertex = null;

        public MainWindow()
            : this( new DefaultActivityLogger(), null )
        {
        }

        public MainWindow( IActivityLogger logger, string directoryPath )
        {
            string[] args = Environment.GetCommandLineArgs();

            if( args.Length >= 2 )
            {
                directoryPath = args[1];
            }

            if( logger is IDefaultActivityLogger )
            {
                _logFileStream = new FileStream( @"AssemblyProberApp.log", FileMode.Create, FileAccess.Write, FileShare.Read, 8192, false );
                _logTextWriter = new StreamWriter( _logFileStream );
                ActivityLoggerTextWriterSink sink = new ActivityLoggerTextWriterSink( _logTextWriter );

                ((IDefaultActivityLogger)logger).Tap.Register( sink );
            }

            IAssemblyLoader l = new AssemblyLoader( logger );

            AssemblyVersionChecker checker = new AssemblyVersionChecker( l );

            _viewModel = new MainWindowViewModel( logger, checker, directoryPath );
            _viewModel.LogFlushRequested += ( s, e ) => { FlushLog(); };
            _viewModel.VertexHighlightRequested += ( s, e ) => { HighlightVertex( e.Vertex ); };
            FlushLog();

            this.DataContext = _viewModel;
            InitializeComponent();

            //((INotifyCollectionChanged)LogListBox.Items).CollectionChanged += LogListBox_CollectionChanged;

            //LogListBox.ScrollIntoView( LogListBox.Items.GetItemAt( LogListBox.Items.Count - 1 ) );
        }

        public void HighlightVertex( Graphing.AssemblyVertex vertex )
        {
            if( highlightedVertex != null )
            {
                highlightedVertex.IsHighlighted = false;
                this.graphLayout.RemoveHighlightFromVertex( highlightedVertex );
            }
            highlightedVertex = vertex;
            if( vertex != null )
            {
                vertex.IsHighlighted = true;
                this.graphLayout.HighlightVertex( vertex, "None" );
            }
        }

        public void FlushLog()
        {
            if( _logTextWriter != null && _logFileStream != null )
            {
                _logTextWriter.Flush();
                _logFileStream.Flush();
            }
        }

        //private void LogListBox_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        //{
        //    if( e.Action == NotifyCollectionChangedAction.Add )
        //    {
        //        //if( e.NewItems.Count > 0 )
        //        //{
        //        //    object lastItem = e.NewItems[e.NewItems.Count - 1];
        //        //    LogListBox.ScrollIntoView( lastItem );
        //        //}
        //        LogListBox.ScrollIntoView( LogListBox.Items[LogListBox.Items.Count-1] );
        //    }
        //}

        private void LoadAssemblyDirectory_Click( object sender, RoutedEventArgs e )
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = _lastLoadedFolder;
            var result = d.ShowDialog();
            if( result == System.Windows.Forms.DialogResult.OK )
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
            if( result == System.Windows.Forms.DialogResult.OK )
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
            if( result == System.Windows.Forms.DialogResult.OK )
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
            if( result == System.Windows.Forms.DialogResult.OK )
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

        private void TreeView_Selected( object sender, RoutedEventArgs e )
        {
            TreeViewItem selectedItem = (TreeViewItem)e.OriginalSource;
            AssemblyInfoViewModel assemblyViewModel = (AssemblyInfoViewModel)selectedItem.Header;
            IAssemblyInfo assembly = assemblyViewModel.AssemblyInfo;

            AssemblyVertex vertex = _viewModel.GetVertexFromAssembly( assembly );
            HighlightVertex( vertex );
        }
    }
}