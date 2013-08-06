using System.Net;
using TinyGithub.Models;

namespace TinyGithub
{
    /// <summary>
    /// Github request response, containing data of a given type.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    public class GithubResponse<T>
    {
        /// <summary>
        /// Status code returned by Github
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Actual content of the response
        /// </summary>
        public T Content { get; private set; }

        /// <summary>
        /// Maximum rate limit
        /// </summary>
        public int RateLimit { get; private set; }

        /// <summary>
        /// Remaining requests before hitting limit
        /// </summary>
        public int RateLimitRemaining { get; private set; }

        /// <summary>
        /// Time at which the rate resets
        /// </summary>
        public long RateLimitReset { get; private set; }

        /// <summary>
        /// Error encountered; Should be null
        /// </summary>
        public GithubError Error { get; private set; }

        internal GithubResponse( HttpStatusCode statusCode, T content, int rateLimit, int rateLimitRemaining, long rateLimitReset, GithubError error )
        {
            StatusCode = statusCode;
            Content = content;
            RateLimit = rateLimit;
            RateLimitRemaining = rateLimitRemaining;
            RateLimitReset = rateLimitReset;
            Error = error;
        }
    }
}