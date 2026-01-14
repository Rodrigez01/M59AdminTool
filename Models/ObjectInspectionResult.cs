using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents the complete inspection result for an object
    /// </summary>
    public partial class ObjectInspectionResult : ObservableObject
    {
        [ObservableProperty]
        private string _objectId = string.Empty;

        [ObservableProperty]
        private string _className = string.Empty;

        public ObservableCollection<PropertyGroup> PropertyGroups { get; } = new();

        /// <summary>
        /// Display header for UI
        /// </summary>
        public string DisplayHeader => !string.IsNullOrEmpty(ObjectId) && !string.IsNullOrEmpty(ClassName)
            ? (ClassName == "LIST" ? $"List {ObjectId}" : $"Object {ObjectId} - Class {ClassName}")
            : "No object loaded";
    }
}
