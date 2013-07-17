using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using CK.Core;
using DependencyVersionChecker;
using System.Linq;
using System.Windows;

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

            var logClient = new ListBoxItemCollectionLoggerClient( _logItems, MAX_LOG_ENTRIES );
            _logger.Output.RegisterClient( logClient );

            _checker = checker;
            _checker.AssemblyCheckComplete += OnNewAssemblyCheck;

            _assemblyViewModels = new ObservableCollection<AssemblyInfoViewModel>();
            PrepareCommands();

            OnNewAssemblyDirectory( new DirectoryInfo( Environment.CurrentDirectory ) );
        }

        public void PrepareCommands()
        {
            ChangeAssemblyFolderCommand = new RelayCommand( ExecuteChangeAssemblyFolder );
        }

        #endregion Constructor/initialization

        #region Methods

        public void OnNewAssemblyDirectory( DirectoryInfo dir )
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

        public void OnNewAssemblyCheck( object sender, AssemblyCheckCompleteEventArgs e )
        {
            var checkedAssemblies = 
                e.AssemblyCompleteEventArgs
                .Where( x => x.ResultingAssembly != null )
                .Select( x => x.ResultingAssembly );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                AssemblyViewModels.Clear();
                foreach( var assembly in checkedAssemblies )
                {
                    _logger.Trace( "Adding assembly {0} to tree root", assembly.AssemblyName );
                    AssemblyViewModels.Add( new AssemblyInfoViewModel( assembly ) );
                }

            } ) );
            _logger.Info( "Checking {0} assemblies...", AssemblyViewModels.Count );

            IEnumerable<DependencyAssembly> deps = e.VersionConflicts;

            int i = 0;
            foreach( var dep in deps )
            {
                i++;
                _logger.Warn( "Found a version mismatch about dependency: {0}", dep.AssemblyName );
                using( _logger.OpenGroup( LogLevel.Warn, dep.AssemblyName ) )
                {
                    foreach( var pair in dep.DependencyLinks )
                    {
                        _logger.Warn( "{0} has a reference to: {1}", pair.Key.AssemblyName, pair.Value.AssemblyFullName );
                    }
                }
            }

            if( i == 0 )
            {
                _logger.Info( "No version mismatch found." );
            }
            else
            {
                _logger.Warn( "{0} version mismatch found.", i );
            }
        }

        #endregion Methods

        #region Command methods

        public void ExecuteChangeAssemblyFolder( object parameter )
        {
            if( !(parameter is DirectoryInfo) )
            {
                throw new ArgumentException( "Parameter must be a DirectoryInfo", "parameter" );
            }

            DirectoryInfo dir = parameter as DirectoryInfo;
            OnNewAssemblyDirectory( dir );
        }

        #endregion Command methods
    }
}