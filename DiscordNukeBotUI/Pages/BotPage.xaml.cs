using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using DiscordNukeBot.Core;

namespace DiscordNukeBot.Pages
{
    public sealed partial class BotPage : Page
    {
        private Bot _bot;
        private NukeSettings _settings;

        public BotPage()
        {
            this.InitializeComponent();
            _settings = NukeSettings.Load(); // Load settings when page is initialized
            AppLogger.LogMessage += OnLogMessage; // Subscribe to log messages

        }

        private async void StartBotButton_Click(object sender, RoutedEventArgs e)
        {
            StartBotButton.IsEnabled = false;
            StopBotButton.IsEnabled = true;
            BotStatusIndicator.IsActive = true;
            LogOutput.Text = string.Empty; // Clear previous logs
            AppLogger.Log("Attempting to start bot...");

            _bot = new Bot(_settings);
            await _bot.RunBotAsync(); // This will run until stopped or error

            // If bot stops for any reason, update UI
            StartBotButton.IsEnabled = true;
            StopBotButton.IsEnabled = false;
            BotStatusIndicator.IsActive = false;
            AppLogger.Log("Bot stopped.");
        }

        private void StopBotButton_Click(object sender, RoutedEventArgs e)
        {
            _bot?.StopBot(); // Request bot to stop
            StartBotButton.IsEnabled = true;
            StopBotButton.IsEnabled = false;
            BotStatusIndicator.IsActive = false;
            AppLogger.Log("Bot stop requested.");
        }

        private void OnLogMessage(object sender, string message)
        {
            // Update UI on the UI thread
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                LogOutput.Text += message + Environment.NewLine;
                // Auto-scroll to the bottom
                LogOutput.SelectionStart = LogOutput.Text.Length;
                LogOutput.SelectionLength = 0;
            });
        }

        // Unsubscribe from events when the page is unloaded to prevent memory leaks
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            AppLogger.LogMessage -= OnLogMessage;
            _bot?.StopBot(); // Ensure bot is stopped if page is unloaded
        }
    }
}


