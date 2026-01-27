using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PathStructure.Abstracts;
namespace PathStructure
{
    /// <inheritdoc />
    public class PathStructure : IPathStructure
    {
        private readonly IPathStructureConfig _config;
        private readonly IReadOnlyList<IPathValidationRule> _validationRules;
        private static readonly char[] PathSeparators = new[] { '\\', '/' };

        /// <summary>
        /// Initializes a new instance of the <see cref="PathStructure"/> class.
        /// </summary>
        public PathStructure(IPathStructureConfig config, IEnumerable<IPathValidationRule> validationRules = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _validationRules = (validationRules ?? Enumerable.Empty<IPathValidationRule>()).ToList();
        }

        /// <inheritdoc />
        public IPathStructureConfig Config => _config;

        /// <inheritdoc />
        public IPathValidationResult ValidatePath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return PathValidationResult.Invalid("Path was empty.");
            }

            var normalizedPath = NormalizePath(fullPath);
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var matchTrail = new List<IPathMatchNode>();

            var matchesRoot = TryMatchNode(_config.Root, normalizedPath, variables, matchTrail, out var failure);
            if (!matchesRoot)
            {
                return PathValidationResult.Invalid(failure ?? "Path did not match the configured structure.");
            }

            foreach (var rule in _validationRules)
            {
                var ruleResult = rule.Validate(normalizedPath, variables, matchTrail);
                if (!ruleResult.IsValid)
                {
                    return ruleResult;
                }
            }

            return PathValidationResult.Valid(variables, matchTrail);
        }

        /// <summary>
        /// Attempts to match a path segment against the configured node tree.
        /// </summary>
        private bool TryMatchNode(
            IPathNode node,
            string remainingPath,
            Dictionary<string, string> variables,
            List<IPathMatchNode> matchTrail,
            out string failure)
        {
            if (node == null)
            {
                failure = "Root node was not configured.";
                return false;
            }

            failure = null;
            var regex = node.GetRegex(_config.RegexOptions);
            var allowPartial = node.Children.Count > 0;
            var lastFailure = $"Pattern '{node.Pattern}' did not match '{remainingPath}'.";

            foreach (var candidateLength in GetCandidateMatchLengths(remainingPath, allowPartial))
            {
                var candidate = remainingPath.Substring(0, candidateLength);
                var match = regex.Match(candidate);
                if (!match.Success || match.Index != 0 || match.Length != candidate.Length)
                {
                    continue;
                }

                var candidateVariables = new Dictionary<string, string>(variables, StringComparer.OrdinalIgnoreCase);
                if (!CaptureVariables(match, regex.GetGroupNames(), candidateVariables, out var candidateFailure))
                {
                    lastFailure = candidateFailure;
                    continue;
                }

                var candidateTrail = new List<IPathMatchNode>(matchTrail)
                {
                    new PathMatchNode(node, match.Value)
                };

                if (node.Children.Count == 0)
                {
                    if (candidateLength == remainingPath.Length)
                    {
                        ApplyMatchOutcome(variables, matchTrail, candidateVariables, candidateTrail);
                        return true;
                    }

                    continue;
                }

                var unmatched = TrimSeparators(remainingPath.Substring(candidateLength));
                if (string.IsNullOrEmpty(unmatched))
                {
                    ApplyMatchOutcome(variables, matchTrail, candidateVariables, candidateTrail);
                    return true;
                }

                foreach (var child in node.Children)
                {
                    var childVariables = new Dictionary<string, string>(candidateVariables, StringComparer.OrdinalIgnoreCase);
                    var childTrail = new List<IPathMatchNode>(candidateTrail);
                    if (TryMatchNode(child, unmatched, childVariables, childTrail, out var childFailure))
                    {
                        ApplyMatchOutcome(variables, matchTrail, childVariables, childTrail);
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(childFailure))
                    {
                        lastFailure = childFailure;
                    }
                }
            }

            failure = lastFailure;
            return false;
        }

        private static IEnumerable<int> GetCandidateMatchLengths(string remainingPath, bool allowPartial)
        {
            if (string.IsNullOrEmpty(remainingPath))
            {
                yield break;
            }

            if (!allowPartial)
            {
                yield return remainingPath.Length;
                yield break;
            }

            var boundaries = new List<int>();
            for (var index = 1; index < remainingPath.Length; index++)
            {
                if (IsSeparator(remainingPath[index]) && !IsSeparator(remainingPath[index - 1]))
                {
                    boundaries.Add(index);
                }
            }

            boundaries.Add(remainingPath.Length);

            for (var index = boundaries.Count - 1; index >= 0; index--)
            {
                yield return boundaries[index];
            }
        }

        private static bool IsSeparator(char value)
        {
            return value == PathSeparators[0] || value == PathSeparators[1];
        }

        private static string TrimSeparators(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.TrimStart(PathSeparators);
        }

        private static void ApplyMatchOutcome(
            Dictionary<string, string> targetVariables,
            List<IPathMatchNode> targetTrail,
            Dictionary<string, string> sourceVariables,
            List<IPathMatchNode> sourceTrail)
        {
            targetVariables.Clear();
            foreach (var kvp in sourceVariables)
            {
                targetVariables[kvp.Key] = kvp.Value;
            }

            targetTrail.Clear();
            targetTrail.AddRange(sourceTrail);
        }

        /// <summary>
        /// Captures named group values into a shared variable dictionary while enforcing consistency.
        /// </summary>
        private static bool CaptureVariables(
            Match match,
            IEnumerable<string> groupNames,
            Dictionary<string, string> variables,
            out string failure)
        {
            failure = null;
            foreach (var groupName in groupNames ?? Enumerable.Empty<string>())
            {
                if (int.TryParse(groupName, out _))
                {
                    continue;
                }

                var value = match.Groups[groupName].Value;
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (variables.TryGetValue(groupName, out var existingValue))
                {
                    if (!string.Equals(existingValue, value, StringComparison.OrdinalIgnoreCase))
                    {
                        failure = $"Variable '{groupName}' was inconsistent ('{existingValue}' vs '{value}').";
                        return false;
                    }
                }
                else
                {
                    variables[groupName] = value;
                }
            }

            return true;
        }

        /// <summary>
        /// Normalizes a path for comparisons.
        /// </summary>
        private static string NormalizePath(string path)
        {
            return path.Trim();
        }
    }
}
