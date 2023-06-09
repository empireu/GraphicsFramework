﻿using ImGuiNET;
using System.Numerics;

using ImGuiN = ImGuiNET.ImGui;

namespace GameFramework.ImGui;

public static class ImGuiStyles
{
    public static void Dark(float alpha = 0.9f)
    {
        var style = ImGuiN.GetStyle();

        style.Alpha = 0.9f;
        style.FrameRounding = 3.0f;
        style.WindowRounding = 5;
        style.AntiAliasedFill = true;
        style.AntiAliasedLines = true;
        style.TabRounding = 3;

        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.94f, 0.94f, 0.94f, 0.94f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(1.00f, 1.00f, 1.00f, 0.94f);
        style.Colors[(int)ImGuiCol.Border] = new Vector4(0.00f, 0.00f, 0.00f, 0.39f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(1.00f, 1.00f, 1.00f, 0.10f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(1.00f, 1.00f, 1.00f, 0.94f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.40f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.96f, 0.96f, 0.96f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 1.00f, 1.00f, 0.51f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.82f, 0.82f, 0.82f, 1.00f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.86f, 0.86f, 0.86f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.98f, 0.98f, 0.98f, 0.53f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.69f, 0.69f, 0.69f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.59f, 0.59f, 0.59f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.49f, 0.49f, 0.49f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.24f, 0.52f, 0.88f, 1.00f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.26f, 0.59f, 0.98f, 0.40f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.06f, 0.53f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.26f, 0.59f, 0.98f, 0.31f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.80f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.00f, 1.00f, 1.00f, 0.50f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.39f, 0.39f, 0.39f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0, 0, 0, 0);

        for (var i = 0; i < Enum.GetValues<ImGuiCol>().Length - 1; i++)
        {
            ref var color = ref style.Colors[i];
            ImGuiN.ColorConvertRGBtoHSV(color.X, color.Y, color.Z, out var h, out var s, out var v);

            if (s < 0.1f)
            {
                v = 1.0f - v;
            }
            ImGuiN.ColorConvertHSVtoRGB(h, s, v, out color.X, out color.Y, out color.Z);

            if (color.W < 1.00f)
            {
                color.W *= alpha;
            }
        }
    }

}