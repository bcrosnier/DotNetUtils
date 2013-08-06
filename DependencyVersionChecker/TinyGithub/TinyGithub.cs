using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using RestSharp;
using TinyGithub.Models;

namespace TinyGithub
{
    /// <summary>
    /// Small-scale GitHub API V3 implementation.
    /// </summary>
    public class TinyGithub
    {
        internal const string GITHUB_API_DOMAIN = @"https://api.github.com/";

        private RestClient _client;

        /// <summary>
        /// Creates a new instance of TinyGithub.
        /// </summary>
        public TinyGithub()
        {
            _client = new RestClient( GITHUB_API_DOMAIN );
            _client.UserAgent = "bcrosnier";
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

            IRestResponse<T> restResponse = _client.Execute<T>( request );

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
                error = new GithubError() { Message = restResponse.Content };
            }

            GithubResponse<T> response = new GithubResponse<T>( restResponse.StatusCode, content, rateLimit, rateLimitRemaining, rateLimitReset, error );

            return response;
        }

        /// <summary>
        /// Keeps only ythe path of a given URL, removing the API base.
        /// </summary>
        /// <param name="fullUrl">URL to trim</param>
        /// <returns>Resource path</returns>
        public static string TrimUrl( string fullUrl )
        {
            Debug.Assert( fullUrl.StartsWith( TinyGithub.GITHUB_API_DOMAIN, StringComparison.InvariantCultureIgnoreCase ), "Commit URL is a GitHub API URL" );

            return fullUrl.Substring( TinyGithub.GITHUB_API_DOMAIN.Length );
        }
    }
}