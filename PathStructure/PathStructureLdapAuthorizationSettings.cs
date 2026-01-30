using System.Collections.Generic;

namespace PathStructure
{
    /// <summary>
    /// Describes LDAP authorization settings.
    /// </summary>
    public class PathStructureLdapAuthorizationSettings
    {
        /// <summary>
        /// Gets or sets the LDAP host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the LDAP port.
        /// </summary>
        public int Port { get; set; } = 389;

        /// <summary>
        /// Gets or sets a value indicating whether SSL should be used.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets or sets the base DN for group searches.
        /// </summary>
        public string BaseDn { get; set; }

        /// <summary>
        /// Gets the allowed LDAP group DNs.
        /// </summary>
        public IList<string> AllowedGroups { get; set; } = new List<string>();
    }
}
