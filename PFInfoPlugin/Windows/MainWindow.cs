using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;

namespace PFInfoPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base(
        "Party Finder Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 300),
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
        List<IPartyFinderListing> pfListingsJoined = plugin.pfListingsJoined.OrderByDescending(i => i).ToList();

        ImGui.Text($"Total Party Finders Joined: {pfListingsJoined.Count}");

        ImGui.PushFont(UiBuilder.IconFont);
        var configButton = FontAwesomeIcon.Cog.ToIconString();
        ImGui.SameLine();
        if (ImGui.Button(configButton))
            plugin.ConfigWindow.IsOpen = !plugin.ConfigWindow.IsOpen;
        ImGui.PopFont();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Configuration");
        }

        if (ImGui.BeginTabBar("TabBar"))
        {
            if (ImGui.BeginTabItem("Current" + "##Tab"))
            {
                if (ImGui.BeginChild("##CurrentRegion"))
                {
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
                    ImGui.EndChild();
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("History" + "##Tab"))
            {
                if (ImGui.BeginChild("##HistoryRegion"))
                {
                    if (pfListingsJoined.Count == 0)
                    {
                        ImGui.TextWrapped("No Party Finders joined - please join a Party Finder first.");
                    }

                    foreach (var joinedListing in pfListingsJoined)
                    {
                        if (ImGui.CollapsingHeader($"{joinedListing.Name} - {joinedListing.Description}")) {
                            if (plugin.Configuration.showName)
                                ImGui.Text($"Name: {joinedListing.Name}");

                            if (plugin.Configuration.showObjective)
                                ImGui.Text($"Objective: {joinedListing.Objective}");

                            if (plugin.Configuration.showMinIlvl)
                                ImGui.Text($"Minimum Item Level: {joinedListing.MinimumItemLevel}");

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
                    }


                    ImGui.EndChild();
                }
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
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
