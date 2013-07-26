using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AssemblyProber
{
    /// <summary>
    /// Class representing a dependency about a certain assembly simple name.
    /// </summary>
    [DebuggerDisplay( "AssemblyInfo = {AssemblyName} ({DependencyLinks.Count})" )]
    public class DependencyAssembly
    {
        /// <summary>
        /// Assembly simple name.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Key: Parent
        /// Value: Dependency
        /// </summary>
        public IDictionary<IAssemblyInfo, IAssemblyInfo> DependencyLinks { get; private set; }

        /// <summary>
        /// Returns true is this dependency name was requested using multiple FullNames.
        /// </summary>
        public bool HasConflict
        {
            get
            {
                bool hasConflict = false;
                string fullName = null;

                foreach ( var link in DependencyLinks )
                {
                    if ( fullName != null && link.Value.FullName != fullName )
                        hasConflict = true;
                    fullName = link.Value.FullName;
                }

                return hasConflict;
            }
        }

        /// <summary>
        /// Gets the total number of dependency links this object has.
        /// </summary>
        public int Links
        {
            get
            {
                return DependencyLinks.Count;
            }
        }

        internal DependencyAssembly( string requestedAssemblyShortName )
        {
            AssemblyName = requestedAssemblyShortName;
            DependencyLinks = new Dictionary<IAssemblyInfo, IAssemblyInfo>();
        }

        internal void Add( IAssemblyInfo sourceAssembly, IAssemblyInfo requestedAssembly )
        {
            Debug.Assert( sourceAssembly.Dependencies.Values.Contains( requestedAssembly ), "requestedAssembly is a dependency of sourceAssembly" );
            Debug.Assert( requestedAssembly.SimpleName == AssemblyName, "requestedAssembly has correct name" );

            if ( DependencyLinks.Keys.Contains( sourceAssembly ) || DependencyLinks.Values.Contains( requestedAssembly ) )
            {
                //throw new InvalidOperationException( "Cannot add the same assembly twice" );
                return;
            }

            DependencyLinks.Add( sourceAssembly, requestedAssembly );
        }
    }
}