namespace TinyGithub.Models
{
    /// <summary>
    /// Git object contained in a commit (ie. a blob/file or another tree)
    /// </summary>
    public class GitCommitObject : GithubObject
    {
        /// <summary>
        /// Mode and permissions of the file (blob) or directory (tree)
        /// </summary>
        public int Mode { get; internal set; }

        /// <summary>
        /// Path of the commit object
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// File size, for blobs
        /// </summary>
        public long Size { get; internal set; }
    }
}