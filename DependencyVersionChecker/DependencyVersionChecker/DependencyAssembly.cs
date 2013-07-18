using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DependencyVersionChecker
{
    [DebuggerDisplay( "AssemblyInfo = {AssemblyName} ({DependencyLinks.Count})" )]
    public class DependencyAssembly
    {
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Key: Parent
        /// Value: Dependency
        /// </summary>
        public IDictionary<IAssemblyInfo, IAssemblyInfo> DependencyLinks { get; private set; }

        public bool HasConflict
        {
            get
            {
                bool hasConflict = false;
                string fullName = null;

                foreach( var link in DependencyLinks )
                {
                    if( fullName != null && link.Value.AssemblyFullName != fullName )
                        hasConflict = true;
                    fullName = link.Value.AssemblyFullName;
                }

                return hasConflict;
            }
        }

        public int Links
        {
            get
            {
                return DependencyLinks.Count;
            }
        }

        public DependencyAssembly( string requestedAssemblyShortName )
        {
            AssemblyName = requestedAssemblyShortName;
            DependencyLinks = new Dictionary<IAssemblyInfo, IAssemblyInfo>();
        }

        internal void Add( IAssemblyInfo sourceAssembly, IAssemblyInfo requestedAssembly )
        {
            Debug.Assert( sourceAssembly.Dependencies.Contains( requestedAssembly ), "requestedAssembly is a dependency of sourceAssembly" );
            Debug.Assert( requestedAssembly.SimpleName == AssemblyName, "requestedAssembly has correct name" );

            if( DependencyLinks.Keys.Contains( sourceAssembly ) || DependencyLinks.Values.Contains( requestedAssembly ) )
            {
                //throw new InvalidOperationException( "Cannot add the same assembly twice" );
                return;
            }

            DependencyLinks.Add( sourceAssembly, requestedAssembly );
        }
    }
}