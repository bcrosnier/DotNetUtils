using CK.Core;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Globalization;
using System.IO;

namespace BCrosnier.SolutionAnalyzer
{
    internal class MenuCommands
    {
        // Get the development environment of VS.
        private readonly DTE2 DTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;

        public MenuCommands()
        {
        }

        public void ThrowOpenSolutionMessage()
        {
            IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService( typeof( SVsUIShell ) );
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "SolutionAnalyzer",
                       string.Format( CultureInfo.CurrentCulture, "Open or create a solution first." ),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_CRITICAL,
                       0,        // false
                       out result ) );
        }

        /// <summary>
        /// Menu item command callback:
        /// Analyze the current solution using the solution analyzer.
        /// </summary>
        internal void AnalyzeSolutionCommand( object sender, EventArgs e )
        {
            if( !DTE2.Solution.IsOpen )
            {
                ThrowOpenSolutionMessage();
                return;
            }
            OpenSolutionAnalyzer();
        }

        /// <summary>
        /// Menu item command callback:
        /// Analyze the current solution version using the solution analyzer.
        /// </summary>
        internal void AnalyzeSolutionVersionCommand( object sender, EventArgs e )
        {
            if( !DTE2.Solution.IsOpen )
            {
                ThrowOpenSolutionMessage();
                return;
            }
            OpenSolutionVersionAnalyzer();
        }

        private void OpenSolutionAnalyzer()
        {
            OpenUtilities( "-AnalyzeSolution", DTE2.Solution.FullName );
        }

        private void OpenSolutionVersionAnalyzer()
        {
            OpenUtilities( "-AnalyzeSolutionVersion", DTE2.Solution.FullName );
        }

        private void OpenSolutionAssemblyAnalyzer()
        {
            OpenUtilities( "-AnalyzeSolutionVersion", Path.GetDirectoryName( DTE2.Solution.FullName ) );
        }

        private void OpenUtilities( string command, string path )
        {
            string executablePath = typeof( DotNetUtilitiesApp.App ).Assembly.Location;

            if( File.Exists( DTE2.Solution.FullName ) && File.Exists( executablePath ) )
            {
                string parameters = command + " \"" + path + "\"";
                System.Diagnostics.Process.Start( executablePath, parameters );
            }
        }

    }
}