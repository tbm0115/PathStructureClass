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

        public PathStructure(PathStructureConfig config, IEnumerable<IPathValidationRule> validationRules = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _validationRules = (validationRules ?? Array.Empty<IPathValidationRule>()).ToList();
        }

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

        private bool TryMatchNode(PathNode node,
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

        private static string NormalizePath(string path)
        {
            return path.Trim();
        }
    }

    public class PathStructureConfig
    {
        public PathStructureConfig(PathNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public PathNode Root { get; }
        public RegexOptions RegexOptions { get; set; } = RegexOptions.IgnoreCase;
    }

    public class PathNode
    {
        public PathNode(string name, string pattern)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public string Name { get; }
        public string Pattern { get; }
        public List<PathNode> Children { get; } = new List<PathNode>();

        public Regex GetRegex(RegexOptions options)
        {
            return new Regex(Pattern, options | RegexOptions.Compiled);
        }
    }

    public class PathMatchNode
    {
        public PathMatchNode(PathNode node, string matchedValue)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            MatchedValue = matchedValue;
        }

        public PathNode Node { get; }
        public string MatchedValue { get; }
    }

    public class PathValidationResult
    {
        private PathValidationResult(bool isValid, string error, IReadOnlyDictionary<string, string> variables, IReadOnlyList<PathMatchNode> matchTrail)
        {
            IsValid = isValid;
            Error = error;
            Variables = variables ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            MatchTrail = matchTrail ?? Array.Empty<PathMatchNode>();
        }

        public bool IsValid { get; }
        public string Error { get; }
        public IReadOnlyDictionary<string, string> Variables { get; }
        public IReadOnlyList<PathMatchNode> MatchTrail { get; }

        public static PathValidationResult Valid(IReadOnlyDictionary<string, string> variables, IReadOnlyList<PathMatchNode> matchTrail)
        {
            return new PathValidationResult(true, null, variables, matchTrail);
        }

        public static PathValidationResult Invalid(string error)
        {
            return new PathValidationResult(false, error, null, null);
        }
    }

    public interface IPathValidationRule
    {
        PathValidationResult Validate(string fullPath, IReadOnlyDictionary<string, string> variables, IReadOnlyList<PathMatchNode> matchTrail);
    }
}
