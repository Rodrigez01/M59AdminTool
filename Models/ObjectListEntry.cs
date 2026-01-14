using System.Collections.Generic;

namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents an object returned by list reader commands (show class, show instances, show all)
    /// </summary>
    public class ObjectListEntry
    {
        /// <summary>
        /// The object ID (from "OBJECT 1234" format)
        /// </summary>
        public string ObjectId { get; set; } = string.Empty;

        /// <summary>
        /// The class name of the object
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// Display text for UI lists
        /// </summary>
        public string DisplayText { get; set; } = string.Empty;

        /// <summary>
        /// Object properties as key-value pairs
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// Raw server response text
        /// </summary>
        public string Raw { get; set; } = string.Empty;
    }
}
