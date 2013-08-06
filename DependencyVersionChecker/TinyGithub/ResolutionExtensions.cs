using System.Collections.Generic;
using TinyGithub.Models;

namespace TinyGithub
{
    /// <summary>
    /// Extension utilities to request full objects from partial ones
    /// </summary>
    public static class ResolutionExtensions
    {
        /// <summary>
        /// Requests a complete GithubCommit object from the available Url.
        /// </summary>
        /// <param name="commit">GithubCommit to use</param>
        /// <param name="github">TinyGithub instance used for requests</param>
        /// <returns>Complete object</returns>
        public static GithubCommit Resolve( this GithubCommit commit, TinyGithub github )
        {
            return GetFromUrl<GithubCommit>( commit.Url, github );
        }

        /// <summary>
        /// Requests a complete GithubRef object from the available Url.
        /// </summary>
        /// <param name="githubRef">GithubRef to use</param>
        /// <param name="github">TinyGithub instance used for requests</param>
        /// <returns>Complete object</returns>
        public static GithubRef Resolve( this GithubRef githubRef, TinyGithub github )
        {
            return GetFromUrl<GithubRef>( githubRef.Url, github );
        }

        /// <summary>
        /// Requests a complete GithubTreeInfo object from the available Url.
        /// </summary>
        /// <param name="tree">GithubTreeInfo to use</param>
        /// <param name="github">TinyGithub instance used for requests</param>
        /// <returns>Complete object</returns>
        public static GithubTreeInfo Resolve( this GithubTreeInfo tree, TinyGithub github )
        {
            return tree.Resolve( github, false );
        }

        /// <summary>
        /// Requests a complete GithubTreeInfo object from the available Url, and whether to return the complete tree or just the root.
        /// </summary>
        /// <param name="tree">GithubTreeInfo to use</param>
        /// <param name="github">TinyGithub instance used for requests</param>
        /// <param name="recursive">Return the complete tree on true, or just the root on false</param>
        /// <returns>Complete object</returns>
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