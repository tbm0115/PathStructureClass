namespace PathStructureClass
{
    /// <summary>
    /// Provides credentials for accessing secured resources.
    /// </summary>
    public interface ICredentialProvider
    {
        /// <summary>
        /// Attempts to retrieve credentials for a target resource.
        /// </summary>
        bool TryGetCredential(string target, out ExplorerCredential credential);
    }
}
