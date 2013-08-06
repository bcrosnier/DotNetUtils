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

        internal AssemblyVersionError(AssemblyVersionErrorType assemblyError, AssemblyVersionInfoCheckResult result)
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
            switch (_assemblyError)
            {
                case AssemblyVersionErrorType.HasFileWithoutVersion:
                    if (!_result.HasNotSharedAssemblyInfo)
                    {
                        _errorMessage = "Has a SharedAssemblyInfo.cs without version.";
                    }
                    else
                    {
                        _errorMessage = "Has one or more AssemblyInfo.cs without version.";
                    }
                    break;
                case AssemblyVersionErrorType.HasMultipleAssemblyFileVersion:
                    _errorMessage = "Plusieurs AssemblyFileVersion ont été trouvées dans la solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleAssemblyInformationVersion:
                    _errorMessage = "Plusieurs AssemblyInformationVersion ont été trouvées dans la solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleAssemblyVersion:
                    _errorMessage = "Plusieurs AssemblyVersion ont été trouvées dans la solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleRelativeLinkInCSProj:
                    _errorMessage = "Des liens relatifs menant à des fichiers différents ont été trouvés.";
                    break;
                case AssemblyVersionErrorType.HasMultipleSharedAssemblyInfo:
                    _errorMessage = "More than one SharedAssemblyInfo file was found in the solution.";
                    break;
                case AssemblyVersionErrorType.HasMultipleVersionInOneAssemblyInfoFile:
                    _errorMessage = "More than one version was found in a project's Properties/AssemblyInfo.cs.";
                    break;
                case AssemblyVersionErrorType.HasOneVersionNotSemanticVersionCompliant:
                    _errorMessage = "One or more versions is not semantic version compliant.";
                    break;
                case AssemblyVersionErrorType.HasRelativeLinkInCSProjNotFound:
                    _errorMessage = "Un fichier sans lien relatif a été trouvé.";
                    break;
                case AssemblyVersionErrorType.HasNotSharedAssemblyInfo:
                    _errorMessage = "No SharedAssemblyInfo file was found in solution directory.";
                    break;
                default:
                    _errorMessage = "Plusieurs AssemblyVersion ont été trouvées dans la solution.";
                    break;
            }
        }

        public UIElement CreateDetailControl()
        {
            if (_assemblyError == AssemblyVersionErrorType.HasFileWithoutVersion)
            {
                ListBox lb = new ListBox();
                IEnumerable<AssemblyVersionInfo> filesWithoutVersion;
                if (!_result.HasNotSharedAssemblyInfo)
                {
                    filesWithoutVersion = _result.SharedAssemblyInfoVersions
                        .Where(x => x.AssemblyVersion == null && x.AssemblyFileVersion == null && x.AssemblyInformationVersion == null);
                }
                else
                {
                    filesWithoutVersion = _result.AssemblyVersions
                        .Where(x => x.AssemblyVersion == null && x.AssemblyFileVersion == null && x.AssemblyInformationVersion == null);
                }

                foreach (var fileWithoutVersion in filesWithoutVersion)
                {
                    lb.Items.Add(fileWithoutVersion.AssemblyInfoFilePath);
                }

                return lb;
            }
            return new TextBox();
        }
    }
}
