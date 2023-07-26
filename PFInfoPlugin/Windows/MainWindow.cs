using System;
using System.Numerics;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    public MainWindow(Plugin plugin) : base(
        "Party Finder Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Plugin = plugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        PartyFinderListing listing = Plugin.pfListing;

        if (listing != null)
        {
            if (Plugin.Configuration.showName)
                ImGui.Text($"Name: {listing.Name}");

            if (Plugin.Configuration.showObjective)
                ImGui.Text($"Objective: {listing.Objective}");

            if (Plugin.Configuration.showDescription)
            {
                String description = Plugin.pfListing.Description.TextValue;
                ImGui.TextWrapped($"{description}");

                if (ImGui.Button("Copy to Clipboard"))
                {
                    CopyToClipboard();
                }
            }
        }
        else
        {
            ImGui.TextWrapped("No Party Finder info found - please join a Party Finder first.");
        }
    }
    
    void CopyToClipboard()
    {
        PluginLog.LogDebug($"Copying to clipboard: " + Plugin.pfListing.Description.TextValue);
        ImGui.SetClipboardText(Plugin.pfListing.Description.TextValue);
        PluginLog.LogDebug($"Copied successfully");
    }
}
