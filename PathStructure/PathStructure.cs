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

            if (node == _config.Root)
            {
                if (node.Children.Count == 0)
                {
                    failure = "No path structures were configured.";
                    return false;
                }

                foreach (var child in node.Children)
                {
                    if (TryMatchNode(child, remainingPath, variables, matchTrail, out failure))
                    {
                        return true;
                    }
                }

                failure = $"No configured path matched '{remainingPath}'.";
                return false;
            }

            failure = null;
            if (string.IsNullOrWhiteSpace(remainingPath))
            {
                failure = $"Pattern '{node.Pattern}' did not match an empty path.";
                return false;
            }

            var segments = SplitSegments(remainingPath);
            if (segments.Count == 0)
            {
                failure = $"Pattern '{node.Pattern}' did not match '{remainingPath}'.";
                return false;
            }

            var separator = DetectSeparator(remainingPath);
            var regex = node.GetRegex(_config.RegexOptions);
            var lastFailure = $"Pattern '{node.Pattern}' did not match '{remainingPath}'.";
            var current = string.Empty;

            for (var index = 0; index < segments.Count; index++)
            {
                current = index == 0 ? segments[index] : $"{current}{separator}{segments[index]}";
                var match = regex.Match(current);
                if (!match.Success || match.Index != 0 || match.Length != current.Length)
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

                var remainingSegments = segments.Skip(index + 1).ToList();
                if (node.Children.Count == 0)
                {
                    if (remainingSegments.Count == 0)
                    {
                        ApplyMatchOutcome(variables, matchTrail, candidateVariables, candidateTrail);
                        return true;
                    }

                    continue;
                }

                if (remainingSegments.Count == 0)
                {
                    ApplyMatchOutcome(variables, matchTrail, candidateVariables, candidateTrail);
                    return true;
                }

                var unmatched = string.Join(separator.ToString(), remainingSegments);
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

        private static List<string> SplitSegments(string path)
        {
            return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static char DetectSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return PathSeparators[0];
            }

            return path.Contains(PathSeparators[0]) ? PathSeparators[0] : PathSeparators[1];
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
