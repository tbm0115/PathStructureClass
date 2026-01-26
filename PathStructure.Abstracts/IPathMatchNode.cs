namespace PathStructure.Abstracts
{
    /// <summary>
    /// Captures a node match during validation.
    /// </summary>
    public interface IPathMatchNode
    {
        /// <summary>
        /// Gets the matched configuration node.
        /// </summary>
        IPathNode Node { get; }

        /// <summary>
        /// Gets the matched substring for the node.
        /// </summary>
        string MatchedValue { get; }
    }
}
