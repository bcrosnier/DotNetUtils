using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AssemblyProber
{
    /// <summary>
    /// Class representing a dependency about a certain assembly simple name.
    /// </summary>
    [DebuggerDisplay( "AssemblyInfo = {AssemblyName} ({DependencyLinks.Count})" )]
    public class AssemblyReferenceName
    {
        /// <summary>
        /// Assembly simple name.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Key: Parent
        /// Value: Dependency
        /// </summary>
        public IDictionary<IAssemblyInfo, IAssemblyInfo> ReferenceLinks { get; private set; }

        /// <summary>
        /// Returns true is this dependency name was requested using multiple FullNames.
        /// </summary>
        public bool HasConflict
        {
            get
            {
                bool hasConflict = false;
                string fullName = null;

                foreach ( var link in ReferenceLinks )
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
                return ReferenceLinks.Count;
            }
        }

        internal AssemblyReferenceName( string requestedAssemblyShortName )
        {
            AssemblyName = requestedAssemblyShortName;
            ReferenceLinks = new Dictionary<IAssemblyInfo, IAssemblyInfo>();
        }

        internal void Add( IAssemblyInfo sourceAssembly, IAssemblyInfo requestedAssembly )
        {
            Debug.Assert( sourceAssembly.Dependencies.Values.Contains( requestedAssembly ), "requestedAssembly is a dependency of sourceAssembly" );
            Debug.Assert( requestedAssembly.SimpleName == AssemblyName, "requestedAssembly has correct name" );

            if ( ReferenceLinks.Keys.Contains( sourceAssembly ) || ReferenceLinks.Values.Contains( requestedAssembly ) )
            {
                //throw new InvalidOperationException( "Cannot add the same assembly twice" );
                return;
            }

            ReferenceLinks.Add( sourceAssembly, requestedAssembly );
        }
    }
}