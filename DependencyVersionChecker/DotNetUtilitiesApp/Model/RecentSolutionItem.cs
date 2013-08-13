using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssemblyProber;

namespace DotNetUtilitiesApp
{
    public class RecentSolutionItem
    {
        private string _projectName;
        private string _slnPath;
        private string _ellipsedPath;

        public string ProjectName { get { return _projectName; } }
        public string SlnPath { get { return _slnPath; } }
        public string EllipsedPath { get { return _ellipsedPath; } }

        public RecentSolutionItem( string slnPath )
        {
            _projectName = Path.GetFileNameWithoutExtension( slnPath );
            _slnPath = slnPath;
            _ellipsedPath = StringUtils.MakeEllipsedPath( slnPath );
        }
    }
}
