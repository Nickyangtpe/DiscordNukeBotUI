using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;

namespace DiscordNukeBot.Core
{
    public class NukeSettings : ObservableObject
    {
        private const string FilePath = "settings.json";

        public bool IsRandomDelayEnabled => !UseRandomDelays;
        public bool IsGranularDelayEnabled => UseGranularDelays;


        // --- General ---
        private string _guildName = "SERVER OBLITERATED";
        public string GuildName { get => _guildName; set => SetProperty(ref _guildName, value); }

        // --- Channels ---
        private string _channelName = "you-have-been-nuked";
        public string ChannelName { get => _channelName; set => SetProperty(ref _channelName, value); }

        private int _channelCount = 50;
        public int ChannelCount { get => _channelCount; set => SetProperty(ref _channelCount, value); }

        // --- Roles ---
        private string _roleName = "nuked-survivor";
        public string RoleName { get => _roleName; set => SetProperty(ref _roleName, value); }

        private int _roleCount = 50;
        public int RoleCount { get => _roleCount; set => SetProperty(ref _roleCount, value); }

        // --- Message Spam ---
        private int _messageSpamCount = 10;
        public int MessageSpamCount { get => _messageSpamCount; set => SetProperty(ref _messageSpamCount, value); }

        private List<string> _spamMessages = new List<string>
{
    "@everyone @here NUKE INCOMING! 💣",
    "@everyone GET READY, SERVER ABOUT TO BE NUKED!",
    "@here TOTAL NUCLEAR CHAOS ACTIVATED!",
    "@everyone 💥 NUKE DETONATED 💥",
    "@everyone OWNED BY VOID NUKE",
    "@here RIP SERVER, IT'S A NUCLEAR APOCALYPSE!",
    "@everyone BOOM! EVERYTHING IS NUKE-WIPED!",
    "@everyone ALERT! NUCLEAR STRIKE IN PROGRESS!",
    "@here https://tenor.com/view/explosion-mushroom-cloud-atomic-bomb-bomb-boom-gif-4464831",
    "@everyone CHAOS MODE: NUKES EVERYWHERE!",
    "@everyone IT'S TOO LATE, SERVER GOT NUKED!",
    "@here 💣💥 NUKE IMPACT DETECTED 💥💣",
    "@everyone ENDGAME: SERVER NUKED!",
    "@everyone 🔥 FIRE, FURY AND NUCLEAR DESTRUCTION 🔥",
    "@everyone EVERYTHING IS GONE, NUKED COMPLETELY!",
    "@everyone NUKE CONFIRMED, HAHAHA!",
    "@here ALERT! SERVER DESTROYED BY NUCLEAR STRIKE!",
    "@everyone LOOK OUT! NUKE IMPACT IMMINENT!"
};

        public List<string> SpamMessages { get => _spamMessages; set => SetProperty(ref _spamMessages, value); }

        // --- Emojis ---
        private string _emojiNamePrefix = "nuked_";
        public string EmojiNamePrefix { get => _emojiNamePrefix; set => SetProperty(ref _emojiNamePrefix, value); }

        private int _emojiCount = 20;
        public int EmojiCount { get => _emojiCount; set => SetProperty(ref _emojiCount, value); }

        private string _emojiImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTVxoLCfz6cAOpuDqeCm7berg7S8QJ-6NixMA&s";
        public string EmojiImageUrl { get => _emojiImageUrl; set => SetProperty(ref _emojiImageUrl, value); }

        // --- Member Actions ---
        private bool _kickAllMembers = false;
        public bool KickAllMembers { get => _kickAllMembers; set => SetProperty(ref _kickAllMembers, value); }

        private bool _kickAllBots = true;
        public bool KickAllBots { get => _kickAllBots; set => SetProperty(ref _kickAllBots, value); }

        private bool _unbanAll = true;
        public bool UnbanAll { get => _unbanAll; set => SetProperty(ref _unbanAll, value); }

        private bool _banMembersInsteadOfKick = false;
        public bool BanMembersInsteadOfKick { get => _banMembersInsteadOfKick; set => SetProperty(ref _banMembersInsteadOfKick, value); }

        private string _banReason = "Nuke Execution.";
        public string BanReason { get => _banReason; set => SetProperty(ref _banReason, value); }

        // --- Permissions ---
        private bool _grantAdminToEveryone = true;
        public bool GrantAdminToEveryone { get => _grantAdminToEveryone; set => SetProperty(ref _grantAdminToEveryone, value); }

        // --- Advanced Delay Settings ---
        private bool _useRandomDelays = true;
        public bool UseRandomDelays { get => _useRandomDelays; set => SetProperty(ref _useRandomDelays, value); }

        private int _minDelayBetweenActionsMs = 100;
        public int MinDelayBetweenActionsMs { get => _minDelayBetweenActionsMs; set => SetProperty(ref _minDelayBetweenActionsMs, value); }

        private int _maxDelayBetweenActionsMs = 300;
        public int MaxDelayBetweenActionsMs { get => _maxDelayBetweenActionsMs; set => SetProperty(ref _maxDelayBetweenActionsMs, value); }

        private int _fixedDelayMs = 200;
        public int FixedDelayMs { get => _fixedDelayMs; set => SetProperty(ref _fixedDelayMs, value); }

        // --- Nuke Strategy ---
        private bool _deleteChannelsFirst = true;
        public bool DeleteChannelsFirst { get => _deleteChannelsFirst; set => SetProperty(ref _deleteChannelsFirst, value); }

        // ##################################################################
        // #                          NEW SETTINGS                          #
        // ##################################################################

