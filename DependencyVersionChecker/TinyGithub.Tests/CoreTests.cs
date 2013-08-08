using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CK.Core;
using NUnit.Framework;
using TinyGithub.Models;

namespace TinyGithub.Tests
{
    [TestFixture]
    [Category( "Online" )]
    public class CoreTests
    {
        internal const string TEST_GITHUB_REF = @"repos/Invenietis/ck-core/git/refs/heads/master";
        private string _githubApiToken;

        [Test]
        public void AnonymousGetRef()
        {
            Github github = new Github();

            GithubResponse<GithubRef> r = github.GithubRequest<GithubRef>( TEST_GITHUB_REF );

            ValidateGithubResponse( r );

            Assert.That( r.Content, Is.InstanceOf( typeof( GithubRef ) ) );

            ValidateGithubRef( r.Content );
        }

        [Test]
        [Category( "LoginTokenRequired" )]
        public void GithubApiTokenLogin()
        {
            InitGithubApiToken();

            Github github = new Github();

            github.SetApiToken( _githubApiToken );

            GithubResponse<GithubUser> r = github.GithubRequest<GithubUser>( "user" );
            ValidateGithubResponse( r );

            Assert.That( r.Content, Is.InstanceOf( typeof( GithubUser ) ) );
            ValidateGithubUser( r.Content );
        }

        [Test]
        [Category( "LoginTokenRequired" )]
        public void LoginDownloadTest()
        {
            InitGithubApiToken();

            Github github = new Github();
            github.SetApiToken( _githubApiToken );

            GithubResponse<GithubRef> refResponse = github.GithubRequest<GithubRef>( TEST_GITHUB_REF );
            ValidateGithubResponse( refResponse );

            Assert.That( refResponse.Content.Object.Type == "commit" );

            string commitResource = Github.TrimUrl( refResponse.Content.Object.Url );

            GithubResponse<GithubCommit> commitResponse = github.GithubRequest<GithubCommit>( commitResource );
            ValidateGithubResponse( commitResponse );
            Assert.That( commitResponse.RateLimitRemaining < refResponse.RateLimitRemaining );

            GithubCommit commit = commitResponse.Content;
            ValidateGithubCommit( commit );

            GithubTreeInfo tree = commit.Tree.Resolve( github, true ).Content;
            ValidateGithubTree( tree );

            List<GitCommitObject> blobObjects = tree.Tree.Where( x => x.Type == "blob" ).ToList();

            CollectionAssert.IsNotEmpty( blobObjects );

            // Test file sha and download
            GithubResponse<GitBlobInfo> blobInfoResponse = blobObjects.First().ResolveAs<GitBlobInfo>( github );
            ValidateGithubResponse( blobInfoResponse );

            GitBlobInfo blobInfo = blobInfoResponse.Content;
            ValidateBlobInfo( blobInfo );
        }

        private static void ValidateBlobInfo( GitBlobInfo blobInfo )
        {
            ValidateSha( blobInfo.Sha );
            ValidateApiUrl( blobInfo.Url );
            Assert.That( blobInfo.Encoding, Is.EqualTo( "base64" ) );
            Assert.That( blobInfo.Content, Is.Not.Null.Or.Empty );
            Assert.That( blobInfo.Size, Is.Positive );

            byte[] data = blobInfo.GetBlobData();
            string dataSha = GetGitBlobSha( data );

            ValidateSha( dataSha );

            Assert.That( blobInfo.Size == data.LongLength );
            Assert.That( blobInfo.Sha == dataSha );
        }

        #region Object validation

        private static void ValidateSha( string sha )
        {
            Assert.That( sha, Is.Not.Null.Or.Empty.And.Matches( "^[a-f0-9]{40}$" ) );
        }

        private static void ValidateApiUrl( string url )
        {
            Assert.That( url, Is.Not.Null.Or.Empty.And.Matches( "^https://api.github.com/" ) );
        }

        private static void ValidateGithubTree( GithubTreeInfo tree )
        {
            ValidateSha( tree.Sha );
            ValidateApiUrl( tree.Url );

            Assert.That( tree.Tree, Is.Not.Null );
            CollectionAssert.AllItemsAreInstancesOfType( tree.Tree, typeof( GitCommitObject ) );
            CollectionAssert.AllItemsAreNotNull( tree.Tree );
            CollectionAssert.AllItemsAreUnique( tree.Tree );

            foreach( GitCommitObject obj in tree.Tree )
            {
                ValidateSha( obj.Sha );
                ValidateApiUrl( obj.Url );
                Assert.That( obj.Mode, Is.Positive );
                Assert.That( obj.Path, Is.Not.Null.Or.Empty );
                Assert.That( obj.Type, Is.Not.Null.Or.Empty );
                Assert.That( obj.Type, Is.EqualTo( "tree" ).Or.EqualTo( "blob" ) );

                if( obj.Type == "blob" )
                {
                    Assert.That( obj.Size, Is.Positive );
                }
            }
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

        #endregion Object validation

        #region Utilities

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

        static byte[] GetBytes( string str )
        {
            byte[] bytes = new byte[str.Length * sizeof( char )];
            Buffer.BlockCopy( str.ToCharArray(), 0, bytes, 0, bytes.Length );
            return bytes;
        }

        #endregion

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

        #endregion Login token init
    }
}