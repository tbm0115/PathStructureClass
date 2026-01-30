using System.Collections.Generic;

namespace PathStructure
{
    /// <summary>
    /// Describes authorization settings for managed installations.
    /// </summary>
    public class PathStructureAuthorizationConfig
    {
        /// <summary>
        /// Gets or sets the authorization mode (none, entra, ldap).
        /// </summary>
        public string Mode { get; set; } = "none";

        /// <summary>
        /// Gets the list of explicitly allowed client identifiers.
        /// </summary>
        public IList<string> AllowedClientIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets the list of explicitly allowed principals.
        /// </summary>
        public IList<string> AllowedPrincipals { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the Microsoft Entra configuration.
        /// </summary>
        public PathStructureEntraAuthorizationSettings Entra { get; set; } = new PathStructureEntraAuthorizationSettings();

        /// <summary>
        /// Gets or sets the LDAP configuration.
        /// </summary>
        public PathStructureLdapAuthorizationSettings Ldap { get; set; } = new PathStructureLdapAuthorizationSettings();
    }
}
