using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace M59AdminTool.ViewModels
{
    public partial class BGFConverterViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BmpFileItem> _files = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConvertAllCommand))]
        private BmpFileItem? _selectedFile;

        [ObservableProperty]
        private string _outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BGF_Output");

        [ObservableProperty]
        private bool _compress = true;

        [ObservableProperty]
        private bool _rotate = false;

        [ObservableProperty]
        private int _shrinkFactor = 1;

        [ObservableProperty]
        private string _bitmapName = "texture";

        [ObservableProperty]
        private int _xOffset = 0;

        [ObservableProperty]
        private int _yOffset = 0;

        [ObservableProperty]
        private BitmapImage? _previewImage;

        [ObservableProperty]
        private string _paletteStatus = "No file";

        [ObservableProperty]
        private string _bitmapSize = "";

        [ObservableProperty]
        private bool _isConverting = false;

        [ObservableProperty]
        private int _progressValue = 0;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        public BGFConverterViewModel()
        {
            // Ensure output directory exists
            if (!Directory.Exists(OutputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(OutputDirectory);
                }
                catch { }
            }
        }

        partial void OnSelectedFileChanged(BmpFileItem? value)
        {
            UpdatePreview();
        }

        [RelayCommand]
        private void AddFiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*",
                Multiselect = true,
                Title = "Select BMP Files"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var filePath in dialog.FileNames)
                {
                    if (!Files.Any(f => f.FilePath == filePath))
                    {
                        var item = new BmpFileItem
                        {
                            FilePath = filePath,
                            FileName = Path.GetFileName(filePath)
                        };

                        // Validate
                        if (DIBHelper.ValidateBitmap(filePath, out string error))
                        {
                            item.IsValid = true;
                            item.StatusText = "✓ OK";

                            var info = DIBHelper.GetBitmapInfo(filePath);
                            item.SizeText = $"{info.Width}x{info.Height}";
                        }
                        else
                        {
                            item.IsValid = false;
                            item.StatusText = $"✗ {error}";
                        }

                        Files.Add(item);
                    }
                }

                // Auto-select first file if nothing is selected
                if (SelectedFile == null && Files.Count > 0)
                {
                    SelectedFile = Files[0];
                }
            }
        }

        [RelayCommand]
        private void RemoveFile()
        {
            if (SelectedFile != null)
            {
                Files.Remove(SelectedFile);
                SelectedFile = null;
            }
        }

        [RelayCommand]
        private void ClearFiles()
        {
            Files.Clear();
            SelectedFile = null;
            PreviewImage = null;
            PaletteStatus = "No file";
            BitmapSize = "";
        }

        [RelayCommand]
        private void BrowseOutput()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Output Directory",
                SelectedPath = OutputDirectory
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputDirectory = dialog.SelectedPath;
            }
        }

        [RelayCommand(CanExecute = nameof(CanConvert))]
        private async Task ConvertAll()
        {
            if (Files.Count == 0)
            {
                System.Windows.MessageBox.Show("No files to convert!", "BGF Converter", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(OutputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(OutputDirectory);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to create output directory:\n{ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }

            IsConverting = true;
            ProgressValue = 0;
            StatusMessage = "Starting conversion...";

            await Task.Run(() =>
            {
                int completed = 0;
                int successful = 0;
                int failed = 0;

                foreach (var file in Files)
                {
                    if (!file.IsValid)
                    {
                        failed++;
                        completed++;
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProgressValue = (completed * 100) / Files.Count;
                            StatusMessage = $"Processing {completed}/{Files.Count} - Skipped {file.FileName} (invalid)";
                        });
                        continue;
                    }

                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusMessage = $"Converting {file.FileName}...";
                        });

                        string outputPath = Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(file.FilePath) + ".bgf");

                        var bitmapData = DIBHelper.LoadBitmapForBGF(file.FilePath, XOffset, YOffset);
                        var options = new BGFWriter.BGFOptions
                        {
                            Name = BitmapName,
                            ShrinkFactor = ShrinkFactor,
                            Compress = Compress,
                            Rotate = Rotate
                        };

                        BGFWriter.WriteBGF(outputPath, bitmapData, options);

                        successful++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            file.StatusText = $"✗ Error: {ex.Message}";
                        });
                    }

                    completed++;
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressValue = (completed * 100) / Files.Count;
                        StatusMessage = $"Processing {completed}/{Files.Count} - Success: {successful}, Failed: {failed}";
                    });
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Conversion Complete! Success: {successful}, Failed: {failed}";
                    System.Windows.MessageBox.Show($"Conversion complete!\n\nSuccessful: {successful}\nFailed: {failed}\n\nOutput: {OutputDirectory}",
                        "BGF Converter", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                });
            });

            IsConverting = false;
            ProgressValue = 0;
        }

        private bool CanConvert() => !IsConverting && Files.Any(f => f.IsValid);

        private void UpdatePreview()
        {
            if (SelectedFile == null || !File.Exists(SelectedFile.FilePath))
            {
                PreviewImage = null;
                PaletteStatus = "No file";
                BitmapSize = "";
                return;
            }

            try
            {
                // Load bitmap for preview
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(SelectedFile.FilePath);
                bitmap.EndInit();
                bitmap.Freeze();

                PreviewImage = bitmap;

                // Get bitmap info
                var info = DIBHelper.GetBitmapInfo(SelectedFile.FilePath);
                BitmapSize = $"{info.Width}x{info.Height}";

                if (info.Is256Colors && info.IsIndexed)
                {
                    PaletteStatus = "✓ 256 colors";
                }
                else
                {
                    PaletteStatus = $"✗ {info.FormatDescription}";
                }
            }
            catch (Exception ex)
            {
                PreviewImage = null;
                PaletteStatus = $"✗ Error: {ex.Message}";
                BitmapSize = "";
            }
        }
    }

    public class BmpFileItem : ObservableObject
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool IsValid { get; set; }

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _sizeText = "";
        public string SizeText
        {
            get => _sizeText;
            set => SetProperty(ref _sizeText, value);
        }
    }
}
