using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Party Finder Info - Configuration")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Show:");

        // can't ref a property, so use a local copy
        var showNameValue = configuration.showName;
        if (ImGui.Checkbox("Leader Name", ref showNameValue))
        {
            configuration.showName = showNameValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            configuration.Save();
        }

        var showObjectiveValue = configuration.showObjective;
        if (ImGui.Checkbox("Objective", ref showObjectiveValue))
        {
            configuration.showObjective = showObjectiveValue;
            configuration.Save();
        }

        var showMinIlvl = configuration.showMinIlvl;
        if (ImGui.Checkbox("Minimum Item Level", ref showMinIlvl))
        {
            configuration.showMinIlvl = showMinIlvl;
            configuration.Save();
        }

        var showDescriptionValue = configuration.showDescription;
        if (ImGui.Checkbox("Description", ref showDescriptionValue))
        {
            configuration.showDescription = showDescriptionValue;
            configuration.Save();
        }
    }
}