        // --- Granular Delay Control (超多延遲設定) ---
        private bool _useGranularDelays = false;
        public bool UseGranularDelays { get => _useGranularDelays; set => SetProperty(ref _useGranularDelays, value); }

        private int _minDelayChannelCreation = 50;
        public int MinDelayChannelCreation { get => _minDelayChannelCreation; set => SetProperty(ref _minDelayChannelCreation, value); }
        private int _maxDelayChannelCreation = 200;
        public int MaxDelayChannelCreation { get => _maxDelayChannelCreation; set => SetProperty(ref _maxDelayChannelCreation, value); }

        private int _minDelayRoleCreation = 50;
        public int MinDelayRoleCreation { get => _minDelayRoleCreation; set => SetProperty(ref _minDelayRoleCreation, value); }
        private int _maxDelayRoleCreation = 200;
        public int MaxDelayRoleCreation { get => _maxDelayRoleCreation; set => SetProperty(ref _maxDelayRoleCreation, value); }

        private int _minDelayMessageSpam = 20;
        public int MinDelayMessageSpam { get => _minDelayMessageSpam; set => SetProperty(ref _minDelayMessageSpam, value); }
        private int _maxDelayMessageSpam = 150;
        public int MaxDelayMessageSpam { get => _maxDelayMessageSpam; set => SetProperty(ref _maxDelayMessageSpam, value); }

        private int _minDelayMemberAction = 100; // Kick/Ban
        public int MinDelayMemberAction { get => _minDelayMemberAction; set => SetProperty(ref _minDelayMemberAction, value); }
        private int _maxDelayMemberAction = 400;
        public int MaxDelayMemberAction { get => _maxDelayMemberAction; set => SetProperty(ref _maxDelayMemberAction, value); }

        // --- Execution Strategy ---
        private bool _randomizeExecutionOrder = false;
        public bool RandomizeExecutionOrder { get => _randomizeExecutionOrder; set => SetProperty(ref _randomizeExecutionOrder, value); }

        private bool _deleteRolesFirst = false;
        public bool DeleteRolesFirst { get => _deleteRolesFirst; set => SetProperty(ref _deleteRolesFirst, value); }

        // --- Server Modification ---
        private bool _changeServerRegion = false;
        public bool ChangeServerRegion { get => _changeServerRegion; set => SetProperty(ref _changeServerRegion, value); }

        private bool _setVerificationLevelToMax = true;
        public bool SetVerificationLevelToMax { get => _setVerificationLevelToMax; set => SetProperty(ref _setVerificationLevelToMax, value); }

        private bool _setNotificationsToMentionsOnly = true;
        public bool SetNotificationsToMentionsOnly { get => _setNotificationsToMentionsOnly; set => SetProperty(ref _setNotificationsToMentionsOnly, value); }

        private List<string> _randomIconUrls = new List<string> { "https://i.imgur.com/T22zJ4L.png", "https://i.imgur.com/d7hI2l6.png" };
        public List<string> RandomIconUrls { get => _randomIconUrls; set => SetProperty(ref _randomIconUrls, value); }

        private bool _useRandomIcon = true;
        public bool UseRandomIcon { get => _useRandomIcon; set => SetProperty(ref _useRandomIcon, value); }

        // --- Member Interaction ---
        private bool _dmAllMembers = false;
        public bool DmAllMembers { get => _dmAllMembers; set => SetProperty(ref _dmAllMembers, value); }

        private List<string> _dmMessages = new List<string> { "You survived... for now." };
        public List<string> DmMessages { get => _dmMessages; set => SetProperty(ref _dmMessages, value); }

        private bool _changeAllNicknames = false;
        public bool ChangeAllNicknames { get => _changeAllNicknames; set => SetProperty(ref _changeAllNicknames, value); }

        private string _newNickname = "Nuked";
        public string NewNickname { get => _newNickname; set => SetProperty(ref _newNickname, value); }

        // --- Webhook Spam ---
        private bool _useWebhookSpam = false;
        public bool UseWebhookSpam { get => _useWebhookSpam; set => SetProperty(ref _useWebhookSpam, value); }

        private int _webhooksPerChannel = 2;
        public int WebhooksPerChannel { get => _webhooksPerChannel; set => SetProperty(ref _webhooksPerChannel, value); }

        private string _webhookUsername = "NUKE ON TOP";
        public string WebhookUsername { get => _webhookUsername; set => SetProperty(ref _webhookUsername, value); }

        // --- Cleanup & Obfuscation ---
        private bool _pruneMembers = false;
        public bool PruneMembers { get => _pruneMembers; set => SetProperty(ref _pruneMembers, value); }

        private int _pruneDays = 1; // Prune members inactive for 1 day
        public int PruneDays { get => _pruneDays; set => SetProperty(ref _pruneDays, value); }

        // Load 和 Save 方法保持不變
        public static NukeSettings Load()
        {
            string filePath = GetSettingsFilePath();

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new NukeSettings();
                    }
                    return JsonConvert.DeserializeObject<NukeSettings>(json) ?? new NukeSettings();
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"Failed to load settings from {filePath}: {ex.Message}");
                    return new NukeSettings();
                }
            }
            else
            {
                return new NukeSettings();
            }
        }


        private static string GetSettingsFilePath()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            string fileName = "Config.json";
            return Path.Combine(localFolder.Path, fileName);
        }

        public void Save()
        {
            try
            {
                string filePath = GetSettingsFilePath();
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
                AppLogger.Info($"Settings saved successfully to {filePath}");
                // AppLogger is not available here, handle logging in the caller
            }
            catch (Exception)
            {
                // AppLogger is not available here, handle logging in the caller
            }
        }
    }
}
