using System;

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