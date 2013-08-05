using CK.Package;
using DotNetUtilitiesApp.WpfUtils;
using ProjectProber;
using ProjectProber.Impl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DotNetUtilitiesApp.VersionAnalyzer
{
    public class VersionAnalyzerViewModel : ViewModel
    {

        #region Fields

        private string _activeSolutionPath;

        private string _currentVersion;

        private string _messageText;

        private ObservableCollection<string> _warnings;

        public ICommand SchmurtzCommand { get; private set; }

        #endregion Fields

        #region Observable properties

        public string ActiveSolutionPath
        {
            get { return _activeSolutionPath; }
            set
            {
                if (value != _activeSolutionPath)
                {
                    _activeSolutionPath = value;
                    GenerateInformationText();
                    RaisePropertyChanged();
                }
            }
        }

        public string CurrentVersion
        {
            get { return _currentVersion; }
            set
            {
                if (value != _currentVersion)
                {
                    _currentVersion = value;
                    GenerateInformationText();
                    RaisePropertyChanged();
                }
            }
        }

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if (value != _messageText)
                {
                    _messageText = value;
                    RaisePropertyChanged();
                }
            }
        }

        //change readonly ?
        public ObservableCollection<string> Warnings
        {
            get { return _warnings; }
        }

        #endregion Observable properties

        #region Constructor

        public VersionAnalyzerViewModel()
        {
            _warnings = new ObservableCollection<string>();
            CurrentVersion = "0.0.0";

            SchmurtzCommand = new RelayCommand(ExecuteSchmurtz);
        }

        private void ExecuteSchmurtz(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion Constructor

        #region Public methods

        public void LoadFromSolution(string slnPath)
        {
            ActiveSolutionPath = slnPath;

            _warnings.Clear();
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(slnPath);
            ShowResultWarnings(result);

            CurrentVersion = GetResultVersion(result);
        }

        #endregion Public methods

        #region private methods
        /// <summary>
        /// Gets a version from a version check result to display as the Current Version in the version update UI.
        /// </summary>
        /// <param name="result">AssemblyVersionInfoCheckResult to use</param>
        /// <returns>Returned version; null if none found</returns>
        private string GetResultVersion(AssemblyVersionInfoCheckResult result)
        {
            var versions = result.Versions.Where(x => x != null);
            if (versions.Count() > 0)
            {
                AssemblyVersionInfo info = versions.First();

                if (info.AssemblyInformationVersion != null)
                {
                    return info.AssemblyInformationVersion.ToString();
                }
                else if (info.AssemblyVersion != null)
                {
                    return info.AssemblyVersion.ToString();
                }
            }
            return null;
        }

        private void ShowResultWarnings(AssemblyVersionInfoCheckResult result)
        {
            if (result.HasMultipleAssemblyVersion)
            {
                Warnings.Add("Plusieurs AssemblyVersion ont été trouvées dans la solution.");
            }

            if (result.HasMultipleAssemblyFileVersion)
            {
                Warnings.Add("Plusieurs AssemblyFileVersion ont été trouvées dans la solution.");
            }

            if (result.HasMultipleAssemblyInformationVersion)
            {
                Warnings.Add("Plusieurs AssemblyInformationVersion ont été trouvées dans la solution.");
            }

            if (result.HasOneVersionNotSemanticVersionCompliant)
            {
                Warnings.Add("One or more versions is not semantic version compliant.");
            }

            if (result.HaveFileWithoutVersion)
            {
                if (result.HasSharedAssemblyInfo)
                {
                    Warnings.Add("Has a SharedAssemblyInfo.cs without version.");
                }
                else
                {
                    Warnings.Add("Has one or more AssemblyInfo.cs without version.");
                }
            }

            if (!result.HasSharedAssemblyInfo)
            {
                Warnings.Add("No SharedAssemblyInfo file was found in solution directory.");
            }

            if (result.HasMultipleRelativeLinkInCSProj)
            {
                Warnings.Add("Des liens relatifs menant à des fichiers différents ont été trouvés.");
            }

            if (result.HasRelativeLinkInCSProjNotFound)
            {
                Warnings.Add("Un fichier sans lien relatif a été trouvé.");
            }


            if (result.HasMultipleSharedAssemblyInfo)
            {
                Warnings.Add("More than one SharedAssemblyInfo file was found in the solution.");
            }

            if (result.HasMultipleVersionInOneAssemblyInfoFile)
            {
                Warnings.Add("More than one version was found in a project's Properties/AssemblyInfo.cs.");
            }

            if (result.Versions.Count == 0)
            {
                Warnings.Add("Couldn't find any version to use.");
            }
        }

        private void GenerateInformationText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Informations \n\n")
                .Append("Solution path :\n")
                .Append(_activeSolutionPath)
                .Append("\n")
                .Append("Current version :\n")
                .Append(_currentVersion);
            MessageText = stringBuilder.ToString();
        }

        #endregion Private methods

    }
}
