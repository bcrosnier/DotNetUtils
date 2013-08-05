using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyGithub.Models
{
    public class GithubCommit
    {
        public string Sha { get; internal set; }
        public string Url { get; internal set; }
        public string HtmlUrl { get; internal set; }

        public GitCommitAuthor Author { get; internal set; }
        public GitCommitAuthor Committer { get; internal set; }

        public GithubTreeInfo Tree { get; internal set; }

        public string Message { get; internal set; }

        public List<GithubCommit> Parents { get; internal set; }
    }
}
