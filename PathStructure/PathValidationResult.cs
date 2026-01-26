using System;
using System.Collections.Generic;
using System.Linq;
using PathStructure.Abstracts;

namespace PathStructure
{
    /// <inheritdoc />
    public class PathValidationResult : IPathValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathValidationResult"/> class.
        /// </summary>
        private PathValidationResult(bool isValid, string error, IReadOnlyDictionary<string, string> variables, IReadOnlyList<IPathMatchNode> matchTrail)
        {
            IsValid = isValid;
            Error = error;
            Variables = variables ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            MatchTrail = matchTrail ?? Enumerable.Empty<IPathMatchNode>().ToArray();
        }

        /// <inheritdoc />
        public bool IsValid { get; }

        /// <inheritdoc />
        public string Error { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> Variables { get; }

        /// <inheritdoc />
        public IReadOnlyList<IPathMatchNode> MatchTrail { get; }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static PathValidationResult Valid(IReadOnlyDictionary<string, string> variables, IReadOnlyList<IPathMatchNode> matchTrail)
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
