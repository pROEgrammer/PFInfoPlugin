using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Party Finder Info - Configuration",
        ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Show:");

        // can't ref a property, so use a local copy
        var showNameValue = this.Configuration.showName;
        if (ImGui.Checkbox("Leader Name", ref showNameValue))
        {
            this.Configuration.showName = showNameValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }

        var showObjectiveValue = this.Configuration.showObjective;
        if (ImGui.Checkbox("Objective", ref showObjectiveValue))
        {
            this.Configuration.showObjective = showObjectiveValue;
            this.Configuration.Save();
        }

        var showDescriptionValue = this.Configuration.showDescription;
        if (ImGui.Checkbox("Description", ref showDescriptionValue))
        {
            this.Configuration.showDescription = showDescriptionValue;
            this.Configuration.Save();
        }
    }
}
