using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CK.Core;
using DotNetUtilitiesApp.WpfUtils;
using NuGet;
using ProjectProber;
using ProjectProber.Interfaces;

namespace DotNetUtilitiesApp.SolutionAnalyzer
{
    internal class SolutionAnalyzerViewModel : ViewModel
    {
        private const string DEFAULT_MESSAGE = @"No solution analyzed. Use the Solution menu to load it.";
        #region Fields

        private string _messageText;

        #endregion Fields

        #region Observable properties

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if( value != _messageText )
                {
                    _messageText = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion Observable properties

        internal SolutionAnalyzerViewModel()
        {
            MessageText = DEFAULT_MESSAGE;
        }

        #region Public methods

        public void AnalyzeSolutionFile( string solutionFilePath )
        {
            CleanUp();

            MessageText = String.Format( "Analyzing solution: {0}...", solutionFilePath );
            Task task = Task.Factory.StartNew( () =>
            {
                DefaultActivityLogger logger = new DefaultActivityLogger();

                logger.Tap.Register( new ActivityLoggerConsoleSink() );

                SolutionCheckResult result = SolutionChecker.CheckSolutionFile( solutionFilePath, logger );
                Invoke.OnAppThread( () =>
                {
                    SetSolutionResults( result );
                } );
            } );
        }

        public void CleanUp()
        {
            MessageText = DEFAULT_MESSAGE;
        }

        #endregion Public methods

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

        #endregion Private static methods

        #region Private methods

        private void SetSolutionResults( SolutionCheckResult result )
        {
            PrintResults( result );
        }

        private void PrintResults( SolutionCheckResult result )
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( String.Format( "Solution analyzed: {0}", result.SolutionPath ) );
            sb.AppendLine( String.Format( "{0} projects, {1} NuGet packageRef references", result.Projects.Count(), result.NuGetPackages.Count() ) );

            if( result.PackagesWithMultipleVersions.Count > 0 )
            {
                sb.AppendLine( String.Format( "{0} NuGet packages are referenced in multiple versions:", result.PackagesWithMultipleVersions.Count ) );
                foreach( var pair in result.PackagesWithMultipleVersions.OrderBy( x => x.Key ) )
                {
                    sb.AppendLine( String.Format( "\t{0} is referenced in {1} versions:",
                        pair.Key, pair.Value.Count()
                        ) );

                    foreach( INuGetPackageReference packageVersion in pair.Value.OrderBy( x => x.Id ).ThenBy( x => x.Version ) )
                    {
                        sb.AppendLine( String.Format( "\t\t{0}, version {1} is referenced by these projects:",
                            packageVersion.Id, packageVersion.Version.ToString()
                            ) );

                        IEnumerable<ISolutionProjectItem> referencingProjects = result.ProjectNugetReferences
                              .Where( x => x.Value.Any( y => y.Id == packageVersion.Id && y.Version == packageVersion.Version.ToString() ) )
                              .Select( x => x.Key );

                        foreach( ISolutionProjectItem project in referencingProjects.OrderBy( x => x.ProjectName ) )
                        {
                            sb.AppendLine( String.Format( "\t\t\t{0} ({1})", project.ProjectName, project.ProjectPath ) );
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine( "All solution projects are using the same NuGet packageRef versions." );
            }

            sb.AppendLine();
            sb.AppendLine( "Projects in solution:" );
            foreach( ISolutionProjectItem project in result.Projects.OrderBy( x => x.ProjectPath ) )
            {
                sb.AppendLine( String.Format( "\t{0} ({1})", project.ProjectName, project.ProjectPath ) );
            }

            sb.AppendLine();
            sb.AppendLine( "NuGet packages referenced by solution projects:" );
            if( result.Projects.Count() > 0 )
            {
                foreach( INuGetPackageReference packageRef in result.NuGetPackages.Keys.OrderBy( x => x.Id ).ThenBy( x => x.Version ) )
                {
                    sb.AppendLine( String.Format( "\t{0}, version {1}", packageRef.Id, packageRef.Version.ToString() ) );
                }
            }
            else
            {
                sb.AppendLine( "This solution's projects do not reference any NuGet packageRef." );
            }

            MessageText = sb.ToString();
        }

        #endregion Private methods
    }
}