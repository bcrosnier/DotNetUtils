using System.Collections.Generic;

namespace TinyGithub.Models
{
    /// <summary>
    /// GitHub commit object model.
    /// </summary>
    public class GithubCommit
    {
        /// <summary>
        /// Sha of the commit object
        /// </summary>
        public string Sha { get; internal set; }

        /// <summary>
        /// API URL of this commit object
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Web URL of this commit on the Github website
        /// </summary>
        public string HtmlUrl { get; internal set; }

        /// <summary>
        /// Commit author information/date
        /// </summary>
        public GitCommitAuthor Author { get; internal set; }

        /// <summary>
        /// Committer information/date
        /// </summary>
        public GitCommitAuthor Committer { get; internal set; }

        /// <summary>
        /// Partial object/tree of this commit
        /// </summary>
        public GithubTreeInfo Tree { get; internal set; }

        /// <summary>
        /// Commit message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Parents of this commit
        /// </summary>
        public List<GithubCommit> Parents { get; internal set; }
    }
}