using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyVersionChecker
{
    public static class DependencyUtils
    {
        public static string MakeRelativePath( string sourcePath, string relativeFolder )
        {
            Uri source = new Uri( sourcePath );
            Uri folder = new Uri( relativeFolder );

            return folder.MakeRelativeUri( source ).ToString();
        }
    }
}