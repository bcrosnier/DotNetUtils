using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyProberApp.Wpf.Graphing;

namespace AssemblyProberApp.Wpf
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
