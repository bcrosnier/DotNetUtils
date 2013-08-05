using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TinyGithub
{
    public class GithubResponse<T>
    {
        public int StatusCode { get; private set; }
        public T Content { get; private set; }
        public int RateLimit { get; private set; }
        public int RateLimitRemaining { get; private set; }
        public long RateLimitReset { get; private set; }


        internal GithubResponse( int statusCode, T content, int rateLimit, int rateLimitRemaining, long rateLimitReset )
        {
            StatusCode = statusCode;
            Content = content;
            RateLimit = rateLimit;
            RateLimitRemaining = rateLimitRemaining;
            RateLimitReset = rateLimitReset;
        }
    }
}
