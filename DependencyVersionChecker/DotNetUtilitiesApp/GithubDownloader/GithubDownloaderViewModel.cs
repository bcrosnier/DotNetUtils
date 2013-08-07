using System.Windows.Input;
using DotNetUtilitiesApp.WpfUtils;

namespace DotNetUtilitiesApp.GithubDownloader
{
    internal class GithubDownloaderViewModel : ViewModel
    {
        #region Fields

        private string _loggedInUsername;
        private int _remainingApiCalls;
        private string _statusText;
        private string _repositoryUser;
        private string _repositoryName;
        private string _repositoryRefName;
        private string _personalApiAccessToken;
        private bool _rememberApiTokenChecked;
        private int _progressValue;
        private bool _isProgressIndeterminate;

        #endregion

        #region Properties

        public ICommand OpenSolutionCommand { get; private set; }

        #endregion

        #region Observed properties

        public string LoggedInUsername
        {
            get { return _loggedInUsername; }
            set
            {
                if( value != _loggedInUsername )
                {
                    _loggedInUsername = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int RemainingApiCalls
        {
            get { return _remainingApiCalls; }
            set
            {
                if( value != _remainingApiCalls )
                {
                    _remainingApiCalls = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if( value != _statusText )
                {
                    _statusText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string RepositoryUser
        {
            get { return _repositoryUser; }
            set
            {
                if( value != _repositoryUser )
                {
                    _repositoryUser = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("GithubRepositoryWebUrl");
                }
            }
        }

        public string RepositoryName
        {
            get { return _repositoryName; }
            set
            {
                if( value != _repositoryName )
                {
                    _repositoryName = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "GithubRepositoryWebUrl" );
                }
            }
        }

        public string RepositoryRefName
        {
            get { return _repositoryRefName; }
            set
            {
                if( value != _repositoryRefName )
                {
                    _repositoryRefName = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "GithubRepositoryWebUrl" );
                }
            }
        }

        public string PersonalApiAccessToken
        {
            get { return _personalApiAccessToken; }
            set
            {
                if( value != _personalApiAccessToken )
                {
                    _personalApiAccessToken = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool RememberApiTokenChecked
        {
            get { return _rememberApiTokenChecked; }
            set
            {
                if( value != _rememberApiTokenChecked )
                {
                    _rememberApiTokenChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if( value != _progressValue )
                {
                    _progressValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsProgressIndeterminate
        {
            get { return _isProgressIndeterminate; }
            set
            {
                if( value != _isProgressIndeterminate )
                {
                    _isProgressIndeterminate = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string GithubRepositoryWebUrl
        {
            get {
                return string.Format( "https://github.com/{0}/{1}/tree/{2}", RepositoryUser, RepositoryName, RepositoryRefName );
            }
        }

        #endregion

        #region Constructor
        internal GithubDownloaderViewModel()
        {
            PrepareCommands();
        }

        private void PrepareCommands()
        {
            OpenSolutionCommand = new RelayCommand( ExecuteOpenSolution, CanExecuteOpenSolution );
        }

        private bool CanExecuteOpenSolution( object obj )
        {
            return !string.IsNullOrEmpty( RepositoryUser ) && !string.IsNullOrEmpty( RepositoryName ) && !string.IsNullOrEmpty( RepositoryRefName );
        }

        private void ExecuteOpenSolution( object obj )
        {
            IsProgressIndeterminate = true;
            //throw new System.NotImplementedException();
        }
        #endregion

        #region Command handlers
        #endregion
    }
}