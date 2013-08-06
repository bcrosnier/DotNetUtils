using System;

namespace TinyGithub.Models
{
    /// <summary>
    /// Git authoring information (name/email/date)
    /// </summary>
    public class GitCommitAuthor
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// E-mail address
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Commit date
        /// </summary>
        public DateTime Date { get; internal set; }
    }
}