using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ProjectProber.Tests
{
    [TestFixture]
    [Category( "Online" )]
    public class GitHubTests
    {
        private string _githubApiToken;

        [Test]
        [Category( "LoginTokenRequired" )]
        public void GetSolutionFiles()
        {
            InitGithubApiToken();
            GithubRepositoryManager githubManager = new GithubRepositoryManager();
            githubManager.SetApiToken( _githubApiToken );

            string githubAuthor = "Invenietis";
            string githubRepoName = "ck-core";
            string githubRef = "master";
            string basePath = Environment.CurrentDirectory;

            githubManager.DownloadHeadRefTestFiles( githubAuthor, githubRepoName, githubRef, basePath );

            DirectoryInfo directory = new DirectoryInfo( Path.Combine( basePath, githubRepoName ) );

            Assert.That( directory.Exists, "Download directory exists: {0}", githubRepoName );

            IEnumerable<FileInfo> solutionFiles = directory.GetFiles( "*.sln" );

            Assert.That( solutionFiles.Count() == 1, "There is only one solution file downloaded in repository root" );

            SolutionCheckResult checkResult = SolutionChecker.CheckSolutionFile( solutionFiles.First().FullName );

            Assert.That( checkResult.Projects.Count() > 0 );
        }

        #region Login token init

        private void InitGithubApiToken()
        {
            bool needConfigFileInit = false;

            Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;

            _githubApiToken = settings["GitHubApiToken"] != null ? settings["GitHubApiToken"].Value : String.Empty;

            if( settings["GitHubApiToken"] == null )
            {
                settings.Add( "GitHubApiToken", "MyApiToken" );
                needConfigFileInit = true;
            }
            else if( string.IsNullOrEmpty( _githubApiToken ) || _githubApiToken == "MyApiToken" )
            {
                settings["GitHubApiToken"].Value = "MyApiToken";
                needConfigFileInit = true;
            }

            if( needConfigFileInit )
            {
                config.Save( ConfigurationSaveMode.Modified );
                Assert.Fail( "Configure your personal api access token in file: {0}", config.FilePath );
            }
        }

        #endregion
    }
}
