using System;
using System.Collections.Generic;

namespace PathStructureClass
{
    /// <summary>
    /// Represents the result of a path validation attempt.
    /// </summary>
    public class PathValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathValidationResult"/> class.
        /// </summary>
        private PathValidationResult(bool isValid, string error, IReadOnlyDictionary<string, string> variables, IReadOnlyList<PathMatchNode> matchTrail)
        {
            IsValid = isValid;
            Error = error;
            Variables = variables ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            MatchTrail = matchTrail ?? Array.Empty<PathMatchNode>();
        }

        /// <summary>
        /// Gets a value indicating whether the path is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the error message when validation fails.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Gets the captured variables from named regex groups.
        /// </summary>
        public IReadOnlyDictionary<string, string> Variables { get; }

        /// <summary>
        /// Gets the ordered list of node matches.
        /// </summary>
        public IReadOnlyList<PathMatchNode> MatchTrail { get; }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static PathValidationResult Valid(IReadOnlyDictionary<string, string> variables, IReadOnlyList<PathMatchNode> matchTrail)
        {
            return new PathValidationResult(true, null, variables, matchTrail);
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        public static PathValidationResult Invalid(string error)
        {
            return new PathValidationResult(false, error, null, null);
        }
    }
}
