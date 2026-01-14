using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents a single object property from "show object" command
    /// </summary>
    public partial class ObjectProperty : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _type = string.Empty;

        [ObservableProperty]
        private string _value = string.Empty;

        [ObservableProperty]
        private string _rawLine = string.Empty;

        [ObservableProperty]
        private bool _isEditable = true;

        [ObservableProperty]
        private string _resolvedValue = string.Empty;

        [ObservableProperty]
        private string _categoryOverride = string.Empty;

        [ObservableProperty]
        private int _listDepth;

        [ObservableProperty]
        private int _sublistIndex = -1;

        [ObservableProperty]
        private int _sublistId = -1;

        [ObservableProperty]
        private int _listIndex = -1;

        /// <summary>
        /// Whether this property is an object reference ($ or OBJECT type)
        /// </summary>
        public bool IsReference => Type == "$" || Type == "OBJECT";

        /// <summary>
        /// Whether this property points to a list
        /// </summary>
        public bool IsList => Type == "LIST";

        /// <summary>
        /// Whether this is a NULL reference ($ 0)
        /// </summary>
        public bool IsNullReference => IsReference && (Value == "0" || string.IsNullOrWhiteSpace(Value));

        /// <summary>
        /// True when this reference points to a real object
        /// </summary>
        public bool HasReferenceTarget => IsReference && !IsNullReference;

        /// <summary>
        /// Extracts list ID from LIST values
        /// </summary>
        public string ListId
        {
            get
            {
                if (!IsList) return string.Empty;
                var parts = Value.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[^1] : string.Empty;
            }
        }

        /// <summary>
        /// True when this list reference points to a real list
        /// </summary>
        public bool HasListTarget => IsList && !string.IsNullOrWhiteSpace(ListId) && ListId != "0";

        /// <summary>
        /// Extracts the referenced object ID from $ {id} or OBJECT {id} format
        /// </summary>
        public string ReferencedObjectId
        {
            get
            {
                if (!IsReference) return string.Empty;

                // Parse "$ 1234" or "OBJECT 1234" to get "1234"
                var parts = Value.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[^1] : string.Empty;
            }
        }

        /// <summary>
        /// Display-friendly value string
        /// </summary>
        public string DisplayValue
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ResolvedValue))
                    return ResolvedValue;

                if (IsNullReference)
                    return $"{Type} 0 (NULL)";
                return $"{Type} {Value}";
            }
        }

        partial void OnResolvedValueChanged(string value)
        {
            OnPropertyChanged(nameof(DisplayValue));
        }

        /// <summary>
        /// Category group for hierarchical display
        /// </summary>
        public string CategoryGroup
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CategoryOverride))
                    return CategoryOverride;

                return PropertyCategorizer.GetCategory(Name, Type);
            }
        }
    }
}
