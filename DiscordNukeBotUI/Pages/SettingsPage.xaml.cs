using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace DiscordNukeBot.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            // 所有資料的載入和更新都由 ViewModel 和 x:Bind 處理，
            // 後置程式碼中不需要任何額外操作。
        }
    }
}

namespace DiscordNukeBot.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
                return !b;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
                return !b;
            return false;
        }
    }
}
