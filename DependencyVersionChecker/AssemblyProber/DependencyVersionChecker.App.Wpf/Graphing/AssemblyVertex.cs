using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AssemblyProber;

namespace AssemblyProberApp.Wpf.Graphing
{
    [DebuggerDisplay( "{Assembly.FullName}" )]
    public class AssemblyVertex
        : ViewModel
    {
        public IAssemblyInfo Assembly { get; private set; }

        public List<IAssemblyInfo> _referencedBy;
        public List<string> _messages;

        private bool _isMarked = false;

        private bool _isHighlighted = false;

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

        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            internal set
            {
                if( value != _isHighlighted )
                {
                    _isHighlighted = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<IAssemblyInfo> LocalReferences
        {
            get
            {
                return Assembly.Dependencies.Values.Where( x => x.BorderName == null );
            }
        }

        public IEnumerable<IAssemblyInfo> ReferencedBy
        {
            get
            {
                return _referencedBy;
            }
        }

        public void AddReferencedBy( IAssemblyInfo parent )
        {
            _referencedBy.Add( parent );
        }

        public void AddMessage( string message )
        {
            _messages.Add( message );
        }

        public IEnumerable<IAssemblyInfo> BorderReferences
        {
            get
            {
                return Assembly.Dependencies.Values.Where( x => x.BorderName != null );
            }
        }

        public bool HasDependencies
        {
            get
            {
                return LocalReferences.Count() > 0;
            }
        }

        public string TooltipText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append( String.Format( "Version {0}\n", Assembly.Version.ToString() ) );


                if( _messages.Count > 0 )
                {
                    sb.Append( "Notes:\n" );
                    foreach( var message in _messages )
                    {
                        sb.Append( String.Format( "- {0}\n", message ) );
                    }
                    sb.Append( "\n" );
                }


                if( _referencedBy.Count > 0 )
                {
                    sb.Append( "Referenced by:\n" );
                    foreach( var parent in _referencedBy.OrderBy( x => x.SimpleName ).OrderBy( x => x.Version ) )
                    {
                        string versionReferenced = parent.Dependencies.Where( x => x.Value == Assembly ).Select( x => x.Key ).FirstOrDefault();
                        sb.Append( String.Format( "- {0} ({1}), as: {2}\n", parent.SimpleName, parent.Version, versionReferenced ) );
                    }
                }
                if( LocalReferences.Count() > 0 )
                {
                    sb.Append( "References:\n" );
                    foreach( var dep in LocalReferences.OrderBy( x => x.Version ).OrderBy( x => x.SimpleName ) )
                    {
                        sb.Append( String.Format( "- {0} ({1})\n", dep.SimpleName, dep.Version ) );
                    }
                }
                if( BorderReferences.Count() > 0 )
                {
                    sb.Append( "Special references:\n" );
                    foreach( var dep in BorderReferences.OrderBy( x => x.Version ).OrderBy( x => x.SimpleName ) )
                    {
                        sb.Append( String.Format( "- {0} v. {1} ({2})\n", dep.SimpleName, dep.Version, dep.BorderName ) );
                    }
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

                if( !String.IsNullOrEmpty( Assembly.Description ) )
                    sb.Append( String.Format( "{0}\n", Assembly.Description ) );

                sb.Append( String.Format( "Assembly version: {0}\n", Assembly.Version ) );

                if( !String.IsNullOrEmpty( Assembly.InformationalVersion ) )
                    sb.Append( String.Format( "Informational version: {0}\n", Assembly.InformationalVersion ) );

                if( !String.IsNullOrEmpty( Assembly.FileVersion ) )
                    sb.Append( String.Format( "File version: {0}\n", Assembly.FileVersion ) );

                if( !String.IsNullOrEmpty( Assembly.Product ) )
                    sb.Append( String.Format( "Product: {0}\n", Assembly.Product ) );

                if( !String.IsNullOrEmpty( Assembly.Trademark ) )
                    sb.Append( String.Format( "Trademark: {0}\n", Assembly.Trademark ) );

                if( !String.IsNullOrEmpty( Assembly.Company ) )
                    sb.Append( String.Format( "By {0}\n", Assembly.Company ) );

                if( !String.IsNullOrEmpty( Assembly.Copyright ) )
                    sb.Append( String.Format( "{0}\n", Assembly.Copyright ) );

                if( Assembly.PublicKeyToken != null && Assembly.PublicKeyToken.Length > 0 )
                    sb.Append( String.Format( "Public key token: {0}\n", StringUtils.ByteArrayToHexString( Assembly.PublicKeyToken ) ) );

                if( Assembly.Paths.Count > 0 )
                    sb.Append( "Found in files:\n" );
                foreach( string s in Assembly.Paths.OrderBy( x => x ) )
                {
                    string path = StringUtils.MakeRelativePath( s, Environment.CurrentDirectory );
                    sb.Append( String.Format( "- {0}\n", path ) );
                }

                return sb.ToString().TrimEnd( '\n' );
            }
        }

        internal AssemblyVertex( IAssemblyInfo assembly )
        {
            _referencedBy = new List<IAssemblyInfo>();
            _messages = new List<string>();
            Assembly = assembly;
        }

        public override string ToString()
        {
            return string.Format( "{0}", Assembly.FullName );
        }
    }
}