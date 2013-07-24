using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace DependencyVersionChecker
{
    public class SerializableAssemblyInfoSet
    {
        private List<AssemblyInfo> _assemblies;

        public SerializableAssemblyInfoSet()
        {
            _assemblies = new List<AssemblyInfo>();
        }

        public void Clear()
        {
            _assemblies.Clear();
        }

        [XmlArray( ElementName = "Assemblies", Order = 1 )]
        [XmlArrayItem( "Assembly" )]
        public AssemblyInfo[] Assemblies
        {
            get { return _assemblies.ToArray(); }
            set
            {
                _assemblies.Clear();
                _assemblies.AddRange( value );
            }
        }

        [XmlArray( ElementName = "References", Order = 2 )]
        [XmlArrayItem( "Reference" )]
        public DependencyLink[] DependencyLinks
        {
            get
            {
                List<DependencyLink> links = new List<DependencyLink>();

                for( int i = 0; i < _assemblies.Count; i++ )
                {
                    foreach( var dep in _assemblies[i].Dependencies )
                    {
                        links.Add( new DependencyLink()
                        {
                            Parent = _assemblies[i].FullName,
                            Child = dep.FullName
                        } );
                    }
                }

                return links.ToArray();
            }

            set
            {
                for( int i = 0; i < value.Length; i++ )
                {
                    AssemblyInfo parent =
                        _assemblies
                        .Where( x => x.FullName == value[i].Parent )
                        .FirstOrDefault();

                    AssemblyInfo child =
                        _assemblies
                        .Where( x => x.FullName == value[i].Child )
                        .FirstOrDefault();

                    Debug.Assert( parent != null, "XML dependency parent reference could be found" );
                    Debug.Assert( child != null, "XML dependency child reference could be found" );

                    parent.InternalDependencies.Add( child );
                }
            }
        }
    }

    public class DependencyLink
    {
        [XmlAttribute( "Parent" )]
        public string Parent { get; set; }

        [XmlAttribute( "Child" )]
        public string Child { get; set; }
    }
}