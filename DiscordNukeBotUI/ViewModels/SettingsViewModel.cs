using CommunityToolkit.Mvvm.ComponentModel;
using DiscordNukeBot.Core;
using System;
using System.Linq;
using System.ComponentModel;

namespace DiscordNukeBot.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private NukeSettings _settings;

        public NukeSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        private string _botToken;
        public string BotToken
        {
            get => _botToken;
            set
            {
                if (SetProperty(ref _botToken, value))
                {
                    // When token is changed, save it securely
                    TokenManager.SaveToken(value);
                }
            }
        }

        public SettingsViewModel()
        {
            // Load settings and token on startup
            Settings = NukeSettings.Load();
            BotToken = TokenManager.GetToken() ?? string.Empty;

            // Listen for property changes on the Settings object
            Settings.PropertyChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            // Automatically save settings whenever a property changes
            Settings.Save();
            AppLogger.Info($"Setting '{e.PropertyName}' updated and saved.");
        }

        // Helper property for binding List<string> to a multiline TextBox
        public string SpamMessagesText
        {
            get => string.Join(Environment.NewLine, Settings.SpamMessages);
            set
            {
                if (SpamMessagesText != value)
                {
                    Settings.SpamMessages = value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    OnPropertyChanged(); // Notify UI that this property has changed
                }
            }
        }

        public string DmMessagesText
        {
            get => string.Join(Environment.NewLine, Settings.DmMessages);
            set
            {
                if (DmMessagesText != value)
                {
                    Settings.DmMessages = value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    OnPropertyChanged();
                }
            }
        }

        public string RandomIconUrlsText
        {
            get => string.Join(Environment.NewLine, Settings.RandomIconUrls);
            set
            {
                if (RandomIconUrlsText != value)
                {
                    Settings.RandomIconUrls = value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    OnPropertyChanged();
                }
            }
        }
    }
}
