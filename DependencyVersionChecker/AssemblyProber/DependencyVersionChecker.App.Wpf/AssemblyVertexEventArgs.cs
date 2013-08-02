using System;
using DotNetUtilitiesApp.AssemblyProber.Graphing;

namespace DotNetUtilitiesApp.AssemblyProber
{
    public class AssemblyVertexEventArgs : EventArgs
    {
        public AssemblyVertex Vertex { get; private set; }

        internal AssemblyVertexEventArgs( AssemblyVertex vertex )
        {
            Vertex = vertex;
        }
    }
}