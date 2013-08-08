using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AssemblyProber;
using CK.Core;
using DotNetUtilitiesApp.AssemblyProber.Graphing;

namespace DotNetUtilitiesApp.AssemblyProber
{
    /// <summary>
    /// Interaction logic for AssemblyProberUserControl.xaml
    /// </summary>
    public partial class AssemblyProberUserControl : System.Windows.Controls.UserControl, IDisposable
    {
        private AssemblyProberViewModel _viewModel;
        private string _activeFolder = Environment.CurrentDirectory;

        private FileStream _logFileStream;
        private TextWriter _logTextWriter;

        private string wpfExtensionsDllPath = typeof( WPFExtensions.ViewModel.Commanding.CommandSink ).Assembly.CodeBase;

        private Graphing.AssemblyVertex highlightedVertex = null;

        public AssemblyProberUserControl()
            : this( new DefaultActivityLogger(), null )
        {
        }

        public AssemblyProberUserControl( IActivityLogger logger, string directoryPath )
        {
            if( !DesignerProperties.GetIsInDesignMode( this ) && logger is IDefaultActivityLogger )
            {
                string logFilePath = Path.Combine( Path.GetTempPath(), @"AssemblyProberApp.log" );
                _logFileStream = new FileStream( logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, false );
                _logTextWriter = new StreamWriter( _logFileStream );
                ActivityLoggerTextWriterSink sink = new ActivityLoggerTextWriterSink( _logTextWriter );

                ((IDefaultActivityLogger)logger).Tap.Register( sink );
            }

            IAssemblyLoader l = new AssemblyLoader( logger );

            AssemblyVersionChecker checker = new AssemblyVersionChecker( l );

            _viewModel = new AssemblyProberViewModel( logger, checker, directoryPath );
            _viewModel.LogFlushRequested += ( s, e ) => { FlushLog(); };
            _viewModel.VertexHighlightRequested += ( s, e ) => { HighlightVertex( e.Vertex ); };
            FlushLog();

            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void HighlightVertex( Graphing.AssemblyVertex vertex )
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

        private void FlushLog()
        {
            if( _logTextWriter != null && _logFileStream != null )
            {
                _logTextWriter.Flush();
                _logFileStream.Flush();
            }
        }

        private void LoadAssemblyDirectory_Click( object sender, RoutedEventArgs e )
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = _activeFolder;
            var result = d.ShowDialog();
            if( result == System.Windows.Forms.DialogResult.OK )
            {
                DirectoryInfo dir = new DirectoryInfo( d.SelectedPath );
                _activeFolder = d.SelectedPath;
                Environment.CurrentDirectory = d.SelectedPath;
                _viewModel.ChangeAssemblyFolderCommand.Execute( dir );
            }
        }

        private void LoadAssemblyFile_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog d = new OpenFileDialog();
            d.InitialDirectory = _activeFolder;
            d.CheckFileExists = true;
            d.Filter = "Binary assemblies (*.dll;*.exe)|*.dll;*.exe";
            DialogResult result = d.ShowDialog();
            if( result == System.Windows.Forms.DialogResult.OK )
            {
                FileInfo file = new FileInfo( d.FileName );
                Environment.CurrentDirectory = file.DirectoryName;
                _viewModel.LoadAssemblyFile( file );
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

        public void CleanUp()
        {
            _viewModel.Reset();
        }

        public void Dispose()
        {
            Dispose( true );
        }

        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                if( _logTextWriter != null )
                    _logTextWriter.Close();
                if( _logFileStream != null )
                    _logFileStream.Close();
            }
        }

        public void SetActiveSolution( string solutionPath )
        {
            _activeFolder = Path.GetDirectoryName( solutionPath );
        }

        //public void CleanUp()
        //{
        //    _viewModel.CleanUp();
        //}
    }
}