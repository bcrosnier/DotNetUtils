using System;
using System.Collections.Generic;
using TinyGithub.Models;

namespace TinyGithub
{
    /// <summary>
    /// Extension utilities on the Github model
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the blob bytes from this blob info, using its encoding.
        /// </summary>
        /// <param name="blobInfo">Blob info to use</param>
        /// <returns>Blob data</returns>
        public static byte[] GetBlobData( this GitBlobInfo blobInfo )
        {
            if( blobInfo.Encoding == "base64" )
            {
                return Convert.FromBase64String( blobInfo.Content );
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a GithubRateLimitStatus fom this response.
        /// </summary>
        /// <typeparam name="T">GithubResponse type</typeparam>
        /// <param name="response">GithubResponse to use</param>
        /// <returns>new GithubRateLimitStatus</returns>
        public static GithubRateLimitStatus GetRateStatus<T>( this GithubResponse<T> response )
        {
            return new GithubRateLimitStatus() { Limit = response.RateLimit, Remaining = response.RateLimitRemaining, Reset = response.RateLimitReset };
        }

        #region Resolve extensions

        /// <summary>
        /// Requests a complete GithubCommit object from the available Url.
        /// </summary>
        /// <param name="commit">GithubCommit to use</param>
        /// <param name="github">Github instance used for requests</param>
        /// <returns>Complete object</returns>
        public static GithubResponse<GithubCommit> Resolve( this GithubCommit commit, Github github )
        {
            return GetFromUrl<GithubCommit>( commit.Url, github );
        }

        /// <summary>
        /// Requests a complete GithubRef object from the available Url.
        /// </summary>
        /// <param name="githubRef">GithubRef to use</param>
        /// <param name="github">Github instance used for requests</param>
        /// <returns>Complete object</returns>
        public static GithubResponse<GithubRef> Resolve( this GithubRef githubRef, Github github )
        {
            return GetFromUrl<GithubRef>( githubRef.Url, github );
        }

        /// <summary>
        /// Requests a complete GithubTreeInfo object from the available Url.
        /// </summary>
        /// <param name="tree">GithubTreeInfo to use</param>
        /// <param name="github">Github instance used for requests</param>
        /// <returns>Complete object</returns>
        public static GithubResponse<GithubTreeInfo> Resolve( this GithubTreeInfo tree, Github github )
        {
            return tree.Resolve( github, false );
        }

        /// <summary>
        /// Requests a complete GithubTreeInfo object from the available Url, and whether to return the complete tree or just the root.
        /// </summary>
        /// <param name="tree">GithubTreeInfo to use</param>
        /// <param name="github">Github instance used for requests</param>
        /// <param name="recursive">Return the complete tree on true, or just the root on false</param>
        /// <returns>Complete object</returns>
        public static GithubResponse<GithubTreeInfo> Resolve( this GithubTreeInfo tree, Github github, bool recursive )
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

        /// <summary>
        /// Tries to download this github object from its URL, given its type.
        /// </summary>
        /// <typeparam name="T">Type requested</typeparam>
        /// <param name="githubObject">Object with Url</param>
        /// <param name="github">Github instance to use</param>
        /// <returns></returns>
        public static GithubResponse<T> ResolveAs<T>( this GithubObject githubObject, Github github )
            where T : new()
        {
            return GetFromUrl<T>( githubObject.Url, github );
        }

        /// <summary>
        /// Tries to download this commit object from its URL, given its type.
        /// </summary>
        /// <typeparam name="T">Type requested</typeparam>
        /// <param name="commitObject">Object with Url</param>
        /// <param name="github">Github instance to use</param>
        /// <returns></returns>
        public static GithubResponse<T> ResolveAs<T>( this GitCommitObject commitObject, Github github )
            where T : new()
        {
            return GetFromUrl<T>( commitObject.Url, github );
        }

        private static GithubResponse<T> GetFromUrl<T>( string url, Github github ) where T : new()
        {
            return GetFromUrl<T>( url, github, null );
        }

        private static GithubResponse<T> GetFromUrl<T>( string url, Github github, IEnumerable<KeyValuePair<string, object>> parameters )
            where T : new()
        {
            string resource = Github.TrimUrl( url );

            GithubResponse<T> response = github.GithubRequest<T>( resource, parameters );

            return response;
        }

        #endregion Resolve extensions
    }
}