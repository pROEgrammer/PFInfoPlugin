using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PFInfoPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base(
        "Party Finder Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 200),
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

                ImGui.PushFont(UiBuilder.IconFont);
                var copyToClipboard = FontAwesomeIcon.Clipboard.ToIconString();
                ImGui.NewLine();
                if (ImGui.Button(copyToClipboard))
                    CopyDescriptionToClipboard();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Copy Description to Clipboard");
                }
            }
        }
        else
        {
            ImGui.TextWrapped("No Party Finder info found - please join a Party Finder first.");
        }
        ImGui.Text($"---");
        ImGui.Text($"Total Party Finders Joined: {pfListingsJoined.Count}");

        ImGui.PushFont(UiBuilder.IconFont);
        var configButton = FontAwesomeIcon.Cog.ToIconString();
        ImGui.NewLine();
        if (ImGui.Button(configButton))
            plugin.ConfigWindow.IsOpen = !plugin.ConfigWindow.IsOpen;
        ImGui.PopFont();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Configuration");
        }
    }

    void CopyDescriptionToClipboard()
    {
        if (plugin.pfListing != null)
        {
            //PluginLog.LogDebug($"Copying to clipboard: " + Plugin.pfListing.Description.TextValue);
            ImGui.SetClipboardText(plugin.pfListing.Description.TextValue);
            //PluginLog.LogDebug($"Copied successfully");
        }
    }
}
