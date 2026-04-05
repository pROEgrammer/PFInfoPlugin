using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using Serilog;
using System;
using System.Collections.Generic;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Party Finder Info";
        private const string CommandNamePFInfo = "/pfinfo";
        private const string CommandNamePFInfoConfig = "/pfinfoconfig";

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Party Finder Info");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        [PluginService]
        internal static IPartyFinderGui PartyFinderGui { get; set; } = null!;
        [PluginService]
        internal static IChatGui ChatGui { get; set; } = null!;

        private List<IPartyFinderListing> pfListings { get; set; } = new();

        public IPartyFinderListing pfListing = null;
        private Boolean isDescriptionIncoming = false;

        public Plugin(
/*          
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ChatGui chatGui
*/
            )
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            //this.Configuration.Initialize(this.PluginInterface);

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

            Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}=== (means it's working)");
        }

        private void OnListing(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
        {
            this.pfListings.Add(listing);
        }

        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            try
            {
                PluginLog.Debug($"Chat Type: " + type);
                PluginLog.Debug($"SysMsg: " + XivChatType.SystemMessage);

                if (this.isDescriptionIncoming && XivChatType.SystemMessage.Equals(type))
                {
                    PluginLog.Debug("Comment coming up!");
                    var pfComment = message.TextValue;
                    PluginLog.Information($"PF INFO COMMENT: " + pfComment);
                    this.isDescriptionIncoming = false;
                    PluginLog.Debug("Iterating through listings");

                    foreach (var listing in this.pfListings)
                    {
                        PluginLog.Debug($"Listing name: " + listing.Name + " - " + listing.Description);

                        if (MessageMatchesListing(listing, message))
                        {
                            PluginLog.Debug("Matched");

                            this.pfListing = listing;
                            this.pfListings = new();
                            break;
                        }
                    }
                }
                else
                {
                    PluginLog.Debug($"Chat Message: " + message);

                    if (message.TextValue.Contains("■Comment")) // TODO: Localization?
                    {
                        this.isDescriptionIncoming = true;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"OnChatMessage Error: {ex}");
            }
        }

        private Boolean MessageMatchesListing(IPartyFinderListing listing, SeString message)
        {
            if (message.TextValue.Equals("None"))
            {
                return listing.Description.TextValue.Equals("");
            }
            return listing.Description.TextValue.Equals(message.TextValue);
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
