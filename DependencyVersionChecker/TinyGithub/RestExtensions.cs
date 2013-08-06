using System.Linq;
using RestSharp;

namespace TinyGithub
{
    /// <summary>
    /// Utility extensions for RestSharp objects
    /// </summary>
    public static class RestExtensions
    {
        /// <summary>
        /// Get a named header from this IRestResponse.
        /// </summary>
        /// <param name="restResponse">IRestResponse to use</param>
        /// <param name="headerName">Target header name</param>
        /// <returns>Item value, or null if not found.</returns>
        public static string GetHeader( this IRestResponse restResponse, string headerName )
        {
            var header = restResponse.Headers
                .Where( x => x.Name == headerName )
                .Select( x => x.Value ).FirstOrDefault();

            if( header != null )
                return header.ToString();

            return null;
        }
    }
}