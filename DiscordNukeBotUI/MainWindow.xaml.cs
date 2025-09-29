// �z�� MainWindow.xaml.cs (���B�L�ݭק�)
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using System;
using DiscordNukeBot.Pages;

namespace DiscordNukeBot
{
    // MainWindow.xaml.cs
    public sealed partial class MainWindow : Window
    {
        private BotPage _botPage;
        private SettingsPage _settingsPage;
        private AboutPage _aboutPage;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Nuke Bot - Control Panel";

            // �إߤ@���������
            _botPage = new BotPage();
            _settingsPage = new SettingsPage();
            _aboutPage = new AboutPage();

            // ��l�ɯ�
            ContentFrame.Content = _botPage;
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag.ToString())
                {
                    case "BotPage":
                        ContentFrame.Content = _botPage;
                        break;
                    case "SettingsPage":
                        ContentFrame.Content = _settingsPage;
                        break;
                    case "AboutPage":
                        ContentFrame.Content = _aboutPage;
                        break;
                }
            }
        }
    }
}
