using ProjectProber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ProjectProber.Impl;
using CK.Package;
using System.Windows.Data;

namespace DotNetUtilitiesApp.VersionAnalyzer
{
    public class AssemblyVersionError
    {
        AssemblyVersionErrorType _assemblyError;
        string _errorMessage;
        AssemblyVersionInfoCheckResult _result;

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        internal AssemblyVersionError( AssemblyVersionErrorType assemblyError, AssemblyVersionInfoCheckResult result )
        {
            _assemblyError = assemblyError;
            _result = result;
            SelectErrorMessage();
        }

        private string CreateSpecificDetailView()
        {
            return string.Empty;
        }

        private void SelectErrorMessage()
        {
            switch( _assemblyError )
            {
                case AssemblyVersionErrorType.HasFileWithoutVersion:
                    if( !_result.HasNotSharedAssemblyInfo )
                    {
                        _errorMessage = "Has a SharedAssemblyInfo.cs without version.";
                    }
                    else
                    {
                        _errorMessage = "More than one AssemblyInfo.cs without version.";
                    }
                    break;
                case AssemblyVersionErrorType.HasMultipleAssemblyFileVersion:
                    _errorMessage = "More than one AssemblyFileVersion was found in the solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleAssemblyInformationVersion:
                    _errorMessage = "More than one AssemblyInformationalVersion was found in the solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleAssemblyVersion:
                    _errorMessage = "More than one AssemblyVersion was found in the solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleRelativeLinkInCSProj:
                    _errorMessage = "Relative links leading to different files were found.";
                    break;
                case AssemblyVersionErrorType.HasMultipleSharedAssemblyInfo:
                    _errorMessage = "More than one SharedAssemblyInfo file was found in the solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleVersionInOneAssemblyInfo:
                    _errorMessage = "Different versions were found in one Properties/AssemblyInfo.cs.";
                    break;
                case AssemblyVersionErrorType.HasOneVersionNotSemanticVersionCompliant:
                    _errorMessage = "At least one version is not semantic version compliant.";
                    break;
                case AssemblyVersionErrorType.HasRelativeLinkInCSProjNotFound:
                    _errorMessage = "A csproj file without relative links was found.";
                    break;
                case AssemblyVersionErrorType.HasNotSharedAssemblyInfo:
                    _errorMessage = "No SharedAssemblyInfo file was found in solution directory.";
                    break;
                default:
                    _errorMessage = "More than one AssemblyVersion was found in the solution.";
                    break;
            }

        }

        public UIElement CreateDetailControl()
        {
            switch( _assemblyError )
            {
                case AssemblyVersionErrorType.HasFileWithoutVersion:
                    return GetDetailForHasFileWithoutVersion();
                case AssemblyVersionErrorType.HasMultipleAssemblyFileVersion:
                    return GetDetailForHasMultipleAssemblyFileVersion();
                case AssemblyVersionErrorType.HasMultipleAssemblyInformationVersion:
                    return GetDetailForHasMultipleAssemblyInformationVersion();
                case AssemblyVersionErrorType.HasMultipleAssemblyVersion:
                    return GetDetailForHasMultipleAssemblyVersion();
                case AssemblyVersionErrorType.HasMultipleRelativeLinkInCSProj:
                    return GetDetailForHasMultipleRelativeLinkInCSProj();
                case AssemblyVersionErrorType.HasMultipleSharedAssemblyInfo:
                    return GetDetailForHasMultipleSharedAssemblyInfo();
                case AssemblyVersionErrorType.HasMultipleVersionInOneAssemblyInfo:
                    return GetDetailForHasMultipleVersionInOneAssemblyInfoFile();
                case AssemblyVersionErrorType.HasOneVersionNotSemanticVersionCompliant:
                    return GetDetailForHasOneVersionNotSemanticVersionCompliant();
                case AssemblyVersionErrorType.HasRelativeLinkInCSProjNotFound:
                    return GetDetailForHasRelativeLinkInCSProjNotFound();
                case AssemblyVersionErrorType.HasNotSharedAssemblyInfo:
                    return GetDetailForHasNotSharedAssemblyInfo();
                default:
                    return null;
            }
        }

