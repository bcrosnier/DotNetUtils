namespace ProjectProber
{
    /// <summary>
    /// Represents a path information contained in AssemblyInfo.cs.
    /// </summary>
    public class CSProjCompileLinkInfo
    {
        /// <summary>
        /// Relative path of SharedAssemblyInfo.cs contained in *.csproj.
        /// </summary>
        public string SharedAssemblyInfoRelativePath { get; private set; }

        /// <summary>
        /// Path of SharedAssemblyInfo.cs contained in *.csproj directory.
        /// </summary>
        public string AssociateLink { get; private set; }
        /// <summary>
        /// Name's project.
        /// </summary>
        public string ProjectName { get; private set; }

        /// <summary>
        /// Construct a CSProjCompileLinkInfo object.
        /// </summary>
        /// <param name="sharedAssemblyInfoRelativePath">Relative path of SharedAssemblyInfo.cs contained in *.csproj.</param>
        /// <param name="link">Path of SharedAssemblyInfo.cs contained in *.csproj directory.</param>
        /// <param name="project">Name's project.</param>
        public CSProjCompileLinkInfo( string sharedAssemblyInfoRelativePath, string link, string project )
        {
            SharedAssemblyInfoRelativePath = sharedAssemblyInfoRelativePath;
            AssociateLink = link;
            ProjectName = project;
        }

        /// <summary>
        /// Determines whether the specified CSProjCompileLinkInfo is equal to the current CSProjCompileLinkInfo.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if SharedAssemblyInfoRelativePath and AssociateLink are equal; otherwise, false.</returns>
        public override bool Equals( object obj )
        {
            CSProjCompileLinkInfo temp = obj as CSProjCompileLinkInfo;
            return temp != null && this.SharedAssemblyInfoRelativePath == temp.SharedAssemblyInfoRelativePath && this.AssociateLink == temp.AssociateLink;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current CSProjCompileLinkInfo.</returns>
        public override int GetHashCode()
        {
            return SharedAssemblyInfoRelativePath.GetHashCode() + AssociateLink.GetHashCode() + ProjectName.GetHashCode();
        }


        public static bool operator ==( CSProjCompileLinkInfo obj1, CSProjCompileLinkInfo obj2 )
        {
            if( ReferenceEquals( obj1, null ) ) return ReferenceEquals( obj2, null );
            return obj1.Equals( obj2 );
        }

        public static bool operator !=( CSProjCompileLinkInfo obj1, CSProjCompileLinkInfo obj2 )
        {
            return !(obj1 == obj2);
        }
    }
}