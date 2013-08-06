using RestSharp;

namespace TinyGithub
{
    /// <summary>
    /// Simple GitHub Personal API Access Token authenticator.
    /// Simply adds access_token parameter to all requests.
    /// </summary>
    internal class GithubApiTokenAuthenticator : IAuthenticator
    {
        private readonly string _apiToken;

        public GithubApiTokenAuthenticator( string apiToken )
        {
            _apiToken = apiToken;
        }

        public void Authenticate( IRestClient client, IRestRequest request )
        {
            request.AddParameter( "access_token", _apiToken );
        }
    }
}