        private UIElement GetDetailForHasNotSharedAssemblyInfo()
        {
            TextBlock tb = new TextBlock();
            tb.Text = "No SharedAssemblyInfo.cs file was Found in \n" + _result.SolutionDirectoryPath;
            return tb;
        }

        private UIElement GetDetailForHasRelativeLinkInCSProjNotFound()
        {
            ListBox lb = new ListBox();
            IEnumerable<CSProjCompileLinkInfo> CSProjwithoutRelativeLink;
            CSProjwithoutRelativeLink = _result.CsProjs
                .Where( x => string.IsNullOrEmpty( x.SharedAssemblyInfoRelativePath ) && string.IsNullOrEmpty( x.AssociateLink ) );
            foreach( var CSProj in CSProjwithoutRelativeLink )
            {
                lb.Items.Add( CSProj.NameProject );
            }

            return lb;
        }

        private UIElement GetDetailForHasOneVersionNotSemanticVersionCompliant()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Path";
            column1.Binding = new Binding( "AssemblyInfoFilePath" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );

            var column2 = new DataGridTextColumn();
            column2.Header = "Version";
            column2.Binding = new Binding( "AssemblyVersion" );

            var column3 = new DataGridTextColumn();
            column3.Header = "File Version";
            column3.Binding = new Binding( "AssemblyFileVersion" );

            var column4 = new DataGridTextColumn();
            column4.Header = "Information version";
            column4.Binding = new Binding( "AssemblyInformationVersion" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );
            dg.Columns.Add( column3 );
            dg.Columns.Add( column4 );

            IEnumerable<AssemblyVersionInfo> filesWithNonSemanticVersion;
            SemanticVersion temp;
            if( !_result.HasNotSharedAssemblyInfo )
            {
                filesWithNonSemanticVersion = _result.SharedAssemblyInfoVersions
                    .Where( x => (x.AssemblyVersion != null && !SemanticVersion.TryParseStrict( x.AssemblyVersion.ToString(), out temp ))
                        || (x.AssemblyFileVersion != null && !SemanticVersion.TryParseStrict( x.AssemblyFileVersion.ToString(), out temp )) );
            }
            else
            {
                filesWithNonSemanticVersion = _result.AssemblyVersions
                    .Where( x => (x.AssemblyVersion != null && !SemanticVersion.TryParseStrict( x.AssemblyVersion.ToString(), out temp ))
                        || (x.AssemblyFileVersion != null && !SemanticVersion.TryParseStrict( x.AssemblyFileVersion.ToString(), out temp )) );
            }

            dg.ItemsSource = filesWithNonSemanticVersion;

            return dg;
        }

        private UIElement GetDetailForHasMultipleVersionInOneAssemblyInfoFile()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Path";
            column1.Binding = new Binding( "AssemblyInfoFilePath" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );

            var column2 = new DataGridTextColumn();
            column2.Header = "Version";
            column2.Binding = new Binding( "AssemblyVersion" );

            var column3 = new DataGridTextColumn();
            column3.Header = "File Version";
            column3.Binding = new Binding( "AssemblyFileVersion" );

            var column4 = new DataGridTextColumn();
            column4.Header = "Information version";
            column4.Binding = new Binding( "AssemblyInformationVersion" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );
            dg.Columns.Add( column3 );
            dg.Columns.Add( column4 );

            IEnumerable<AssemblyVersionInfo> filesWithNonSemanticVersion;
            if( !_result.HasNotSharedAssemblyInfo )
            {
                filesWithNonSemanticVersion = _result.SharedAssemblyInfoVersions
                    .Where( x => x.AssemblyVersion != x.AssemblyFileVersion
                        || (!string.IsNullOrEmpty( x.AssemblyInformationalVersion )
                        && x.AssemblyVersion.ToString() != x.AssemblyInformationalVersion) );
            }
            else
            {
                filesWithNonSemanticVersion = _result.AssemblyVersions
                    .Where( x => x.AssemblyVersion != x.AssemblyFileVersion
                        || (!string.IsNullOrEmpty( x.AssemblyInformationalVersion )
                        && x.AssemblyVersion.ToString() != x.AssemblyInformationalVersion) );
            }

            dg.ItemsSource = filesWithNonSemanticVersion;

