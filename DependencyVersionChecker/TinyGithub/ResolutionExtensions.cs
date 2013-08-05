using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyGithub.Models;

namespace TinyGithub
{
    public static class ResolutionExtensions
    {
        public static GithubCommit Resolve( this GithubCommit commit, TinyGithub github )
        {
            return GetFromUrl<GithubCommit>( commit.Url, github );
        }

        public static GithubRef Resolve( this GithubRef githubRef, TinyGithub github )
        {
            return GetFromUrl<GithubRef>( githubRef.Url, github );
        }

        public static GithubTreeInfo Resolve( this GithubTreeInfo tree, TinyGithub github)
        {
            return tree.Resolve( github, false );
        }

        public static GithubTreeInfo Resolve( this GithubTreeInfo tree, TinyGithub github, bool recursive )
        {
            if( recursive )
            {
                List<KeyValuePair<string, object>> additionalParameters = new List<KeyValuePair<string, object>>();
                var pair = new KeyValuePair<string, object>( "recursive", 1 );

                additionalParameters.Add( pair );

                return GetFromUrl<GithubTreeInfo>( tree.Url, github, additionalParameters );
            }
            else
            {
                return GetFromUrl<GithubTreeInfo>( tree.Url, github );
            }

        }

        private static T GetFromUrl<T>( string url, TinyGithub github ) where T : new()
        {
            return GetFromUrl<T>( url, github, null );
        }

        private static T GetFromUrl<T>( string url, TinyGithub github, IEnumerable<KeyValuePair<string, object>> parameters )
            where T : new()
        {
            string resource = TinyGithub.TrimUrl( url );

            GithubResponse<T> response = github.GithubRequest<T>( resource, parameters );

            return response.Content;
        }
    }
}
