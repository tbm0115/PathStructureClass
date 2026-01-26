using System.Collections.Generic;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Defines a custom validation rule for validated paths.
    /// </summary>
    public interface IPathValidationRule
    {
        /// <summary>
        /// Validates the path using captured variables and match context.
        /// </summary>
        IPathValidationResult Validate(string fullPath, IReadOnlyDictionary<string, string> variables, IReadOnlyList<IPathMatchNode> matchTrail);
    }
}
