namespace TinyGithub.Models
{
    /// <summary>
    /// GitHub ref model
    /// </summary>
    /// <example>https://api.github.com/repos/Invenietis/ck-core/git/refs/heads/master</example>
    public class GithubRef
    {
        /// <summary>
        /// Full ref name
        /// </summary>
        public string Ref { get; internal set; }

        /// <summary>
        /// API URL of this Ref object
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Object referenced by the ref
        /// </summary>
        public GithubObject Object { get; internal set; }
    }
}