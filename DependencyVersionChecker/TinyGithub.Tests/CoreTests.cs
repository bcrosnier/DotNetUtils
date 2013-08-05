using System;
using System.Configuration;
using NUnit.Framework;
using TinyGithub.Models;

namespace TinyGithub.Tests
{
    [TestFixture]
    public class CoreTests
    {
        internal const string TEST_GITHUB_REF = @"repos/Invenietis/ck-core/git/refs/heads/master";
        private string _githubApiToken;

        [SetUp]
        public void Setup()
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
                Assert.Fail( "Configure the test api token in file: {0}", config.FilePath );
            }
        }

        [Test]
        public void AnonymousGetRef()
        {
            TinyGithub github = new TinyGithub();

            GithubResponse<GithubRef> r = github.GithubRequest<GithubRef>( TEST_GITHUB_REF );

            ValidateGithubResponse( r );

            Assert.That( r.Content, Is.InstanceOf( typeof( GithubRef ) ) );

            ValidateGithubRef( r.Content );

        }

        public static void ValidateGithubResponse<T>( GithubResponse<T> r )
        {
            Assert.That( r, Is.Not.Null );
            Assert.That( r.RateLimitRemaining <= r.RateLimit );
            Assert.That( r.Content, Is.Not.Null );
        }

        public static void ValidateGithubRef( GithubRef r )
        {
            Assert.That( r.Ref, Is.Not.Null.Or.Empty );
            Assert.That( r.Url, Is.Not.Null.Or.Empty );
            Assert.That( r.Object, Is.Not.Null );
            Assert.That( r.Object.Sha, Is.Not.Null.Or.Empty );
            Assert.That( r.Object.Type, Is.Not.Null.Or.Empty );
            Assert.That( r.Object.Type == "commit" );
            Assert.That( r.Object.Url, Is.Not.Null.Or.Empty );
        }

        public static void ValidateGithubUser( GithubUser u )
        {
            Assert.That( u.Id, Is.GreaterThan( 0 ) );
            Assert.That( u.Login, Is.Not.Null.Or.Empty );
            Assert.That( u.Name, Is.Not.Null.Or.Empty );
            Assert.That( u.Url, Is.Not.Null.Or.Empty );
        }

        public static void ValidateGitAuthorInfo( GitCommitAuthor a )
        {
            Assert.That( a.Name, Is.Not.Null.Or.Empty );
            Assert.That( a.Email, Is.Not.Null.Or.Empty );
            Assert.That( a.Date, Is.Not.Null.Or.Empty );
        }

        [Test]
        public void GithubApiTokenLogin()
        {
            TinyGithub github = new TinyGithub();

            github.SetApiToken( _githubApiToken );

            GithubResponse<GithubUser> r = github.GithubRequest<GithubUser>( "user" );

            ValidateGithubResponse( r );

            Assert.That( r.Content, Is.InstanceOf( typeof( GithubUser ) ) );

            ValidateGithubUser( r.Content );
        }

        [Test]
        public void LoginGetResolveRefs()
        {
            TinyGithub github = new TinyGithub();
            github.SetApiToken( _githubApiToken );

            GithubResponse<GithubRef> refResponse = github.GithubRequest<GithubRef>( TEST_GITHUB_REF );

            Assert.That( refResponse.Content.Object.Type == "commit" );

            string commitResource = TinyGithub.TrimUrl( refResponse.Content.Object.Url );

            GithubResponse<GithubCommit> commitResponse = github.GithubRequest<GithubCommit>( commitResource );

            Assert.That( commitResponse.RateLimitRemaining < refResponse.RateLimitRemaining );

            Assert.That( commitResponse.Content, Is.Not.Null );

            GithubCommit commit = commitResponse.Content;

            ValidateGithubCommit( commit );

            GithubTreeInfo tree = commit.Tree.Resolve(github, true);
        }

        public static void ValidateGithubCommit( GithubCommit c )
        {
            Assert.That( c.Sha, Is.Not.Null.Or.Empty );
            Assert.That( c.Url, Is.Not.Null.Or.Empty );
            Assert.That( c.HtmlUrl, Is.Not.Null.Or.Empty );

            Assert.That( c.Message, Is.Not.Null.Or.Empty );

            Assert.That( c.Author, Is.Not.Null );
            ValidateGitAuthorInfo( c.Author );

            Assert.That( c.Committer, Is.Not.Null );
            ValidateGitAuthorInfo( c.Committer );

            Assert.That( c.Tree, Is.Not.Null ); // TODO: Validation
            Assert.That( c.Parents, Is.Not.Null ); // TODO: Validation
        }
    }
}