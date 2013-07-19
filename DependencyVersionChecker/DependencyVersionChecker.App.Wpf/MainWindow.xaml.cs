using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
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

        public MainWindow()
        {
            AssemblyLoader.BorderChecker c = ( newReference ) =>
            {
                string company = AssemblyLoader.GetCustomAttributeString( newReference, @"System.Reflection.AssemblyCompanyAttribute " );

                /** Microsoft tokens:
                 * "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
                 * "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                 * "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                 * "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
                 * */

                string[] microsoftTokens = new string[] { "b77a5c561934e089", "31bf3856ad364e35", "b03f5f7f11d50a3a", "7cec85d7bea7798e" };

                string token = BitConverter.ToString( newReference.Name.PublicKeyToken ).Replace( "-", string.Empty ).ToLowerInvariant();

                if( microsoftTokens.Contains( token ) || company == "Microsoft" )
                {
                    return "Microsoft";
                }
                if( newReference.MainModule.FullyQualifiedName.StartsWith( Environment.SystemDirectory ) )
                {
                    return "System";
                }
                return null;
            };

            AssemblyVersionChecker checker = new AssemblyVersionChecker( new AssemblyLoader( c ) );
            _viewModel = new MainWindowViewModel( checker );
            this.DataContext = _viewModel;
            InitializeComponent();

            ((INotifyCollectionChanged)LogListBox.Items).CollectionChanged += LogListBox_CollectionChanged;

        }

        void LogListBox_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                if( e.NewItems.Count > 0 )
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
    }
}