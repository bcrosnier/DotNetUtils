namespace TinyGithub.Models
{
    /// <summary>
    /// Github object of variable type.
    /// </summary>
    public class GithubObject
    {
        /// <summary>
        /// Object Sha
        /// </summary>
        public string Sha { get; internal set; }

        /// <summary>
        /// Object type (ref/tag/blob/tree)
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Object URL
        /// </summary>
        public string Url { get; internal set; }
    }
}