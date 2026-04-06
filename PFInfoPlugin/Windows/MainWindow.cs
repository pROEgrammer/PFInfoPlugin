using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base(
        "Party Finder Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        IPartyFinderListing listing = plugin.pfListing;
        List<IPartyFinderListing> pfListingsJoined = plugin.pfListingsJoined;

        if (listing != null)
        {
            if (plugin.Configuration.showName)
                ImGui.Text($"Name: {listing.Name}");

            if (plugin.Configuration.showObjective)
                ImGui.Text($"Objective: {listing.Objective}");

            if (plugin.Configuration.showMinIlvl)
                ImGui.Text($"Minimum Item Level: {listing.MinimumItemLevel}");

            if (plugin.Configuration.showDescription)
            {
                String description = plugin.pfListing.Description.TextValue;
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
        ImGui.Text($"---");
        ImGui.Text($"Total Party Finders Joined: {pfListingsJoined.Count}");
    }

    void CopyToClipboard()
    {
        //PluginLog.LogDebug($"Copying to clipboard: " + Plugin.pfListing.Description.TextValue);
        ImGui.SetClipboardText(plugin.pfListing.Description.TextValue);
        //PluginLog.LogDebug($"Copied successfully");
    }
}
