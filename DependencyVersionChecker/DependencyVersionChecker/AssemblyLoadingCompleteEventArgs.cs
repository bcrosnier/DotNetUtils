using System;
using System.Diagnostics;
using System.IO;

namespace DependencyVersionChecker
{
    [DebuggerDisplay( "AssemblyInfo = {AssemblyFile}" )]
    public class AssemblyLoadingCompleteEventArgs : EventArgs
    {
        public Exception Exception
        {
            get;
            private set;
        }

        public FileInfo AssemblyFile
        {
            get;
            private set;
        }

        public IAssemblyInfo ResultingAssembly
        {
            get;
            private set;
        }

        internal AssemblyLoadingCompleteEventArgs( FileInfo f, IAssemblyInfo a )
            : this( f, a, null )
        {
        }

        internal AssemblyLoadingCompleteEventArgs( FileInfo f, Exception e )
            : this( f, null, e )
        {
        }

        internal AssemblyLoadingCompleteEventArgs( FileInfo f, IAssemblyInfo assembly, Exception error )
        {
            AssemblyFile = f;
            Exception = error;
            ResultingAssembly = assembly;
        }
    }
}