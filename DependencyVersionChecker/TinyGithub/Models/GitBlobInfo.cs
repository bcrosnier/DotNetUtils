using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyGithub.Models
{
    /// <summary>
    /// Github blob information. From model.
    /// </summary>
    public class GitBlobInfo
    {
        /// <summary>
        /// Object Sha
        /// </summary>
        public string Sha { get; internal set; }

        /// <summary>
        /// Object size
        /// </summary>
        public int Size { get; internal set; }

        /// <summary>
        /// Object Url
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Object encoded content
        /// </summary>
        public string Content { get; internal set; }

        /// <summary>
        /// Object encoding
        /// </summary>
        public string Encoding { get; internal set; }
    }
}
