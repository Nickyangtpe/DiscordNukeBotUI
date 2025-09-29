using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordNukeBot.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DiscordNukeBot.ViewModels
{
    public partial class BotViewModel : ObservableObject
    {
        private Bot _bot;
        private NukeSettings _settings;

        [ObservableProperty]
        private bool _isBotRunning;

        [ObservableProperty]
        private bool _isBotStarting;

        [ObservableProperty]
        private ObservableCollection<string> _logOutput;

        public ICommand StartBotCommand { get; }
        public ICommand StopBotCommand { get; }

        public BotViewModel()
        {
            _settings = NukeSettings.Load();
            _logOutput = new ObservableCollection<string>();
            AppLogger.LogMessage += OnLogMessage;

            StartBotCommand = new AsyncRelayCommand(StartBotAsync);
            StopBotCommand = new RelayCommand(StopBot);
        }

        private async Task StartBotAsync()
        {
            IsBotStarting = true;
            IsBotRunning = false;
            LogOutput.Clear();
            AppLogger.Log("Attempting to start bot...");

            _bot = new Bot(_settings);
            await _bot.RunBotAsync(); // This will run until stopped or error

            // If bot stops for any reason, update UI
            IsBotStarting = false;
            IsBotRunning = false;
            AppLogger.Log("Bot stopped.");
        }

        private void StopBot()
        {
            _bot?.StopBot(); // Request bot to stop
            IsBotRunning = false;
            IsBotStarting = false;
            AppLogger.Log("Bot stop requested.");
        }

        private void OnLogMessage(object sender, string message)
        {
            // Ensure UI update happens on the UI thread (DispatcherQueue is handled by the Page)
            _logOutput.Add(message);
        }

        // Clean up event subscription when the ViewModel is no longer needed
        public void Cleanup()
        {
            AppLogger.LogMessage -= OnLogMessage;
            _bot?.StopBot();
        }
    }
}


