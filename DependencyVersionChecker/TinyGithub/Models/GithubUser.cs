namespace TinyGithub.Models
{
    /// <summary>
    /// GitHub model used for users.
    /// </summary>
    /// <example>https://api.github.com/users/bcrosnier</example>
    public class GithubUser
    {
        /// <summary>
        /// Username
        /// </summary>
        public string Login { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long Id { get; internal set; }

        /// <summary>
        /// User profile API URL
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// User full name
        /// </summary>
        public string Name { get; internal set; }
    }
}