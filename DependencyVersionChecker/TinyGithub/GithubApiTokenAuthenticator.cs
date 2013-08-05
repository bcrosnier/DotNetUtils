using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace TinyGithub
{
    class GithubApiTokenAuthenticator : IAuthenticator
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
