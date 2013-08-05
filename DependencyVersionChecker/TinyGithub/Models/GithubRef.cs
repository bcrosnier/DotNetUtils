using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyGithub.Models
{
    public class GithubRef
    {
        public string Ref { get; internal set; }

        public string Url { get; internal set; }

        public GithubObject Object { get; internal set; }
    }
}
