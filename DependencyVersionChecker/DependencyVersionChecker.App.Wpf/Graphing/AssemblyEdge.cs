using System.Diagnostics;
using QuickGraph;

namespace DependencyVersionCheckerApp.Wpf.Graphing
{
    [DebuggerDisplay( "{Source.Assembly} -> {Target.Assembly}" )]
    public class AssemblyEdge : Edge<AssemblyVertex>
    {
        public int ID
        {
            get;
            private set;
        }

        public AssemblyEdge( int id, AssemblyVertex source, AssemblyVertex target )
            : base( source, target )
        {
            ID = id;
        }
    }
}