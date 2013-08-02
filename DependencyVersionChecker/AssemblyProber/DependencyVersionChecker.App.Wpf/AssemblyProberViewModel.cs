using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using AssemblyProber;
using CK.Core;
using DotNetUtilitiesApp.AssemblyProber.Graphing;
using DotNetUtilitiesApp.WpfUtils;

namespace DotNetUtilitiesApp.AssemblyProber
{
    public class AssemblyProberViewModel
        : ViewModel
    {
        //private readonly int MAX_LOG_ENTRIES = 1000; // Maximum number of log entries in the collection

        #region Members

        public event EventHandler LogFlushRequested;

        public event EventHandler<AssemblyVertexEventArgs> VertexHighlightRequested;

        private AssemblyVersionChecker _checker;
        private ObservableCollection<AssemblyInfoViewModel> _assemblyViewModels;
        private DirectoryInfo _assemblyDirectory;
        private IActivityLogger _logger;
        private ObservableCollection<ListBoxItem> _logItems;
        private bool _isSystemAssembliesEnabled;

        public ICommand ChangeAssemblyFolderCommand { get; private set; }

        public ICommand ToggleSystemAssembliesCommand { get; private set; }

        private AssemblyGraph _graph;
        private List<IAssemblyInfo> _drawnAssemblies;
        private List<IAssemblyInfo> _activeAssemblies;
        private List<AssemblyVertex> _drawnVertices;
        private List<AssemblyEdge> _drawnEdges;

        private string _statusBarText;
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
                if( _layoutAlgorithmTypes.Contains( value ) )
                {
                    _layoutAlgorithmType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string StatusBarText
        {
            get { return _statusBarText; }
            set
            {
                if( value != _statusBarText )
                {
                    _statusBarText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsSystemAssembliesEnabled
        {
            get
            {
                return _isSystemAssembliesEnabled;
            }
            set
            {
                if( value != _isSystemAssembliesEnabled )
                {
                    _isSystemAssembliesEnabled = value;
                    LoadAssemblies( _activeAssemblies );
                    RaisePropertyChanged();
                }
            }
        }

        #endregion Observed properties

        #region Constructor/initialization

        public AssemblyProberViewModel( IActivityLogger parentLogger, AssemblyVersionChecker checker, string openAtPath )
        {
            if( checker == null )
            {
                throw new ArgumentNullException( "checker" );
            }

            _logger = parentLogger;
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

            StatusBarText = @"Ready.";

            //var logClient = new ListBoxItemCollectionLoggerClient( _logItems, MAX_LOG_ENTRIES );
            //parentLogger.Output.RegisterClient( logClient );

            _checker = checker;

            _assemblyViewModels = new ObservableCollection<AssemblyInfoViewModel>();
            PrepareCommands();

            if( openAtPath != null )
            {
                ChangeAssemblyDirectory( new DirectoryInfo( openAtPath ) );
            }
            //else
            //{
            //    ChangeAssemblyDirectory(new DirectoryInfo(Environment.CurrentDirectory));
            //}
        }

        public void PrepareCommands()
        {
            ChangeAssemblyFolderCommand = new RelayCommand( ExecuteChangeAssemblyFolder );
            ToggleSystemAssembliesCommand = new RelayCommand( ExecuteToggleSystemAssemblies );
        }

        #endregion Constructor/initialization

        #region Public Methods

        public void ChangeAssemblyDirectory( DirectoryInfo dir )
        {
            _logger.Info( "Loading directory: {0}", dir.FullName );
            StatusBarText = String.Format( "Loading directory: {0}", dir.FullName );
            if( dir.Exists )
            {
                _assemblyDirectory = dir;

                _checker.Reset();
                _checker.AddDirectory( dir, true );

                Task.Factory.StartNew( () =>
                {
                    AssemblyCheckResult r = _checker.Check();
                    InvokeOnAppThread( () =>
                    {
                        _activeAssemblies = r.Assemblies.ToList();
                        LoadAssemblies( _activeAssemblies, r.VersionConflicts, r.ReferenceVersionMismatches );
                        StatusBarText = String.Format( "{0} found in directory", _activeAssemblies.Count );
                    } );
                } );
            }
            else
            {
                _logger.Error( "Directory does not exist: {0}", dir.FullName );
            }
        }

        public void ChangeAssemblyFile( FileInfo file )
        {
            _logger.Info( "Loading file: {0}", file.FullName );
            StatusBarText = String.Format( "Loading file: {0}", file.FullName );
            if( file.Exists )
            {
                _assemblyDirectory = null;

                _checker.Reset();
                _checker.AddFile( file );

                Task.Factory.StartNew( () =>
                {
                    AssemblyCheckResult r = _checker.Check();
                    InvokeOnAppThread( () =>
                    {
                        _activeAssemblies = r.Assemblies.ToList();
                        LoadAssemblies( _activeAssemblies, r.VersionConflicts, r.ReferenceVersionMismatches );
                        StatusBarText = String.Format( "{0} found in directory", _activeAssemblies.Count );
                    } );
                } );
            }
            else
            {
                _logger.Error( "File does not exist: {0}", file.FullName );
            }
        }

        public void LoadAssemblies( IEnumerable<IAssemblyInfo> assemblies )
        {
            LoadAssemblies( assemblies, null, null );
        }

        public void LoadAssemblies( IEnumerable<IAssemblyInfo> assemblies, IEnumerable<AssemblyReferenceName> conflicts, IEnumerable<AssemblyReference> refMismatches )
        {
            if( !_isSystemAssembliesEnabled )
            {
                assemblies = assemblies.Where( x => x.BorderName == null );
            }

            if( conflicts == null )
            {
                conflicts = AssemblyVersionChecker.GetConflictsFromAssemblyList( assemblies );
            }

            if( refMismatches == null )
            {
                refMismatches = AssemblyVersionChecker.GetReferenceMismatches( assemblies );
            }

            AssemblyViewModels.Clear();
            foreach( var assembly in assemblies.OrderBy( x => x.FullName ) )
            {
                _logger.Trace( "Adding assembly {0} to tree root", assembly.SimpleName );
                AssemblyViewModels.Add( new AssemblyInfoViewModel( assembly ) );
            }

            PrepareGraph( assemblies );

            _logger.Info( "Checking {0} assemblies...", AssemblyViewModels.Count );

            int count = conflicts.Count();
            foreach( var dep in conflicts )
            {
                _logger.Warn( "Found a version mismatch about dependency: {0}", dep.AssemblyName );
                using( _logger.OpenGroup( LogLevel.Warn, dep.AssemblyName ) )
                {
                    foreach( var pair in dep.ReferenceLinks )
                    {
                        _logger.Warn( "{0} has a reference to: {1}", pair.Key.SimpleName, pair.Value.FullName );
                        Graph.MarkAssembly( pair.Value );
                        Graph.AddAssemblyMessage( pair.Value, String.Format( "There is a duplicate {0} with a different version.", pair.Value.SimpleName ) );
                    }
                }
            }

            if( count == 0 )
            {
                _logger.Info( "No version mismatch found." );
            }
            else
            {
                _logger.Warn( "{0} version mismatches found.", count );
            }

            foreach( var refMismatch in refMismatches )
            {
                _logger.Warn( "Reference mismatch: [{0} {1}] references [{2} {3}], but [{4} {5}] was resolved.",
                    refMismatch.Parent.SimpleName, refMismatch.Parent.Version.ToString(),
                    refMismatch.ReferenceNameAssemblyObject.SimpleName, refMismatch.ReferenceNameAssemblyObject.Version.ToString(),
                    refMismatch.ReferencedAssembly.SimpleName, refMismatch.ReferencedAssembly.Version.ToString() );

                Graph.MarkAssembly( refMismatch.ReferencedAssembly );
                Graph.AddAssemblyMessage( refMismatch.ReferencedAssembly,
                    String.Format( "Referenced as version {0} by [{1} {2}].",
                    refMismatch.ReferenceNameAssemblyObject.Version.ToString(),
                    refMismatch.Parent.SimpleName, refMismatch.Parent.Version.ToString()
                    ) );
            }

            RaiseLogFlushRequested();
        }

        public void SaveXmlFile( FileInfo fileToWrite )
        {
            using( XmlWriter w = XmlWriter.Create( fileToWrite.FullName ) )
            {
                List<IAssemblyInfo> assemblies = new List<IAssemblyInfo>();

                foreach( var assembly in this._drawnAssemblies )
                {
                    ListReferencedAssemblies( assembly, assemblies );
                }

                AssemblyInfoXmlSerializer.SerializeTo( assemblies, w );
            }
        }

        public void LoadXmlFile( FileInfo fileToRead )
        {
            IEnumerable<IAssemblyInfo> assemblies;

            using( XmlReader r = XmlReader.Create( fileToRead.FullName ) )
            {
                assemblies = AssemblyInfoXmlSerializer.DeserializeFrom( r );
            }
            _activeAssemblies = assemblies.ToList();
            LoadAssemblies( assemblies );
        }

        #endregion Public Methods

        #region Private methods

        private void RaiseLogFlushRequested()
        {
            if( LogFlushRequested != null )
            {
                LogFlushRequested( this, null );
            }
        }

        private void RaiseVertexHighlightRequested( AssemblyVertex vertex )
        {
            if( VertexHighlightRequested != null )
            {
                VertexHighlightRequested( this, new AssemblyVertexEventArgs( vertex ) );
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

            foreach( var pair in assembly.Dependencies )
            {
                IAssemblyInfo dep = pair.Value;
                if( !_isSystemAssembliesEnabled && dep.BorderName != null )
                    continue;

                AssemblyVertex vDep = PrepareVertexFromAssembly( dep );
                AssemblyEdge depEdge = new AssemblyEdge( v, vDep );

                vDep.AddReferencedBy( v.Assembly );
                _drawnEdges.Add( depEdge );
                Graph.AddEdge( depEdge );
            }

            return v;
        }

        private void PrepareGraph( IEnumerable<IAssemblyInfo> assemblies )
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

        #endregion Private methods

        #region Event handler methods

        #endregion Event handler methods

        #region Private static methods

        private static void InvokeOnAppThread( Action action )
        {
            Dispatcher dispatchObject = System.Windows.Application.Current.Dispatcher;
            if( dispatchObject == null || dispatchObject.CheckAccess() )
            {
                action();
            }
            else
            {
                dispatchObject.BeginInvoke( action );
            }
        }

        private static IList<IAssemblyInfo> ListReferencedAssemblies( IAssemblyInfo assembly )
        {
            return ListReferencedAssemblies( assembly, new List<IAssemblyInfo>() );
        }

        private static IList<IAssemblyInfo> ListReferencedAssemblies( IAssemblyInfo assembly, IList<IAssemblyInfo> existingAssemblies )
        {
            if( existingAssemblies.Contains( assembly ) )
                return existingAssemblies;

            existingAssemblies.Add( assembly );

            foreach( var pair in assembly.Dependencies )
            {
                IAssemblyInfo dep = pair.Value;

                ListReferencedAssemblies( dep, existingAssemblies );
            }

            return existingAssemblies;
        }

        #endregion Private static methods

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

        public void ExecuteToggleSystemAssemblies( object parameter )
        {
            IsSystemAssembliesEnabled = !IsSystemAssembliesEnabled;
        }

        #endregion Command methods

        internal AssemblyVertex GetVertexFromAssembly( IAssemblyInfo assembly )
        {
            return _drawnVertices.Where( x => x.Assembly == assembly ).FirstOrDefault();
        }
    }
}