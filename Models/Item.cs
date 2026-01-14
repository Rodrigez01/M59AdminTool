using System.ComponentModel;
using System.Runtime.CompilerServices;
using M59AdminTool.Services;

namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents an item that can be spawned in Meridian 59
    /// </summary>
    public class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _className = string.Empty;
        private string _englishName = string.Empty;
        private string? _germanName;
        private string _dmCommand = string.Empty;
        private string? _kodFile;
        private string? _category;

        public string ClassName
        {
            get => _className;
            set
            {
                _className = value;
                OnPropertyChanged();
            }
        }

        public string EnglishName
        {
            get => _englishName;
            set
            {
                _englishName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string? GermanName
        {
            get => _germanName;
            set
            {
                _germanName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string DmCommand
        {
            get => _dmCommand;
            set
            {
                _dmCommand = value;
                OnPropertyChanged();
            }
        }

        public string? KodFile
        {
            get => _kodFile;
            set
            {
                _kodFile = value;
                OnPropertyChanged();
            }
        }

        public string? Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the localized name for display
        /// </summary>
        public string DisplayName => GetLocalizedName(LocalizationService.Instance.CurrentLanguage);

        /// <summary>
        /// Gets the localized name based on current language
        /// </summary>
        public string GetLocalizedName(Language language)
        {
            return language == Language.German && !string.IsNullOrEmpty(GermanName)
                ? GermanName
                : EnglishName;
        }

        /// <summary>
        /// Call this to update DisplayName when language changes
        /// </summary>
        public void RefreshDisplayName()
        {
            OnPropertyChanged(nameof(DisplayName));
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
