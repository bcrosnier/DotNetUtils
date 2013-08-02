using System;
using System.Globalization;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BCrosnier.SolutionAnalyzer
{
    internal static class MenuCommands
    {
        // Get the development environment of VS.
        private static readonly DTE2 DTE2 = Package.GetGlobalService( typeof( DTE ) ) as DTE2;

        private static readonly string UTILITIES_EXECUTABLE_PATH = typeof( DotNetUtilitiesApp.App ).Assembly.Location;

        internal static void AnalyzeSolutionCommand( object sender, EventArgs e )
        {
            if( !DTE2.Solution.IsOpen )
            {
                ThrowOpenSolutionMessage();
                return;
            }

            OpenSolutionAnalyzer();
        }

        private static void ThrowOpenSolutionMessage()
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

            return;
        }

        private static void OpenSolutionAnalyzer()
        {
            OpenUtilities( "-AnalyzeSolution", DTE2.Solution.FullName );
        }

        private static void OpenSolutionVersionAnalyzer()
        {
            OpenUtilities( "-AnalyzeSolutionVersion", DTE2.Solution.FullName );
        }

        private static void OpenUtilities( string command, string path )
        {
            if( File.Exists( UTILITIES_EXECUTABLE_PATH ) )
            {
                string parameters = command + " \"" + path + "\"";
                System.Diagnostics.Process.Start( UTILITIES_EXECUTABLE_PATH, parameters );
            }
        }
    }
}