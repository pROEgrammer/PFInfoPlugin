using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using SamplePlugin.Windows;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Party Finder Info";
        private const string CommandNamePFInfo = "/pfinfo";
        private const string CommandNamePFInfoConfig = "/pfinfoconfig";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Party Finder Info");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        [PluginService]
        internal static PartyFinderGui PartyFinderGui { get; set; } = null!;
        internal static ChatGui ChatGui { get; set; } = null!;

        private List<PartyFinderListing> pfListings { get; set; } = new();

        public PartyFinderListing pfListing = null;
        private Boolean isDescriptionIncoming = false;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ChatGui chatGui)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandNamePFInfo, new CommandInfo(OnCommand)
            {
                HelpMessage = "Displays the PF info window."
            });
            this.CommandManager.AddHandler(CommandNamePFInfoConfig, new CommandInfo(OnCommandConfig)
            {
                HelpMessage = "Displays the PF info configuration window."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            // Hook onto PF event
            try
            {
                PartyFinderGui.ReceiveListing += this.OnListing;
            }
            catch (Exception ex)
            {
                PluginLog.LogError($"PartyFinderGui Error: {ex}");
            }

            // Hook onto chat message
            try
            {
                chatGui.ChatMessage += this.OnChatMessage;
            }
            catch (Exception ex)
            {
                PluginLog.LogError($"ChatGui Error: {ex}");
            }
        }

        private void OnListing(PartyFinderListing listing, PartyFinderListingEventArgs args)
        {
            this.pfListings.Add(listing);
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            try
            {
                PluginLog.LogDebug($"Chat Type: " + type);
                PluginLog.LogDebug($"SysMsg: " + XivChatType.SystemMessage);

                if (this.isDescriptionIncoming && XivChatType.SystemMessage.Equals(type))
                {
                    PluginLog.LogDebug("Comment coming up!");
                    var pfComment = message.TextValue;
                    PluginLog.LogInformation($"PF INFO COMMENT: " + pfComment);
                    this.isDescriptionIncoming = false;
                    PluginLog.LogDebug("Iterating through listings");

                    foreach (var listing in this.pfListings)
                    {
                        PluginLog.LogDebug($"Listing name: " + listing.Name + " - " + listing.Description);

                        if (MessageMatchesListing(listing, message))
                        {
                            PluginLog.LogDebug("Matched");

                            this.pfListing = listing;
                            this.pfListings = new();
                            break;
                        }
                    }
                }
                else
                {
                    PluginLog.LogDebug($"Chat Message: " + message);

                    if (message.TextValue.Contains("■Comment")) // TODO: Localization?
                    {
                        this.isDescriptionIncoming = true;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.LogError($"Error: {ex}");
            }
        }

        private Boolean MessageMatchesListing(PartyFinderListing listing, SeString message)
        {
            if (message.TextValue.Equals("None"))
            {
                return listing.Description.TextValue.Equals("");
            }
            return listing.Description.TextValue.Equals(message.TextValue);
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            this.CommandManager.RemoveHandler(CommandNamePFInfo);
            this.CommandManager.RemoveHandler(CommandNamePFInfoConfig);

            PartyFinderGui.ReceiveListing -= this.OnListing;
            ChatGui.ChatMessage -= this.OnChatMessage;
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
    }
}
