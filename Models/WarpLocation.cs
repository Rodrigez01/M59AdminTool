using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using M59AdminTool.Services;

namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents a warp/teleport location in Meridian 59
    /// </summary>
    public class WarpLocation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _name = string.Empty;
        private string? _nameDe;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string? NameDe
        {
            get => _nameDe;
            set
            {
                _nameDe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string Category { get; set; } = string.Empty;
        public string? RoomId { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string? Description { get; set; }
        public bool IsFavorite { get; set; }

        // For hierarchical display (like in the Guide Helper)
        public ObservableCollection<WarpLocation>? SubLocations { get; set; }
        public bool HasSubLocations => SubLocations != null && SubLocations.Count > 0;

        /// <summary>
        /// Gets the localized name for display
        /// </summary>
        public string DisplayName => GetLocalizedName(LocalizationService.Instance.CurrentLanguage);

        /// <summary>
        /// Gets the localized name based on current language
        /// </summary>
        public string GetLocalizedName(Services.Language language)
        {
            return language == Services.Language.German && !string.IsNullOrEmpty(NameDe)
                ? NameDe
                : Name;
        }

        /// <summary>
        /// Call this to update DisplayName when language changes
        /// </summary>
        public void RefreshDisplayName()
        {
            System.Diagnostics.Debug.WriteLine($"RefreshDisplayName for '{Name}', NameDe='{NameDe}', DisplayName='{DisplayName}'");
            OnPropertyChanged(nameof(DisplayName));
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Category for organizing warp locations
    /// </summary>
    public class WarpCategory
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<WarpLocation> Locations { get; set; } = new();
        public bool IsExpanded { get; set; } = true;
    }

    public class WarpCategoryView
    {
        public WarpCategoryView(WarpCategory source, ObservableCollection<WarpLocation> locations)
        {
            Source = source;
            Locations = locations;
        }

        public WarpCategory Source { get; }

        public string Name
        {
            get => Source.Name;
            set => Source.Name = value;
        }

        public ObservableCollection<WarpLocation> Locations { get; }

        public bool IsExpanded
        {
            get => Source.IsExpanded;
            set => Source.IsExpanded = value;
        }
    }
}
