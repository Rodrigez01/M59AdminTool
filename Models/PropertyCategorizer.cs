namespace M59AdminTool.Models
{
    /// <summary>
    /// Utility class for categorizing object properties into hierarchical groups
    /// </summary>
    public static class PropertyCategorizer
    {
        /// <summary>
        /// Gets the category group for a property based on its name and type
        /// </summary>
        /// <param name="propertyName">Property name (e.g., "piHealth", "poOwner")</param>
        /// <param name="propertyType">Property type (e.g., "INT", "$", "RESOURCE")</param>
        /// <returns>Category name</returns>
        public static string GetCategory(string propertyName, string propertyType)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return "Other Properties";

            if (propertyName.StartsWith("[", System.StringComparison.Ordinal))
                return "List Items";

            // Type-based categorization (takes precedence)
            if (propertyType == "RESOURCE")
                return "Resources";

            if (propertyType == "LIST")
                return "Lists";

            if (propertyType == "TIMER")
                return "Timers";

            if (propertyType == "CLASS")
                return "Classes";

            if (propertyType == "$" || propertyType == "OBJECT")
                return "Pointers & References";

            // Prefix-based categorization
            if (propertyName.StartsWith("vr", System.StringComparison.OrdinalIgnoreCase) ||
                propertyName.StartsWith("pr", System.StringComparison.OrdinalIgnoreCase) ||
                propertyName.StartsWith("vb", System.StringComparison.OrdinalIgnoreCase))
                return "Identity & Core";

            if (propertyName.StartsWith("pi", System.StringComparison.OrdinalIgnoreCase))
                return "Stats & Attributes";

            if (propertyName.StartsWith("po", System.StringComparison.OrdinalIgnoreCase))
                return "Pointers & References";

            if (propertyName.StartsWith("pl", System.StringComparison.OrdinalIgnoreCase))
                return "Lists";

            if (propertyName.StartsWith("pt", System.StringComparison.OrdinalIgnoreCase))
                return "Timers";

            // Catch-all
            return "Other Properties";
        }

        /// <summary>
        /// Gets the sort order for a category (lower number = displayed first)
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <returns>Sort order (1-99)</returns>
        public static int GetCategoryOrder(string categoryName)
        {
            return categoryName switch
            {
                "ItemAtt in Lists" => 1,
                "List Items" => 2,
                "Identity & Core" => 3,
                "Stats & Attributes" => 4,
                "Pointers & References" => 5,
                "Resources" => 6,
                "Lists" => 7,
                "Timers" => 8,
                "Classes" => 9,
                "Other Properties" => 99,
                _ => 50  // Unknown categories in the middle
            };
        }
    }
}
