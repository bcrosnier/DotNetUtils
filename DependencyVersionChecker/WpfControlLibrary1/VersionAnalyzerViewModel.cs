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
using System.Windows;
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
        private ObservableCollection<UIElement> _detailItems;

        public ICommand ViewDetailCommand { get; private set; }

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
            
        public ObservableCollection<AssemblyVersionError> AssemblyVersionErrors
        {
            get { return _assemblyVersionErrors; }
        }

        public ObservableCollection<UIElement> DetailItems
        {
            get { return _detailItems; }
        }

        #endregion Observable properties

        #region Constructor

        public VersionAnalyzerViewModel()
        {
            _assemblyVersionErrors = new ObservableCollection<AssemblyVersionError>();
            _detailItems = new ObservableCollection<UIElement>();

            CurrentVersion = "0.0.0";

            ViewDetailCommand = new RelayCommand(ExecuteViewModel);
        }

        private void ExecuteViewModel(object obj)
        {
            _detailItems.Clear();

            AssemblyVersionError e = obj as AssemblyVersionError;

            _detailItems.Add(e.CreateDetailControl());
        }

        #endregion Constructor

        #region Public methods

        public void LoadFromSolution(string slnPath)
        {
            CleanUp();

            ActiveSolutionPath = slnPath;
            _result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(slnPath);
            ShowResultWarnings();

            CurrentVersion = GetResultVersion(_result);
        }

        public void CleanUp()
        {
            ActiveSolutionPath = string.Empty;
            CurrentVersion = string.Empty;
            DetailItems.Clear();
            AssemblyVersionErrors.Clear();
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
