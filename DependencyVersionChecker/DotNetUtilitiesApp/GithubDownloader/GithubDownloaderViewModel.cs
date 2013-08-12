using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using DotNetUtilitiesApp.Properties;
using DotNetUtilitiesApp.WpfUtils;
using TinyGithub;
using TinyGithub.Models;

namespace DotNetUtilitiesApp.GithubDownloader
{
    internal class GithubDownloaderViewModel : ViewModel
    {
        #region Fields

        public event EventHandler<StringEventArgs> RaisedWarning;

        public event EventHandler<StringEventArgs> SolutionPathAvailable;

        private readonly DirectoryInfo _cacheDirectory;
        private string _loggedInUsername;
        private string _remainingApiCalls;
        private string _statusText;
        private string _repositoryUser;
        private string _repositoryName;
        private string _repositoryRefName;
        private string _personalApiAccessToken;
        private bool _rememberApiTokenChecked;
        private int _progressValue;
        private bool _isProgressIndeterminate;
        private string _githubRepositoryUrl;
        private bool _urlChangeLocked;

        private Github _github;

        #endregion Fields

        #region Properties

        public ICommand OpenSolutionCommand { get; private set; }

        #endregion Properties

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

        public string RemainingApiCalls
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
                    UpdateGithubUrl();
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
                    UpdateGithubUrl();
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
                    UpdateGithubUrl();
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
                    _github.SetApiToken( value );
                    UpdateLoginName();
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
                    SaveSettings();
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
                    RaisePropertyChanged( "CanExecuteOpenSolution" );
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
                    RaisePropertyChanged( "CanExecuteOpenSolution" );
                }
            }
        }

        public string GithubRepositoryWebUrl
        {
            get
            {
                return _githubRepositoryUrl;
            }
            set
            {
                if( value != _githubRepositoryUrl )
                {
                    _githubRepositoryUrl = value;
                    RaisePropertyChanged();
                    _urlChangeLocked = true;
                    ParseGithubUrl( value );
                    _urlChangeLocked = false;
                }
            }
        }

        #endregion Observed properties

        #region Constructor/Disposition

        internal GithubDownloaderViewModel( DirectoryInfo cacheDirectory )
        {
            _github = new Github();

            _cacheDirectory = cacheDirectory;

            LoadSettings();

            PrepareCommands();

            UpdateRateLimit();
        }

        private void PrepareCommands()
        {
            OpenSolutionCommand = new RelayCommand( ExecuteOpenSolution, CanExecuteOpenSolution );
        }

        #endregion Constructor

        private void LoadSettings()
        {
            RepositoryUser = Settings.Default.RepositoryUser;
            RepositoryName = Settings.Default.RepositoryName;
            RepositoryRefName = Settings.Default.RepositoryRef;

            PersonalApiAccessToken = Settings.Default.PersonalApiToken;
        }

        private void SaveSettings()
        {
            Settings.Default.RepositoryUser = RepositoryUser;
            Settings.Default.RepositoryName = RepositoryName;
            Settings.Default.RepositoryRef = RepositoryRefName;

            Settings.Default.Save();
        }

        #region Private methods

        private void UpdateRateLimit<T>( GithubResponse<T> response )
        {
            RemainingApiCalls = response.RateLimitRemaining.ToString();
        }

        async private void UpdateRateLimit()
        {
            IsProgressIndeterminate = true;
            RemainingApiCalls = "...";
            RemainingApiCalls = await UpdateRateLimitAsync();
            IsProgressIndeterminate = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private Task<string> UpdateRateLimitAsync()
        {
            Task<string> updateRateTask = new Task<string>( () =>
            {
                GithubResponse<object> response = _github.GithubRequest<object>( "rate_limit" );
                return response.RateLimitRemaining.ToString();
            } );

            updateRateTask.Start();
            return updateRateTask;
        }

        async private void UpdateLoginName()
        {
            if( string.IsNullOrEmpty( PersonalApiAccessToken ) )
                return;

            IsProgressIndeterminate = true;
            LoggedInUsername = "...";
            LoggedInUsername = await UpdateLoginNameAsync();
            IsProgressIndeterminate = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private Task<string> UpdateLoginNameAsync()
        {
            Task<string> updateRateTask = new Task<string>( () =>
            {
                GithubResponse<GithubUser> response = _github.GithubRequest<GithubUser>( "user" );
                UpdateRateLimit( response );
                if( response.StatusCode != System.Net.HttpStatusCode.OK )
                {
                    RaiseWarning( String.Format( "API error: [{1} {0}] {2}", response.StatusCode.ToString(), ((int)response.StatusCode).ToString(), response.Error.Message ) );
                    return response.StatusCode.ToString();
                }
                else
                {
                    return response.Content.Login;
                }
            } );

            updateRateTask.Start();
            return updateRateTask;
        }

        private void RaiseWarning( string message )
        {
            if( RaisedWarning != null )
            {
                StringEventArgs args = new StringEventArgs( message );
                Invoke.OnAppThread( () =>
                {
                    RaisedWarning( this, args );
                } );
            }
        }

        private void RaiseSlnPathAvailable( string slnPath )
        {
            if( SolutionPathAvailable != null )
            {
                StringEventArgs args = new StringEventArgs( slnPath );
                Invoke.OnAppThread( () =>
                {
                    SolutionPathAvailable( this, args );
                } );
            }
        }

        private static string GetGitBlobSha( byte[] data )
        {
            using( SHA1Managed sha1 = new SHA1Managed() )
            {
                string header = "blob " + data.LongLength.ToString() + "\0";

                byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes( header );

                byte[] finalHash = headerBytes.Concat( data ).ToArray();

                byte[] hash = sha1.ComputeHash( finalHash );
                StringBuilder output = new StringBuilder( 2 * hash.Length );
                foreach( byte b in hash )
                {
                    output.AppendFormat( "{0:x2}", b );
                }

                return output.ToString();
            }
        }

        private static string GetBlobSha( FileInfo file )
        {
            byte[] fileBytes = File.ReadAllBytes( file.FullName );

            return GetGitBlobSha( fileBytes );
        }

        private void ParseGithubUrl( string url )
        {
            string pattern = @"^(?:https?://)?(?:www\.)?github\.com/([a-zA-Z0-9-_.]*)/([a-zA-Z0-9-_.]*)(?:/tree/([a-zA-Z0-9-_.]+))?";

            Match m = Regex.Match( url, pattern );
            if( m.Success )
            {
                RepositoryUser = m.Groups[1].Value;
                RepositoryName = m.Groups[2].Value;

                if( m.Groups[3].Success )
                {
                    RepositoryRefName = m.Groups[3].Value;
                }
                else
                {
                    RepositoryRefName = String.Empty;
                }
            }
        }

        private void UpdateGithubUrl()
        {
            if( _urlChangeLocked )
                return;
            if( string.IsNullOrEmpty( RepositoryUser ) || string.IsNullOrEmpty( RepositoryName ) )
            {
                _githubRepositoryUrl = String.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.Append( string.Format( "https://github.com/{0}/{1}", RepositoryUser, RepositoryName ) );

                if( !string.IsNullOrEmpty( RepositoryRefName ) && RepositoryRefName != "master" )
                {
                    sb.Append( string.Format( "/tree/{0}", RepositoryRefName ) );
                }

                _githubRepositoryUrl = sb.ToString();
            }

            RaisePropertyChanged( "GithubRepositoryWebUrl" );
        }

        #endregion Private methods

        #region Command handlers

        private bool CanExecuteOpenSolution( object obj )
        {
            return !IsProgressIndeterminate && (ProgressValue == 0 || ProgressValue == 100) && !string.IsNullOrEmpty( RepositoryUser ) && !string.IsNullOrEmpty( RepositoryName );
        }

        async private void ExecuteOpenSolution( object obj )
        {
            IsProgressIndeterminate = true;
            SaveSettings();

            if( String.IsNullOrEmpty( RepositoryRefName ) )
            {
                RepositoryRefName = "master";
            }

            IEnumerable<string> slnPaths = await DoDownloadOpenSolution();

            IsProgressIndeterminate = false;
            ProgressValue = 100;

            if( slnPaths != null )
            {
                if( slnPaths.Count() > 1 )
                {
                    Dictionary<string,string> selections = new Dictionary<string, string>();

                    var MatchingChars =
                        from len in Enumerable.Range( 0, slnPaths.Min( s => s.Length ) ).Reverse()
                        let possibleMatch = slnPaths.First().Substring( 0, len )
                        where slnPaths.All( f => f.StartsWith( possibleMatch ) )
                        select possibleMatch;

                    string LongestDir = Path.GetDirectoryName( MatchingChars.First() );

                    foreach( string longName in slnPaths )
                    {
                        string longPath = Path.GetFullPath( longName );

                        string shortName = longPath.Substring( LongestDir.Length );
                        selections.Add( longPath, shortName );
                    }

                    ChoiceWindowResult<string> result = ChoiceWindow.ShowSelectWindow<string>( "Select solution", "More than one solution was found in this repository.\nPlease choose a solution file:", selections.Values );

                    if( result.Result == System.Windows.MessageBoxResult.OK && result.Selected != null )
                    {
                        string selectedPath = selections.Where( x => x.Value == result.Selected ).Select( x => x.Key ).First();
                        RaiseSlnPathAvailable( selectedPath );
                    }
                }
                else if( slnPaths.Count() == 1 )
                {
                    RaiseSlnPathAvailable( slnPaths.First() );
                }
            }
            else
            {
                ProgressValue = 0;
            }
        }

        #endregion Command handlers

        #region Github download task

        private Task<IEnumerable<string>> DoDownloadOpenSolution()
        {
            Task<IEnumerable<string>> task = new Task<IEnumerable<string>>( DoDownloadOpenSolutionTask );
            task.Start();
            return task;
        }

        private IEnumerable<string> DoDownloadOpenSolutionTask()
        {
            string baseDirectory = _cacheDirectory.FullName;
            string directoryPath = Path.Combine( baseDirectory, RepositoryUser, RepositoryName, RepositoryRefName );
            DirectoryInfo directory = new DirectoryInfo( directoryPath );

            string resource = String.Format( "repos/{0}/{1}/git/refs/heads/{2}", RepositoryUser, RepositoryName, RepositoryRefName );

            Invoke.OnAppThread( () =>
            {
                StatusText = "Getting ref...";
            } );

            // Get ref
            GithubResponse<GithubRef> response = _github.GithubRequest<GithubRef>( resource );
            Invoke.OnAppThread( () =>
            {
                UpdateRateLimit( response );
            } );
            if( response.StatusCode != System.Net.HttpStatusCode.OK )
            {
                RaiseWarning( String.Format( "API error: [{1} {0}] {2}", response.StatusCode.ToString(), ((int)response.StatusCode).ToString(), response.Error.Message ) );
                return null;
            }
            GithubRef refObject = response.Content;

            // Get ref's commit
            Debug.Assert( refObject.Object.Type == "commit" );

            Invoke.OnAppThread( () =>
            {
                StatusText = "Getting commit...";
            } );

            GithubResponse<GithubCommit> commitResponse = refObject.Object.ResolveAs<GithubCommit>( _github );
            Invoke.OnAppThread( () =>
            {
                UpdateRateLimit( commitResponse );
            } );
            if( response.StatusCode != System.Net.HttpStatusCode.OK )
            {
                RaiseWarning( String.Format( "API error: [{1} {0}] {2}", response.StatusCode.ToString(), ((int)response.StatusCode).ToString(), response.Error.Message ) );
                return null;
            }
            GithubCommit headCommit = commitResponse.Content;

            // Get commit's tree
            Invoke.OnAppThread( () =>
            {
                StatusText = "Getting tree...";
            } );

            GithubResponse<GithubTreeInfo> treeResponse = headCommit.Tree.Resolve( _github, true );
            Invoke.OnAppThread( () =>
            {
                UpdateRateLimit( treeResponse );
            } );
            if( response.StatusCode != System.Net.HttpStatusCode.OK )
            {
                RaiseWarning( String.Format( "API error: [{1} {0}] {2}", response.StatusCode.ToString(), ((int)response.StatusCode).ToString(), response.Error.Message ) );
                return null;
            }
            GithubTreeInfo treeInfo = treeResponse.Content;

            // Get select tree files

            IEnumerable<GitCommitObject> fileObjects = treeInfo.Tree.Where( x => x.Type == "blob" );

            List<GitCommitObject> objectsToGet = new List<GitCommitObject>();

            // Get solution files from commit tree:
            // Solution files
            objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( ".sln", StringComparison.InvariantCultureIgnoreCase ) ) );
            // AssemblyInfo files (also includes SharedAssemblyInfo)
            objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( "AssemblyInfo.cs", StringComparison.InvariantCultureIgnoreCase ) ) );
            // C# project files
            objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( ".csproj", StringComparison.InvariantCultureIgnoreCase ) ) );
            // NuGet packageRef configuration files
            objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( "packages.config", StringComparison.InvariantCultureIgnoreCase ) ) );

            if( !objectsToGet.Any( x => x.Path.EndsWith( ".sln", StringComparison.InvariantCultureIgnoreCase ) ) )
            {
                RaiseWarning( "No solution file found in this repository." );
                return null;
            }

            Invoke.OnAppThread( () =>
            {
                IsProgressIndeterminate = false;
                ProgressValue = 0;
            } );

            int objectCount = objectsToGet.Count;
            int i = 0;

            if( !directory.Exists )
            {
                directory.Create();
            }

            List<FileInfo> downloadedFiles = new List<FileInfo>();
            List<FileInfo> filesInCache = new DirectoryInfo( directoryPath ).GetFiles( "*", SearchOption.AllDirectories ).ToList();

            foreach( var objectToGet in objectsToGet )
            {
                i++;
                Invoke.OnAppThread( () =>
                {
                    ProgressValue = (int)(((double)i) / objectCount * 100.0);
                    StatusText = String.Format( "Downloading: {0}", objectToGet.Path );
                } );

                string destPath = Path.Combine( directoryPath, objectToGet.Path );

                FileInfo destFile = new FileInfo( Path.GetFullPath( destPath ) );

                if( filesInCache.Any( x => Path.GetFullPath( x.FullName ) == Path.GetFullPath( destFile.FullName ) ) )
                {
                    filesInCache.Remove( filesInCache.Where( x => Path.GetFullPath( x.FullName ) == Path.GetFullPath( destFile.FullName ) ).First() );
                }

                downloadedFiles.Add( destFile );

                if( !destFile.Exists || objectToGet.Size != destFile.Length || objectToGet.Sha != GetBlobSha( destFile ) )
                {
                    GithubResponse<GitBlobInfo> blobInfoResponse = objectToGet.ResolveAs<GitBlobInfo>( _github );
                    GitBlobInfo blobInfo = blobInfoResponse.Content;
                    Invoke.OnAppThread( () =>
                    {
                        UpdateRateLimit( blobInfoResponse );
                    } );
                    if( response.StatusCode != System.Net.HttpStatusCode.OK )
                    {
                        RaiseWarning( String.Format( "API error: [{1} {0}] {2}", response.StatusCode.ToString(), ((int)response.StatusCode).ToString(), response.Error.Message ) );
                        return null;
                    }

                    string sourceUrl = objectToGet.Url;

                    _github.DownloadBlobInfo( blobInfo, destPath );
                }
            }

            return objectsToGet.Where( x => x.Path.EndsWith( ".sln", StringComparison.InvariantCultureIgnoreCase ) ).Select( x => Path.Combine( directoryPath, x.Path ) );
        }

        #endregion Github download task
    }
}