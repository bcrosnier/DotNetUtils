using DotNetUtilitiesApp.AssemblyProber.Graphing;
using System;

namespace DotNetUtilitiesApp.AssemblyProber
{
    public class AssemblyVertexEventArgs : EventArgs
    {
        public AssemblyVertex Vertex { get; private set; }

        internal AssemblyVertexEventArgs(AssemblyVertex vertex)
        {
            Vertex = vertex;
        }
    }
}