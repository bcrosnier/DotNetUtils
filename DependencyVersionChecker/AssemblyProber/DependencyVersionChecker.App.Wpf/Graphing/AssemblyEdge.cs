using AssemblyProber;
using QuickGraph;
using System.Diagnostics;

namespace DotNetUtilitiesApp.AssemblyProber.Graphing
{
    [DebuggerDisplay("{Source.Assembly} -> {Target.Assembly}")]
    public class AssemblyEdge : Edge<AssemblyVertex>
    {
        public IAssemblyInfo Parent { get; private set; }

        public IAssemblyInfo Child { get; private set; }

        public string Description
        {
            get
            {
                return string.Format("{0} depends on {1}", Parent.SimpleName, Child.SimpleName);
            }
        }

        public AssemblyEdge(AssemblyVertex source, AssemblyVertex target)
            : base(source, target)
        {
            Child = target.Assembly;
            Parent = source.Assembly;
        }
    }
}