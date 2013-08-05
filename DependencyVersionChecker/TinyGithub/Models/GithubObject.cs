using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyGithub.Models
{
    public class GithubObject
    {
        public string Sha { get; internal set; }

        public string Type { get; internal set; }

        public string Url { get; internal set; }
    }
}
