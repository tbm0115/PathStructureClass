namespace PathStructure.Abstracts
{
    /// <summary>
    /// Defines the path validation service that evaluates paths against a structure tree.
    /// </summary>
    public interface IPathStructure
    {
        /// <summary>
        /// Gets the active configuration for the structure.
        /// </summary>
        IPathStructureConfig Config { get; }

        /// <summary>
        /// Validates a full path and returns the validation result.
        /// </summary>
        IPathValidationResult ValidatePath(string fullPath);
    }
}
