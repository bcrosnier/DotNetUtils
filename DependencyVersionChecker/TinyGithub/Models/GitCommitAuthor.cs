using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyGithub.Models
{
    public class GitCommitAuthor
    {
        public string Name { get; internal set; }
        public string Email { get; internal set; }
        public DateTime Date { get; internal set; }
    }
}
