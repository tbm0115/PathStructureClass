namespace PathStructureClass
{
    /// <summary>
    /// Represents credentials for accessing secured resources.
    /// </summary>
    public sealed class ExplorerCredential
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerCredential"/> class.
        /// </summary>
        public ExplorerCredential(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets the username for the credential.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the password for the credential.
        /// </summary>
        public string Password { get; }
    }
}
