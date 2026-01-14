using CommunityToolkit.Mvvm.ComponentModel;
using M59AdminTool.Models;
using M59AdminTool.Services;

namespace M59AdminTool.ViewModels
{
    public partial class WarpLocationViewModel : ObservableObject
    {
        private readonly WarpLocation _warpLocation;
        private readonly LocalizationService _localization;

        public WarpLocationViewModel(WarpLocation warpLocation)
        {
            _warpLocation = warpLocation;
            _localization = LocalizationService.Instance;
            _localization.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LocalizationService.CurrentLanguage))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            };
        }

        public WarpLocation Model => _warpLocation;

        public string DisplayName => _warpLocation.GetLocalizedName(_localization.CurrentLanguage);

        public string Name
        {
            get => _warpLocation.Name;
            set => _warpLocation.Name = value;
        }

        public string? NameDe
        {
            get => _warpLocation.NameDe;
            set => _warpLocation.NameDe = value;
        }

        public string Category
        {
            get => _warpLocation.Category;
            set => _warpLocation.Category = value;
        }

        public string? RoomId
        {
            get => _warpLocation.RoomId;
            set => _warpLocation.RoomId = value;
        }

        public int? X
        {
            get => _warpLocation.X;
            set => _warpLocation.X = value;
        }

        public int? Y
        {
            get => _warpLocation.Y;
            set => _warpLocation.Y = value;
        }

        public string? Description
        {
            get => _warpLocation.Description;
            set => _warpLocation.Description = value;
        }

        public bool IsFavorite
        {
            get => _warpLocation.IsFavorite;
            set => _warpLocation.IsFavorite = value;
        }
    }
}
