using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using RestSharp;
using TinyGithub.Models;

namespace TinyGithub
{
    /// <summary>
    /// Small-scale GitHub API V3 implementation.
    /// </summary>
    public class Github
    {
        internal const string GITHUB_API_DOMAIN = @"https://api.github.com/";

        private RestClient _client;
        private WebClient _webClient;

        /// <summary>
        /// Creates a new instance of Github.
        /// </summary>
        public Github()
        {
            _webClient = new WebClient();
            _webClient.Headers.Add( "Accept", "application/vnd.github.v3" );
            _webClient.Headers.Add( "User-Agent", "TinyGithub API [DEVELOPMENT]" );

            _client = new RestClient( GITHUB_API_DOMAIN );
            _client.UserAgent = "TinyGithub API [DEVELOPMENT]";
            _client.AddDefaultHeader( "Accept", "application/vnd.github.v3" );
        }

        /// <summary>
        /// Sets a personal API access token used in all new GitHub requests with this instance.
        /// All new requests will be authenticated with the user owning the token.
        /// </summary>
        /// <remarks>
        /// To create a Personal API Access Tokens, visit https://github.com/settings/applications .
        /// </remarks>
        /// <param name="apiToken">Personal API Access Token to use</param>
        public void SetApiToken( string apiToken )
        {
            _client.Authenticator = new GithubApiTokenAuthenticator( apiToken );

            _webClient.QueryString.Clear();
            _webClient.QueryString.Add( "access_token", apiToken );
        }

        /// <summary>
        /// Executes a new request on the GitHub API, returning the given type.
        /// </summary>
        /// <typeparam name="T">GitHub response type to use.</typeparam>
        /// <param name="requestString">Requested resource path</param>
        /// <returns>GithubResponse object, containing the requested type.</returns>
        /// <example>
        /// GithubResponse&lt;GithubCommit&gt; myCommitObject =
        ///     tinyGithub.GithubRequest&lt;GithubCommit&gt;("repos/Invenietis/ck-core/git/commits/44cb944bb9fde224ac8b67d03c868ca3c7cbf6e3");
        /// </example>
        public GithubResponse<T> GithubRequest<T>( string requestString )
            where T : new()
        {
            return Request<T>( requestString, null );
        }

        /// <summary>
        /// Executes a new request on the GitHub API with the given parameters, returning the given type.
        /// </summary>
        /// <typeparam name="T">GitHub response type to use.</typeparam>
        /// <param name="requestString">Requested resource path</param>
        /// <param name="parameters">Parameter pairs (name/value) to use</param>
        /// <returns>GithubResponse object, containing the requested type.</returns>
        /// <example>
        /// GithubResponse&lt;GithubTreeInfo&gt; myCommitObject =
        ///     tinyGithub.GithubRequest&lt;GithubTreeInfo&gt;(
        ///         "repos/Invenietis/ck-core/git/trees/f78b036cb31ccf139583a0d830ddefa61cc9d493",
        ///         parameters
        ///     );
        /// </example>
        public GithubResponse<T> GithubRequest<T>( string requestString, IEnumerable<KeyValuePair<string, object>> parameters )
            where T : new()
        {
            return Request<T>( requestString, parameters );
        }

        private GithubResponse<T> Request<T>( string requestString, IEnumerable<KeyValuePair<string, object>> parameters )
            where T : new()
        {
            var request = new RestRequest();
            request.Resource = requestString;

            if( parameters != null )
            {
                foreach( var pair in parameters )
                {
                    request.AddParameter( pair.Key, pair.Value );
                }
            }

            IRestResponse<T> restResponse;
            try
            {
                restResponse = _client.Execute<T>( request );
            }
            catch( Exception ex )
            {
                GithubError netError = new GithubError() { Message = ex.Message };
                return new GithubResponse<T>( HttpStatusCode.ServiceUnavailable, default(T), 0, 0, 0, netError );
            }

            string rateLimitHeader = restResponse.GetHeader( "X-RateLimit-Limit" );
            string rateLimitRemainingHeader = restResponse.GetHeader( "X-RateLimit-Remaining" );
            string rateLimitResetHeader = restResponse.GetHeader( "X-RateLimit-Reset" );

            int rateLimit = rateLimitHeader != null ? int.Parse( rateLimitHeader ) : 0;
            int rateLimitRemaining = rateLimitRemainingHeader != null ? int.Parse( rateLimitRemainingHeader ) : 0;
            long rateLimitReset = rateLimitResetHeader != null ? long.Parse( rateLimitResetHeader ) : 0;

            GithubError error = null;
            T content = restResponse.Data;

            if( restResponse.StatusCode != HttpStatusCode.OK )
            {
                var js = new RestSharp.Deserializers.JsonDeserializer();

                try
                {
                    error = js.Deserialize<GithubError>( restResponse );
                }
                catch( Exception ex )
                {
                    error = new GithubError() { Message = String.Format("Failed to parse: {0}", ex.Message ) };
                }
            }

            GithubResponse<T> response = new GithubResponse<T>( restResponse.StatusCode, content, rateLimit, rateLimitRemaining, rateLimitReset, error );

            return response;
        }

        /// <summary>
        /// Saves blob data to a file, creating any directory it should be in
        /// </summary>
        /// <param name="blobInfo">Blob data to use</param>
        /// <param name="targetFilePath">Filename to write in</param>
        public void DownloadBlobInfo( GitBlobInfo blobInfo, string targetFilePath )
        {
            DirectoryInfo directory = new DirectoryInfo( Path.GetDirectoryName( targetFilePath ) );
            if( !directory.Exists )
                directory.Create();

            using( Stream writeStream = File.Open( targetFilePath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
            {
                byte[] data = blobInfo.GetBlobData();
                writeStream.Write( data, 0, data.Length );
            }
        }

        /// <summary>
        /// Keeps only the path of a given URL, removing the API base.
        /// </summary>
        /// <param name="fullUrl">URL to trim</param>
        /// <returns>Resource path</returns>
        public static string TrimUrl( string fullUrl )
        {
            Debug.Assert( fullUrl.StartsWith( Github.GITHUB_API_DOMAIN, StringComparison.InvariantCultureIgnoreCase ), "Commit URL is a GitHub API URL" );

            return fullUrl.Substring( Github.GITHUB_API_DOMAIN.Length );
        }
    }
}