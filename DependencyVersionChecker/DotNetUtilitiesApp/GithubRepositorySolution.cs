using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtilitiesApp
{
    /// <summary>
    /// Solution path, within a Github repository folder
    /// </summary>
    public class GithubRepositorySolution
    {
        /// <summary>
        /// Active solution path, relative to RepositoryDirectoryPath
        /// </summary>
        public string SolutionPath { get; internal set; }

        /// <summary>
        /// Available solution paths, relative to RepositoryDirectoryPath
        /// </summary>
        public IEnumerable<string> AvailableSolutions { get; internal set; }

        /// <summary>
        /// Repository directory path
        /// </summary>
        public string RepositoryDirectoryPath { get; internal set; }

        public override string ToString()
        {
            return SolutionPath;
        }
    }
}