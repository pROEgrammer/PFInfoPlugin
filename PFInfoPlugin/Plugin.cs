using Dalamud.Game.Command;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using PFInfoPlugin.Windows;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PFInfoPlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Party Finder Info";
        private const string CommandNamePFInfo = "/pfinfo";
        private const string CommandNamePFInfoConfig = "/pfinfoconfig";

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static IPartyList PartyList { get; private set; } = null!;

        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Party Finder Info");

        public ConfigWindow ConfigWindow { get; init; }
        public MainWindow MainWindow { get; init; }

        [PluginService]
        internal static IPartyFinderGui PartyFinderGui { get; set; } = null!;
        [PluginService]
        internal static IChatGui ChatGui { get; set; } = null!;
        [PluginService]
        internal static IPlayerState playerState { get; set; } = null!;
        public string playerName = "";

        public IPartyFinderListing pfListing { get; set; } = null!;
        public List<IPartyFinderListing> pfListingsJoined { get; set; } = new();

        private ConcurrentDictionary<int, List<IPartyFinderListing>> pfListings { get; set; } = new();

        public string pfComment = "";
        private Boolean isDescriptionIncoming = false;

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            CommandManager.AddHandler(CommandNamePFInfo, new CommandInfo(OnCommand)
            {
                HelpMessage = "Displays the PF info window."
            });
            CommandManager.AddHandler(CommandNamePFInfoConfig, new CommandInfo(OnCommandConfig)
            {
                HelpMessage = "Displays the PF info configuration window."
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            // Hook onto PF event
            try
            {
                PartyFinderGui.ReceiveListing += this.OnListing;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"PartyFinderGui Error: {ex}");
            }

            // Hook onto chat message
            try
            {
                ChatGui.ChatMessage += OnChatMessage;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"ChatGui Error: {ex}");
            }

            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

            if (playerState.IsLoaded)
            {
                playerName = playerState.CharacterName;
            }
        }

        private void OnListing(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
        {
            PluginLog.Debug($"OnListing updating batch: [{args.BatchNumber}] {listing.Name} - {listing.Description}");

            if (!this.pfListings.ContainsKey(args.BatchNumber))
            {
                this.pfListings[args.BatchNumber] = [];
            }

            this.pfListings[args.BatchNumber].Add(listing);
        }

        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            try
            {
                //PluginLog.Debug($"Chat Type: {type} - SysMyg: {XivChatType.SystemMessage});

                var partyFinderLeaderName = "";

                if (XivChatType.SystemMessage.Equals(type))
                {
                    if (this.isDescriptionIncoming)
                    {
                        PluginLog.Debug("PF comment coming up!");
                        PluginLog.Debug($"PF COMMENT: " + message.TextValue);
                        pfComment = message.TextValue.ToString();
                    }
                    else
                    {
                        PluginLog.Debug("SystemMessage coming up!");
                        PluginLog.Debug($"SystemMessage COMMENT: " + message.TextValue);
                    }
                    this.isDescriptionIncoming = false;

                    if (message.TextValue.Contains("■Comment")) // TODO: Localization?
                        this.isDescriptionIncoming = true;

                    if (message.TextValue.Contains("You join "))
                    {
                        var regex = new Regex("You join (.* .*)([A-Z][a-z]*)'s party for .*.");
                        var match = regex.Match(message.TextValue);
                        if (match.Success) {
                            partyFinderLeaderName = match.Groups[1].Value;
                            PluginLog.Debug($"partyFinderLeaderName: {partyFinderLeaderName}");
                        }
                    }

                    if (!pfComment.IsNullOrEmpty() && this.pfListings.Count > 0)
                    {
                        PluginLog.Debug($"Iterating through listings");
                        var currentKey = pfListings.Keys.Max();
                        foreach (var listing in this.pfListings[currentKey])
                        {
                            PluginLog.Debug($"Listing name: " + listing.Name + " - " + listing.Description);

                            if (MessageMatchesListing(listing, pfComment, partyFinderLeaderName))
                            {
                                PluginLog.Debug($"DING DING DING - Matched");
                                PluginLog.Information($"Party Finder Joined: {listing.Name} - {listing.Description}");

                                this.pfListing = listing;
                                pfListingsJoined.Add(listing);
                                this.pfListings = new();
                                partyFinderLeaderName = "";
                                pfComment = "";
                                break;
                            }
                            if (this.pfListing != null) break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"OnChatMessage Error: {ex}");
            }
        }


        private Boolean MessageMatchesListing(IPartyFinderListing listing, string pfComment, string partyFinderLeaderName)
        {
            PluginLog.Debug($"MessageMatchesListing: {listing.Name} - {pfComment}");

            if (pfComment.Equals("None"))
            {
                return listing.Description.TextValue.Equals("");
            }
            PluginLog.Debug($"MessageMatchesListing: {listing.Description.TextValue.Equals(pfComment)}");
            PluginLog.Debug($"MessageMatchesListing: {listing.Name.TextValue.Equals(partyFinderLeaderName)}");

            return listing.Description.TextValue.Equals(pfComment) && listing.Name.TextValue.Equals(partyFinderLeaderName);
        }

        public void Dispose()
        {

            // Unregister all actions to not leak anything during disposal of plugin
            PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

            WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            CommandManager.RemoveHandler(CommandNamePFInfo);
            CommandManager.RemoveHandler(CommandNamePFInfoConfig);

            PartyFinderGui.ReceiveListing -= this.OnListing;

            ChatGui.ChatMessage -= OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void OnCommandConfig(string command, string args)
        {
            // in response to the slash command, just display our main ui
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        public void ToggleMainUi() => MainWindow.Toggle();
    }
}
