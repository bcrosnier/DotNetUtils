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

        private AssemblyVersionInfoCheckResult _result;

        private ObservableCollection<AssemblyVersionError> _assemblyVersionErrors;

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

        public ObservableCollection<AssemblyVersionError> AssemblyVersionErrors
        {
            get { return _assemblyVersionErrors; }
        }

        #endregion Observable properties

        #region Constructor

        public VersionAnalyzerViewModel()
        {
            _warnings = new ObservableCollection<string>();
            _assemblyVersionErrors = new ObservableCollection<AssemblyVersionError>();
            CurrentVersion = "0.0.0";

            SchmurtzCommand = new RelayCommand(ExecuteSchmurtz);
        }

        private void ExecuteSchmurtz(object obj)
        {
            //throw new NotImplementedException();
        }

        #endregion Constructor

        #region Public methods

        public void LoadFromSolution(string slnPath)
        {
            ActiveSolutionPath = slnPath;

            _warnings.Clear();
            _assemblyVersionErrors.Clear();
            _result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(slnPath);
            ShowResultWarnings();

            CurrentVersion = GetResultVersion(_result);
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

        private void ShowResultWarnings()
        {
            if (_result.HasMultipleAssemblyVersion)
            {
                _assemblyVersionErrors.Add( new AssemblyVersionError( AssemblyVersionErrorType.HasMultipleAssemblyVersion, _result ) );
            }
            if (_result.HasMultipleAssemblyFileVersion)
            {
                _assemblyVersionErrors.Add( new AssemblyVersionError( AssemblyVersionErrorType.HasMultipleAssemblyFileVersion, _result ) );
            }
            if (_result.HasMultipleAssemblyInformationVersion)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasMultipleAssemblyInformationVersion, _result));
            }
            if (_result.HasOneVersionNotSemanticVersionCompliant)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasOneVersionNotSemanticVersionCompliant, _result));
            }
            if (_result.HasFileWithoutVersion)
            {
                    _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasFileWithoutVersion, _result));
            }
            if (_result.HasNotSharedAssemblyInfo)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasNotSharedAssemblyInfo, _result));
            }
            if (_result.HasMultipleRelativeLinkInCSProj)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasMultipleRelativeLinkInCSProj, _result));
            }
            if (_result.HasRelativeLinkInCSProjNotFound)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasRelativeLinkInCSProjNotFound, _result));
            }
            if (_result.HasMultipleSharedAssemblyInfo)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasMultipleSharedAssemblyInfo, _result));
            }
            if (_result.HasMultipleVersionInOneAssemblyInfoFile)
            {
                _assemblyVersionErrors.Add(new AssemblyVersionError(AssemblyVersionErrorType.HasMultipleVersionInOneAssemblyInfoFile, _result));
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
