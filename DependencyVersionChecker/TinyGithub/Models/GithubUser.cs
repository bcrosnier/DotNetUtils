using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyGithub.Models
{
    public class GithubUser
    {
        public string Login { get; internal set; }
        public long Id { get; internal set; }
        public string Url { get; internal set; }
        public string Name { get; internal set; }
    }
}
