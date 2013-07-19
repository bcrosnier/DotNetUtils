using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using CK.Core;
using DependencyVersionChecker;
using DependencyVersionCheckerApp.Wpf.Graphing;

namespace DependencyVersionCheckerApp.Wpf
{
    public class MainWindowViewModel
        : ViewModel
    {
        private readonly int MAX_LOG_ENTRIES = 50; // Maximum number of log entries in the collection

        #region Members

        private AssemblyVersionChecker _checker;
        private ObservableCollection<AssemblyInfoViewModel> _assemblyViewModels;
        private DirectoryInfo _assemblyDirectory;
        private IDefaultActivityLogger _logger;
        private ObservableCollection<ListBoxItem> _logItems;

        public ICommand ChangeAssemblyFolderCommand;

        private AssemblyGraph _graph;
        private List<IAssemblyInfo> _drawnAssemblies;
        private List<AssemblyVertex> _drawnVertices;
        private List<AssemblyEdge> _drawnEdges;

        private string _layoutAlgorithmType;
        private List<String> _layoutAlgorithmTypes = new List<string>();

        #endregion Members

        #region Observed properties

        public ObservableCollection<AssemblyInfoViewModel> AssemblyViewModels
        {
            get
            {
                return _assemblyViewModels;
            }
            private set
            {
                if( value != _assemblyViewModels )
                {
                    _assemblyViewModels = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<ListBoxItem> LogData
        {
            get
            {
                return this._logItems;
            }

            private set
            {
                if( this._logItems != value )
                {
                    this._logItems = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AssemblyGraph Graph
        {
            get
            {
                return this._graph;
            }

            private set
            {
                if( this._graph != value )
                {
                    this._graph = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<String> LayoutAlgorithmTypes
        {
            get { return _layoutAlgorithmTypes; }
        }

        public string LayoutAlgorithmType
        {
            get { return _layoutAlgorithmType; }
            set
            {
                _layoutAlgorithmType = value;
                RaisePropertyChanged();
            }
        }

        #endregion Observed properties

        #region Constructor/initialization

        public MainWindowViewModel( AssemblyVersionChecker checker )
        {
            if( checker == null )
            {
                throw new ArgumentNullException( "checker" );
            }

            _logger = new DefaultActivityLogger();
            _logger.Filter = LogLevelFilter.None;
            _logItems = new ObservableCollection<ListBoxItem>();

            _graph = new AssemblyGraph( true );

            //Add Layout Algorithm Types
            _layoutAlgorithmTypes.Add( "BoundedFR" );
            _layoutAlgorithmTypes.Add( "Circular" );
            _layoutAlgorithmTypes.Add( "CompoundFDP" );
            _layoutAlgorithmTypes.Add( "EfficientSugiyama" );
            _layoutAlgorithmTypes.Add( "FR" );
            _layoutAlgorithmTypes.Add( "ISOM" );
            _layoutAlgorithmTypes.Add( "KK" );
            _layoutAlgorithmTypes.Add( "LinLog" );
            _layoutAlgorithmTypes.Add( "Tree" );

            //Pick a default Layout Algorithm Type
            LayoutAlgorithmType = "ISOM";

            var logClient = new ListBoxItemCollectionLoggerClient( _logItems, MAX_LOG_ENTRIES );
            _logger.Output.RegisterClient( logClient );

            _checker = checker;
            _checker.AssemblyCheckComplete += OnNewAssemblyCheck;

            _assemblyViewModels = new ObservableCollection<AssemblyInfoViewModel>();
            PrepareCommands();

            ChangeAssemblyDirectory( new DirectoryInfo( Environment.CurrentDirectory ) );
        }

        public void PrepareCommands()
        {
            ChangeAssemblyFolderCommand = new RelayCommand( ExecuteChangeAssemblyFolder );
        }

        #endregion Constructor/initialization

        #region Methods

        public void ChangeAssemblyDirectory( DirectoryInfo dir )
        {
            _logger.Info( "Loading directory: {0}", dir.FullName );
            if( dir.Exists )
            {
                _assemblyDirectory = dir;

                _checker.Reset();
                _checker.AddDirectory( dir, true );
                _checker.Check();
            }
            else
            {
                _logger.Error( "Directory does not exist: {0}", dir.FullName );
            }
        }

        public void ChangeAssemblyFile( FileInfo file )
        {
            _logger.Info( "Loading file: {0}", file.FullName );
            if( file.Exists )
            {
                _assemblyDirectory = null;

                _checker.Reset();
                _checker.AddFile( file );
                _checker.Check();
            }
            else
            {
                _logger.Error( "File does not exist: {0}", file.FullName );
            }
        }

        public void PrepareGraph( IEnumerable<IAssemblyInfo> assemblies )
        {
            Graph = new AssemblyGraph( true );
            _drawnAssemblies = new List<IAssemblyInfo>();
            _drawnEdges = new List<AssemblyEdge>();
            _drawnVertices = new List<AssemblyVertex>();

            foreach( IAssemblyInfo assembly in assemblies )
            {
                PrepareVertexFromAssembly( assembly );
            }

            RaisePropertyChanged( "Graph" );
        }

        public void LoadAssemblies( IEnumerable<IAssemblyInfo> assemblies )
        {
            LoadAssemblies( assemblies, null );
        }

        public void LoadAssemblies( IEnumerable<IAssemblyInfo> assemblies, IEnumerable<DependencyAssembly> conflicts )
        {
            if( conflicts == null )
            {
                conflicts = AssemblyVersionChecker.GetConflictsFromAssemblyList( assemblies );
            }

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                AssemblyViewModels.Clear();
                foreach( var assembly in assemblies )
                {
                    _logger.Trace( "Adding assembly {0} to tree root", assembly.SimpleName );
                    AssemblyViewModels.Add( new AssemblyInfoViewModel( assembly ) );
                }

                PrepareGraph( assemblies );
            } ) );
            _logger.Info( "Checking {0} assemblies...", AssemblyViewModels.Count );

            int i = 0;
            foreach( var dep in conflicts )
            {
                i++;
                _logger.Warn( "Found a version mismatch about dependency: {0}", dep.AssemblyName );
                using( _logger.OpenGroup( LogLevel.Warn, dep.AssemblyName ) )
                {
                    foreach( var pair in dep.DependencyLinks )
                    {
                        _logger.Warn( "{0} has a reference to: {1}", pair.Key.SimpleName, pair.Value.AssemblyFullName );
                        Graph.MarkAssembly( pair.Value );
                    }
                }
            }

            if( i == 0 )
            {
                _logger.Info( "No version mismatch found." );
            }
            else
            {
                _logger.Warn( "{0} version mismatches found.", i );
            }
        }

        private AssemblyVertex PrepareVertexFromAssembly( IAssemblyInfo assembly )
        {
            if( _drawnAssemblies.Contains( assembly ) )
                return _drawnVertices.Where( x => x.Assembly == assembly ).First();

            AssemblyVertex v = new AssemblyVertex( assembly );

            _drawnAssemblies.Add( assembly );
            _drawnVertices.Add( v );

            Graph.AddVertex( v );

            foreach( IAssemblyInfo dep in assembly.Dependencies )
            {
                AssemblyVertex vDep = PrepareVertexFromAssembly( dep );
                AssemblyEdge depEdge = new AssemblyEdge( vDep, v );
                _drawnEdges.Add( depEdge );
                Graph.AddEdge( depEdge );
            }

            return v;
        }

        private static IList<AssemblyInfo> ListReferencedAssemblies( AssemblyInfo assembly )
        {
            return ListReferencedAssemblies( assembly, new List<AssemblyInfo>() );
        }

        private static IList<AssemblyInfo> ListReferencedAssemblies( AssemblyInfo assembly, IList<AssemblyInfo> existingAssemblies )
        {
            if( existingAssemblies.Contains( assembly ) )
                return existingAssemblies;

            existingAssemblies.Add( assembly );

            foreach( AssemblyInfo dep in assembly.Dependencies )
            {
                ListReferencedAssemblies( dep, existingAssemblies );
            }

            return existingAssemblies;
        }

        #endregion Methods

        #region Event handler methods

        public void OnNewAssemblyCheck( object sender, AssemblyCheckCompleteEventArgs e )
        {
            var checkedAssemblies =
                e.AssemblyCompleteEventArgs
                .Where( x => x.ResultingAssembly != null )
                .Select( x => x.ResultingAssembly );

            LoadAssemblies( checkedAssemblies, e.VersionConflicts );
        }

        #endregion Event handler methods

        #region Command methods

        public void ExecuteChangeAssemblyFolder( object parameter )
        {
            if( !(parameter is DirectoryInfo) )
            {
                throw new ArgumentException( "Parameter must be a DirectoryInfo", "parameter" );
            }

            DirectoryInfo dir = parameter as DirectoryInfo;
            ChangeAssemblyDirectory( dir );
        }

        public void SaveXmlFile( FileInfo fileToWrite )
        {
            XmlSerializer serializer = new XmlSerializer( typeof( SerializableAssemblyInfoSet ), new Type[] { typeof( AssemblyInfo ) } );

            SerializableAssemblyInfoSet set = new SerializableAssemblyInfoSet();

            List<AssemblyInfo> assemblies = new List<AssemblyInfo>();

            foreach( var assembly in this._drawnAssemblies )
            {
                ListReferencedAssemblies( (AssemblyInfo)assembly, assemblies );
            }

            set.Assemblies = assemblies.ToArray();

            using( Stream fs = fileToWrite.OpenWrite() )
            {
                serializer.Serialize( fs, set );
            }
        }

        public void LoadXmlFile( FileInfo fileToRead )
        {
            XmlSerializer serializer = new XmlSerializer( typeof( SerializableAssemblyInfoSet ), new Type[] { typeof( AssemblyInfo ) } );

            List<AssemblyInfo> assemblies;

            using( Stream fs = fileToRead.OpenRead() )
            {
                SerializableAssemblyInfoSet set = serializer.Deserialize( fs ) as SerializableAssemblyInfoSet;
                assemblies = set.Assemblies.ToList();
            }

            LoadAssemblies( assemblies );
        }

        #endregion Command methods
    }
}