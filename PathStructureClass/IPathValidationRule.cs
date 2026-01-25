using System.Collections.Generic;

namespace PathStructureClass
{
    /// <summary>
    /// Defines a custom validation rule for validated paths.
    /// </summary>
    public interface IPathValidationRule
    {
        /// <summary>
        /// Validates the path using captured variables and match context.
        /// </summary>
        PathValidationResult Validate(string fullPath, IReadOnlyDictionary<string, string> variables, IReadOnlyList<PathMatchNode> matchTrail);
    }
}
