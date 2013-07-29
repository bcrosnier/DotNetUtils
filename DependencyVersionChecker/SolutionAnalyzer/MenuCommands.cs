using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BCrosnier.SolutionAnalyzer
{
    internal class MenuCommands
    {
        // Get the development environment of VS.
        readonly DTE2 DTE2 = Package.GetGlobalService( typeof( DTE ) ) as DTE2;

        IActivityLogger _logger;
        bool _customBuildRunning;

        public MenuCommands( IActivityLogger logger )
        {
            // TODO: Complete member initialization
            this._logger = logger;
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        internal void AnalyzeSolutionAssembliesCommand( object sender, EventArgs e )
        {
            if( !DTE2.Solution.IsOpen )
            {
                // (BC) I'm keeping this here; UI examples can be useful later.
                IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService( typeof( SVsUIShell ) );
                Guid clsid = Guid.Empty;
                int result;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "SolutionAnalyzer",
                           string.Format( CultureInfo.CurrentCulture, "A solution must be opened in order to work." ),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_CRITICAL,
                           0,        // false
                           out result ) );

                return;
            }

            _logger.Trace( "Command running: AnalyzeSolutionAssembliesCommand" );
            _logger.Trace( DTE2.Solution.SolutionBuild.BuildState.ToString() );
            // User has requested solution assembly analyzing. Three things to do:
            // 1. Clean the solution
            // 2. Build with the selected config
            // 3. Open the program on the solution directory
            //DTE2.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            //DTE2.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
            //DTE2.Solution.SolutionBuild.Clean();

            OpenAssemblyAnalyzer();
        }

        void BuildEvents_OnBuildProjConfigDone( string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success )
        {
            _logger.Trace( "OnBuildProjConfigDone: {0} {1} {2} {3} {4}", Project, ProjectConfig, Platform, SolutionConfig, Success.ToString() );
        }

        void BuildEvents_OnBuildDone( vsBuildScope Scope, vsBuildAction Action )
        {
            _logger.Trace( "OnBuildDone: {0} {1}", Scope.ToString(), Action.ToString() );
            _logger.Trace( "Solution build state: {0}", DTE2.Solution.SolutionBuild.BuildState.ToString() );
            // Unsubscribe self
            DTE2.Events.BuildEvents.OnBuildDone -= BuildEvents_OnBuildDone;

            if( Scope == vsBuildScope.vsBuildScopeSolution && Action == vsBuildAction.vsBuildActionClean )
            {
                DTE2.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
                DTE2.Solution.SolutionBuild.Build();
            }
            //else if( Scope == vsBuildScope.vsBuildScopeSolution && Action == vsBuildAction.vsBuildActionBuild )
            //{
            //    // Unsubscribe self
            //    DTE2.Events.BuildEvents.OnBuildDone -= BuildEvents_OnBuildDone;

            //    // Get solution folder
            //    string folder = Path.GetDirectoryName( DTE2.Solution.FullName );
            //    if( Directory.Exists( folder ) )
            //    {
            //        AssemblyProberApp.Wpf.MainWindow window = new AssemblyProberApp.Wpf.MainWindow( _logger, folder );
            //        window.Show();
            //    }
            //}
            else
            {
                _logger.Trace( "Solution build state: {0}", DTE2.Solution.SolutionBuild.BuildState.ToString() );
            }
        }

        void OpenAssemblyAnalyzer()
        {
            // Get solution folder
            string folder = Path.GetDirectoryName( DTE2.Solution.FullName );
            string dirPath = System.IO.Path.GetDirectoryName( new Uri( System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase ).LocalPath );
            string assemblyAppPath = Path.Combine( dirPath, @"AssemblyProberApp.Wpf.exe" );

            if( Directory.Exists( folder ) && File.Exists( assemblyAppPath ) )
            {
                System.Diagnostics.Process.Start( assemblyAppPath, "\"" + folder + "\"" );

                // We cannot use the below code, unfortunately: VS extensions require all references to have a strong name (ie. to be signed),
                // and WPFExtensions doesn't have one.

                //AssemblyProberApp.Wpf.MainWindow window = new AssemblyProberApp.Wpf.MainWindow( _logger, folder );
                //window.Show();
            }
        }
    }
}
