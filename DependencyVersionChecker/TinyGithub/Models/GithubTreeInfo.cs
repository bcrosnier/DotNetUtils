using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyGithub.Models
{
    public class GithubTreeInfo
    {
        public string Sha { get; internal set; }
        public string Url { get; internal set; }

        public List<GitCommitObject> Tree { get; internal set; }
    }
}
