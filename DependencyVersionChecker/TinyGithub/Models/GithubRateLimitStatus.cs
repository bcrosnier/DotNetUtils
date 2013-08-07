using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyGithub.Models
{
    /// <summary>
    /// Github rate limit status.
    /// </summary>
    /// <remarks>
    /// See: http://developer.github.com/v3/#rate-limiting
    /// </remarks>
    public class GithubRateLimitStatus
    {
        /// <summary>
        /// Maximum number of requests that the consumer is permitted to make per hour.
        /// </summary>
        public int Limit { get; internal set; }

        /// <summary>
        /// Number of requests remaining in the current rate limit window.
        /// </summary>
        public int Remaining { get; internal set; }

        /// <summary>
        /// The time at which the current rate limit window resets in UTC epoch seconds.
        /// </summary>
        public long Reset { get; internal set; }
    }
}
