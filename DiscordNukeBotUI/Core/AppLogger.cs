using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace DiscordNukeBot.Core
{
    public static class AppLogger
    {
        public static event EventHandler<string> LogMessage;

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {level.ToString().ToUpper()}: {message}";
            LogMessage?.Invoke(null, formattedMessage);
        }

        public static void Info(string message) => Log(message, LogLevel.Info);
        public static void Success(string message) => Log(message, LogLevel.Success);
        public static void Warning(string message) => Log(message, LogLevel.Warning);
        public static void Error(string message) => Log(message, LogLevel.Error);
        public static void Critical(string message) => Log(message, LogLevel.Critical);
    }

    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error,
        Critical
    }
}

