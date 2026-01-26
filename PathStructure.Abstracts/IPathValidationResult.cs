using System.Collections.Generic;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Represents the result of a path validation attempt.
    /// </summary>
    public interface IPathValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the path is valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets the error message when validation fails.
        /// </summary>
        string Error { get; }

        /// <summary>
        /// Gets the captured variables from named regex groups.
        /// </summary>
        IReadOnlyDictionary<string, string> Variables { get; }

        /// <summary>
        /// Gets the ordered list of node matches.
        /// </summary>
        IReadOnlyList<IPathMatchNode> MatchTrail { get; }
    }
}
