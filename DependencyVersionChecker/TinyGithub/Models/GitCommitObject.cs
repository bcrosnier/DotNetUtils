using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyGithub.Models
{
    public class GitCommitObject
    {
        public int Mode { get; internal set; }
        public string Type { get; internal set; }
        public string Sha { get; internal set; }
        public string Path { get; internal set; }
        public long Size { get; internal set; }
        public string Url { get; internal set; }
    }
}
