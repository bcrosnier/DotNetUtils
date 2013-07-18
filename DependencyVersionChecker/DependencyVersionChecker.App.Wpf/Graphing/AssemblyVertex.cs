using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DependencyVersionChecker;

namespace DependencyVersionCheckerApp.Wpf.Graphing
{
    [DebuggerDisplay( "{Assembly.AssemblyFullName}" )]
    public class AssemblyVertex
        : ViewModel
    {
        public IAssemblyInfo Assembly { get; private set; }

        private bool _isMarked = false;

        public bool IsMarked
        {
            get
            {
                return _isMarked;
            }
            internal set
            {
                if( value != _isMarked )
                {
                    _isMarked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TooltipText
        {
            get
            {
                if( Assembly.Dependencies.Count() == 0 )
                    return "No references";

                StringBuilder sb = new StringBuilder();
                sb.Append( "References:\n" );
                foreach( var dep in Assembly.Dependencies )
                {
                    sb.Append( String.Format( "- {0} ({1})\n", dep.SimpleName, dep.Version ) );
                }

                return sb.ToString().TrimEnd( '\n' );
            }
        }

        public string AssemblyDetails
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append( String.Format( "{0}\n", Assembly.AssemblyFullName ) );
                sb.Append( String.Format( "Version: {0}\n", Assembly.Version ) );

                if( !String.IsNullOrEmpty( Assembly.InformationalVersion ) )
                    sb.Append( String.Format( "Product version: {0}\n", Assembly.InformationalVersion ) );

                if( !String.IsNullOrEmpty( Assembly.FileVersion ) )
                    sb.Append( String.Format( "File version: {0}\n", Assembly.FileVersion ) );

                return sb.ToString().TrimEnd( '\n' );
            }
        }

        public AssemblyVertex( IAssemblyInfo assembly )
        {
            Assembly = assembly;
        }

        public override string ToString()
        {
            return string.Format( "{0}", Assembly.AssemblyFullName );
        }
    }
}