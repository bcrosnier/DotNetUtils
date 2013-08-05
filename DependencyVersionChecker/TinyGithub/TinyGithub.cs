using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace TinyGithub
{
    public class TinyGithub
    {
        internal const string GITHUB_API_DOMAIN = @"https://api.github.com/";

        private RestClient _client;

        public TinyGithub()
        {
            _client = new RestClient( GITHUB_API_DOMAIN );
            _client.UserAgent = "bcrosnier";
            _client.AddDefaultHeader( "Accept", "application/vnd.github.v3" );
        }

        public void SetApiToken( string apiToken )
        {
            _client.Authenticator = new GithubApiTokenAuthenticator( apiToken );
        }

        public GithubResponse<T> GithubRequest<T>( string requestString )
            where T : new()
        {
            return Request<T>( requestString, null );
        }
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


            string rateLimitHeader = restResponse.Headers
                .Where( x => x.Name == "X-RateLimit-Limit" )
                .Select( x => x.Value ).FirstOrDefault().ToString();

            string rateLimitRemainingHeader = restResponse.Headers
                .Where( x => x.Name == "X-RateLimit-Remaining" )
                .Select( x => x.Value ).FirstOrDefault().ToString();

            string rateLimitResetHeader = restResponse.Headers
                .Where( x => x.Name == "X-RateLimit-Reset" )
                .Select( x => x.Value ).FirstOrDefault().ToString();

            int rateLimit = int.Parse( rateLimitHeader );
            int rateLimitRemaining = int.Parse( rateLimitRemainingHeader );
            long rateLimitReset = long.Parse( rateLimitResetHeader );

            T content = restResponse.Data;

            GithubResponse<T> response = new GithubResponse<T>( 200, content, rateLimit, rateLimitRemaining, rateLimitReset );

            return response;
        }

        public static string TrimUrl(string fullUrl )
        {
            Debug.Assert( fullUrl.StartsWith( TinyGithub.GITHUB_API_DOMAIN, StringComparison.InvariantCultureIgnoreCase ), "Commit URL is a GitHub API URL" );

            return fullUrl.Substring( TinyGithub.GITHUB_API_DOMAIN.Length );
        }
    }
}
