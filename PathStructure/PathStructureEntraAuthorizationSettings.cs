using System.Collections.Generic;

namespace PathStructure
{
    /// <summary>
    /// Describes Microsoft Entra authorization settings.
    /// </summary>
    public class PathStructureEntraAuthorizationSettings
    {
        /// <summary>
        /// Gets or sets the Entra tenant identifier.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the authority host for token validation.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the expected audience for access tokens.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets the allowed Entra group identifiers.
        /// </summary>
        public IList<string> AllowedGroups { get; set; } = new List<string>();
    }
}
