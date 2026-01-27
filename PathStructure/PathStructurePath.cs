namespace PathStructure
{
    /// <summary>
    /// Describes a path entry in the configuration file.
    /// </summary>
    public class PathStructurePath
    {
        /// <summary>
        /// Gets or sets the regex pattern for the path.
        /// </summary>
        public string Regex { get; set; }

        /// <summary>
        /// Gets or sets the display name for the path.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the flavor text template for the path.
        /// </summary>
        public string FlavorTextTemplate { get; set; }

        /// <summary>
        /// Gets or sets the background color for the path.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for the path.
        /// </summary>
        public string ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the icon for the path.
        /// </summary>
        public string Icon { get; set; }
    }
}
