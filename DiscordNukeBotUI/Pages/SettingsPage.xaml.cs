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
            // �Ҧ���ƪ����J�M��s���� ViewModel �M x:Bind �B�z�A
            // ��m�{���X�����ݭn�����B�~�ާ@�C
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
