using System;
using System.Windows.Markup;

namespace M59AdminTool.Services
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LocExtension : MarkupExtension
    {
        public LocExtension(string key)
        {
            Key = key ?? string.Empty;
        }

        public string Key { get; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new System.Windows.Data.Binding($"[{Key}]")
            {
                Source = LocalizationService.Instance,
                Mode = System.Windows.Data.BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
