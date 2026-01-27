namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents a path entry in the configuration.
    /// </summary>
    public interface IPathStructurePath
    {
        /// <summary>
        /// Gets the regex pattern for the path.
        /// </summary>
        string Regex { get; }

        /// <summary>
        /// Gets the display name for the path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the flavor text template for the path.
        /// </summary>
        string FlavorTextTemplate { get; }

        /// <summary>
        /// Gets the background color for the path.
        /// </summary>
        string BackgroundColor { get; }

        /// <summary>
        /// Gets the foreground color for the path.
        /// </summary>
        string ForegroundColor { get; }

        /// <summary>
        /// Gets the icon for the path.
        /// </summary>
        string Icon { get; }
    }
}