            return dg;
        }

        private UIElement GetDetailForHasMultipleSharedAssemblyInfo()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Path";
            column1.Binding = new Binding( "AssemblyInfoFilePath" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );

            var column2 = new DataGridTextColumn();
            column2.Header = "Version";
            column2.Binding = new Binding( "AssemblyVersion" );

            var column3 = new DataGridTextColumn();
            column3.Header = "File Version";
            column3.Binding = new Binding( "AssemblyFileVersion" );

            var column4 = new DataGridTextColumn();
            column4.Header = "Information version";
            column4.Binding = new Binding( "AssemblyInformationVersion" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );
            dg.Columns.Add( column3 );
            dg.Columns.Add( column4 );

            dg.ItemsSource = _result.SharedAssemblyInfoVersions;

            return dg;
        }

        private UIElement GetDetailForHasMultipleRelativeLinkInCSProj()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Project Path";
            column1.Binding = new Binding( "Project" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );

            var column2 = new DataGridTextColumn();
            column2.Header = "Relative path";
            column2.Binding = new Binding( "SharedAssemblyInfoRelativePath" );

            var column3 = new DataGridTextColumn();
            column3.Header = "Associate path";
            column3.Binding = new Binding( "AssociateLink" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );
            dg.Columns.Add( column3 );

            dg.ItemsSource = _result.CsProjs;

            return dg;
        }

        private UIElement GetDetailForHasMultipleAssemblyVersion()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Path";
            column1.Binding = new Binding( "AssemblyInfoFilePath" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );


            var column2 = new DataGridTextColumn();
            column2.Header = "Version";
            column2.Binding = new Binding( "AssemblyVersion" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );
            if( !_result.HasNotSharedAssemblyInfo )
            {
                dg.ItemsSource = _result.SharedAssemblyInfoVersions;
            }
            else
            {
                dg.ItemsSource = _result.AssemblyVersions;
            }

            return dg;
        }

        private UIElement GetDetailForHasMultipleAssemblyInformationVersion()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Path";
            column1.Binding = new Binding( "AssemblyInfoFilePath" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );


            var column2 = new DataGridTextColumn();
            column2.Header = "Information version";
            column2.Binding = new Binding( "AssemblyInformationVersion" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );
            if( !_result.HasNotSharedAssemblyInfo )
            {
                dg.ItemsSource = _result.SharedAssemblyInfoVersions;
            }
            else
            {
                dg.ItemsSource = _result.AssemblyVersions;
            }

            return dg;
        }

        private UIElement GetDetailForHasMultipleAssemblyFileVersion()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            dg.HorizontalAlignment = HorizontalAlignment.Stretch;

            var column1 = new DataGridTextColumn();
            column1.Header = "Path";
            column1.Binding = new Binding( "AssemblyInfoFilePath" );
            column1.Width = new DataGridLength( 340 );
            column1.ElementStyle = new Style();
            column1.ElementStyle.Setters.Add( new Setter( TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right ) );


            var column2 = new DataGridTextColumn();
            column2.Header = "File version";
            column2.Binding = new Binding( "AssemblyFileVersion" );

            dg.Columns.Add( column1 );
            dg.Columns.Add( column2 );

            if( !_result.HasNotSharedAssemblyInfo )
            {
                dg.ItemsSource = _result.SharedAssemblyInfoVersions;
            }
            else
            {
                dg.ItemsSource = _result.AssemblyVersions;
            }

            return dg;
        }

        private UIElement GetDetailForHasFileWithoutVersion()
        {
            ListBox lb = new ListBox();
            IEnumerable<AssemblyVersionInfo> filesWithoutVersion;
            if( !_result.HasNotSharedAssemblyInfo )
            {
                filesWithoutVersion = _result.SharedAssemblyInfoVersions
                    .Where( x => x.AssemblyVersion == null && x.AssemblyFileVersion == null && string.IsNullOrEmpty( x.AssemblyInformationalVersion ) );
            }
            else
            {
                filesWithoutVersion = _result.AssemblyVersions
                    .Where( x => x.AssemblyVersion == null && x.AssemblyFileVersion == null && string.IsNullOrEmpty( x.AssemblyInformationalVersion ) );
            }

            foreach( var fileWithoutVersion in filesWithoutVersion )
            {
                lb.Items.Add( fileWithoutVersion.AssemblyInfoFilePath );
            }

            return lb;
        }
    }
}
