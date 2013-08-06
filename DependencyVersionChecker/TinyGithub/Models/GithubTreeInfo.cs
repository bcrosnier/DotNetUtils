using System.Collections.Generic;

namespace TinyGithub.Models
{
    /// <summary>
    /// Github model used for trees.
    /// </summary>
    /// <example>
    /// https://api.github.com/repos/Invenietis/ck-core/git/trees/f78b036cb31ccf139583a0d830ddefa61cc9d493
    /// </example>
    public class GithubTreeInfo
    {
        /// <summary>
        /// Tree Sha object
        /// </summary>
        public string Sha { get; internal set; }

        /// <summary>
        /// Tree API URL
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Commit objects in the tree
        /// </summary>
        public List<GitCommitObject> Tree { get; internal set; }
    }
}