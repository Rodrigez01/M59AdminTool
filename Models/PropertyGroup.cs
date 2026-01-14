using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents a group of related properties for hierarchical display in TreeView
    /// </summary>
    public partial class PropertyGroup : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private bool _isExpanded = true;

        public ObservableCollection<ObjectProperty> Properties { get; } = new();

        /// <summary>
        /// Display name with property count
        /// </summary>
        public string DisplayName => $"{Name} ({Properties.Count})";
    }
}
