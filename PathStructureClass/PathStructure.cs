using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace PathStructureClass
{
    /// <summary>
    /// Service for validating file system paths against a configured tree of regex-based nodes.
    /// </summary>
    public class PathStructure
    {
        private readonly PathStructureConfig _config;
        private readonly IReadOnlyList<IPathValidationRule> _validationRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathStructure"/> class.
        /// </summary>
        public PathStructure(PathStructureConfig config, IEnumerable<IPathValidationRule> validationRules = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _validationRules = (validationRules ?? Array.Empty<IPathValidationRule>()).ToList();
        }

        /// <summary>
        /// Gets the active configuration.
        /// </summary>
        public PathStructureConfig Config => _config;

        /// <summary>
        /// Validates a full path and returns details about matches and captured variables.
        /// </summary>
        public PathValidationResult ValidatePath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return PathValidationResult.Invalid("Path was empty.");
            }

            var normalizedPath = NormalizePath(fullPath);
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var matchTrail = new List<PathMatchNode>();

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
            PathNode node,
            string remainingPath,
            Dictionary<string, string> variables,
            List<PathMatchNode> matchTrail,
            out string failure)
        {
            failure = null;
            if (node == null)
            {
                failure = "Root node was not configured.";
                return false;
            }

            var regex = node.GetRegex(_config.RegexOptions);
            var match = regex.Match(remainingPath);
            if (!match.Success)
            {
                failure = $"Pattern '{node.Pattern}' did not match '{remainingPath}'.";
                return false;
            }

            if (!CaptureVariables(match, variables, out failure))
            {
                return false;
            }

            matchTrail.Add(new PathMatchNode(node, match.Value));

            if (node.Children.Count == 0)
            {
                return match.Value.Length == remainingPath.Length;
            }

            var unmatched = remainingPath.Substring(match.Value.Length).TrimStart('\\');
            if (string.IsNullOrEmpty(unmatched))
            {
                return true;
            }

            foreach (var child in node.Children)
            {
                var childVariables = new Dictionary<string, string>(variables, StringComparer.OrdinalIgnoreCase);
                var childTrail = new List<PathMatchNode>(matchTrail);
                if (TryMatchNode(child, unmatched, childVariables, childTrail, out failure))
                {
                    variables.Clear();
                    foreach (var kvp in childVariables)
                    {
                        variables[kvp.Key] = kvp.Value;
                    }

                    matchTrail.Clear();
                    matchTrail.AddRange(childTrail);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Captures named group values into a shared variable dictionary while enforcing consistency.
        /// </summary>
        private static bool CaptureVariables(Match match, Dictionary<string, string> variables, out string failure)
        {
            failure = null;
            foreach (var groupName in match.Groups.Keys)
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
