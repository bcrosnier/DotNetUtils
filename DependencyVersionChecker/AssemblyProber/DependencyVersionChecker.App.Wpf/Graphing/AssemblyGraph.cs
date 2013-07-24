using System.Linq;
using AssemblyProber;
using GraphSharp.Controls;
using QuickGraph;

namespace AssemblyProberApp.Wpf.Graphing
{
    public class AssemblyGraph
        : BidirectionalGraph<AssemblyVertex, AssemblyEdge>
    {
        public AssemblyGraph()
        {
        }

        public AssemblyGraph( bool allowParallelEdges )
            : base( allowParallelEdges ) { }

        public AssemblyGraph( bool allowParallelEdges, int vertexCapacity )
            : base( allowParallelEdges, vertexCapacity ) { }

        public void MarkAssembly( IAssemblyInfo assembly )
        {
            AssemblyVertex vertex =
                this.Vertices
                .Where( x => x.Assembly == assembly )
                .FirstOrDefault();

            if ( vertex != null )
                vertex.IsMarked = true;
        }
    }

    public class AssemblyGraphLayout : GraphLayout<AssemblyVertex, AssemblyEdge, AssemblyGraph> { }
}