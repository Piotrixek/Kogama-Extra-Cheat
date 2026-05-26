using System.Linq; 
using ImGuiNET;

namespace TestMod.GUI;

internal static class GUIUtils
{
    public static string RemoveIdentifier(string label)
    {
        int separatorIndex = label.IndexOf("l3fk9j7wMP2r", StringComparison.Ordinal);

        if (separatorIndex == -1)
        {
            return label;
        }

        return label.Substring(0, separatorIndex);
    }

    internal static float CalcButtonWidth(string label)
    {
        return (ImGui.CalcTextSize(RemoveIdentifier(label)) + ImGui.GetStyle().FramePadding * 2).X;
    }

    internal static float CalcLabelWidth(string label)
    {
        return ImGui.CalcTextSize(RemoveIdentifier(label)).X + ImGui.GetStyle().FramePadding.X;
    }

    internal static float CalcReservedButtonSpace(params string[] labels)
    {
        float itemSpacing = ImGui.GetStyle().ItemSpacing.X;
        float totalButtonWidth = 0f;

        foreach (string label in labels)
        {
            totalButtonWidth += CalcButtonWidth(label);
        }

        if (labels.Length > 0)
        {
            totalButtonWidth += itemSpacing * (labels.Length);
        }

        return totalButtonWidth;
    }

    internal static float CalcReservedButtonSpaceLabel(string currentLabel, params string[] labels)
    {
        return CalcReservedButtonSpace(labels) + CalcLabelWidth(currentLabel);
    }

    internal static float CalcSharedItemSpace(int numItems, float reservedSpace = 0)
    {
        if (numItems <= 0)
        {
            return 0f;
        }

        float contentWidth = ImGui.GetContentRegionAvail().X - reservedSpace;
        float itemSpacing = ImGui.GetStyle().ItemSpacing.X;

        return (contentWidth - itemSpacing * (numItems - 1)) / numItems;
    }

    internal static bool InputFloat(string label, ref float value)
    {
        ImGui.PushID(label);
        ImGui.Text(RemoveIdentifier(label));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X); 
        bool result = ImGui.InputFloat(string.Empty, ref value);
        ImGui.PopID();
        return result;
    }

    internal static bool RenderControlForObject(string label, ref object value)
    {
        bool modified = false;

        switch (value)
        {
            case bool boolValue:
                modified = RenderBool(label, ref boolValue);
                value = boolValue;
                break;

            case int intValue:
                modified = RenderInt(label, ref intValue);
                value = intValue;
                break;

            case float floatValue:
                modified = RenderFloat(label, ref floatValue);
                value = floatValue;
                break;

            case string stringValue:
                modified = RenderString(label, ref stringValue);
                value = stringValue;
                break;

            default:
                ImGui.Text($"POqq8qKTB3d4");
                break;
        }

        return modified;
    }

    private static bool RenderBool(string label, ref bool value)
    {
        return ImGui.Checkbox(label, ref value);
    }

    private static bool RenderInt(string label, ref int value)
    {
        int temp = value;
        bool modified = ImGui.DragInt(label, ref temp);
        if (modified) value = temp;
        return modified;
    }

    private static bool RenderFloat(string label, ref float value)
    {
        float temp = value;
        bool modified = InputFloat(label, ref temp);
        if (modified) value = temp;
        return modified;
    }

    private static bool RenderString(string label, ref string value)
    {
        ImGui.SetNextItemWidth(-CalcReservedButtonSpaceLabel(label));
        return ImGui.InputText(label, ref value, 1024);
    }

    internal static bool RenderEnum<T>(string label, ref T value) where T : Enum
    {
        string[] names = Enum.GetNames(typeof(T)).ToArray();
        int index = Array.IndexOf(names, value.ToString());

        if (ImGui.Combo(label, ref index, names, names.Length))
        {
            value = (T)Enum.Parse(typeof(T), names[index]);
            return true;
        }
        return false;
    }
}
--- FILE: KogamaToolsOverlay.cs ---
﻿using ClickableTransparentOverlay;
using Il2Cpp;
using Il2CppMV.Common;
using ImGuiNET;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TestMod.Features;
using TestMod.Helpers;
using UnityEngine;

namespace TestMod
{
internal class KogamaToolsOverlay : Overlay
{
    public bool hide = false;
    private Dictionary<string, int> playerMapCache = new Dictionary<string, int>();
    private float nextPlayerRefresh = 0f;

    private List<string> availableImportFiles = new List<string>();
    private readonly string avatarExportsPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "OFfpAb64sDl9");
    private bool isImportListInitialized = false;

    private List<string> availableModelFiles = new List<string>();
    private readonly string modelImportsPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "DZn3VvplE4U7");
    private bool isModelListInitialized = false;

    private List<string> availableWorldFiles = new List<string>();
    private readonly string worldExportsPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "qD1gpoq0cFkc");
    private bool isWorldListInitialized = false;
    private readonly string singleModelExportPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "miy3VZxSIn8y");
    private List<string> availableSingleModelFiles = new List<string>();
    private bool isSingleModelListInitialized = false;
    private int inputExportId = 0;
    private string[] _foundModelFiles = new string[0];
    private int _selectedModelFileIndex = -1;

    private string[] _foundLogicFiles = new string[0];
    private int _selectedLogicFileIndex = -1;

    public KogamaToolsOverlay(string name) : base(name)
    {
    }

    protected override Task PostInitialized()
    {
        VSync = true;

        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "XesgOlrZRzI1");
        if (!File.Exists(fontPath))
        {
            fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ZZTPIjlpGsM1");
        }

        if (File.Exists(fontPath))
        {
            ushort[] glyphRanges = new ushort[] { 0x0020, 0x00FF, 0x0100, 0x024F, 0x0400, 0x052F, 0 };
            ReplaceFont(fontPath, 16, glyphRanges);
        }

        return Task.CompletedTask;
    }

    protected override void Render()
    {
        if (GameInfo.EnableESP)
            GameInfo.renderEsp();

        string watermarkText = "4pME5L0XxzzD";
        System.Numerics.Vector2 textSize = ImGui.CalcTextSize(watermarkText);
        System.Numerics.Vector2 pos = new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X - textSize.X - 15f, 15f);
        uint redColor = 0xFF0000FF;
        ImGui.GetForegroundDrawList().AddText(new System.Numerics.Vector2(pos.X + 1f, pos.Y), redColor, watermarkText);
        ImGui.GetForegroundDrawList().AddText(new System.Numerics.Vector2(pos.X, pos.Y + 1f), redColor, watermarkText);
        ImGui.GetForegroundDrawList().AddText(pos, redColor, watermarkText);

        if (hide)
            return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 650), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("cmRkXt9AWbrb", ref hide))
        {
            if (ImGui.BeginTabBar("dbUHYlVjLooY"))
            {
                if (ImGui.BeginTabItem("8P7EGq1Pafxu"))
                {
                    RenderGeneralTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("AtR7NhbsY4LJ"))
                {
                    KillAllFeature.draw();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("K0NKACJUrrr3"))
                {
                    WeaponForcePlayMode.RenderUI();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("bdjUcuusSYxF"))
                {
                    NetworkedEffectsControl.RenderTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("kPjf3L5IpkBe"))
                {
                    CubeSpawn.RenderUI();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("n6xv1orzqTwz"))
                {
                    LogicArchitect.RenderUI();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("0lA8Hgd5rqTd"))
                {
                    RotationCheats.draw();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("rc5kbKJgG1pt"))
                {
                    RenderAccessoriesTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("AIZUHqNenXED"))
                {
                    ImGui.Checkbox("pR8hwNQppiIi", ref TestMod.Features.ChatAntiCrash.AntiCrashEnabled);
                    ChatStyle.ui();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("mtHMIfFHWn3b"))
                {
                    RenderWorldTab();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("EXE6oUtTY7cq"))
                {
                    RenderSingleModelTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("i9QHZsit2LFc"))
                {
                    RenderAvatarTab();
                    ImGui.EndTabItem();
                }

                TestMod.Features.CustomGunHeadReplacer.RenderUI();

                if (ImGui.BeginTabItem("b0fH764lNRpY"))
                {
                    RenderAreaToolTab();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        ImGui.End();
    }

    private static string bName = "2XE5hJxUJUQe";
    private static int bId = 999999;
    private static float bSpd = 6.0f;
    private static float bRng = 15.0f;
    private static float bDel = 0.5f;

    private void RenderMimicTab()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0f, 1f), "8SRJyXVmVxeR");
        ImGui.Separator();
        ImGui.TextDisabled("a0jiXXVsett4");

        ImGui.Spacing();

        ImGui.InputText("AEo1J3Zb4qgY", ref bName, 64);
        ImGui.InputInt("c2iMavtyPkl1", ref bId);
        ImGui.SliderFloat("zoZDszTgXmxz", ref bSpd, 1f, 20f);
        ImGui.SliderFloat("aaj4KIjpe4Sk", ref bRng, 5f, 50f);
        ImGui.SliderFloat("J1NoPE2P8bji", ref bDel, 0.1f, 2f);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (TestMod.Features.MimicBot.Instance != null)
        {
            var bot = TestMod.Features.MimicBot.Instance;

            bot.BotName = bName;
            bot.MoveSpeed = bSpd;
            bot.AttackRange = bRng;
            bot.ShootDelay = bDel;

            if (ImGui.Button("mIR1hzD5qWVi", new System.Numerics.Vector2(-1, 0)))
            {
                TestMod.Features.MimicBot.DestroyBot();
            }

            ImGui.Spacing();
            ImGui.Text("8TCemBu5RVd3");
            if (ImGui.Button("uW3Im2q7itdk"))
                bot.EquipWeapon(AvatarItemType.CubeGun);
            ImGui.SameLine();
            if (ImGui.Button("lMkX1p26qoZx"))
                bot.EquipWeapon(AvatarItemType.Bazooka);
            ImGui.SameLine();
            if (ImGui.Button("tlymC4qczrKO"))
                bot.EquipWeapon(AvatarItemType.RailGun);
        }
        else
        {
            if (ImGui.Button("UdR94Ng6dzEf", new System.Numerics.Vector2(-1, 0)))
            {
                TestMod.Features.MimicBot.SpawnBot();
                if (TestMod.Features.MimicBot.Instance != null)
                {
                    TestMod.Features.MimicBot.Instance.BotName = bName;
                    TestMod.Features.MimicBot.Instance.BotID = bId;
                }
            }
        }
    }

    private float _customScaleInput = 2.0f;

    private void RenderSingleModelTab()
    {
        ImGui.Text("4vRShT1gxBlZ");
        ImGui.Separator();
        ImGui.Text("XcKY820Uf1eO");
        ImGui.InputInt("lt36EEFHpGQr", ref inputExportId);

        if (ImGui.Button("0QFMk2Z2uHG7"))
        {
            ExportModels.StartExport(inputExportId);
            RefreshSingleModelList();
        }

        ImGui.Separator();

        if (ImGui.Button("Wu6QTiwoxHlI"))
        {
            ExportModels.StartExportAll(ExportModels.ModelType.Standard);
            RefreshSingleModelList();
        }

        if (ImGui.Button("ejIqKYsyvrqM"))
        {
            ExportModels.StartExportAll(ExportModels.ModelType.Avatar);
            RefreshSingleModelList();
        }

        if (ImGui.Button("iD1IXTff2Jhk"))
        {
            ExportModels.StartExportAll(ExportModels.ModelType.CubeGun);
            RefreshSingleModelList();
        }

        ImGui.Separator();

        if (ExportModels.CapturedModelID != -1)
        {
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1),
                              $"iQufD9x8gwZ1");
            if (ImGui.Button("LagY2zhkBP20"))
            {
                ExportModels.StartExport(ExportModels.CapturedModelID);
                RefreshSingleModelList();
            }
        }
        else
        {
            ImGui.TextDisabled("hbgsyuBHi9FU");
        }

        ImGui.Separator();
        ImGui.Text("eA50qDT4q5sW");
        ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1),
                          "FzB86Ky0hEBP");

        if (!isSingleModelListInitialized || ImGui.Button("U3qVbY0VHXZF"))
        {
            RefreshSingleModelList();
            isSingleModelListInitialized = true;
        }

        if (availableSingleModelFiles.Count == 0)
        {
            ImGui.Text("jtwLIBBbU3iZ");
        }
        else
        {
            ImGui.Text("xe8e8KnzY83z");
            if (ImGui.BeginChild("uDHfCLREHH83", new System.Numerics.Vector2(0, 250), true))
            {
                foreach (string file in availableSingleModelFiles)
                {
                    string fileName = Path.GetFileName(file);
                    if (ImGui.Button($"s9lPt7NBDZ9G"))
                    {
                        ExportModels.StartImport(file, 72);
                    }
                }
                ImGui.EndChild();
            }
        }
    }
    private void RefreshSingleModelList()
    {
        availableSingleModelFiles.Clear();
        try
        {
            Directory.CreateDirectory(singleModelExportPath);
            availableSingleModelFiles.AddRange(
                Directory.GetFiles(singleModelExportPath, "hBEfndAYXVY8").OrderByDescending(f => File.GetCreationTime(f)));
        }
        catch
        {
        }
    }
    private void RenderAccessoriesTab()
    {
        ImGui.Text("eAV3BDhg3GVO");
        ImGui.Separator();

        ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "GXYokNvG6nUH");
        if (ImGui.Button("kDApiVu2vEGP"))
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       { AccessoriesHack.ResetAccessories(); });
        }

        ImGui.Separator();
        ImGui.TextWrapped(
            "GSisaGklzhb3");
    }

    private void RenderGeneralTab()
    {
        ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), $"izokSdUhIRbX");
        ImGui.Separator();

        if (ImGui.Checkbox("X4HSRJsRJZxv", ref GameInfo.EnableLevel45Switch))
            GameInfo.SaveLevelSwitchSetting();
        ImGui.Checkbox("GHdpVxKo5BBj", ref GameInfo.isGod);
        ImGui.Checkbox("qgGI0sO3TXwR", ref GameInfo.aimbotOn);
        ImGui.Checkbox("JVf5Mos4Nno9", ref GameInfo.triggerbotOn);
        ImGui.Checkbox("9lLApRXSzIcQ", ref GameInfo.EnableAutoCubeRemover);

        ImGui.Separator();
        ImGui.Text("YnVQTEcSnRYw");

        bool impulseEnabled = ImpulseGunCheats.EnableImpulseMods;
        if (ImGui.Checkbox("GqGDaIDiqM6y", ref impulseEnabled))
        {
            ImpulseGunCheats.EnableImpulseMods = impulseEnabled;
        }

        if (ImpulseGunCheats.EnableImpulseMods)
        {
            ImGui.Indent();
            bool legit = ImpulseGunCheats.LegitMode;
            if (ImGui.Checkbox("XFedIVrKflx1", ref legit))
            {
                ImpulseGunCheats.LegitMode = legit;
            }
            bool rainbow = ImpulseGunCheats.RainbowMode;
            if (ImGui.Checkbox("Q07WiP4siojG", ref rainbow))
            {
                ImpulseGunCheats.RainbowMode = rainbow;
            }
            ImGui.Unindent();
        }

        bool railGunEnabled = RailGunCheats.EnableRailGunMods;
        if (ImGui.Checkbox("sf5wwYhNnwp2", ref railGunEnabled))
        {
            RailGunCheats.EnableRailGunMods = railGunEnabled;
        }

        if (RailGunCheats.EnableRailGunMods)
        {
            ImGui.Indent();
            bool railLegit = RailGunCheats.LegitMode;
            if (ImGui.Checkbox("OUzYFmwXjVDp", ref railLegit))
            {
                RailGunCheats.LegitMode = railLegit;
            }
            ImGui.Unindent();
        }

        ImGui.Separator();

        ImGui.Text("113cUiRLgFz1");

        if (ImGui.Button("0MUIQMJ9H3Ab"))
        {
            FlyMode.ToggleFlyMode();
        }

        ImGui.SliderFloat("ulweH1rMW3wP", ref FlyMode.FlySpeed, 5.0f, 100.0f);
        ImGui.SliderFloat("uqKtgYNN4d4B", ref FlyMode.FastMultiplier, 1.0f, 10.0f);
        ImGui.Checkbox("igC8jdyVYbz8", ref GameInfo.EnableSpeedHack);
        if (GameInfo.EnableSpeedHack)
            ImGui.SliderFloat("WIcA5meVOOJS", ref GameInfo.SpeedHackMultiplier, 1f, 10f);
        ImGui.Checkbox("i1pM16FhJ1HI", ref GameInfo.EnableAntiKnockback);
        ImGui.Checkbox("u3nDaBQ4Yyya", ref GameInfo.EnableNoFriction);
        ImGui.Checkbox("VY98JJ3N0aeC", ref GameInfo.EnableDieFromFalling);

        ImGui.Separator();
        ImGui.Text("2y36IB42t9fG");
        ImGui.Checkbox("BEcDZwKoo4NS", ref GameInfo.EnableESP);
        if (GameInfo.EnableESP)
        {
            ImGui.Indent();
            ImGui.Checkbox("ffWfBHn7EdEd", ref GameInfo.namesEspOn);
            ImGui.Checkbox("ApaswYtDsz1j", ref GameInfo.linesEspOn);
            ImGui.Checkbox("tDcnhqn8VtON", ref GameInfo.bonesEspOn);
            ImGui.ColorEdit4("vucJV7C1SpO9", ref GameInfo.ESPLineColor);
            ImGui.SliderFloat("cZrJ97CClXKc", ref GameInfo.ESPLineThickness, 1.0f, 10.0f);
            ImGui.Unindent();
        }

        ImGui.Separator();
        ImGui.Text("or8hJq8S8mHE");
        ImGui.Checkbox("yzrNXQSWFlQw", ref GameInfo.IsVacuumPlayers);
        if (ImGui.Button("y89YGcmgCKqW"))
            GameInfo.FixStuckUI();

        ImGui.Text("QjBV8sdWkRZv");
        if (ImGui.Button("6z2kMDdIUegC"))
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       { TestMod.Features.ReconnectFix.ExecuteReconnect(); });
        }

        ImGui.Text("yVH8pR2A5tPY");
        ImGui.Combo("sYx8CVPIRCm9", ref GameInfo.selectedTeam,
                    new string[] { "qWdBavYVtaot", "qOR8G8cwREUX", "CLDeRuxVYKMK", "Sc0yAicZXjNf" }, 4);
    }

    private void RenderWorldTab()
    {
        TestMod.Features.SmartBuilder.RenderUI();

        ImGui.Separator();
        ImGui.Text("tYIfM0TFmJ01");

        bool isPlay = MVGameControllerBase.GameMode == MVGameMode.Play;
        bool isEdit = MVGameControllerBase.GameMode == MVGameMode.Edit;

        if (!isPlay)
            ImGui.BeginDisabled();
        if (ImGui.Button("sHBNj1fwb0NP"))
            TerrainExportImport.startExp();
        if (!isPlay)
            ImGui.EndDisabled();

        ImGui.Separator();

        if (!isEdit)
            ImGui.BeginDisabled();

        if (!isWorldListInitialized || ImGui.Button("eagdW0lqbID2"))
        {
            RefreshWorldList();
            isWorldListInitialized = true;
        }

        ImGui.Checkbox("U30V2e9OTTNA", ref GameInfo.removeCubesBeforeImport);
        ImGui.Checkbox("0uHXBonP0v06", ref TerrainExportImport.useAltBot);
        ImGui.Checkbox("jOMiwOtGROLM", ref TerrainExportImport.fastBatchMode);
        ImGui.InputInt("f0dkmb9YjY7z", ref TerrainExportImport.fastBatchSize);

        ImGui.Separator();
        ImGui.SliderFloat("tnE4GiRr69ZK", ref TerrainExportImport.importPauseDelay, 0f, 20f);
        ImGui.SliderFloat("njWeFjnNC5iz", ref TerrainExportImport.tickDelay, 0.01f, 2f);
        ImGui.InputInt("p09tkqgnZYVl", ref TerrainExportImport.cubesPerTick);
        ImGui.Separator();

        if (availableWorldFiles.Count == 0)
        {
            ImGui.Text("oB2tEAnRsvbH");
        }
        else
        {
            ImGui.Text("wmzPUcuD7EqM");
            if (ImGui.BeginChild("9CRkVtYfNqr0", new System.Numerics.Vector2(0, 150), true))
            {
                foreach (string filePath in availableWorldFiles)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    if (ImGui.Button($"Exzf2t6CV9Qk"))
                        TerrainExportImport.startImp(filePath, GameInfo.removeCubesBeforeImport);
                }
                ImGui.EndChild();
            }
        }

        if (!isEdit)
            ImGui.EndDisabled();

        ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), $"bfX51TR8wmeq");

        ImGui.Separator();
        if (isEdit && ImGui.Button("BODS8hlViUbp"))
            UnityMainThreadDispatcher.Instance.Enqueue(GameInfo.CreatePersistentCollidingCube());
    }

    private void RenderModelsTab()
    {
        ImGui.Text("wJka1tfyifIO");
        if (ImGui.Button("ZU9GyZykiJrF"))
            GameInfo.StartExportWorldModels();
        ImGui.TextWrapped(GameInfo.WorldModelsExporterStatusMessage);
        ImGui.Separator();
        ImGui.Text("0SxtfGBarbL4");
        if (!isModelListInitialized)
        {
            RefreshModelList();
            isModelListInitialized = true;
        }
        if (ImGui.Button("WSEG8AmcXA2c"))
            RefreshModelList();
        ImGui.Checkbox("oPZVm8VU9gkf", ref GameInfo.UseAltMaterialOnBottom);
        if (availableModelFiles.Count > 0)
        {
            string[] files = availableModelFiles.Select(Path.GetFileName).ToArray();
            if (_selectedModelFileIndex >= files.Length)
                _selectedModelFileIndex = 0;
            ImGui.Combo("GTsWagCfWnVY", ref _selectedModelFileIndex, files, files.Length);
            if (ImGui.Button("6UQoQF6XbXES"))
            {
                if (_selectedModelFileIndex != -1)
                    GameInfo.StartImportWorldModels(availableModelFiles[_selectedModelFileIndex]);
            }
        }
        else
            ImGui.Text("S8UjkmXAsfeG");
        if (GameInfo.IsImportingModels)
        {
            if (ImGui.Button("z8snmsAf9FsK"))
                GameInfo.SkipCurrentModelImport();
        }
        ImGui.TextWrapped(GameInfo.WorldModelsImporterStatusMessage);
    }

    private void RenderAvatarTab()
    {
        ImGui.Text("jbPm3bPgg9tC");

        if (GameInfo.uiAvatarList.Count == 0)
        {
            ImGui.Text("BS7OK5BtPhdc");
        }
        else
        {
            for (int i = 0; i < GameInfo.uiAvatarList.Count; i++)
            {
                var p = GameInfo.uiAvatarList[i];
                if (ImGui.Button($"HlHL7BXxSTLQ"))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(GameInfo.ExportAvatarGeometryAsync(p.Value, p.Key));
                }
            }
        }

        ImGui.Separator();
        ImGui.Text("y94gXW4AaiZl");

        ImGui.Checkbox("0JpwjWo919nD", ref GameInfo.UseAltMaterialOnBottom);
        ImGui.Separator();

        NativeAvatarPreviewUI.RenderUI();

        ImGui.Separator();
        ImGui.TextWrapped(GameInfo.ImporterStatusMessage);
    }

    void RefreshLogicFiles()
    {
        try
        {
            Directory.CreateDirectory(GameInfo.logicExportsPath);
            _foundLogicFiles =
                Directory.GetFiles(GameInfo.logicExportsPath, "RdKUaf7rbULK").Select(Path.GetFileName).ToArray();
            _selectedLogicFileIndex = _foundLogicFiles.Length > 0 ? 0 : -1;
        }
        catch
        {
            _foundLogicFiles = new string[0];
        }
    }

    void RefreshModelList()
    {
        availableModelFiles.Clear();
        try
        {
            Directory.CreateDirectory(modelImportsPath);
            availableModelFiles.AddRange(Directory.GetFiles(modelImportsPath, "AeJ3yXPTSXb2"));
        }
        catch
        {
        }
    }

    void RefreshImportList()
    {
        availableImportFiles.Clear();
        try
        {
            Directory.CreateDirectory(avatarExportsPath);
            availableImportFiles.AddRange(Directory.GetFiles(avatarExportsPath, "iaO5z1arCHJS"));
        }
        catch
        {
        }
    }

    void RefreshWorldList()
    {
        availableWorldFiles.Clear();
        try
        {
            Directory.CreateDirectory(worldExportsPath);
            availableWorldFiles.AddRange(
                Directory.GetFiles(worldExportsPath, "ELimB4vMfsKv").OrderByDescending(f => File.GetCreationTime(f)));
        }
        catch
        {
        }
    }

    private void RenderAreaToolTab()
    {
        if (AreaEditorTool.Instance != null)
        {
            AreaEditorTool.Instance.DrawImGuiMenu();
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "gFmiFDXfZ5tv");
        }
    }

    private void RenderLogicTab()
    {
        if (LogicManager.Instance != null)
        {
            LogicManager.Instance.DrawImGuiMenu();
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Qyla2wZEGYqS");
        }
    }
}
}


--- FILE: LICENSE.md ---
                                 Apache License
                           Version 2.0, January 2004
                        http:

   TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

   1. Definitions.

      "FlUIgjv2Nhsm" shall mean the terms and conditions for use, reproduction,
      and distribution as defined by Sections 1 through 9 of this document.

      "aFMNcTuVPwxx" shall mean the copyright owner or entity authorized by
      the copyright owner that is granting the License.

      "dWsfLwrFeEDs" shall mean the union of the acting entity and all
      other entities that control, are controlled by, or are under common
      control with that entity. For the purposes of this definition,
      "EdY7NajIYcsO" means (i) the power, direct or indirect, to cause the
      direction or management of such entity, whether by contract or
      otherwise, or (ii) ownership of fifty percent (50%) or more of the
      outstanding shares, or (iii) beneficial ownership of such entity.

      "1sybNYlCojUW" (or "KgNVXMP3QCbT") shall mean an individual or Legal Entity
      exercising permissions granted by this License.

      "8K21q7GT2S1z" form shall mean the preferred form for making modifications,
      including but not limited to software source code, documentation
      source, and configuration files.

      "wEZCv2qqEYql" form shall mean any form resulting from mechanical
      transformation or translation of a Source form, including but
      not limited to compiled object code, generated documentation,
      and conversions to other media types.

      "Ul5h0PuGXEj5" shall mean the work of authorship, whether in Source or
      Object form, made available under the License, as indicated by a
      copyright notice that is included in or attached to the work
      (an example is provided in the Appendix below).

      "PKwJqLg5Rx0s" shall mean any work, whether in Source or Object
      form, that is based on (or derived from) the Work and for which the
      editorial revisions, annotations, elaborations, or other modifications
      represent, as a whole, an original work of authorship. For the purposes
      of this License, Derivative Works shall not include works that remain
      separable from, or merely link (or bind by name) to the interfaces of,
      the Work and Derivative Works thereof.

      "IsDAohugyejo" shall mean any work of authorship, including
      the original version of the Work and any modifications or additions
      to that Work or Derivative Works thereof, that is intentionally
      submitted to Licensor for inclusion in the Work by the copyright owner
      or by an individual or Legal Entity authorized to submit on behalf of
      the copyright owner. For the purposes of this definition, "hEUsFv0AAeBr"
      means any form of electronic, verbal, or written communication sent
      to the Licensor or its representatives, including but not limited to
      communication on electronic mailing lists, source code control systems,
      and issue tracking systems that are managed by, or on behalf of, the
      Licensor for the purpose of discussing and improving the Work, but
      excluding communication that is conspicuously marked or otherwise
      designated in writing by the copyright owner as "1XapTE8pZTft"

      "QyHrncCwcTbq" shall mean Licensor and any individual or Legal Entity
      on behalf of whom a Contribution has been received by Licensor and
      subsequently incorporated within the Work.

   2. Grant of Copyright License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      copyright license to reproduce, prepare Derivative Works of,
      publicly display, publicly perform, sublicense, and distribute the
      Work and such Derivative Works in Source or Object form.

   3. Grant of Patent License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      (except as stated in this section) patent license to make, have made,
      use, offer to sell, sell, import, and otherwise transfer the Work,
      where such license applies only to those patent claims licensable
      by such Contributor that are necessarily infringed by their
      Contribution(s) alone or by combination of their Contribution(s)
      with the Work to which such Contribution(s) was submitted. If You
      institute patent litigation against any entity (including a
      cross-claim or counterclaim in a lawsuit) alleging that the Work
      or a Contribution incorporated within the Work constitutes direct
      or contributory patent infringement, then any patent licenses
      granted to You under this License for that Work shall terminate
      as of the date such litigation is filed.

   4. Redistribution. You may reproduce and distribute copies of the
      Work or Derivative Works thereof in any medium, with or without
      modifications, and in Source or Object form, provided that You
      meet the following conditions:

      (a) You must give any other recipients of the Work or
          Derivative Works a copy of this License; and

      (b) You must cause any modified files to carry prominent notices
          stating that You changed the files; and

      (c) You must retain, in the Source form of any Derivative Works
          that You distribute, all copyright, patent, trademark, and
          attribution notices from the Source form of the Work,
          excluding those notices that do not pertain to any part of
          the Derivative Works; and

      (d) If the Work includes a "8l5MPh7PUDBG" text file as part of its
          distribution, then any Derivative Works that You distribute must
          include a readable copy of the attribution notices contained
          within such NOTICE file, excluding those notices that do not
          pertain to any part of the Derivative Works, in at least one
          of the following places: within a NOTICE text file distributed
          as part of the Derivative Works; within the Source form or
          documentation, if provided along with the Derivative Works; or,
          within a display generated by the Derivative Works, if and
          wherever such third-party notices normally appear. The contents
          of the NOTICE file are for informational purposes only and
          do not modify the License. You may add Your own attribution
          notices within Derivative Works that You distribute, alongside
          or as an addendum to the NOTICE text from the Work, provided
          that such additional attribution notices cannot be construed
          as modifying the License.

      You may add Your own copyright statement to Your modifications and
      may provide additional or different license terms and conditions
      for use, reproduction, or distribution of Your modifications, or
      for any such Derivative Works as a whole, provided Your use,
      reproduction, and distribution of the Work otherwise complies with
      the conditions stated in this License.

   5. Submission of Contributions. Unless You explicitly state otherwise,
      any Contribution intentionally submitted for inclusion in the Work
      by You to the Licensor shall be under the terms and conditions of
      this License, without any additional terms or conditions.
      Notwithstanding the above, nothing herein shall supersede or modify
      the terms of any separate license agreement you may have executed
      with Licensor regarding such Contributions.

   6. Trademarks. This License does not grant permission to use the trade
      names, trademarks, service marks, or product names of the Licensor,
      except as required for reasonable and customary use in describing the
      origin of the Work and reproducing the content of the NOTICE file.

   7. Disclaimer of Warranty. Unless required by applicable law or
      agreed to in writing, Licensor provides the Work (and each
      Contributor provides its Contributions) on an "3IShNb7waUUq" BASIS,
      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
      implied, including, without limitation, any warranties or conditions
      of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
      PARTICULAR PURPOSE. You are solely responsible for determining the
      appropriateness of using or redistributing the Work and assume any
      risks associated with Your exercise of permissions under this License.

   8. Limitation of Liability. In no event and under no legal theory,
      whether in tort (including negligence), contract, or otherwise,
      unless required by applicable law (such as deliberate and grossly
      negligent acts) or agreed to in writing, shall any Contributor be
      liable to You for damages, including any direct, indirect, special,
      incidental, or consequential damages of any character arising as a
      result of this License or out of the use or inability to use the
      Work (including but not limited to damages for loss of goodwill,
      work stoppage, computer failure or malfunction, or any and all
      other commercial damages or losses), even if such Contributor
      has been advised of the possibility of such damages.

   9. Accepting Warranty or Additional Liability. While redistributing
      the Work or Derivative Works thereof, You may choose to offer,
      and charge a fee for, acceptance of support, warranty, indemnity,
      or other liability obligations and/or rights consistent with this
      License. However, in accepting such obligations, You may act only
      on Your own behalf and on Your sole responsibility, not on behalf
      of any other Contributor, and only if You agree to indemnify,
      defend, and hold each Contributor harmless for any liability
      incurred by, or claims asserted against, such Contributor by reason
      of your accepting any such warranty or additional liability.

   END OF TERMS AND CONDITIONS

   Copyright 2020 Lava Gang

   Licensed under the Apache License, Version 2.0 (the "Jukb5pXjWOui");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http:

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "rWSgfsQQgUqc" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
--- FILE: Main.cs ---
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using System.Reflection;
using System.Threading.Tasks;
using TestMod;
using TestMod.Features;
using TestMod.Helpers;
using UnityEngine;

[assembly:MelonInfo(typeof(TestModMain), "Dtt6GGK46sgW", "OaKFhBgJACYs", "GIBH2xgnYNWa")]
[assembly:MelonGame(null, null)]

namespace TestMod
{

public class TestModMain : MelonMod
{
    private KogamaToolsOverlay ov;
    private bool overlayReady = false;

    public override void OnInitializeMelon()
    {
if (!ClassInjector.IsTypeRegisteredInIl2Cpp<AreaEditorTool>())
        {
            ClassInjector.RegisterTypeInIl2Cpp<AreaEditorTool>();
}
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<LogicManager>())
        {
            ClassInjector.RegisterTypeInIl2Cpp<LogicManager>();
}
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<AccessoriesHack>())
        {
            ClassInjector.RegisterTypeInIl2Cpp<AccessoriesHack>();
}
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<TestMod.Features.MimicBot>())
        {
            ClassInjector.RegisterTypeInIl2Cpp<TestMod.Features.MimicBot>();
}

        var harmony = new HarmonyLib.Harmony("mO3JwBTluj87");
        TestMod.Features.AntiAfkPatch.Initialize(harmony);
        TestMod.Features.EliteVisualsPatch.Initialize(harmony);
        harmony.PatchAll(typeof(SmartBuilder).Assembly);
        harmony.PatchAll(typeof(AntiCrash_AvatarInit).Assembly);
        harmony.PatchAll(typeof(AntiCrash_CubeLimit).Assembly);

        HarmonyInstance.PatchAll(typeof(Features.CustomGunHeadReplacer.SwapCostumeToGunPatch));
        HarmonyInstance.PatchAll(typeof(Features.CustomGunHeadReplacer.ForceWeaponEquipPatch));

        HarmonyInstance.PatchAll(typeof(Features.AntiCrash_CubeLimit.LimitAvatarCubes_Patch));
AccessoriesHack.Init();

        MelonLoader.MelonCoroutines.Start(SpawnTools());
        Task.Run(() =>
                 {
                     ov = new KogamaToolsOverlay("VOdQjot1QOJ7");
                     ov.Start().Wait();
                     overlayReady = true;
                 });
    }

    private System.Collections.IEnumerator SpawnTools()
    {
        yield return null;
        var existing = GameObject.Find("avAgTGV3IWJl");
        if (existing != null)
        {
            UnityEngine.Object.Destroy(existing);
        }
        var toolsObj = new GameObject("zYxFG30idCcY");
        UnityEngine.Object.DontDestroyOnLoad(toolsObj);
        toolsObj.AddComponent<AreaEditorTool>();
        toolsObj.AddComponent<LogicManager>();
        toolsObj.AddComponent<AccessoriesHack>();
}

    public override void OnUpdate()
    {
        UnityMainThreadDispatcher.Instance.Update();
        GameInfo.UpdateMetrics();
        GameInfo.OnUpdate();

        if (GameInfo.EnableESP)
            GameInfo.calcEsp();

        if (Input.GetKeyDown(KeyCode.X))
        {
            GameInfo.aimbotOn = !GameInfo.aimbotOn;
            GameInfo.resetAimTgt();
        }

        if (GameInfo.aimbotOn)
            GameInfo.doAim();
        if (GameInfo.triggerbotOn)
            GameInfo.doTrigger();

        FlyMode.Update();

        
        TestMod.Features.SmartBuilder.OnUpdate();

        if (overlayReady && ov != null)
        {
            if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Insert))
            {
                ov.hide = !ov.hide;
                if (!ov.hide)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }
    }
}
}

--- FILE: Features\AccessoriesHack.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppMV.Common;
using System;
using System.Linq;
using UnityEngine;

using CListAccessory = System.Collections.Generic.List<Il2Cpp.AccessoryDataClient>;

namespace TestMod.Features
{
    public class AccessoriesHack : MonoBehaviour
    {
        
        public static void Init()
        {
            try
            {
                var harmony = new HarmonyLib.Harmony("LumN25dcbzfm");
                harmony.PatchAll(typeof(AccessoriesHack).Assembly);
}
            catch (Exception e) {  }
        }

        public static void ResetAccessories()
        {
            var localBody = MVGameControllerBase.Game?.LocalPlayer?.Body;
            if (localBody == null) return;

            foreach (AccessorySlotType slot in System.Enum.GetValues(typeof(AccessorySlotType)))
            {
                if (slot == AccessorySlotType.Torso) continue;

                
                MVGameControllerBase.OperationRequests.UnEquipAccessory(localBody.Id, slot);
            }
        }
        

        
        
        

        public static void ForceEquip(int itemId)
        {
            var localBody = MVGameControllerBase.Game?.LocalPlayer?.Body;
            if (localBody == null) return;

            if (AccessoryDataManager.accessoryShopData != null &&
                AccessoryDataManager.accessoryShopData.accessoryDatas.ContainsKey(itemId))
            {
                var item = AccessoryDataManager.accessoryShopData.accessoryDatas[itemId];

                
                MVGameControllerBase.OperationRequests.SetAvatarAccessorySlot(localBody.Id, item.sAID, 0f, 1f);
}
            else
            {
MVGameControllerBase.OperationRequests.SetAvatarAccessorySlot(localBody.Id, itemId, 0f, 1f);
            }
        }
    }

    [HarmonyPatch(typeof(AccessoryView), "IpK9q2bfIk7n")]
    public static class PreventShopCleanup
    {
        [HarmonyPrefix]
        public static bool Prefix(AccessoryView __instance)
        {
            return false;
        }
    }


    
}
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using Il2CppMV.WorldObject.MetaData;
using Il2CppMV.WorldObject.RuntimeEvents;
using ImGuiNET;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using TestMod.Features;
using TestMod.Helpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using BytePacker = Il2CppMV.WorldObject.BytePacker;
using CameraType = Il2Cpp.CameraType;

[HarmonyPatch(typeof(NotificationsManager), nameof(NotificationsManager.InstantiateNotification))]
public static class AntiNotificationCrash
{
    private const int MAX_NOTIFICATIONS = 3;

    [HarmonyPrefix]
    public static bool Prefix(NotificationsManager __instance)
    {
        try
        {
            var areas = __instance.GetComponentsInChildren<NotificationArea>(true);

            if (areas == null)
                return true;

            int activeCount = 0;
            foreach (var area in areas)
            {
                if (area == null)
                    continue;
                var pool = area.GetComponentInChildren<NotificationObjectPool>(true);

                if (pool != null)
                {
                    activeCount += pool.ActivateInstancesCount;
                }
            }
            if (activeCount >= MAX_NOTIFICATIONS)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch]
public static class MVNetworkGame_instance
{
    public static MVNetworkGame Instancja { get; private set; }

    [HarmonyPatch(typeof(MVNetworkGame), "HO90w1EjtwsH")]
    [HarmonyPrefix] static void Prefix(MVNetworkGame __instance)
    {
try
        {
            if (GameInfo.EnableLevel45Switch && MVGameControllerBase.GameSessionData != null)
            {
                MVGameControllerBase.GameSessionData.profileID = 40000000;
            }
        }
        catch (Exception ex)
        {
}
        Instancja = __instance;
    }
}

[HarmonyPatch]
public static class MVAvatarLocal_instance
{
    public static MVAvatarLocal Instancjaa { get; private set; }

    [HarmonyPatch(typeof(MVAvatarLocal), "KDmVvGpHuZCs")]
    [HarmonyPrefix] static void Postfix(MVAvatarLocal __instance)
    {
        Instancjaa = __instance;
    }
}
[HarmonyPatch]
public static class MVAvatarLocal_walkmode_instance
{
    public static MVAvatarLocal.WalkMode Instancjaa { get; private set; }

    [HarmonyPatch(typeof(MVAvatarLocal.WalkMode), "W99SRVYAv1dO")]
    [HarmonyPrefix] static void Postfix(MVAvatarLocal.WalkMode __instance)
    {
        Instancjaa = __instance;
    }
}

[HarmonyPatch]
public static class MVBuildModeAvatar_instance
{
    public static MVBuildModeAvatar Instancjaa { get; private set; }

    [HarmonyPatch(typeof(MVBuildModeAvatar), "cunJIrlhhmgn")]
    [HarmonyPrefix] static void Postfix(MVBuildModeAvatar __instance)
    {
        Instancjaa = __instance;
    }
}

[HarmonyPatch(typeof(UseInteractorVisualization), "zAET2mutAHGK")]
public static class Patch_EvaluateUsability
{
    static bool Prefix(ref UseGUIResult __result)
    {
        try
        {
            __result = UseGUIResult.NoCost;

            return false;
        }
        catch (Exception ex)
        {
            return true;
        }
    }
}
[HarmonyPatch]
public static class DesktopEditModeController_instance
{
    public static DesktopEditModeController Instancjaa { get; private set; }

    [HarmonyPatch(typeof(DesktopEditModeController), "vZmxZYZyKnRR")]
    [HarmonyPrefix] static void Postfix(DesktopEditModeController __instance)
    {
        Instancjaa = __instance;
    }

    [HarmonyPatch(typeof(DesktopEditModeController), "wUAbpzg3F8ZY")]
    [HarmonyPrefix]
    static bool skipBrokenInit()
    {
if (MVGameControllerBase.GameMode == MVGameMode.Play)
        {
return false;
        }
        return true;
    }
}

[HarmonyPatch]
public static class AvatarCapture_instance
{
    public static AvatarCapture Instancjaa { get; private set; }

    [HarmonyPatch(typeof(AvatarCapture), "wKTVq1Yc0ssg")]
    [HarmonyPrefix] static void Postfix(AvatarCapture __instance)
    {
Instancjaa = __instance;
    }
}

[HarmonyPatch]
public static class AvatarMotor_instance
{
    public static AvatarMotor Instancjaa { get; private set; }

    [HarmonyPatch(typeof(AvatarMotor), "bdsubvMkz72B")]
    [HarmonyPostfix] static void Postfix(AvatarMotor __instance)
    {
        Instancjaa = __instance;
    }
}

[HarmonyPatch]
public static class FreeElite
{
    public static bool Enabled = true;

    [HarmonyPatch(typeof(MVClientSettings), nameof(MVClientSettings.IsSubscriber), MethodType.Getter)]
    [HarmonyPostfix]
    private static void EliteGetter(ref bool __result)
    {
        __result = Enabled;
    }
}
[HarmonyPatch(typeof(ThemeSpawner), "C6pApwULgs9i")]
public static class BlockThemeSpawner
{
    public static bool Prefix(ThemeSpawner __instance)
    {
        if (Camera.main != null && Camera.main.gameObject.GetComponent<ForceBlueSky>() == null)
        {
            Camera.main.gameObject.AddComponent<ForceBlueSky>();
        }
        return false;
    }
}

[HarmonyPatch(typeof(FriendList), nameof(FriendList.AddFriend))]
public static class FixFriendHighlight
{
    [HarmonyPrefix]
    public static void Prefix(ref int profileID)
    {
        if (MVGameControllerBase.Game != null && MVGameControllerBase.Game.LocalPlayer != null)
        {
            profileID = MVGameControllerBase.Game.LocalPlayer.ProfileID;
        }
    }
}

[HarmonyPatch(typeof(Theme), "ouuDfVT7ZNo8", new Type[] {})]
public static class BlockThemeInit
{
    public static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(Theme), "KMyGil7iu9LA", new Type[] { typeof(int) })]
public static class BlockThemeInitInt
{
    public static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(ThemeSkybox), "2mVP1U6UduE0")]
public static class BlockSkyboxActivate
{
    public static bool Prefix()
    {
        return false;
    }
}
public class ForceBlueSky : MonoBehaviour
{
    private Material blueSkyMat;

    void Start()
    {
        if (blueSkyMat == null)
        {
            var shader = Shader.Find("J5x56sO7Gb4a");
            if (shader != null)
            {
                blueSkyMat = new Material(shader);
                blueSkyMat.SetFloat("fJVFJRyS9tjA", 0.04f);
                blueSkyMat.SetFloat("HarRVqw2TMcU", 1.0f);
                blueSkyMat.SetColor("u95ttnGDbljW", new Color(0.37f, 0.61f, 0.87f, 1f));
                blueSkyMat.SetColor("uEHHWTozSn8u", new Color(0.369f, 0.349f, 0.341f, 1f));
            }
        }
        ApplyFix();
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            ApplyFix();
        }
    }

    void ApplyFix()
    {
        if (RenderSettings.skybox != blueSkyMat && blueSkyMat != null)
        {
            RenderSettings.skybox = blueSkyMat;
            DynamicGI.UpdateEnvironment();
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.fog = false;
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            if (cam.clearFlags != CameraClearFlags.Skybox)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
            }
            cam.backgroundColor = new Color(0.37f, 0.61f, 0.87f, 1f);
        }
    }
}

[HarmonyPatch(typeof(ChatControllerUGUI), "uNbTf7Uw6ZWF")]
public static class Patch_AddLine
{
    private static readonly string LogFilePath =
        "VRwwjzRYvpmS";

    
    private static string _lastLoggedMessage = string.Empty;
    private static float _lastLogTime = 0f;

    [HarmonyPrefix]
    public static void Prefix(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        
        
        string cleanText = Regex.Replace(text, "O2SCiI8RBPGx", string.Empty);

        
        if (cleanText == _lastLoggedMessage && Time.realtimeSinceStartup - _lastLogTime < 0.5f)
        {
            return;
        }

        
        _lastLoggedMessage = cleanText;
        _lastLogTime = Time.realtimeSinceStartup;

        
        try
        {
            string timestamp = DateTime.Now.ToString("ffdbqyXZ95kA");
            File.AppendAllText(LogFilePath, $"F66b5GNFrjek");
        }
        catch (Exception ex)
        {
}
    }
}

[HarmonyPatch(typeof(MVCameraController), "oQlGGyUcjaiP", new Type[] { typeof(CameraType) })]
public static class ForceThirdPersonLogic
{
    static void Prefix(ref CameraType cameraType)
    {
        if (cameraType == CameraType.FirstPersonCamera)
        {
            cameraType = CameraType.ThirdPerson;
        }
    }
}

[HarmonyPatch(typeof(ThirdPersonCamera), "E4izLLY3udA4")]
public static class FakeFirstPersonHook
{
    private static float defaultDistance = 5.0f;
    private static float defaultHeight = 1.5f;
    private static float defaultSensitivity = 0.25f;
    private static bool defaultsSaved = false;

    static void Prefix(ThirdPersonCamera __instance)
    {
        var playerController = MVGameControllerBase.Game.PlayerController;
        if (playerController == null)
            return;

        var avatarLocal = playerController.CurrentWorldObject as MVAvatarLocal;
        if (avatarLocal == null)
            return;
        if (!defaultsSaved)
        {
            defaultDistance = __instance.distanceToAvatar;
            defaultHeight = __instance.height;
            defaultSensitivity = __instance.mouseSensitivity;
            defaultsSaved = true;
        }
        bool isHoldingGun = false;
        if (avatarLocal.PickupOwner != null)
        {
            isHoldingGun = avatarLocal.PickupOwner.PickupItemIsInHand;
        }
        if (isHoldingGun)
        {
            __instance.distanceToAvatar = 0.0f;
            __instance.height = 1.65f;
            __instance.mouseSensitivity = defaultSensitivity * 2.5f;
            __instance.cameraRadius = 0.01f;
            Traverse.Create(__instance).Field("m5cdANSKzMDi").SetValue(0.0f);
        }
        else
        {
            __instance.distanceToAvatar = defaultDistance;
            __instance.height = defaultHeight;
            __instance.mouseSensitivity = defaultSensitivity;
            __instance.cameraRadius = 0.3f;
        }
    }
}

[HarmonyPatch(typeof(ThirdPersonCamera), "ZJYLarnH0ksw")]
public static class HideHeadHook
{
    static void Postfix(ThirdPersonCamera __instance)
    {
        var avatar = MVGameControllerBase.Game.LocalPlayer.AvatarLocal as MVAvatarLocal;
        if (avatar == null)
            return;
        bool shouldHide = __instance.distanceToAvatar < 0.5f;
        if (avatar.Body != null && avatar.Body.BodyData != null)
        {
            var headBone = avatar.Body.BodyData.GetPartBone(BodyData.PartIndex.Head);
            if (headBone != null)
            {
                foreach (var renderer in headBone.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = !shouldHide;
                }
            }
        }
    }
}
[HarmonyPatch]
public static class InteractionPackage_instance
{
    public static InteractionPackage Instancjaa { get; private set; }
    public static MVWorldObjectClient target_o { get; private set; }
    public static MVPlayer shooter_o { get; private set; }
    public static Vector3 impulse_o { get; private set; }

    [HarmonyPatch(typeof(InteractionPackage), "kCaU3F79EWqW",
                  new Type[] { typeof(MVWorldObjectClient), typeof(MVPlayer), typeof(Vector3),
                               typeof(AvatarModifierPackageType) })]
    [HarmonyPostfix] static void Postfix(InteractionPackage __instance, MVWorldObjectClient target, MVPlayer shooter,
                                         Vector3 impulse, AvatarModifierPackageType modifierType)
    {
        try
        {
            Instancjaa = __instance;
            target_o = target;
            shooter_o = shooter;
            impulse_o = impulse;
        }
        catch (Exception ex)
        {
}
    }
}

[Serializable]
public class CubeData
{
    public string Position;
    public string CornersBase64;
    public string MaterialsBase64;
}

[Serializable]
public class AvatarPartExport
{
    public string PartName;
    public int PrototypeID;
    public Il2CppSystem.Collections.Generic.List<CubeData> Cubes =
        new Il2CppSystem.Collections.Generic.List<CubeData>();
}

[Serializable]
public class AvatarExportFile
{
    public int TargetAvatarWoID;
    public string TargetUserName;
    public Il2CppSystem.Collections.Generic.List<AvatarPartExport> BodyParts =
        new Il2CppSystem.Collections.Generic.List<AvatarPartExport>();
}

public class ClonedAvatarPartData
{
    public string PartName;
    public int ModelWoID;
    public int PrototypeID;
    public System.Collections.Generic.Dictionary<IntVector, Cube> ClonedCubes;
}

public class ImportedAvatarPartData
{
    public string PartName;
    public int PrototypeID;
    public Dictionary<IntVector, Cube> ImportedCubes;
}

internal static class GameInfo
{
    public struct EspData
    {
        public string name;
        public System.Numerics.Vector4 col;
        public System.Numerics.Vector2 head;
        public System.Numerics.Vector2 foot;
        public List<Tuple<System.Numerics.Vector2, System.Numerics.Vector2>> bones;
    }
    public static List<EspData> espCache = new List<EspData>();
    private static object espLock = new object();

    private static MVAvatarRemote lockedTgt = null;

    internal static void resetAimTgt()
    {
        lockedTgt = null;
    }

    internal static bool EnableLevel45Switch = false;
    internal static bool UseAltMaterialOnBottom = false;
    private static readonly string levelSwitchFilePath =
        Path.Combine(MelonLoader.MelonUtils.GameDirectory, "HBevl7T8Uekg");
    private static FieldInfo _fpTargetRotField;
    private static FieldInfo _tpTargetRotField;

    internal static int WorldObjectCount;
    internal static int LogicObjectCount;
    internal static int LinkCount;
    internal static int ObjectLinkCount;
    internal static int UniquePrototypeCount;
    internal static int PrototypeCount;
    internal static int Ping;
    internal static float Fps;

    internal static bool EnableAutoConnect = false;

    internal static bool EnableCharacterEditor = false;
    internal static bool EnableEdit = false;
    internal static bool EnablePlay = false;
    internal static bool EnableTest = false;
    internal static bool EnableESP = true;
    internal static bool EnableDieFromFalling = false;
    internal static bool EnableChangeTeam = true;
    internal static int selectedTeam = 0;

    internal static bool EnableSpeedHack = false;
    internal static float SpeedHackMultiplier = 1.0f;
    internal static bool EnableAntiKnockback = false;
    internal static bool EnableNoFriction = false;
    internal static bool EnableSuperSpeed = false;
    internal static float SuperSpeedValue = 8.0f;

    internal static bool isGod = false;
    internal static bool isBurn = false;

    internal static int counter2 = 0;
    internal static int mycounter = 0;

    internal static bool IsRailgunKillAll = false;
    internal static bool IsExplodeEveryone = false;
    internal static bool IsVacuumPlayers = false;

    internal static bool aimbotOn = false;
    internal static bool triggerbotOn = false;

    internal static bool EnableEditModeMovement = false;
    private static bool isEditMovementActive = false;

    internal static bool namesEspOn = true;
    internal static bool linesEspOn = true;
    internal static bool bonesEspOn = false;

    internal static bool impulseAllOn = false;
    private static float nextImpulseTime = 0f;

    internal static int selectedWeaponIndex = 0;
    internal static string[] weaponNames = Enum.GetNames(typeof(AvatarItemType)).ToArray();

    internal static System.Numerics.Vector4 ESPLineColor = new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f);

    private static bool _responseReceived = false;
    private static int _newWoId = -1;
    private static Il2CppSystem.EventHandler _cubeCreationHandler;

    internal static float ESPLineThickness = 2.0f;

    internal static bool EnableAutoCubeRemover = false;
    private static float _nextCubeRemoveTime = 0f;
    private const int MAX_REMOVALS = 30;
    private const float COOLDOWN_TIME = 5.1f;

    public static string WorldModelsExporterStatusMessage = "mBkBwbVT1Pub";
    public static readonly string _modelsExportPath =
        Path.Combine(MelonLoader.MelonUtils.GameDirectory, "d3JML7DVoAx2");
    public static string WorldModelsImporterStatusMessage = "j59PDyeFLxQW";

    private static bool _isImportingModels = false;
    private static bool _skipCurrentModelImport = false;
    public static bool IsImportingModels => _isImportingModels;

    public static string CubePlacerStatusMessage = "b9GXQabE3ZkK";
    public static int buildMode = 0;
    public static float cubePlacerDelay = 0.4f;
    public static bool smartBuilderEnabled = false;
    public static bool isBuildingCoroutineRunning = false;
    public static IntVector builderPreviewTarget;
    public static int targetModelId = -1;
    public static bool builderHasValidTarget = false;

    public static int cubesToPlace = 10;
    public static int placementOffset = 0;
    public static int placementAxis = 0;
    public static int wallWidth = 5;
    public static int wallHeight = 4;
    public static int stairSteps = 5;
    public static int stairWidth = 2;
    public static int stairDir = 0;
    public static int trapRadius = 2;
    public static int trapHeight = 3;

    public static string[] buildModes = new string[] { "5MkmJIjJlKR8", "IqsHsGgIAuVw", "pCo2rzHzestn", "jLAsEu7JJwWi", "xHZmlIvGr8m9" };
    public static string[] axisNames = new string[] { "QxfcCNRhobVr", "wQQpjZHYWg5X", "ClptXG2cknRB" };
    public static string[] dirNames = new string[] { "j8glpgj3ILOO", "nLUwYmiMLw19", "OQoWi9hDNVcp", "Unuh0pNl2AAp" };

    private static Mesh previewCubeMesh;
    private static Material previewMat;

    public static string LogicExporterStatusMessage = "pbvBHJbNzVry";
    public static string LogicImporterStatusMessage = "EsyFU3QW5iEq";
    public static readonly string logicExportsPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "d9VHFcZPbajD");
    private static Dictionary<int, int> oldIdToNewIdMap = new Dictionary<int, int>();

    private static System.Collections.Generic.Dictionary<IntVector, Cube> trackedCubes =
        new System.Collections.Generic.Dictionary<IntVector, Cube>();
    public static bool isCheckin = false;
    public static float checkDelay = 5.0f;

    public static void calcEsp()
    {
        if (!EnableESP)
            return;
        if (!namesEspOn && !linesEspOn && !bonesEspOn)
            return;

        var cam = MVGameControllerBase.MainCameraManager?.MainCamera;
        if (cam == null)
            return;

        var screenH = Screen.height;
        var tempList = new List<EspData>();
        var players = new List<MVPlayer>();
        if (MVGameControllerBase.Game?.MVPlayerContainer?.Values != null)
        {
            foreach (var p in MVGameControllerBase.Game.MVPlayerContainer.Values)
                players.Add(p);
        }

        foreach (var p in players)
        {
            if (p == null)
                continue;

            var woc = MVGameControllerBase.WOCM.GetWorldObjectClient(p.SpawnRolesManager.SpawnRoleId);
            if (woc == null)
                continue;

            var av = woc.TryCast<MVAvatarRemote>();
            if (av?.Body?.BodyData == null)
                continue;

            var headB = av.Body.BodyData.GetPartBone("12h4RpzuixUj");
            if (headB == null)
                continue;
            var headW = headB.position;
            var headS = cam.WorldToScreenPoint(headW);

            if (headS.z <= 0)
                continue;

            var data = new EspData();
            data.name = p.UserProfileData.UserName;
            data.col = ESPLineColor;
            if (MVGameControllerBase.Game.TeamManager.TeamCount() > 1)
            {
                if (p.Team == MVTeam.Blue)
                    data.col = new System.Numerics.Vector4(0.2f, 0.3f, 1f, 1f);
                else if (p.Team == MVTeam.Red)
                    data.col = new System.Numerics.Vector4(1f, 0.2f, 0.2f, 1f);
                else if (p.Team == MVTeam.Yellow)
                    data.col = new System.Numerics.Vector4(1f, 0.9f, 0.2f, 1f);
                else if (p.Team == MVTeam.Green)
                    data.col = new System.Numerics.Vector4(0.2f, 1f, 0.3f, 1f);
            }
            data.head = new System.Numerics.Vector2(headS.x, screenH - headS.y);
            if (linesEspOn)
            {
                var footW = woc.GameObject.transform.position;
                var footS = cam.WorldToScreenPoint(footW);
                if (footS.z > 0)
                {
                    data.foot = new System.Numerics.Vector2(footS.x, screenH - footS.y);
                }
            }
            if (bonesEspOn)
            {
                data.bones = new List<Tuple<System.Numerics.Vector2, System.Numerics.Vector2>>();
                System.Numerics.Vector2 getBone(string n)
                {
                    var b = av.Body.BodyData.GetPartBone(n);
                    if (b == null)
                        return System.Numerics.Vector2.Zero;
                    var s = cam.WorldToScreenPoint(b.position);
                    if (s.z <= 0)
                        return System.Numerics.Vector2.Zero;
                    return new System.Numerics.Vector2(s.x, screenH - s.y);
                }

                var head = getBone("SxAnNs6kTLwI");
                var torso = getBone("fRRWkbmcCOqd");
                var rArm = getBone("P5Gfq1bPnjwj");
                var lArm = getBone("QxfoY4q2R1BO");
                var rUpLeg = getBone("zkm0IIgnlPbc");
                var rLowLeg = getBone("GYTyA5HchpOs");
                var lUpLeg = getBone("RmZaKzndG09i");
                var lLowLeg = getBone("wOIjmi7ax3PU");
                if (torso != System.Numerics.Vector2.Zero)
                {
                    if (head != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(head, torso));
                    if (rArm != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(torso, rArm));
                    if (lArm != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(torso, lArm));
                    if (rUpLeg != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(torso, rUpLeg));
                    if (lUpLeg != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(torso, lUpLeg));
                    if (rUpLeg != System.Numerics.Vector2.Zero && rLowLeg != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(rUpLeg, rLowLeg));
                    if (lUpLeg != System.Numerics.Vector2.Zero && lLowLeg != System.Numerics.Vector2.Zero)
                        data.bones.Add(new Tuple<System.Numerics.Vector2, System.Numerics.Vector2>(lUpLeg, lLowLeg));
                }
            }

            tempList.Add(data);
        }
        lock (espLock)
        {
            espCache = tempList;
        }
    }

    static GameInfo()
    {
        LoadLevelSwitchSetting();
    }

    internal static void LoadLevelSwitchSetting()
    {
        try
        {
            if (File.Exists(levelSwitchFilePath))
                EnableLevel45Switch = File.ReadAllText(levelSwitchFilePath).Trim() == "pSYxumrPH7ml";
        }
        catch
        {
        }
    }

    internal static void SaveLevelSwitchSetting()
    {
        try
        {
            File.WriteAllText(levelSwitchFilePath, EnableLevel45Switch ? "hFhIuc8GYlUt" : "U2tCrg4T3Odg");
        }
        catch
        {
        }
    }

    [Serializable]
    public class WorldExportData
    {
        public Dictionary<IntVector, Cube> TerrainCubes;
        public List<PrototypeData> CustomPrototypes;
        public List<WorldObjectData> WorldObjects;
        public List<LinkData> Links;
        public List<ObjectLinkData> ObjectLinks;
    }

    [Serializable]
    public class PrototypeData
    {
        public int PrototypeID;
        public Dictionary<IntVector, Cube> Cubes;
    }

    [Serializable]
    public class WorldObjectData
    {
        public int OriginalID;
        public WorldObjectType ObjectType;
        public int GroupID;
        public Dictionary<object, object> Data;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    [Serializable]
    public class LinkData
    {
        public int OutputWOID;
        public int InputWOID;
    }

    [Serializable]
    public class ObjectLinkData
    {
        public int ConnectorWOID;
        public int ObjectWOID;
    }

    public static string WorldExporterStatusMessage = "Id0KuCyq6dA5";
    public static string WorldImporterStatusMessage = "V7Mp1qFKviH7";
    internal static bool removeCubesBeforeImport = true;

    private struct CubeTargetData
    {
        public float distance;
        public MVCubeModelBase model;
        public IntVector pos;
    }

    internal static bool EnableNpcMode = false;
    private static bool isNpcModeActive = false;
    private static ClientSideNPCInteractable _npcInteractable = null;
    private static AvatarInteractable _originalInteractable = null;

    internal static int selectedPowerIndex = 0;
    internal static string[] powerNames =
        Enum.GetNames(typeof(AvatarModifierPackageType)).Where(p => p != "s9A42zowpw3Q").ToArray();

    public static string TerrainExporterStatusMessage = "CpWERMQhY0Ez";
    public static string TerrainImporterStatusMessage = "jlvNrkviKzIk";
    public static int cubesPerTick = 1;
    public static float tickDelay = 0.05f;
    public static float importPauseDelay = 5.0f;
    private const int CORNERS_ARRAY_SIZE_TERRAIN = 8;
    private const int MATERIALS_ARRAY_SIZE_TERRAIN = 6;

    private static DesktopAvatarEditModeController _editModeController;
    private static DesktopAvatarEditModeController EditModeController
    {
        get {
            if (_editModeController == null || _editModeController.gameObject == null)
            {
                var controllers = GameObject.FindObjectsOfType<DesktopAvatarEditModeController>();
                if (controllers != null && controllers.Count > 0)
                {
                    _editModeController = controllers[0];
                }
            }
            return _editModeController;
        }
    }

    public static void StartExportLogic()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
            LogicExporterStatusMessage = "4au3GLMeZbtD";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(ExportLogicAsync());
    }

    private static IEnumerator ExportLogicAsync()
    {
        LogicExporterStatusMessage = "OW4azJTtT4zD";
        var logicObjects = new List<WorldObjectData>();
        var logicLinks = new List<LinkData>();
        var logicObjectLinks = new List<ObjectLinkData>();
        bool errorOccurred = false;
        yield return null;

        try
        {
            int rootGroupId = MVGameControllerBase.WOCM.RootGroup.Id;
            var allWos = new List<MVWorldObjectClient>();
            foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
            {
                allWos.Add(wo);
            }

            LogicExporterStatusMessage = $"p0JlfjgFP8WJ";

            foreach (var wo in allWos)
            {
                if (wo == null || wo.HasInteractionFlag(InteractionFlags.IsTerrain) ||
                    wo.WorldObjectType == WorldObjectType.PlayModeAvatar || wo.Id == rootGroupId)
                {
                    continue;
                }
                bool isLikelyLogic = wo.WorldObjectType != WorldObjectType.CubeModel && wo.GroupId == rootGroupId;

                bool isRootCubeModel = (wo.WorldObjectType == WorldObjectType.CubeModel) && wo.GroupId == rootGroupId;

                if (isLikelyLogic || isRootCubeModel)
                {
                    var woData = new WorldObjectData { OriginalID = wo.Id,
                                                       ObjectType = wo.WorldObjectType,
                                                       GroupID = wo.GroupId,
                                                       Position = wo.Position,
                                                       Rotation = wo.Rotation,
                                                       Scale = wo.Scale,
                                                       Data = SanitizeDictionaryForWorld(wo.Data) };
                    if (wo.Data != null && wo.Data.ContainsKey((Il2CppSystem.Object)40))
                    {
                        if (!woData.Data.ContainsKey(40))
                        {
try
                            {
                                object itemIdVal = SanitizeObjectForWorld(wo.Data[(Il2CppSystem.Object)40]);
                                if (itemIdVal != null)
                                    woData.Data[40] = itemIdVal;
                            }
                            catch
                            {
                            }
                        }
                    }
                    logicObjects.Add(woData);
                }
            }

            LogicExporterStatusMessage = $"5xojaAxh99E2";
            foreach (var link in MVGameControllerBase.Game.worldNetwork.links.links.Values)
            {
                logicLinks.Add(new LinkData { OutputWOID = link.outputWOID, InputWOID = link.inputWOID });
            }
            foreach (var objLink in MVGameControllerBase.Game.worldNetwork.objectLinks.objectLinks.Values)
            {
                logicObjectLinks.Add(new ObjectLinkData { ConnectorWOID = objLink.objectConnectorWOID,
                                                          ObjectWOID = objLink.objectWOID });
            }
        }
        catch (Exception ex)
        {
            LogicExporterStatusMessage = $"BVZxtBMefz86";
errorOccurred = true;
        }
        if (errorOccurred)
            yield break;

        LogicExporterStatusMessage =
            $"UsG0LivbDMFc";
        yield return null;

        try
        {
            var bp = new BytePacker();
            bp.Write(logicObjects.Count);
            foreach (var woData in logicObjects)
            {
                bp.Write(woData.OriginalID);
                bp.Write((int)woData.ObjectType);
                bp.Write(woData.GroupID);
                bp.Write(woData.Position.x);
                bp.Write(woData.Position.y);
                bp.Write(woData.Position.z);
                bp.Write(woData.Rotation.x);
                bp.Write(woData.Rotation.y);
                bp.Write(woData.Rotation.z);
                bp.Write(woData.Rotation.w);
                bp.Write(woData.Scale.x);
                bp.Write(woData.Scale.y);
                bp.Write(woData.Scale.z);
                WriteObject(bp, woData.Data);
            }

            bp.Write(logicLinks.Count);
            foreach (var link in logicLinks)
            {
                bp.Write(link.OutputWOID);
                bp.Write(link.InputWOID);
            }

            bp.Write(logicObjectLinks.Count);
            foreach (var link in logicObjectLinks)
            {
                bp.Write(link.ConnectorWOID);
                bp.Write(link.ObjectWOID);
            }

            Directory.CreateDirectory(logicExportsPath);
            string fName = $"jBCW2RFmXkDk";
            string fPath = Path.Combine(logicExportsPath, fName);
            File.WriteAllText(fPath, Convert.ToBase64String(bp.ToArray()));

            LogicExporterStatusMessage = $"GbcpGhFU0sma";
        }
        catch (Exception ex)
        {
            LogicExporterStatusMessage = $"Vw67ze7bE5ne";
}
    }
    private static readonly HashSet<WorldObjectType> ItemCreationTypes =
        new HashSet<WorldObjectType> { WorldObjectType.And,
                                       WorldObjectType.Negate,
                                       WorldObjectType.PressurePlate,
                                       WorldObjectType.PulseBox,
                                       WorldObjectType.TimeTrigger,
                                       WorldObjectType.ToggleBox,
                                       WorldObjectType.ModelToggle,
                                       WorldObjectType.TextMsg,
                                       WorldObjectType.PointLight,
                                       WorldObjectType.SoundEmitter,
                                       WorldObjectType.PickupItemSpawner,
                                       WorldObjectType.Blueprint,
                                       WorldObjectType.CheckPoint,
                                       WorldObjectType.PickupCubeGun,
                                       WorldObjectType.GamePointChest };
    public static void StartImportLogic(string filePath)
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
            LogicImporterStatusMessage = "shhJi0V0bHzp";
            return;
        }
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            LogicImporterStatusMessage = "DBi3vagf5N08";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(ImportLogicAsync(filePath));
    }

    private static IEnumerator ImportLogicAsync(string filePath)
    {
        LogicImporterStatusMessage = "3bjx47JQtmmR";
        var importedObjects = new List<WorldObjectData>();
        var importedLinks = new List<LinkData>();
        var importedObjectLinks = new List<ObjectLinkData>();
        bool readFileSuccess = false;

        try
        {
            byte[] byteData = Convert.FromBase64String(File.ReadAllText(filePath));
            var bp = new BytePacker(byteData);

            int objCount = bp.ReadInt32();
            for (int i = 0; i < objCount; i++)
            {
                var woData = new WorldObjectData {
                    OriginalID = bp.ReadInt32(),
                    ObjectType = (WorldObjectType)bp.ReadInt32(),
                    GroupID = bp.ReadInt32(),
                    Position = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                    Rotation = new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                    Scale = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                    Data = ReadObject(bp) as Dictionary<object, object> ?? new Dictionary<object, object>()
                };
                importedObjects.Add(woData);
            }

            int linkCount = bp.ReadInt32();
            for (int i = 0; i < linkCount; i++)
            {
                importedLinks.Add(new LinkData { OutputWOID = bp.ReadInt32(), InputWOID = bp.ReadInt32() });
            }

            int objLinkCount = bp.ReadInt32();
            for (int i = 0; i < objLinkCount; i++)
            {
                importedObjectLinks.Add(
                    new ObjectLinkData { ConnectorWOID = bp.ReadInt32(), ObjectWOID = bp.ReadInt32() });
            }

            LogicImporterStatusMessage = $"2u1TkzDXrQgR";
            readFileSuccess = true;
        }
        catch (Exception ex)
        {
            LogicImporterStatusMessage = $"oKgn04GRPOCd";
}
        if (!readFileSuccess)
            yield break;

        oldIdToNewIdMap.Clear();
        var currentWorldObjects = new List<MVWorldObjectClient>();
        foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            currentWorldObjects.Add(wo);
        }

        int matchedCount = 0;
        int createdCount = 0;
        const float CREATE_DELAY = 0.2f;

        LogicImporterStatusMessage = "1mM5l5cxx82L";
        yield return null;
        var objectsToMatch = new List<WorldObjectData>(importedObjects);
        var objectsInWorldCopy = new List<MVWorldObjectClient>(currentWorldObjects);

        foreach (var savedObj in objectsToMatch)
        {
            if (oldIdToNewIdMap.ContainsKey(savedObj.OriginalID))
                continue;
            if (savedObj.ObjectType == WorldObjectType.CubeModel)
            {
                MVWorldObjectClient bestMatch = null;
                float closestDist = 0.1f;

                var potentialMatches = new List<MVWorldObjectClient>(objectsInWorldCopy);
                foreach (var currentObj in potentialMatches)
                {
                    if (currentObj.WorldObjectType == savedObj.ObjectType && currentObj.Scale == savedObj.Scale)
                    {
                        float dist = Vector3.Distance(currentObj.Position, savedObj.Position);
                        if (dist < closestDist)
                        {
                            bestMatch = currentObj;
                            closestDist = dist;
                        }
                    }
                }

                if (bestMatch != null)
                {
                    oldIdToNewIdMap[savedObj.OriginalID] = bestMatch.Id;
                    matchedCount++;
                    objectsInWorldCopy.Remove(bestMatch);
                }
            }
        }
        LogicImporterStatusMessage = $"3Ss0rYIE5mJV";
        yield return null;
        World world = MVGameControllerBase.Game.World;
        Il2CppSystem.EventHandler<InitializedGameQueryDataEventArgs> handler = null;
        int currentOriginalId = -1;

        handler = new Action<Il2CppSystem.Object, InitializedGameQueryDataEventArgs>(
            (sender, e) =>
            {
                if (_waitForCreationResponse &&
                    e.InstigatorActorNumber == MVGameControllerBase.Game.LocalPlayer.ActorNr)
                {
                    _newWoId = e.RootWO.Id;
                    _responseReceived = true;
if (currentOriginalId != -1)
                    {
                        oldIdToNewIdMap[currentOriginalId] = _newWoId;
                    }
                }
            });
        world.InitializedGameQueryData += handler;

        foreach (var savedObj in importedObjects)
        {
            if (oldIdToNewIdMap.ContainsKey(savedObj.OriginalID))
                continue;
            bool isLogicItem = ItemCreationTypes.Contains(savedObj.ObjectType);
            int itemID = -1;
            bool hasItemID = false;

            if (isLogicItem && savedObj.Data.TryGetValue(40, out var itemIdObject))
            {
                try
                {
                    itemID = Convert.ToInt32(itemIdObject);
                    if (itemID > 0)
                        hasItemID = true;
                }
                catch
                {
}
            }

            _responseReceived = false;
            _newWoId = -1;
            _waitForCreationResponse = true;
            currentOriginalId = savedObj.OriginalID;
            float timeout = Time.time + 10f;
            bool requestSent = false;

            if (isLogicItem && hasItemID)
            {
                LogicImporterStatusMessage =
                    $"LwTiSwcvlfcB";
                try
                {
                    MVGameControllerBase.OperationRequests.AddItemToWorld(
                        itemID, MVGameControllerBase.WOCM.RootGroup.Id, savedObj.Position, savedObj.Rotation, true,
                        false, false);
                    requestSent = true;
                }
                catch (Exception ex)
                {
_waitForCreationResponse = false;
                    continue;
                }
            }
            else
            {
                BuiltInItem itemType = GetBuiltInItemFromType(savedObj.ObjectType);
                if (itemType == default(BuiltInItem))
                {
_waitForCreationResponse = false;
                    continue;
                }

                LogicImporterStatusMessage =
                    $"A6eym5RUyP9b";
                var creationData =
                    new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
                creationData.Add((byte)1, savedObj.Scale.x);
                creationData.Add((byte)2, (byte)21);
                creationData.Add((byte)3, MVGameControllerBase.Game.LocalPlayer.ProfileID);

                try
                {
                    MVGameControllerBase.OperationRequests.RequestBuiltInItem(
                        itemType, MVGameControllerBase.WOCM.RootGroup.Id, creationData, savedObj.Position,
                        savedObj.Rotation, savedObj.Scale, true, false);
                    requestSent = true;
                }
                catch (Exception ex)
                {
_waitForCreationResponse = false;
                    continue;
                }
            }
            if (requestSent)
            {
                while (!_responseReceived && Time.time < timeout)
                {
                    yield return null;
                }
            }
            _waitForCreationResponse = false;

            if (!_responseReceived || _newWoId == -1)
            {
continue;
            }
            createdCount++;
            var newObj = MVGameControllerBase.WOCM.GetWorldObjectClient(_newWoId);
            if (newObj != null && savedObj.Data != null && savedObj.Data.Count > 0)
            {
                var il2cppData =
                    new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
                foreach (var pair in savedObj.Data)
                {
                    try
                    {
                        object key = pair.Key;
                        object val = pair.Value;
                    }
                    catch (Exception ex)
                    {
}
                }
                if (il2cppData.Count > 0)
                {
                    bool dataUpdated = false;
                    try
                    {
                        MVGameControllerBase.OperationRequests.UpdateWorldObjectRunTimeData(_newWoId, il2cppData);
                        dataUpdated = true;
                    }
                    catch (Exception ex)
                    {
}

                    if (dataUpdated)
                        yield return new UnityEngine.WaitForSeconds(CREATE_DELAY / 2);
                }
            }
            yield return new UnityEngine.WaitForSeconds(CREATE_DELAY);
        }

        if (handler != null)
        {
            world.InitializedGameQueryData -= handler;
        }

        LogicImporterStatusMessage =
            $"fyInVBSWkEzG";
        yield return null;
        int linksCreated = 0;
        foreach (var linkData in importedLinks)
        {
            if (oldIdToNewIdMap.TryGetValue(linkData.OutputWOID, out int newOutId) &&
                oldIdToNewIdMap.TryGetValue(linkData.InputWOID, out int newInId))
            {
                bool linkAdded = false;
                try
                {
                    Link newLink = new Link { outputWOID = newOutId, inputWOID = newInId };
                    MVGameControllerBase.OperationRequests.AddLink(newLink);
                    linksCreated++;
                    linkAdded = true;
                }
                catch (Exception ex)
                {
}
                if (linkAdded)
                    yield return new UnityEngine.WaitForSeconds(CREATE_DELAY / 2);
            }
            else
            {
}
        }

        foreach (var objLinkData in importedObjectLinks)
        {
            if (oldIdToNewIdMap.TryGetValue(objLinkData.ConnectorWOID, out int newConnectorId) &&
                oldIdToNewIdMap.TryGetValue(objLinkData.ObjectWOID, out int newTargetId))
            {
                bool objLinkAdded = false;
                try
                {
                    ObjectLink newObjLink =
                        new ObjectLink { objectConnectorWOID = newConnectorId, objectWOID = newTargetId };
                    MVGameControllerBase.OperationRequests.AddObjectLink(newObjLink);
                    linksCreated++;
                    objLinkAdded = true;
                }
                catch (Exception ex)
                {
}
                if (objLinkAdded)
                    yield return new UnityEngine.WaitForSeconds(CREATE_DELAY / 2);
            }
            else
            {
}
        }

        LogicImporterStatusMessage =
            $"nxanscRXxXih";
    }
    private static BuiltInItem GetBuiltInItemFromType(WorldObjectType objType)
    {
        switch (objType)
        {
        case WorldObjectType.CubeModel:
            return BuiltInItem.CubeModel;
        default:
            return default(BuiltInItem);
        }
    }

    public static void startCheckin()
    {
        if (isCheckin)
            return;
        isCheckin = true;
        UnityMainThreadDispatcher.Instance.Enqueue(checkEm());
    }

    public static void stopCheckin()
    {
        isCheckin = false;
    }

    public static void clearTracked()
    {
        trackedCubes.Clear();
    }

    public static System.Collections.IEnumerator checkEm()
    {
        while (isCheckin)
        {
            yield return new UnityEngine.WaitForSeconds(checkDelay);

            if (MVGameControllerBase.GameMode != MVGameMode.Play)
            {
                continue;
            }

            var terrain = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>();
            if (terrain == null)
            {
                continue;
            }

            var allPos = new System.Collections.Generic.List<IntVector>(trackedCubes.Keys);
            var replaced = 0;

            foreach (var pos in allPos)
            {
                if (!terrain.ContainsCube(pos))
                {
                    if (trackedCubes.TryGetValue(pos, out var oldCube))
                    {
                        terrain.AddCube(pos, oldCube);
                        replaced++;
                    }
                }
            }

            if (replaced > 0)
            {
                terrain.HandleDelta();
            }
        }
    }
    public static void StartPlacingCubes()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(placeCubes());
    }

    public static System.Collections.IEnumerator placeCubes()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
            CubePlacerStatusMessage = "ZUCeYPqbINBV";
            yield break;
        }

        float waitEnd = Time.realtimeSinceStartup + 0.5f;
        while (Time.realtimeSinceStartup < waitEnd)
            yield return null;

        var player = MVAvatarLocal_instance.Instancjaa;
        byte matId = 21;

        if (player != null && player.CurrentItem?.Value != null)
        {
            try
            {
                var itemVal = player.CurrentItem.Value.TryCast<
                    Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();
                Il2CppSystem.Object itemDataKey = (Il2CppSystem.Object) "e7Sf2Vrpe4VO";

                if (itemVal != null && itemVal.ContainsKey(itemDataKey))
                {
                    var itemDict = itemVal[itemDataKey]
                                       .TryCast<Il2CppSystem.Collections.Generic
                                                    .Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();
                    Il2CppSystem.Object matKey = (Il2CppSystem.Object) "a9G9RJs6UJ5q";

                    if (itemDict != null && itemDict.ContainsKey(matKey))
                    {
                        var matObj = itemDict[matKey];
                        var unboxPtr = Il2CppInterop.Runtime.IL2CPP.il2cpp_object_unbox(matObj.Pointer);
                        matId = System.Runtime.InteropServices.Marshal.ReadByte(unboxPtr);
                    }
                }
            }
            catch (Exception ex)
            {
matId = 21;
            }
        }

        var terrain = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>();
        if (terrain == null)
        {
            CubePlacerStatusMessage = "UNHabzw4tbCO";
            yield break;
        }

        var playerWOC = MVGameControllerBase.WOCM.GetWorldObjectClient(player.Id);
        if (playerWOC == null)
        {
            yield break;
        }
        var playerWorldPos = playerWOC.Transform.position;

        var startY = (short)Mathf.Round(playerWorldPos.y);
        if (placementAxis == 0 || placementAxis == 2)
        {
            startY += 1;
        }
        var startPos =
            new IntVector((short)Mathf.Round(playerWorldPos.x), startY, (short)Mathf.Round(playerWorldPos.z));

        var idCorners = CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);
        var safeCube = new Cube(idCorners, Cube.CreateMaterialArray(matId));

        var placed = 0;

        CubePlacerStatusMessage = "DAAvBoXypDfl";
        for (int i = 0; i < cubesToPlace; i++)
        {
            var offset = placementOffset + i;
            var pos = new IntVector(startPos.x, startPos.y, startPos.z);
            switch (placementAxis)
            {
            case 0:
                pos.x += (short)offset;
                break;
            case 1:
                pos.y += (short)offset;
                break;
            case 2:
                pos.z += (short)offset;
                break;
            }

            terrain.AddCube(pos, safeCube);
            if (!trackedCubes.ContainsKey(pos))
            {
                trackedCubes.Add(pos, safeCube);
            }

            placed++;
            terrain.HandleDelta();

            CubePlacerStatusMessage = $"ArbTdMNSWVYQ";

            float stepWait = Time.realtimeSinceStartup + cubePlacerDelay;
            while (Time.realtimeSinceStartup < stepWait)
                yield return null;
        }
        CubePlacerStatusMessage = $"ELyAa5B2Ej4X";
    }
    public static byte GetEquippedMaterialId()
    {
        byte matId = 21;
        var player = MVAvatarLocal_instance.Instancjaa;
        if (player != null && player.CurrentItem?.Value != null)
        {
            try
            {
                var itemVal = player.CurrentItem.Value.TryCast<
                    Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();
                if (itemVal != null && itemVal.ContainsKey((Il2CppSystem.Object) "VTkz4ly5DVTl"))
                {
                    var itemDict = itemVal[(Il2CppSystem.Object) "p6QsD9iKwsB6"]
                                       .TryCast<Il2CppSystem.Collections.Generic
                                                    .Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();
                    if (itemDict != null && itemDict.ContainsKey((Il2CppSystem.Object) "7n29edAx7pAK"))
                    {
                        var matObj = itemDict[(Il2CppSystem.Object) "NSCgoAX8Tz4b"];
                        var unboxPtr = Il2CppInterop.Runtime.IL2CPP.il2cpp_object_unbox(matObj.Pointer);
                        matId = System.Runtime.InteropServices.Marshal.ReadByte(unboxPtr);
                    }
                }
            }
            catch
            {
            }
        }
        return matId;
    }

    public static List<IntVector> CalculateBlocksToPlace(IntVector startPos)
    {
        List<IntVector> blocksToPlace = new List<IntVector>();

        if (buildMode == 0) 
        {
            for (int i = 0; i < cubesToPlace; i++)
            {
                var pos = new IntVector(startPos.x, startPos.y, startPos.z);
                if (placementAxis == 0)
                    pos.x += (short)(placementOffset + i);
                else if (placementAxis == 1)
                    pos.y += (short)(placementOffset + i);
                else if (placementAxis == 2)
                    pos.z += (short)(placementOffset + i);
                blocksToPlace.Add(pos);
            }
        }
        else if (buildMode == 1) 
        {
            for (int w = 0; w < wallWidth; w++)
            {
                for (int h = 0; h < wallHeight; h++)
                {
                    var pos = new IntVector(startPos.x, startPos.y, startPos.z);
                    if (placementAxis == 0)
                        pos.x += (short)(placementOffset + w);
                    else
                        pos.z += (short)(placementOffset + w);
                    pos.y += (short)h;
                    blocksToPlace.Add(pos);
                }
            }
        }
        else if (buildMode == 2) 
        {
            for (int s = 0; s < stairSteps; s++)
            {
                for (int w = 0; w < stairWidth; w++)
                {
                    var pos = new IntVector(startPos.x, startPos.y, startPos.z);
                    pos.y += (short)s;
                    if (stairDir == 0)
                    {
                        pos.x += (short)(placementOffset + s);
                        pos.z += (short)w;
                    }
                    else if (stairDir == 1)
                    {
                        pos.x -= (short)(placementOffset + s);
                        pos.z += (short)w;
                    }
                    else if (stairDir == 2)
                    {
                        pos.z += (short)(placementOffset + s);
                        pos.x += (short)w;
                    }
                    else if (stairDir == 3)
                    {
                        pos.z -= (short)(placementOffset + s);
                        pos.x += (short)w;
                    }
                    blocksToPlace.Add(pos);
                }
            }
        }
        else if (buildMode == 3) 
        {
            var radius = 6f;
            var angleStep = 0.25f;
            for (int i = 0; i < 70; i++)
            {
                var angle = i * angleStep;
                var x = startPos.x + Mathf.RoundToInt(radius * Mathf.Cos(angle));
                var z = startPos.z + Mathf.RoundToInt(radius * Mathf.Sin(angle));
                var y = startPos.y + i / 4;
                blocksToPlace.Add(new IntVector((short)x, (short)y, (short)z));
            }
        }
        else if (buildMode == 4) 
        {
            for (int x = -trapRadius; x <= trapRadius; x++)
            {
                for (int z = -trapRadius; z <= trapRadius; z++)
                {
                    for (int y = 0; y <= trapHeight; y++)
                    {
                        bool isWall = (Mathf.Abs(x) == trapRadius || Mathf.Abs(z) == trapRadius);
                        bool isRoof = (y == trapHeight);
                        bool isFloor = (y == -1);

                        if (isWall || isRoof || isFloor)
                            blocksToPlace.Add(new IntVector((short)(startPos.x + x), (short)(startPos.y + y),
                                                            (short)(startPos.z + z)));
                    }
                }
            }
        }
        return blocksToPlace;
    }

    public static void DrawSmartBuilderPreview()
    {
        builderHasValidTarget = false;
        targetModelId = -1;

        if (!smartBuilderEnabled || isBuildingCoroutineRunning || MVGameControllerBase.GameMode != MVGameMode.Play)
            return;

        var cam = MVGameControllerBase.MainCameraManager?.MainCamera;
        if (cam == null)
            return;

        if (previewCubeMesh == null)
        {
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewCubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.Destroy(tempCube);
        }

        Material previewMaterial = null;
        if (PrefabPool.Instance != null && PrefabPool.Instance.InsertPreviewMaterial != null)
        {
            previewMaterial = PrefabPool.Instance.InsertPreviewMaterial;
        }

        if (previewCubeMesh == null || previewMaterial == null)
            return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        int mask = ~((1 << 2) | (1 << 4));

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask, QueryTriggerInteraction.Ignore))
        {
            var hitWoc = MVWorldObjectClientManager.GetMVObject(hit.transform);
            var hitModel = hitWoc?.TryCast<MVCubeModelBase>();

            if (hitModel == null)
            {
                hitModel = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                               ?.TryCast<MVCubeModelBase>();
            }

            if (hitModel != null)
            {
                Vector3 localPoint = hitModel.transform.InverseTransformPoint(hit.point + hit.normal * 0.1f);
                IntVector centerPos = new IntVector((short)Mathf.Round(localPoint.x), (short)Mathf.Round(localPoint.y),
                                                    (short)Mathf.Round(localPoint.z));

                builderPreviewTarget = centerPos;
                targetModelId = hitModel.Id;
                builderHasValidTarget = true;

                List<IntVector> blocks = CalculateBlocksToPlace(centerPos);
                foreach (var b in blocks)
                {
                    Vector3 localBlockPos = new Vector3(b.x, b.y, b.z);
                    Vector3 worldBlockPos = hitModel.transform.TransformPoint(localBlockPos);

                    Graphics.DrawMesh(
                        previewCubeMesh,
                        Matrix4x4.TRS(worldBlockPos, hitModel.transform.rotation, hitModel.transform.localScale),
                        previewMaterial, 0);
                }
            }
        }
    }

    public static void StartBuildingStructureAtPlayer()
    {
        var player = MVAvatarLocal_instance.Instancjaa;
        if (player == null)
            return;
        var woc = MVGameControllerBase.WOCM.GetWorldObjectClient(player.Id);
        if (woc == null)
            return;

        var terrain = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                          ?.TryCast<MVCubeModelBase>();
        if (terrain == null)
            return;

        var playerPos = woc.Transform.position;
        var localPos = terrain.transform.InverseTransformPoint(playerPos);
        var startPos = new IntVector((short)Mathf.Round(localPos.x), (short)Mathf.Round(localPos.y + 1f),
                                     (short)Mathf.Round(localPos.z));

        StartBuildingStructureAt(terrain, startPos, GetEquippedMaterialId());
    }

    public static void StartBuildingStructureAt(MVCubeModelBase targetModel, IntVector startPos, byte matId)
    {
        if (isBuildingCoroutineRunning)
            return;
        isBuildingCoroutineRunning = true;
        UnityMainThreadDispatcher.Instance.Enqueue(buildStructureAt(targetModel, startPos, matId));
    }

    public static System.Collections.IEnumerator buildStructureAt(MVCubeModelBase terrain, IntVector startPos,
                                                                  byte matId)
    {
        CubePlacerStatusMessage = "wKfvM6zRNt6I";

        if (terrain == null)
        {
            CubePlacerStatusMessage = "1JDaDko7BDak";
            isBuildingCoroutineRunning = false;
            yield break;
        }

        if (buildMode == 4)
        {
            var player = MVAvatarLocal_instance.Instancjaa;
            MVPlayer victim = null;
            var players = MVGameControllerBase.Game.MVPlayerContainer.Values;
            var tempList = new List<MVPlayer>();
            foreach (var p in players)
            {
                if (p.ActorNr != player.ownerActorNr)
                    tempList.Add(p);
            }

            if (tempList.Count > 0)
                victim = tempList[UnityEngine.Random.Range(0, tempList.Count)];

            if (victim != null)
            {
                var vicWoc = MVGameControllerBase.WOCM.GetWorldObjectClient(victim.SpawnRolesManager.SpawnRoleId);
                if (vicWoc != null)
                {
                    var vPos = vicWoc.Transform.position;
                    var localVPos = terrain.transform.InverseTransformPoint(vPos);
                    startPos = new IntVector((short)Mathf.Round(localVPos.x), (short)Mathf.Round(localVPos.y),
                                             (short)Mathf.Round(localVPos.z));
                }
            }
        }

        List<IntVector> blocksToPlace = CalculateBlocksToPlace(startPos);
        var idCorners = CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);
        var safeCube = new Cube(idCorners, Cube.CreateMaterialArray(matId));

        int placed = 0;
        float safeDelay = Mathf.Max(cubePlacerDelay, 0.38f);

        foreach (var pos in blocksToPlace)
        {
            terrain.AddCube(pos, safeCube);
            placed++;
            terrain.HandleDelta();

            CubePlacerStatusMessage = $"ssAkhsXAQ5J7";
            float stepWait = Time.realtimeSinceStartup + safeDelay;
            while (Time.realtimeSinceStartup < stepWait)
                yield return null;
        }

        CubePlacerStatusMessage = "LbJT8W0KgXVc";
        isBuildingCoroutineRunning = false;
    }

    internal static void UpdateMetrics()
    {
        if (MVGameControllerBase.Game == null)
            return;
        if (MVGameControllerBase.WOCM == null)
            return;
        if (MVGameControllerBase.Game.LocalPlayer == null)
            return;

        try
        {
            if (MVGameControllerBase.WOCM.worldObjects != null)
                WorldObjectCount = MVGameControllerBase.WOCM.worldObjects.Count;
            PrototypeCount = 0;
            if (MVGameControllerBase.Game.worldNetwork?.links?.links != null)
                LinkCount = MVGameControllerBase.Game.worldNetwork.links.links.Count;

            if (MVGameControllerBase.Game.worldNetwork?.objectLinks?.objectLinks != null)
                ObjectLinkCount = MVGameControllerBase.Game.worldNetwork.objectLinks.objectLinks.Count;

            if (MVGameControllerBase.Game.worldNetwork?.worldInventory?.runtimePrototypes != null)
                UniquePrototypeCount = MVGameControllerBase.Game.worldNetwork.worldInventory.runtimePrototypes.Count;

            if (MVGameControllerBase.Game.Peer != null)
                Ping = MVGameControllerBase.Game.Peer.RoundTripTime;

            if (isGod)
                god();

            if (impulseAllOn)
                doImpulseAll();

            if (IsRailgunKillAll)
                RailgunKillAll();

            if (IsVacuumPlayers)
                VacuumPlayers();

            Fps = 1.0f / Time.smoothDeltaTime;
            if (EnableAutoCubeRemover && Time.time > _nextCubeRemoveTime)
                doAutoCubeRemove();

            if (MVNetworkGame_instance.Instancja != null)
            {
                if (EnableAutoConnect && MVGameControllerBase.GameSessionData != null)
                {
                    MVNetworkGame_instance.Instancja.Peer.Connect(MVGameControllerBase.GameSessionData.serverIP,
                                                                  "Blk3OBKhfSS2");
EnableAutoConnect = false;
                }
                if (EnableCharacterEditor)
                {
                    EnableCharacterEditor = false;
                    MVGameControllerBase.LevelLoader.LoadScenes(
                        MVGameMode.CharacterEditor, MVGameControllerBase.IsTouristSession, false,
                        new Action(MVNetworkGame_instance.Instancja.operationRequests.Syncronize));
                }
                if (EnableEdit)
                {
                    EnableEdit = false;
                    MVGameControllerBase.LevelLoader.LoadScenes(
                        MVGameMode.Edit, MVGameControllerBase.IsTouristSession, false,
                        new Action(MVNetworkGame_instance.Instancja.operationRequests.Syncronize));
                }
                if (EnablePlay)
                {
                    EnablePlay = false;
                    MVGameControllerBase.LevelLoader.LoadScenes(
                        MVGameMode.Play, MVGameControllerBase.IsTouristSession, false,
                        new Action(MVNetworkGame_instance.Instancja.operationRequests.Syncronize));
                }
            }

            if (MVAvatarLocal_walkmode_instance.Instancjaa != null &&
                MVAvatarLocal_walkmode_instance.Instancjaa.mvAvatar != null)
            {
                if (EnableDieFromFalling &&
                    MVAvatarLocal_walkmode_instance.Instancjaa.mvAvatar.InteractableLocal != null &&
                    MVAvatarLocal_walkmode_instance.Instancjaa.mvAvatar.Health != null)
                {
                    MVAvatarLocal_walkmode_instance.Instancjaa.mvAvatar.Health.Value = 0f;
                    MVAvatarLocal_walkmode_instance.Instancjaa.mvAvatar.InteractableLocal.DieFromFalling();
                    MVAvatarLocal_walkmode_instance.Instancjaa.mvAvatar.Health.Value = 100f;
                    EnableDieFromFalling = false;
                }
            }

            if (AvatarMotor_instance.Instancjaa != null)
            {
                if (EnableSpeedHack)
                {
                    AvatarMotor_instance.Instancjaa.speedBoostSetting = SpeedHackMultiplier;
                }
                else
                {
                    AvatarMotor_instance.Instancjaa.speedBoostSetting = 1.0f;
                }

                if (AvatarMotor_instance.Instancjaa.impactState != null)
                {
                    if (EnableAntiKnockback)
                    {
                        AvatarMotor_instance.Instancjaa.impactState.impactDamageMultiplier = 0.0f;
                    }
                    else
                    {
                        AvatarMotor_instance.Instancjaa.impactState.impactDamageMultiplier = 1.0f;
                    }
                }

                if (EnableNoFriction)
                {
                    AvatarMotor_instance.Instancjaa.frictionMultiplier = 0.0f;
                }
                else
                {
                    AvatarMotor_instance.Instancjaa.frictionMultiplier = 1.0f;
                }
            }
        }
        catch (Exception)
        {
        }
    }
    private enum DataType : byte
    {
        Null,
        String,
        Int,
        Float,
        Bool,
        Vector3,
        Quaternion,
        Dictionary,
        IntVector,
        List
    }

    private static T GetStructField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            return (T)field.GetValue(obj);
        }

        var prop = obj.GetType().GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null)
        {
            return (T)prop.GetValue(obj);
        }

        throw new InvalidOperationException(
            $"LGsR1mwOT2yq");
    }

    private static void WriteObject(BytePacker bp, object obj)
    {
        if (obj == null)
        {
            bp.Write((byte)DataType.Null);
            return;
        }

        if (obj is string s)
        {
            bp.Write((byte)DataType.String);
            var strBytes = Encoding.UTF8.GetBytes(s);
            bp.Write(strBytes.Length);
            bp.Write(strBytes);
        }
        else if (obj is int i)
        {
            bp.Write((byte)DataType.Int);
            bp.Write(i);
        }
        else if (obj is float f)
        {
            bp.Write((byte)DataType.Float);
            bp.Write(f);
        }
        else if (obj is bool b)
        {
            bp.Write((byte)DataType.Bool);
            bp.Write(b ? (byte)1 : (byte)0);
        }
        else if (obj is Vector3 v)
        {
            bp.Write((byte)DataType.Vector3);
            bp.Write(v.x);
            bp.Write(v.y);
            bp.Write(v.z);
        }
        else if (obj is Quaternion q)
        {
            bp.Write((byte)DataType.Quaternion);
            bp.Write(q.x);
            bp.Write(q.y);
            bp.Write(q.z);
            bp.Write(q.w);
        }
        else if (obj is IntVector iv)
        {
            bp.Write((byte)DataType.IntVector);
            bp.Write(iv.x);
            bp.Write(iv.y);
            bp.Write(iv.z);
        }
        else if (obj is Dictionary<object, object> dict)
        {
            bp.Write((byte)DataType.Dictionary);
            bp.Write(dict.Count);
            foreach (var pair in dict)
            {
                WriteObject(bp, pair.Key);
                WriteObject(bp, pair.Value);
            }
        }
        else if (obj is List<object> list)
        {
            bp.Write((byte)DataType.List);
            bp.Write(list.Count);
            foreach (var item in list)
            {
                WriteObject(bp, item);
            }
        }
        else
        {
bp.Write((byte)DataType.Null);
        }
    }

    private static object ReadObject(BytePacker bp)
    {
        DataType type = (DataType)bp.ReadByte();
        switch (type)
        {
        case DataType.String:
            int len = bp.ReadInt32();
            if (len < 0 || len > (bp.Length - bp.Position))
                throw new InvalidDataException("f6TNGlwaz4ht");
            return Encoding.UTF8.GetString(bp.ReadBytes(len));
        case DataType.Int:
            return bp.ReadInt32();
        case DataType.Float:
            return bp.ReadSingle();
        case DataType.Bool:
            return bp.ReadByte() == 1;
        case DataType.Vector3:
            return new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle());
        case DataType.Quaternion:
            return new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle());
        case DataType.IntVector:
            return new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
        case DataType.Dictionary:
            int count = bp.ReadInt32();
            var dict = new Dictionary<object, object>(count);
            for (int i = 0; i < count; i++)
            {
                object key = ReadObject(bp);
                if (key == null)
                    throw new InvalidDataException("qYsnNFhmBEJs");
                object value = ReadObject(bp);
                dict[key] = value;
            }
            return dict;
        case DataType.List:
            int listCount = bp.ReadInt32();
            var list = new List<object>(listCount);
            for (int i = 0; i < listCount; i++)
            {
                list.Add(ReadObject(bp));
            }
            return list;
        case DataType.Null:
        default:
            return null;
        }
    }
    private static unsafe object SanitizeObject(object obj)
    {
        if (obj == null)
            return null;
        if (obj is string || obj is int || obj is float || obj is bool || obj is byte || obj is Vector3 ||
            obj is Quaternion || obj is IntVector)
        {
            return obj;
        }
        if (obj is Il2CppObjectBase il2cppObj)
        {
            if (il2cppObj.Pointer == IntPtr.Zero)
                return null;

            try
            {
                var classPtr = IL2CPP.il2cpp_object_get_class(il2cppObj.Pointer);
                if (classPtr == IntPtr.Zero)
                    return null;
                var il2cppTypeName = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_class_get_name(classPtr));
                var il2cppNamespace = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_class_get_namespace(classPtr));
                if (il2cppNamespace == "jqbefYEGZ8Ev")
                {
                    IntPtr unboxed = IL2CPP.il2cpp_object_unbox(il2cppObj.Pointer);
                    if (il2cppTypeName == "nXmu6DcpiYUO")
                        return *(int *)unboxed;
                    if (il2cppTypeName == "2k61SUMPfV73")
                        return *(float *)unboxed;
                    if (il2cppTypeName == "vGM0SgMrSjKl")
                        return *(bool *)unboxed;
                    if (il2cppTypeName == "fOi1SuRkUl39")
                        return *(byte *)unboxed;
                    if (il2cppTypeName == "n1HMEZu6QicT")
                        return Convert.ToString(obj);
                }
                if (obj is
                        Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> subDict)
                {
                    return SanitizeDictionary(subDict);
                }
                if (obj is Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> il2cppList)
                {
                    var sanitizedList = new List<object>();
                    foreach (var item in il2cppList)
                    {
                        var clean = SanitizeObject(item);
                        if (clean != null)
                            sanitizedList.Add(clean);
                    }
                    return sanitizedList;
                }
            }
            catch
            {
                return null;
            }
        }
        var type = obj.GetType();
        if (type.IsEnum)
            return Convert.ToInt32(obj);

        return null;
    }

    private static Dictionary<object, object> SanitizeDictionary(
        Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> source)
    {
        if (source == null)
            return new Dictionary<object, object>();
        var sanitized = new Dictionary<object, object>();

        foreach (var pair in source)
        {
            object key = SanitizeObject(pair.Key);
            if (key == null)
                continue;

            object val = SanitizeObject(pair.Value);
            sanitized[key] = val;
        }
        return sanitized;
    }

    public static void StartCreateNewEditorCubeModel()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(CreateNewEditorCubeModelAsync());
    }

    private static IEnumerator CreateNewEditorCubeModelAsync()
    {
_responseReceived = false;
        _newWoId = -1;
        World world = MVGameControllerBase.Game.World;
        Il2CppSystem.EventHandler<InitializedGameQueryDataEventArgs> handler = null;

        handler = new Action<Il2CppSystem.Object, InitializedGameQueryDataEventArgs>(
            (sender, e) =>
            {
                if (e.InstigatorActorNumber == MVGameControllerBase.Game.LocalPlayer.ActorNr)
                {
                    _newWoId = e.RootWO.Id;
                    _responseReceived = true;
if (handler != null)
                    {
                        world.InitializedGameQueryData -= handler;
                    }
                }
            });

        world.InitializedGameQueryData += handler;
var customData = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
        customData.Add((byte)1, 1f);
        customData.Add((byte)2, (byte)21);
        customData.Add((byte)3, MVGameControllerBase.Game.LocalPlayer.ProfileID);

        var position = MVGameControllerBase.MainCameraManager.MainCamera.transform.position +
                       MVGameControllerBase.MainCameraManager.MainCamera.transform.forward * 15f;
        var rotation = Quaternion.identity;
        var scale = Vector3.one;
        int rootGroupId = MVGameControllerBase.WOCM.RootGroup.Id;
MVGameControllerBase.OperationRequests.RequestBuiltInItem(BuiltInItem.CubeModel, rootGroupId, customData,
                                                                  position, rotation, scale, true, false);

        float timeout = Time.time + 10f;
        while (!_responseReceived && Time.time < timeout)
        {
            yield return null;
        }

        if (!_responseReceived)
        {
if (handler != null)
            {
                world.InitializedGameQueryData -= handler;
            }
            yield break;
        }

        if (_newWoId == -1)
        {
yield break;
        }

        var newCubeModel = MVGameControllerBase.WOCM.GetWorldObjectClient(_newWoId)?.TryCast<MVCubeModelBase>();
        if (newCubeModel == null)
        {
yield break;
        }
try
        {
var matId = (byte)21;
            var cube =
                new Cube(CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners), Cube.CreateMaterialArray(matId));

            int size = 5;
            int half = size / 2;

            for (int x = -half; x <= half; x++)
            {
                for (int z = -half; z <= half; z++)
                {
                    var pos = new IntVector(x, 0, z);
                    newCubeModel.AddCube(pos, cube);
                }
            }

            newCubeModel.HandleDelta();
}
        catch (Exception ex)
        {
}
    }

    public static void RailgunKillAll()
    {
        var localPlayer = MVGameControllerBase.Game.LocalPlayer;
        if (localPlayer == null || localPlayer.AvatarLocal == null)
            return;

        var myPickupOwner = localPlayer.AvatarLocal.PickupOwner;

        foreach (MVPlayer targetPlayer in MVGameControllerBase.Game.MVPlayerContainer.Values)
        {
            if (targetPlayer.ActorNr == localPlayer.ActorNr)
                continue;
            MVWorldObjectClient targetWO = MVGameControllerBase.WOCM.GetWorldObjectClient(targetPlayer.WoId);

            if (targetWO != null)
            {
                var handler = targetWO.InteractionDataHandlerBase;

                if (handler != null)
                {
                    InteractionData killPacket = RailgunHitPackage.Create();

                    handler.HandleInteraction(myPickupOwner, killPacket, false);
                }
            }
        }
    }

    public static void AdvancedGhostKillAll()
    {
        var localPlayer = MVGameControllerBase.Game.LocalPlayer;
        if (localPlayer == null || localPlayer.AvatarLocal == null)
            return;

        var myPickupOwner = localPlayer.AvatarLocal.PickupOwner;
        float impulseStrength = 2500f;

        foreach (MVPlayer targetPlayer in MVGameControllerBase.Game.MVPlayerContainer.Values)
        {
            if (targetPlayer.ActorNr == localPlayer.ActorNr)
                continue;
            MVWorldObjectClient targetWO = MVGameControllerBase.WOCM.GetWorldObjectClient(targetPlayer.WoId);

            if (targetWO != null)
            {
                var handler = targetWO.InteractionDataHandlerBase;

                if (handler != null)
                {
                    Vector3 localPos = localPlayer.AvatarLocal.Transform.position;
                    Vector3 targetPos = targetWO.GetTargetPosition();
                    Vector3 direction = (targetPos - localPos).normalized;
                    Vector3 impulseVector = direction * impulseStrength;
                    InteractionData killPacket = AdvancedGhostBodyRotateWeaponPackage.Create(1000f, impulseVector);
                    handler.HandleInteraction(myPickupOwner, killPacket, true);
                }
            }
        }
    }

    public static void VacuumPlayers()
    {
        var localPlayer = MVGameControllerBase.Game.LocalPlayer;
        if (localPlayer == null || localPlayer.AvatarLocal == null)
            return;

        var myPickupOwner = localPlayer.AvatarLocal.PickupOwner;
        Vector3 myPos = localPlayer.AvatarLocal.Position;

        foreach (MVPlayer targetPlayer in MVGameControllerBase.Game.MVPlayerContainer.Values)
        {
            if (targetPlayer.ActorNr == localPlayer.ActorNr)
                continue;

            MVWorldObjectClient targetWO = MVGameControllerBase.WOCM.GetWorldObjectClient(targetPlayer.WoId);

            if (targetWO != null && targetWO.InteractionDataHandlerBase != null)
            {
                Vector3 directionToMe = (myPos - targetWO.Position).normalized;
                Vector3 vacuumForce = directionToMe * 3000f;
                InteractionData vacuumPacket = ImpulseHitPackage.Create(vacuumForce);
                targetWO.InteractionDataHandlerBase.HandleInteraction(myPickupOwner, vacuumPacket, false);
            }
        }
    }

    internal static void doImpulseAll()
    {
        foreach (MVPlayer player in MVGameControllerBase.Game.MVPlayerContainer.Values)
        {
            if (player.ActorNr == MVGameControllerBase.Game.LocalPlayer.ActorNr)
                continue;
            MVWorldObjectClient targetAvatar = MVGameControllerBase.WOCM.GetWorldObjectClient(player.WoId);
            if (targetAvatar != null && targetAvatar.InteractionDataHandlerBase != null)
            {
                Vector3 force = Vector3.up * 8000f;
                InteractionData impulseData = ImpulseHitPackage.Create(force);
                targetAvatar.InteractionDataHandlerBase.HandleInteraction(null, impulseData, false);
            }
        }
    }

    internal static void doEquipWeapon()
    {
        var me = MVGameControllerBase.Game?.LocalPlayer;
        if (me == null)
            return;

        var myAvatarWoc = MVGameControllerBase.WOCM.GetWorldObjectClient(me.SpawnRolesManager.SpawnRoleId);
        if (myAvatarWoc == null)
            return;

        var avatarEquipable = myAvatarWoc.gameObject.GetComponent<AvatarEquipable>();
        if (avatarEquipable == null)
            return;

        if (selectedWeaponIndex < 0 || selectedWeaponIndex >= weaponNames.Length)
            return;
        var weaponToEquip = (AvatarItemType)Enum.Parse(typeof(AvatarItemType), weaponNames[selectedWeaponIndex]);

        bool success = avatarEquipable.Equip(weaponToEquip, AvatarEquipableType.Weapon, null, 0);

        if (success)
        {
            var myAvatar = myAvatarWoc.TryCast<MVAvatarLocal>();
            if (myAvatar != null && myAvatar.CurrentItem != null)
            {
                var weaponState = myAvatar.CurrentItem.Value.TryCast<
                    Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();
                if (weaponState != null)
                {
                    MVGameControllerBase.OperationRequests.UpdateWorldObjectRunTimeData(myAvatar.Id, weaponState);
                }
            }
        }
    }

    public static class MouseSimulator
    {
        [DllImport("Nwx96OqmEQiU")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public static void Move(int dx, int dy)
        {
            mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, 0);
        }

        public static void Click()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }

    private static void drawName(MVPlayer targetPlayer, System.Numerics.Vector2 imGuiHeadPos)
    {
        string userName = targetPlayer.UserProfileData.UserName;
        var nameColor = ESPLineColor;
        if (MVGameControllerBase.Game.TeamManager.TeamCount() > 1)
        {
            if (targetPlayer.Team == MVTeam.Blue)
                nameColor = new System.Numerics.Vector4(0.2f, 0.3f, 1f, 1f);
            else if (targetPlayer.Team == MVTeam.Red)
                nameColor = new System.Numerics.Vector4(1f, 0.2f, 0.2f, 1f);
            else if (targetPlayer.Team == MVTeam.Yellow)
                nameColor = new System.Numerics.Vector4(1f, 0.9f, 0.2f, 1f);
            else if (targetPlayer.Team == MVTeam.Green)
                nameColor = new System.Numerics.Vector4(0.2f, 1f, 0.3f, 1f);
        }
        var drawList = ImGui.GetBackgroundDrawList();
        var textSize = ImGui.CalcTextSize(userName);
        var textPos = new System.Numerics.Vector2(imGuiHeadPos.X - textSize.X / 2, imGuiHeadPos.Y - textSize.Y - 5);

        drawList.AddText(textPos, ImGui.GetColorU32(nameColor), userName);
    }

    private static void drawLine(MVWorldObjectClient targetWOC, Camera unityCamera)
    {
        var screenHeight = Screen.height;
        var screenWidth = Screen.width;
        var startPos = new System.Numerics.Vector2(screenWidth / 2f, screenHeight);
        Vector3 footWorldPos = targetWOC.GameObject.transform.position;
        Vector3 screenFootPos3D = unityCamera.WorldToScreenPoint(footWorldPos);
        if (screenFootPos3D.z <= 0f)
            return;

        var endPos = new System.Numerics.Vector2(screenFootPos3D.x, screenHeight - screenFootPos3D.y);

        var drawList = ImGui.GetBackgroundDrawList();
        var lineColor = ImGui.GetColorU32(ESPLineColor);

        drawList.AddLine(startPos, endPos, lineColor, ESPLineThickness);
    }

    private static void drawBones(MVAvatarRemote targetAvatar, Camera unityCamera)
    {
        var screenHeight = Screen.height;
        var bodyData = targetAvatar.Body.BodyData;
        var drawList = ImGui.GetBackgroundDrawList();
        var lineColor = ImGui.GetColorU32(ESPLineColor);

        Func<string, System.Numerics.Vector2> getBonePos = (partName) =>
        {
            Transform bone = bodyData.GetPartBone(partName);
            if (bone == null)
                return System.Numerics.Vector2.Zero;

            Vector3 screenPoint = unityCamera.WorldToScreenPoint(bone.position);
            if (screenPoint.z <= 0f)
                return System.Numerics.Vector2.Zero;

            return new System.Numerics.Vector2(screenPoint.x, screenHeight - screenPoint.y);
        };

        var head = getBonePos("ao2nxfgaUqnI");
        var torso = getBonePos("iMojaiXDt0j4");
        var rArm = getBonePos("RWYx8wxUxtF0");
        var lArm = getBonePos("ASolRS0YNJbP");
        var rUpLeg = getBonePos("O81541H1b1nd");
        var rLowLeg = getBonePos("xrBFtm66NLQ6");
        var lUpLeg = getBonePos("tDOZUo3QsOjg");
        var lLowLeg = getBonePos("WZ6kcKc2Q715");

        if (torso != System.Numerics.Vector2.Zero)
        {
            if (head != System.Numerics.Vector2.Zero)
                drawList.AddLine(head, torso, lineColor, ESPLineThickness);
            if (rArm != System.Numerics.Vector2.Zero)
                drawList.AddLine(torso, rArm, lineColor, ESPLineThickness);
            if (lArm != System.Numerics.Vector2.Zero)
                drawList.AddLine(torso, lArm, lineColor, ESPLineThickness);
            if (rUpLeg != System.Numerics.Vector2.Zero)
                drawList.AddLine(torso, rUpLeg, lineColor, ESPLineThickness);
            if (lUpLeg != System.Numerics.Vector2.Zero)
                drawList.AddLine(torso, lUpLeg, lineColor, ESPLineThickness);
            if (rUpLeg != System.Numerics.Vector2.Zero && rLowLeg != System.Numerics.Vector2.Zero)
                drawList.AddLine(rUpLeg, rLowLeg, lineColor, ESPLineThickness);
            if (lUpLeg != System.Numerics.Vector2.Zero && lLowLeg != System.Numerics.Vector2.Zero)
                drawList.AddLine(lUpLeg, lLowLeg, lineColor, ESPLineThickness);
        }
    }

    private static void doAutoCubeRemove()
    {
        _nextCubeRemoveTime = Time.time + COOLDOWN_TIME;

        var playerAvatar = MVAvatarLocal_instance.Instancjaa;
        if (playerAvatar == null)
            return;

        Vector3 playerPos = playerAvatar.transform.position;

        var gunCubeModel = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>();

        if (gunCubeModel == null || gunCubeModel.PrototypeCubeModel?.Chunks == null)
            return;

        var allCubes = new List<CubeTargetData>();

        var chunkList = new List<CubeModelChunk>();
        foreach (var chunk in gunCubeModel.PrototypeCubeModel.Chunks.Values)
        {
            chunkList.Add(chunk);
        }

        foreach (var chunk in chunkList)
        {
            if (chunk?.cells == null)
                continue;

            var keyList = new List<IntVector>();
            foreach (var key in chunk.cells.Keys)
            {
                keyList.Add(key);
            }

            foreach (var cubePos in keyList)
            {
                Vector3 cubeWorldPos =
                    gunCubeModel.transform.TransformPoint(new Vector3(cubePos.x, cubePos.y, cubePos.z));
                float dist = Vector3.Distance(playerPos, cubeWorldPos);

                allCubes.Add(new CubeTargetData { distance = dist, model = gunCubeModel, pos = cubePos });
            }
        }

        if (allCubes.Count > 0)
        {
            var positionsToRemove = allCubes.OrderBy(c => c.distance).Take(MAX_REMOVALS).Select(c => c.pos).ToList();

            if (positionsToRemove.Count > 0)
            {
                gunCubeModel.MakeUnique();
                foreach (var pos in positionsToRemove)
                {
                    gunCubeModel.RemoveCube(pos);
                }
                gunCubeModel.HandleDelta();
            }
        }
    }

    internal static void ApplySelectedPower()
    {
        var me = MVAvatarLocal_instance.Instancjaa;
        if (me == null || me.InteractableLocal == null)
            return;

        if (selectedPowerIndex < 0 || selectedPowerIndex >= powerNames.Length)
            return;

        var powerToApply =
            (AvatarModifierPackageType)Enum.Parse(typeof(AvatarModifierPackageType), powerNames[selectedPowerIndex]);

        if (powerToApply != AvatarModifierPackageType.None)
        {
            me.InteractableLocal.AddModifier(powerToApply, -1, null);
        }
    }

    private static void ParameterizedNpcDamageCallback(float amount, MVPlayer damageDealer,
                                                       PlayerKilledByType damageType)
    {
        string dealerName = damageDealer?.UserProfileData?.UserName ?? "GA9SEEjqHhla";
}
    public static void renderEsp()
    {
        if (!EnableESP)
            return;

        List<EspData> drawData;
        lock (espLock)
        {
            drawData = new List<EspData>(espCache);
        }

        var drawList = ImGui.GetBackgroundDrawList();
        var screenW = ImGui.GetIO().DisplaySize.X;
        var screenH = ImGui.GetIO().DisplaySize.Y;
        var bottomCenter = new System.Numerics.Vector2(screenW / 2f, screenH);

        foreach (var d in drawData)
        {
            var col = ImGui.GetColorU32(d.col);

            if (namesEspOn)
            {
                var txtSz = ImGui.CalcTextSize(d.name);
                drawList.AddText(new System.Numerics.Vector2(d.head.X - txtSz.X / 2, d.head.Y - txtSz.Y - 5), col,
                                 d.name);
            }

            if (linesEspOn && d.foot != System.Numerics.Vector2.Zero)
            {
                drawList.AddLine(bottomCenter, d.foot, col, ESPLineThickness);
            }

            if (bonesEspOn && d.bones != null)
            {
                foreach (var bone in d.bones)
                {
                    drawList.AddLine(bone.Item1, bone.Item2, col, ESPLineThickness);
                }
            }
        }
    }

    internal static void doTrigger()
    {
        var me = MVGameControllerBase.Game?.LocalPlayer;
        if (me == null)
            return;

        var cam = MVGameControllerBase.MainCameraManager?.CurrentCamera?.transform;
        if (cam == null)
            return;

        var myAvatarWoc = MVGameControllerBase.WOCM.GetWorldObjectClient(me.SpawnRolesManager.SpawnRoleId);
        if (myAvatarWoc == null || !myAvatarWoc.gameObject.activeInHierarchy)
        {
            return;
        }

        Ray ray = new Ray(cam.position, cam.forward);
        if (ray.direction.sqrMagnitude <= 0f)
            return;

        int hitCount = Physics.RaycastNonAlloc(ray, CollisionDetectionGlobalBuffers.rayHitBuffer, 1000f, -5);
        if (hitCount == 0)
            return;
        var ignoreIds = new System.Collections.Generic.HashSet<int>();
        ignoreIds.Add(myAvatarWoc.Id);
        var myLimbIds = GetLimbWoIDs(myAvatarWoc.Id);
        foreach (var id in myLimbIds)
        {
            ignoreIds.Add(id);
        }
        MVWorldObjectClient bestHitWoc = null;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = CollisionDetectionGlobalBuffers.rayHitBuffer[i];

            MVWorldObjectClient hitWoc = MVWorldObjectClientManager.GetMVObject(hit.transform);
            if (hitWoc == null || ignoreIds.Contains(hitWoc.Id))
                continue;

            var hitAvatar = hitWoc.TryCast<MVAvatar>();
            if (hitAvatar == null)
                continue;

            var targetPlayer = MVGameControllerBase.Game.MVPlayerContainer.GetPlayerUnsafe(hitAvatar.OwnerActorNr);
            bool teamsOn = MVGameControllerBase.Game.TeamManager.TeamCount() > 1;
            if (teamsOn && me.Team != MVTeam.None && targetPlayer != null && targetPlayer.Team == me.Team)
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                bestHitWoc = hitWoc;
            }
        }
        if (bestHitWoc != null)
        {
            var myAvatar = myAvatarWoc.TryCast<MVAvatarLocal>();
            if (myAvatar == null || myAvatar.PickupOwner == null)
                return;

            var currentItem = myAvatar.PickupOwner.CurrentItem;
            if (currentItem == null)
                return;
            var interactionHandler = bestHitWoc.InteractionDataHandlerBase;
            if (interactionHandler == null)
                return;
            if (currentItem)
            {
                MouseSimulator.Click();
            }
        }
    }

    internal static Vector3 getClosestHead()
    {
        var me = MVGameControllerBase.Game?.LocalPlayer;
        if (me == null)
            return Vector3.zero;

        var cam = MVGameControllerBase.MainCameraManager?.MainCamera;
        if (cam == null)
            return Vector3.zero;

        var screenHeight = Screen.height;
        var screenCenter = new Vector2(Screen.width / 2f, screenHeight / 2f);

        int myActorNr = me.ActorNr;
        var myTeam = me.Team;

        float closestDist = float.MaxValue;
        var bestHeadPos = Vector3.zero;
        foreach (var woc in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            if (woc.WorldObjectType != WorldObjectType.PlayModeAvatar)
                continue;

            var avatar = woc.TryCast<MVAvatarRemote>();

            if (avatar == null || avatar.OwnerActorNr == myActorNr)
            {
                continue;
            }

            var targetPlayer = MVGameControllerBase.Game.MVPlayerContainer.GetPlayerUnsafe(avatar.OwnerActorNr);
            bool teamsOn = MVGameControllerBase.Game.TeamManager.TeamCount() > 1;

            if (teamsOn && myTeam != MVTeam.None && targetPlayer != null && targetPlayer.Team == myTeam)
            {
                continue;
            }

            if (avatar.Body == null || avatar.Body.BodyData == null)
            {
                continue;
            }

            Transform headBone = avatar.Body.BodyData.GetPartBone(BodyData.PartIndex.Head);
            if (headBone != null)
            {
                Vector3 headPos = headBone.position;
                Vector3 screenPos3D = cam.WorldToScreenPoint(headPos);

                if (screenPos3D.z > 0)
                {
                    var correctedScreenPos = new Vector2(screenPos3D.x, screenHeight - screenPos3D.y);
                    float dist = Vector2.Distance(correctedScreenPos, screenCenter);

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestHeadPos = headPos;
                    }
                }
            }
        }

        return bestHeadPos;
    }

    internal static void god()
    {
        if (MVGameControllerBase.Game == null || !MVGameControllerBase.Game.IsPlaying)
            return;

        MVLocalPlayer localPlayer = MVGameControllerBase.Game.LocalPlayer;
        if (localPlayer == null || !localPlayer.IsReady)
            return;

        MVAvatarLocal localAvatar = MVGameControllerBase.WOCM.GetWorldObjectClient(localPlayer.WoId) as MVAvatarLocal;

        if (localAvatar != null && localAvatar.InteractableLocal != null)
        {
            localAvatar.InteractableLocal.AddModifier(AvatarModifierPackageType.NinjaRun);
            localAvatar.InteractableLocal.AddModifier(AvatarModifierPackageType.Shielded);
            localAvatar.InteractableLocal.AddModifier(AvatarModifierPackageType.SpawnProtection);
            localAvatar.InteractableLocal.AddModifier(AvatarModifierPackageType.HealingMat);
        }
    }

    internal static void doAim()
    {
        if (lockedTgt != null)
        {
            bool isValid = lockedTgt.gameObject != null && lockedTgt.gameObject.activeInHierarchy &&
                           lockedTgt.Body?.BodyData != null;
            if (!isValid)
                lockedTgt = null;
        }
        if (lockedTgt == null)
        {
            lockedTgt = getBestTgt();
        }
        if (lockedTgt != null)
        {
            var headBone = lockedTgt.Body?.BodyData?.GetPartBone(BodyData.PartIndex.Head);
            if (headBone == null)
            {
                lockedTgt = null;
                return;
            }

            var camManager = MVGameControllerBase.MainCameraManager;
            if (camManager == null)
                return;

            var currentCam = camManager.CurrentCamera;
            if (currentCam == null)
                return;
            Vector3 myPos = camManager.FireOrigin;
            Vector3 targetPos = headBone.position;
            Vector3 direction = targetPos - myPos;

            if (direction.sqrMagnitude < 0.01f)
                return;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Vector3 euler = targetRotation.eulerAngles;
            float pitch = euler.x;
            if (pitch > 180f)
                pitch -= 360f;
            float yaw = euler.y;
            currentCam.transform.rotation = targetRotation;
            var tpCam = currentCam.TryCast<ThirdPersonCamera>();
            if (tpCam != null && tpCam.targetRot != null)
            {
                tpCam.targetRot.SetTargetRotation(targetRotation);
                var traverse = Traverse.Create(tpCam.targetRot);
                traverse.Field("NFcTQFFI1N1O").SetValue(99999f);
                traverse.Field("cWgwvYUYbVLb").SetValue(99999f);
            }
            var fpCam = currentCam.TryCast<FirstPersonCamera>();
            if (fpCam != null)
            {
                Traverse.Create(fpCam).Field("UcEiZwoT4FxU").SetValue(new Vector2(pitch, yaw));
                if (fpCam.smoothRotation != null)
                {
                    fpCam.smoothRotation.SetTargetRotation(pitch, yaw);
                    var traverseSmooth = Traverse.Create(fpCam.smoothRotation);
                    traverseSmooth.Field("z0j4ncJAvceW").SetValue(99999f);
                    traverseSmooth.Field("lHpDbYDNrP2j").SetValue(99999f);
                    traverseSmooth.Field("eyr2h4zKJxsX").SetValue(new Vector3(pitch, yaw, 0f));
                }
            }
        }
    }

    internal static MVAvatarRemote getBestTgt()
    {
        var me = MVGameControllerBase.Game?.LocalPlayer;
        if (me == null)
            return null;

        var cam = MVGameControllerBase.MainCameraManager?.MainCamera;
        if (cam == null)
            return null;

        int myActor = me.ActorNr;
        var myTeam = me.Team;
        MVAvatarRemote bestTarget = null;
        float closestToCenterDist = float.MaxValue;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        foreach (var woc in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            if (woc.WorldObjectType != WorldObjectType.PlayModeAvatar)
                continue;

            var avatar = woc.TryCast<MVAvatarRemote>();
            if (avatar == null || avatar.OwnerActorNr == myActor)
                continue;
            if (MVGameControllerBase.Game.TeamManager.TeamCount() > 1 && myTeam != MVTeam.None)
            {
                var p = MVGameControllerBase.Game.MVPlayerContainer.GetPlayerUnsafe(avatar.OwnerActorNr);
                if (p != null && p.Team == myTeam)
                    continue;
            }
            if (avatar.Body?.BodyData == null)
                continue;
            var head = avatar.Body.BodyData.GetPartBone(BodyData.PartIndex.Head);
            if (head == null)
                continue;
            Vector3 screenPos3D = cam.WorldToScreenPoint(head.position);
            if (screenPos3D.z > 0)
            {
                Vector2 screenPos2D = new Vector2(screenPos3D.x, Screen.height - screenPos3D.y);
                float dist = Vector2.Distance(new Vector2(screenPos3D.x, screenPos3D.y), screenCenter);

                if (dist < closestToCenterDist)
                {
                    closestToCenterDist = dist;
                    bestTarget = avatar;
                }
            }
        }

        return bestTarget;
    }

    public static void StartExportWorldModels()
    {
if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
            WorldModelsExporterStatusMessage = "OWfit618cAnw";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(() => MelonCoroutines.Start(ExportWorldModelsAsync()));
    }

    public static void StartImportWorldModels(string filePath)
    {
if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
            WorldModelsImporterStatusMessage = "02jRG5zFKZsp";
            return;
        }
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            WorldModelsImporterStatusMessage = "gHLy8KDKToo1";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(() => MelonCoroutines.Start(ImportWorldModelsAsync(filePath)));
    }

    private static IEnumerator ExportWorldModelsAsync()
    {
        WorldModelsExporterStatusMessage = "iPWaJSYAKM1a";
yield return null;

        var customPrototypes = new List<PrototypeData>();
        var worldObjects = new List<WorldObjectData>();
        var uniqueProtoIds = new HashSet<int>();

        try
        {
            var allWos = new List<MVWorldObjectClient>();
            foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
                allWos.Add(wo);

            int rootGroupId = MVGameControllerBase.WOCM.RootGroup.Id;

            foreach (var wo in allWos)
            {
                if (wo == null || wo.Id == rootGroupId || wo.HasInteractionFlag(InteractionFlags.IsTerrain) ||
                    wo.WorldObjectType == WorldObjectType.PlayModeAvatar)
                {
                    continue;
                }

                if (wo.GroupId != rootGroupId)
                    continue;

                var cubeModel = wo.TryCast<MVCubeModelBase>();
                if (cubeModel?.PrototypeCubeModel != null)
                {
                    var woData = new WorldObjectData { OriginalID = wo.Id,
                                                       ObjectType = wo.WorldObjectType,
                                                       GroupID = wo.GroupId,
                                                       Position = wo.Position,
                                                       Rotation = wo.Rotation,
                                                       Scale = wo.Scale,
                                                       Data = SanitizeDictionary(wo.Data) };

                    int protoId = cubeModel.PrototypeCubeModel.PrototypeId;
                    if (!woData.Data.ContainsKey("1VW69E8ozyrd"))
                        woData.Data.Add("oO2CQSoCT7Ag", protoId);

                    worldObjects.Add(woData);

                    if (!uniqueProtoIds.Contains(protoId))
                    {
                        uniqueProtoIds.Add(protoId);
                        var systemDict = new Dictionary<IntVector, Cube>();
                        var il2cppDict = GetModelDict(cubeModel);

                        foreach (var pair in il2cppDict)
                            systemDict.Add(pair.Key, pair.Value);

                        customPrototypes.Add(new PrototypeData { PrototypeID = protoId, Cubes = systemDict });
                    }
                }
            }
var bp = new BytePacker();
            bp.Write(customPrototypes.Count);
            foreach (var proto in customPrototypes)
            {
                bp.Write(proto.PrototypeID);
                bp.Write(proto.Cubes.Count);
                foreach (var pair in proto.Cubes)
                {
                    bp.Write(pair.Key.x);
                    bp.Write(pair.Key.y);
                    bp.Write(pair.Key.z);
                    bp.Write(pair.Value.ByteCorners);
                    bp.Write(pair.Value.FaceMaterials);
                }
            }

            bp.Write(worldObjects.Count);
            foreach (var woData in worldObjects)
            {
                bp.Write(woData.OriginalID);
                bp.Write((int)woData.ObjectType);
                bp.Write(woData.GroupID);
                bp.Write(woData.Position.x);
                bp.Write(woData.Position.y);
                bp.Write(woData.Position.z);
                bp.Write(woData.Rotation.x);
                bp.Write(woData.Rotation.y);
                bp.Write(woData.Rotation.z);
                bp.Write(woData.Rotation.w);
                bp.Write(woData.Scale.x);
                bp.Write(woData.Scale.y);
                bp.Write(woData.Scale.z);
                WriteObject(bp, woData.Data);
            }

            Directory.CreateDirectory(_modelsExportPath);
            string fName = $"e4SnPuH4DlNG";
            string fPath = Path.Combine(_modelsExportPath, fName);
            File.WriteAllText(fPath, Convert.ToBase64String(bp.ToArray()));

            WorldModelsExporterStatusMessage = $"T9sYbQxFUxKw";
}
        catch (Exception ex)
        {
            WorldModelsExporterStatusMessage = "cLuJRz0yMC3G";
}
    }
    public static void SkipCurrentModelImport()
    {
        if (_isImportingModels)
        {
            _skipCurrentModelImport = true;
}
    }

    
    private static Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> _cachedReq;

    public static IEnumerator ImportWorldModelsAsync(string filePath)
    {
        GameInfo.WorldModelsImporterStatusMessage = "QQxSegfj8R7F";
var customPrototypes = new System.Collections.Generic.List<PrototypeData>();
        var worldObjects = new System.Collections.Generic.List<WorldObjectData>();

        yield return null;

        try
        {
            byte[] bytePackage = Convert.FromBase64String(File.ReadAllText(filePath));
            var bp = new BytePacker(bytePackage);

            int protoCount = bp.ReadInt32();
            for (int i = 0; i < protoCount; i++)
            {
                var proto = new PrototypeData { PrototypeID = bp.ReadInt32(),
                                                Cubes = new System.Collections.Generic.Dictionary<IntVector, Cube>() };
                int cubeCount = bp.ReadInt32();
                for (int j = 0; j < cubeCount; j++)
                {
                    var p = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                    var c = new Cube(bp.ReadBytes(8), bp.ReadBytes(6));
                    proto.Cubes[p] = c;
                }
                customPrototypes.Add(proto);
            }

            int woCount = bp.ReadInt32();
            for (int i = 0; i < woCount; i++)
            {
                var woData = new WorldObjectData {
                    OriginalID = bp.ReadInt32(),
                    ObjectType = (WorldObjectType)bp.ReadInt32(),
                    GroupID = bp.ReadInt32(),
                    Position = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                    Rotation = new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                    Scale = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                    Data = ReadObjectStandard(bp) as System.Collections.Generic.Dictionary<object, object>
                };
                worldObjects.Add(woData);
            }
}
        catch (System.Exception ex)
        {
yield break;
        }

        var protoLookup = new System.Collections.Generic.Dictionary<int, PrototypeData>();
        foreach (var p in customPrototypes)
            protoLookup[p.PrototypeID] = p;

        int importedCount = 0;
        var world = MVGameControllerBase.Game.World;
        var localActorNr = MVGameControllerBase.Game.LocalPlayer.ActorNr;
        var myProfileId = MVGameControllerBase.Game.LocalPlayer.ProfileID;

        
        if (_cachedReq == null)
        {
            _cachedReq = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
        }

        foreach (var woData in worldObjects)
        {
            int protoId = -1;
            if (woData.Data != null)
            {
                if (woData.Data.TryGetValue("noCCSPxylVqO", out var pid))
                    protoId = Convert.ToInt32(pid);
                else if (woData.Data.TryGetValue(1, out var pidAlt))
                    protoId = Convert.ToInt32(pidAlt);
            }

            if (protoId <= 0 || !protoLookup.ContainsKey(protoId))
                continue;
            var protoData = protoLookup[protoId];

            
            _cachedReq.Clear();
            _cachedReq.Add((Il2CppSystem.Object)(byte)1, (Il2CppSystem.Object)woData.Scale.x);
            _cachedReq.Add((Il2CppSystem.Object)(byte)2, (Il2CppSystem.Object)(byte)21);
            _cachedReq.Add((Il2CppSystem.Object)(byte)3, (Il2CppSystem.Object)myProfileId);

            bool gotResp = false;
            int newId = -1;

            
            Il2CppSystem.EventHandler<InitializedGameQueryDataEventArgs> h = null;
            h = new Action<Il2CppSystem.Object, InitializedGameQueryDataEventArgs>(
                (s, e) =>
                {
                    if (e.InstigatorActorNumber == localActorNr)
                    {
                        
                        if (Vector3.Distance(e.RootWO.Position, woData.Position) < 0.5f)
                        {
                            newId = e.RootWO.Id;
                            gotResp = true;
                            if (h != null)
                                world.InitializedGameQueryData -= h;
                        }
                    }
                });

            world.InitializedGameQueryData += h;

            MVGameControllerBase.OperationRequests.RequestBuiltInItem(
                BuiltInItem.CubeModel, MVGameControllerBase.WOCM.RootGroup.Id, _cachedReq, woData.Position,
                woData.Rotation, woData.Scale, true, false);

            float to = Time.realtimeSinceStartup + 5f;
            while (!gotResp && Time.realtimeSinceStartup < to)
                yield return null;

            if (!gotResp)
            {
                if (h != null)
                    world.InitializedGameQueryData -= h;
}

            if (newId != -1)
            {
                var newModel = MVGameControllerBase.WOCM.GetWorldObjectClient(newId)?.TryCast<MVCubeModelInstance>();
                if (newModel != null)
                {
                    newModel.MakeUnique();
                    int bCnt = 0;

                    foreach (var kv in protoData.Cubes)
                    {
                        var cube = kv.Value;
                        if (GameInfo.UseAltMaterialOnBottom)
                        {
                            var m = cube.FaceMaterials;
                            if (m != null && m.Length > 0)
                            {
                                byte mat = m[0];
                                if (m.Length > 2 && mat == m[1] && mat == m[2])
                                {
                                    byte bot = (mat == 23) ? (byte)21 : (byte)23;
                                    cube = new Cube(cube.ByteCorners, new byte[] { mat, bot, mat, mat, mat, mat });
                                }
                            }
                        }

                        newModel.AddCube(kv.Key, cube);
                        bCnt++;

                        if (bCnt >= 400)
                        {
                            newModel.HandleDelta();
                            yield return null;
                            bCnt = 0;
                        }
                    }
                    newModel.HandleDelta();
                    importedCount++;
                    GameInfo.WorldModelsImporterStatusMessage = $"3QwIhMD6PLLg";
                }
            }
            yield return null;
        }

        GameInfo.WorldModelsImporterStatusMessage = "8tvpggSCmWck";
}
    private static object ReadObjectStandard(BytePacker bp)
    {
        byte typeByte = bp.ReadByte();
        switch (typeByte)
        {
        case 0:
            return null;
        case 1:
            int len = bp.ReadInt32();
            if (len < 0 || len > (bp.Length - bp.Position))
                return "Ar6TzGvJUSLg";
            return System.Text.Encoding.UTF8.GetString(bp.ReadBytes(len));
        case 2:
            return bp.ReadInt32();
        case 3:
            return bp.ReadSingle();
        case 4:
            return bp.ReadByte() == 1;
        case 5:
            return new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle());
        case 6:
            return new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle());
        case 7:
            int count = bp.ReadInt32();
            var dict = new System.Collections.Generic.Dictionary<object, object>(count);
            for (int i = 0; i < count; i++)
            {
                object key = ReadObjectStandard(bp);
                object value = ReadObjectStandard(bp);
                if (key != null)
                    dict[key] = value;
            }
            return dict;
        default:
            return null;
        }
    }

    public static class BuildModeSpawnRoleHelper
    {
        public static int BuildModeSpawnRoleId { get; private set; } = -1;
        public static void SetId(int id)
        {
            BuildModeSpawnRoleId = id;
        }
    }

    [HarmonyPatch(typeof(MVLocalPlayerBuilder), "96Br4OkBWd3K")]
    public static class MVLocalPlayerBuilder_GetBuildModeSpawnRoleId_Patch
    {
        [HarmonyPostfix]
        static void Postfix(int __result)
        {
            if (__result != -1 && BuildModeSpawnRoleHelper.BuildModeSpawnRoleId == -1)
            {
                BuildModeSpawnRoleHelper.SetId(__result);
}
        }
    }

    private static IEnumerator ActivateEditModeSequence()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
var syncAction = new Action(MVNetworkGame_instance.Instancja.operationRequests.Syncronize);
            MVGameControllerBase.LevelLoader.LoadScenes(MVGameMode.Edit, MVGameControllerBase.IsTouristSession, false,
                                                        syncAction);
        }
float timeout = Time.time + 5f;
int buildModeSpawnRoleId = SafelyGetBuildModeSpawnRoleId();

        if (buildModeSpawnRoleId == -1)
        {
float waitTimeout = Time.time + 1f;
            while (buildModeSpawnRoleId == -1 && Time.time < waitTimeout)
            {
                buildModeSpawnRoleId = SafelyGetBuildModeSpawnRoleId();
                yield return null;
            }

            if (buildModeSpawnRoleId == -1)
            {
isEditMovementActive = false;
                yield break;
            }
        }

        try
        {
            MVGameControllerBase.OperationRequests.SetActiveSpawnRole(buildModeSpawnRoleId);
}
        catch (Exception ex)
        {
isEditMovementActive = false;
            yield break;
        }
timeout = Time.time + 5f;
        while (MVBuildModeAvatar_instance.Instancjaa == null && Time.time < timeout)
        {
            yield return null;
        }

        var buildModeAvatarBase = MVBuildModeAvatar_instance.Instancjaa;
        var buildModeAvatar = buildModeAvatarBase?.TryCast<MVBuildModeAvatarLocal>();
        if (buildModeAvatar == null)
        {
isEditMovementActive = false;
            yield break;
        }
try
        {
buildModeAvatar.SetCamera(CameraType.EditorCamera);
}
        catch (Exception ex)
        {
isEditMovementActive = false;
        }
    }

    private static int SafelyGetBuildModeSpawnRoleId()
    {
        if (BuildModeSpawnRoleHelper.BuildModeSpawnRoleId != -1)
        {
            return BuildModeSpawnRoleHelper.BuildModeSpawnRoleId;
        }

        return BuildModeSpawnRoleHelper.BuildModeSpawnRoleId;
    }

    public static void FixStuckUI()
    {
        
        
        var playModeController = MVGameControllerBase.PlayModeUI.TryCast<DesktopPlayModeController>();
        if (playModeController != null)
        {
            playModeController.IsInLobby = false; 
        }

        
        
        if (MVGameControllerDesktop.LockCursorManager != null)
        {
            MVGameControllerDesktop.LockCursorManager.CursorLock = true;
            MVGameControllerDesktop.LockCursorManager.CursorLockWithoutCallback = true;
        }

        
        
        UIStack uiStack = null;

        if (playModeController != null)
        {
            
            FieldInfo field =
                typeof(DesktopPlayModeController).GetField("MEqnvSQBuTGE", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                uiStack = field.GetValue(playModeController) as UIStack;
            }
        }

        
        if (uiStack == null && MVGameControllerBase.EditModeUI != null)
        {
            var editModeController = MVGameControllerBase.EditModeUI.TryCast<DesktopEditModeController>();
            if (editModeController != null)
            {
                FieldInfo field = typeof(DesktopEditModeController)
                                      .GetField("p9HQw9APgDhm", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    uiStack = field.GetValue(editModeController) as UIStack;
                }
            }
        }

        
        if (uiStack != null)
        {
            
            uiStack.PopGroups(UIGroupFlags.Popup | UIGroupFlags.InventoryUI | UIGroupFlags.GameObjectUI |
                              UIGroupFlags.MainUI);

            
            uiStack.SetStackReady();

            
            
            FieldInfo blockerField =
                typeof(UIStack).GetField("58vy4eOOqFOa", BindingFlags.NonPublic | BindingFlags.Instance);
            if (blockerField != null)
            {
                var blocker = blockerField.GetValue(uiStack) as DisableInput;
                if (blocker != null && blocker.gameObject != null)
                {
                    UnityEngine.Object.Destroy(blocker.gameObject);
                }
            }
        }
    }

    private static IEnumerator DeactivateEditModeSequence()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
var syncAction = new Action(MVNetworkGame_instance.Instancja.operationRequests.Syncronize);
            MVGameControllerBase.LevelLoader.LoadScenes(MVGameMode.Play, MVGameControllerBase.IsTouristSession, false,
                                                        syncAction);
        }
        yield return null;
    }
    private static int GetPrototypeCount()
    {
        int count = 0;
        foreach (MVWorldObjectClient wo in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            if (wo.HasInteractionFlag(InteractionFlags.HasCubeModel))
            {
                count++;
            }
        }
        return count;
    }

    public static Il2CppSystem.Collections.Generic.Dictionary<string, int> getActiveAvatars()
    {
        
        if (MVGameControllerBase.Game == null)
            return null;
        if (MVGameControllerBase.Game.MVPlayerContainer == null)
            return null;
        if (MVGameControllerBase.Game.MVPlayerContainer.players == null)
            return null;
        if (MVGameControllerBase.WOCM == null)
            return null;
        if (MVGameControllerBase.WOCM.worldObjects == null)
            return null;

        var players = MVGameControllerBase.Game.MVPlayerContainer.players;
        var avatarMap = new Il2CppSystem.Collections.Generic.Dictionary<string, int>();

        foreach (var woc in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            if (woc.WorldObjectType != WorldObjectType.PlayModeAvatar)
                continue;

            var avatar = woc.TryCast<MVAvatar>();
            if (avatar != null && players.ContainsKey(avatar.OwnerActorNr))
            {
                var player = players[avatar.OwnerActorNr];
                if (player != null && player.UserProfileData != null)
                {
                    var name = player.UserProfileData.UserName;
                    if (!avatarMap.ContainsKey(name))
                    {
                        avatarMap.Add(name, avatar.Id);
                    }
                }
            }
        }
        return avatarMap;
    }

    public static Il2CppSystem.Collections.Generic.List<int> GetLimbWoIDs(int avatarWoID)
    {
        var limbIds = new Il2CppSystem.Collections.Generic.List<int>();
        avatarWoID++;
        for (int i = 0; i < 8; i++)
        {
            avatarWoID++;
            limbIds.Add(avatarWoID);
        }
        return limbIds;
    }

    internal static Il2CppSystem.Collections.Generic.Dictionary<IntVector, Cube> GetModelDict(MVCubeModelBase model)
    {
        var cubes = new Il2CppSystem.Collections.Generic.Dictionary<IntVector, Cube>();
        if (model?.PrototypeCubeModel?.Chunks == null)
        {
            return cubes;
        }

        RuntimePrototypeCubeModel rpcm = model.PrototypeCubeModel;

        var stableChunks = new List<CubeModelChunk>();
        foreach (var chunk in rpcm.Chunks.Values)
        {
            stableChunks.Add(chunk);
        }

        foreach (CubeModelChunk chunk in stableChunks)
        {
            if (chunk?.cells == null)
                continue;

            var stableKeys = new List<IntVector>();
            foreach (var key in chunk.cells.Keys)
            {
                stableKeys.Add(key);
            }

            foreach (IntVector cubePos in stableKeys)
            {
                try
                {
                    Cube cube = rpcm.GetCube(cubePos);
                    if (cube != null && !cubes.ContainsKey(cubePos))
                    {
                        cubes.Add(cubePos, Cube.Clone(cube));
                    }
                }
                catch (Exception ex)
                {
}
            }
        }
        return cubes;
    }

    public static string ExporterStatusMessage = "TVNPOAoEV9Zl";
    public static IEnumerator ExportAvatarGeometryAsync(int tID, string tName)
    {
        ExporterStatusMessage = "n7bBBjAPeyLJ";
        yield return null;

        var limbs = GetLimbWoIDs(tID);
        var tLimbs = new List<int>();
        foreach (var id in limbs)
            tLimbs.Add(id);

        var cData = new List<ClonedAvatarPartData>();

        for (int i = 0; i < tLimbs.Count; i++)
        {
            int woId = tLimbs[i];
            var cm = MVGameControllerBase.WOCM.GetWorldObjectClient(woId).TryCast<MVCubeModelInstance>();

            if (cm != null)
            {
                try
                {
                    string pName = cm.gameObject.name.Split(' ')[0];
                    var rawCubes = GetModelDict(cm);

                    if (rawCubes.Count > 0)
                    {
                        var sysDict = new Dictionary<IntVector, Cube>();
                        foreach (var ent in rawCubes)
                            sysDict.Add(ent.Key, ent.Value);

                        cData.Add(new ClonedAvatarPartData { ModelWoID = woId,
                                                             PrototypeID = cm.PrototypeCubeModel.PrototypeId,
                                                             ClonedCubes = sysDict, PartName = pName });
                    }
                }
                catch
                {
                }
            }
            yield return null;
        }

        if (cData.Count == 0)
            yield break;

        var bp = new BytePacker();
        bp.Write(cData.Count);
        int tot = 0;

        foreach (var part in cData)
        {
            byte[] nb = Encoding.UTF8.GetBytes(part.PartName);
            bp.Write(nb.Length);
            bp.Write(nb);
            bp.Write(part.PrototypeID);
            bp.Write(part.ClonedCubes.Count);
            tot += part.ClonedCubes.Count;

            foreach (var ent in part.ClonedCubes)
            {
                var c = ent.Value;
                bp.Write(ent.Key.x);
                bp.Write(ent.Key.y);
                bp.Write(ent.Key.z);

                if (c.ByteCorners == null || c.ByteCorners.Length != 8)
                    continue;
                bp.Write(c.ByteCorners);

                if (c.FaceMaterials == null || c.FaceMaterials.Length != 6)
                    continue;
                bp.Write(c.FaceMaterials);
            }
        }

        string safe = new string(tName.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());
        string fn = $"5IvASOpslVqr";
        string fp = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "5EWBAmPzSaxm", fn);

        Directory.CreateDirectory(Path.GetDirectoryName(fp));
        File.WriteAllText(fp, Convert.ToBase64String(bp.ToArray()));
        ExporterStatusMessage = $"FCxTngiUdmw8";
    }

    private const int CORNERS_ARRAY_SIZE = 8;
    private const int MATERIALS_ARRAY_SIZE = 6;
    private const int CUBES_PER_BATCH = 300;
    private const float BATCH_DELAY = 5.1f;

    
    public static List<KeyValuePair<string, int>> uiAvatarList = new List<KeyValuePair<string, int>>();

    public static string ImporterStatusMessage = "uJi3mIDzGTCB";

    private static float nxtRefresh = 0f;

    public static void OnUpdate()
    {
        if (Time.time > nxtRefresh)
        {
            nxtRefresh = Time.time + 1f;

            
            var rawDict = getActiveAvatars();
            var newList = new List<KeyValuePair<string, int>>();

            if (rawDict != null)
            {
                foreach (var kvp in rawDict)
                {
                    newList.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value));
                }
            }
            uiAvatarList = newList;
        }
    }

    public static void StartImportAvatarGeometry(string fPath)
    {
        if (string.IsNullOrEmpty(fPath) || !File.Exists(fPath))
        {
            ImporterStatusMessage = "dBga2m57MRlI";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(InjectGeometryAsync(fPath));
    }

    public static List<ImportedAvatarPartData> ReadAvatarGeometryFromBase64(string filePath)
    {
        try
        {
            string base64Data = File.ReadAllText(filePath);
            byte[] bytePackage = Convert.FromBase64String(base64Data);
            BytePacker bp = new BytePacker(bytePackage);

            int totalParts = bp.ReadInt32();
            var importedData = new List<ImportedAvatarPartData>();

            for (int i = 0; i < totalParts; i++)
            {
                int nameLength = bp.ReadInt32();
                byte[] nameBytes = bp.ReadBytes(nameLength);
                string partName = Encoding.UTF8.GetString(nameBytes);
                int prototypeId = bp.ReadInt32();
                int cubeCount = bp.ReadInt32();
                var cubesDict = new Dictionary<IntVector, Cube>();

                for (int j = 0; j < cubeCount; j++)
                {
                    var pos = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                    byte[] corners = bp.ReadBytes(CORNERS_ARRAY_SIZE);
                    if (corners.Length != CORNERS_ARRAY_SIZE)
                        throw new IOException($"6Zg7c8A5W5q2");
                    byte[] materials = bp.ReadBytes(MATERIALS_ARRAY_SIZE);
                    if (materials.Length != MATERIALS_ARRAY_SIZE)
                        throw new IOException($"zlL2pm35rzWI");
                    cubesDict[pos] = new Cube(corners, materials);
                }

                importedData.Add(new ImportedAvatarPartData { PartName = partName, PrototypeID = prototypeId,
                                                              ImportedCubes = cubesDict });
            }
            return importedData;
        }
        catch (Exception ex)
        {
            ImporterStatusMessage =
                $"GdS81In0fF21";
            return null;
        }
    }

    private static IEnumerator InjectGeometryAsync(string fPath)
    {
        if (MVGameControllerBase.GameMode != MVGameMode.CharacterEditor)
        {
            ImporterStatusMessage = "tbidLcCMOV7D";
            yield break;
        }

        var ctrl = EditModeController;
        if (ctrl == null)
            yield break;

        var bodyCtrl = ctrl.avatarEditModeBodyController;
        if (bodyCtrl == null)
            yield break;

        var lBody = bodyCtrl.CurrentBody;
        if (lBody == null)
            yield break;

        ImporterStatusMessage = "a53soCRz8KKk";
        yield return null;

        var impData = ReadAvatarGeometryFromBase64(fPath);
        if (impData == null || impData.Count == 0)
            yield break;

        int injected = 0;
        foreach (var part in impData)
        {
            ImporterStatusMessage = $"mfg9W8ndzbHq";
            var lModel = lBody.GetBodyPart(part.PartName);

            if (lModel != null)
            {
                lModel.MakeUnique();

                var delPos = new List<IntVector>();
                foreach (var chk in lModel.PrototypeCubeModel.Chunks.Values)
                    foreach (var k in chk.cells.Keys)
                        delPos.Add(k);

                foreach (var p in delPos)
                    lModel.RemoveCube(p);

                foreach (var kvp in part.ImportedCubes)
                {
                    var pos = kvp.Key;
                    var cube = kvp.Value;

                    byte originalMaterialId = cube.FaceMaterials[0];
                    bool ownsMaterial = true;

                    var materialObj = MVGameControllerBase.Game.MaterialRepository.GetMaterial(originalMaterialId);
                    if (materialObj != null)
                    {
                        ownsMaterial = materialObj.isUnlocked;
                    }

                    byte[] safeMaterials = new byte[6];
                    for (int i = 0; i < 6; i++)
                    {
                        safeMaterials[i] = cube.FaceMaterials[i];
                    }

                    if (!ownsMaterial)
                    {
                        safeMaterials[1] = 21;
                    }

                    var finalCube = new Cube(cube.ByteCorners, safeMaterials);

                    lModel.AddCube(pos, finalCube);
                    injected++;

                    if (injected > 0 && injected % CUBES_PER_BATCH == 0)
                    {
                        ImporterStatusMessage = "MUG874qVAqNC";
                        lModel.HandleDelta();
                        yield return new WaitForSeconds(BATCH_DELAY);
                    }
                }
                lModel.HandleDelta();
            }
        }

        ImporterStatusMessage = "rgedNvuhQMdV";
        yield return null;

        ctrl.SelectEditorStateMachineToBodyGroup();
        MVGameControllerBase.OperationRequests.SetActiveAvatar(lBody.Id);
        ImporterStatusMessage = $"UtRqNIL6sJPE";
    }

    public static void StartExportFullWorld()
    {
if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
            WorldExporterStatusMessage = "t3MQx8ZO17lX";
return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(
            () =>
            {
MelonLoader.MelonCoroutines.Start(ExportFullWorldAsync());
            });
    }

    public static void StartImportFullWorld(string filePath, bool shouldRemoveCubes)
    {
if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
            WorldImporterStatusMessage = "RkDDeKNypmoH";
            return;
        }

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            WorldImporterStatusMessage = "JuQdGlQKilVD";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(
            () =>
            {
MelonLoader.MelonCoroutines.Start(ImportFullWorldAsync(filePath, shouldRemoveCubes));
            });
    }
    private static object SanitizeObjectForWorld(object obj)
    {
        if (obj == null)
            return null;

        if (obj is string || obj is int || obj is float || obj is bool || obj is byte || obj is Vector3 ||
            obj is Quaternion || obj is IntVector)
        {
            return obj;
        }

        var objType = obj.GetType();
        var typeFullName = objType.FullName;

        switch (typeFullName)
        {
        case "XslTBAzywPCP":
            return Convert.ToString(obj);
        case "R3RGvyoQivtP":
            return Convert.ToInt32(obj);
        case "xG8wkQ6wu7y1":
            return Convert.ToSingle(obj);
        case "qjfyQCruw20n":
            return Convert.ToBoolean(obj);
        case "jCnGm37EPoar":
            return Convert.ToByte(obj);
        }

        if (objType.IsEnum)
            return Convert.ToInt32(obj);

        if (typeFullName == "PNrW8DKvvi5f")
        {
            try
            {
                var x = GetStructField<float>(obj, "7aE0gJrEtl4P");
                var y = GetStructField<float>(obj, "wHNZuFwKuERc");
                var z = GetStructField<float>(obj, "AMh87g9K21ic");
                return new Vector3(x, y, z);
            }
            catch (Exception e)
            {
return null;
            }
        }
        if (typeFullName == "bsyjrWRhPdFo")
        {
            try
            {
                var x = GetStructField<float>(obj, "cRP9ygtO2dZl");
                var y = GetStructField<float>(obj, "F9bLnUCqZIFL");
                var z = GetStructField<float>(obj, "PmxN1q8UMSTx");
                var w = GetStructField<float>(obj, "7cBJM9uYRcnb");
                return new Quaternion(x, y, z, w);
            }
            catch (Exception e)
            {
return null;
            }
        }

        if (obj is Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> subDict)
        {
            return SanitizeDictionaryForWorld(subDict);
        }

        if (obj is Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> il2cppList)
        {
            var sanitizedList = new List<object>();
            foreach (var item in il2cppList)
            {
                sanitizedList.Add(SanitizeObjectForWorld(item));
            }
            return sanitizedList;
        }

        string valueAsString = obj.ToString();
        if (int.TryParse(valueAsString, out int intResult))
            return intResult;
        if (float.TryParse(valueAsString, System.Globalization.NumberStyles.Any,
                           System.Globalization.CultureInfo.InvariantCulture, out float floatResult))
            return floatResult;
return null;
    }

    private static Dictionary<object, object> SanitizeDictionaryForWorld(
        Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> source)
    {
        if (source == null)
            return new Dictionary<object, object>();
        var sanitized = new Dictionary<object, object>();

        foreach (var pair in source)
        {
            object key = SanitizeObjectForWorld(pair.Key);
            if (key == null)
            {
continue;
            }

            object sanitizedValue = SanitizeObjectForWorld(pair.Value);

            if (sanitizedValue == null && pair.Value != null)
            {
                continue;
            }

            sanitized[key] = sanitizedValue;
        }
        return sanitized;
    }
    private static IEnumerator ExportFullWorldAsync()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
            WorldExporterStatusMessage = "M1bkDGRtPGdN";
            yield break;
        }

        WorldExporterStatusMessage = "3jyiWrN8M5qG";
yield return null;

        var exportData = new WorldExportData { TerrainCubes = new Dictionary<IntVector, Cube>(),
                                               CustomPrototypes = new List<PrototypeData>(),
                                               WorldObjects = new List<WorldObjectData>(), Links = new List<LinkData>(),
                                               ObjectLinks = new List<ObjectLinkData>() };
        WorldExporterStatusMessage = "egHgw2OlKY0x";
        yield return null;

        try
        {
            MVCubeModelBase terrainModel = null;
            terrainModel = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelPrototypeTerrain>();
            if (terrainModel == null)
            {
terrainModel = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                                   ?.TryCast<MVCubeModelBase>();
            }
            if (terrainModel == null)
            {
foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
                {
                    if (wo.HasInteractionFlag(InteractionFlags.IsTerrain))
                    {
                        var candidate = wo.TryCast<MVCubeModelBase>();
                        if (candidate != null)
                        {
                            terrainModel = candidate;
break;
                        }
                    }
                }
            }

            if (terrainModel != null)
            {
var terrainDict = GetModelDict(terrainModel);

                if (terrainDict.Count > 0)
                {
                    foreach (var pair in terrainDict)
                    {
                        exportData.TerrainCubes.Add(pair.Key, pair.Value);
                    }
}
                else
                {
}
            }
            else
            {
WorldExporterStatusMessage = "FeqOwrG6PtOG";
            }
        }
        catch (Exception ex)
        {
            WorldExporterStatusMessage = $"4bE9WmEcEFrC";
}
        WorldExporterStatusMessage = "XsOZiAtseUUg";
        yield return null;

        try
        {
            var uniqueProtoIds = new HashSet<int>();
            var allWorldObjects = new List<MVWorldObjectClient>();
            foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
                allWorldObjects.Add(wo);
foreach (var wo in allWorldObjects)
            {
                if (wo == null || wo.Id == MVGameControllerBase.WOCM.RootGroup.Id ||
                    wo.HasInteractionFlag(InteractionFlags.IsTerrain) ||
                    wo.WorldObjectType == WorldObjectType.PlayModeAvatar)
                {
                    continue;
                }
                var woData = new WorldObjectData { OriginalID = wo.Id,
                                                   ObjectType = wo.WorldObjectType,
                                                   GroupID = wo.GroupId,
                                                   Position = wo.Position,
                                                   Rotation = wo.Rotation,
                                                   Scale = wo.Scale,
                                                   Data = SanitizeDictionaryForWorld(wo.Data) };
                exportData.WorldObjects.Add(woData);
                var cubeModel = wo.TryCast<MVCubeModelBase>();
                if (cubeModel?.PrototypeCubeModel != null)
                {
                    int pId = cubeModel.PrototypeCubeModel.PrototypeId;
                    if (!uniqueProtoIds.Contains(pId))
                    {
                        uniqueProtoIds.Add(pId);
                        var protoCubes = new Dictionary<IntVector, Cube>();
                        var il2cppDict = GetModelDict(cubeModel);

                        foreach (var pair in il2cppDict)
                            protoCubes.Add(pair.Key, pair.Value);

                        exportData.CustomPrototypes.Add(new PrototypeData { PrototypeID = pId, Cubes = protoCubes });
                    }
                }
            }
}
        catch (Exception ex)
        {
}
        WorldExporterStatusMessage = "1EiCY450Vhx0";
        yield return null;

        try
        {
            foreach (var link in MVGameControllerBase.Game.worldNetwork.links.links.Values)
                exportData.Links.Add(new LinkData { OutputWOID = link.outputWOID, InputWOID = link.inputWOID });

            foreach (var link in MVGameControllerBase.Game.worldNetwork.objectLinks.objectLinks.Values)
                exportData.ObjectLinks.Add(
                    new ObjectLinkData { ConnectorWOID = link.objectConnectorWOID, ObjectWOID = link.objectWOID });
}
        catch (Exception ex)
        {
}
        WorldExporterStatusMessage = "2DQAkhuYVwmY";
        yield return null;

        try
        {
var bp = new BytePacker();
            bp.Write(exportData.TerrainCubes.Count);
            foreach (var pair in exportData.TerrainCubes)
            {
                bp.Write(pair.Key.x);
                bp.Write(pair.Key.y);
                bp.Write(pair.Key.z);
                bp.Write(pair.Value.ByteCorners);
                bp.Write(pair.Value.FaceMaterials);
            }
            bp.Write(exportData.CustomPrototypes.Count);
            foreach (var proto in exportData.CustomPrototypes)
            {
                bp.Write(proto.PrototypeID);
                bp.Write(proto.Cubes.Count);
                foreach (var pair in proto.Cubes)
                {
                    bp.Write(pair.Key.x);
                    bp.Write(pair.Key.y);
                    bp.Write(pair.Key.z);
                    bp.Write(pair.Value.ByteCorners);
                    bp.Write(pair.Value.FaceMaterials);
                }
            }
            bp.Write(exportData.WorldObjects.Count);
            foreach (var woData in exportData.WorldObjects)
            {
                bp.Write(woData.OriginalID);
                bp.Write((int)woData.ObjectType);
                bp.Write(woData.GroupID);
                bp.Write(woData.Position.x);
                bp.Write(woData.Position.y);
                bp.Write(woData.Position.z);
                bp.Write(woData.Rotation.x);
                bp.Write(woData.Rotation.y);
                bp.Write(woData.Rotation.z);
                bp.Write(woData.Rotation.w);
                bp.Write(woData.Scale.x);
                bp.Write(woData.Scale.y);
                bp.Write(woData.Scale.z);
                WriteObject(bp, woData.Data);
            }
            bp.Write(exportData.Links.Count);
            foreach (var link in exportData.Links)
            {
                bp.Write(link.OutputWOID);
                bp.Write(link.InputWOID);
            }
            bp.Write(exportData.ObjectLinks.Count);
            foreach (var link in exportData.ObjectLinks)
            {
                bp.Write(link.ConnectorWOID);
                bp.Write(link.ObjectWOID);
            }

            string base64Output = Convert.ToBase64String(bp.ToArray());
            string fName = $"A4c54MhakYrJ";
            string fPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "YfaPlLVfClM1", fName);
            Directory.CreateDirectory(Path.GetDirectoryName(fPath));
            File.WriteAllText(fPath, base64Output);

            WorldExporterStatusMessage = $"jY1tcDL7h9yB";
}
        catch (Exception ex)
        {
            WorldExporterStatusMessage = $"JYxRLfo7M24k";
}
    }
    private static int _newlyCreatedWoId = -1;
    private static bool _waitForCreationResponse = false;
    private static IEnumerator ImportFullWorldAsync(string filePath, bool shouldRemoveCubes)
    {
        WorldImporterStatusMessage = "vJ7fJsXkK3z3";
yield return null;

        WorldExportData importData;
        try
        {
            byte[] bytePackage = Convert.FromBase64String(File.ReadAllText(filePath));
            var bp = new BytePacker(bytePackage);
            importData = new WorldExportData { TerrainCubes = new Dictionary<IntVector, Cube>(),
                                               CustomPrototypes = new List<PrototypeData>(),
                                               WorldObjects = new List<WorldObjectData>(), Links = new List<LinkData>(),
                                               ObjectLinks = new List<ObjectLinkData>() };
            int terrainCount = bp.ReadInt32();
for (int i = 0; i < terrainCount; i++)
            {
                var p = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                var c = new Cube(bp.ReadBytes(CORNERS_ARRAY_SIZE_TERRAIN), bp.ReadBytes(MATERIALS_ARRAY_SIZE_TERRAIN));
                importData.TerrainCubes[p] = c;
            }
            int protoCount = bp.ReadInt32();
            for (int i = 0; i < protoCount; i++)
            {
                var proto =
                    new PrototypeData { PrototypeID = bp.ReadInt32(), Cubes = new Dictionary<IntVector, Cube>() };
                int cubeCount = bp.ReadInt32();
                for (int j = 0; j < cubeCount; j++)
                {
                    var p = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                    var c =
                        new Cube(bp.ReadBytes(CORNERS_ARRAY_SIZE_TERRAIN), bp.ReadBytes(MATERIALS_ARRAY_SIZE_TERRAIN));
                    proto.Cubes[p] = c;
                }
                importData.CustomPrototypes.Add(proto);
            }
            int woCount = bp.ReadInt32();
            for (int i = 0; i < woCount; i++)
            {
                var woData =
                    new WorldObjectData { OriginalID = bp.ReadInt32(),
                                          ObjectType = (WorldObjectType)bp.ReadInt32(),
                                          GroupID = bp.ReadInt32(),
                                          Position = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                                          Rotation = new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(),
                                                                    bp.ReadSingle()),
                                          Scale = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                                          Data = ReadObject(bp) as Dictionary<object, object> };
                importData.WorldObjects.Add(woData);
            }
            int linkCount = bp.ReadInt32();
            for (int i = 0; i < linkCount; i++)
                importData.Links.Add(new LinkData { OutputWOID = bp.ReadInt32(), InputWOID = bp.ReadInt32() });
            int objLinkCount = bp.ReadInt32();
            for (int i = 0; i < objLinkCount; i++)
                importData.ObjectLinks.Add(
                    new ObjectLinkData { ConnectorWOID = bp.ReadInt32(), ObjectWOID = bp.ReadInt32() });
        }
        catch (Exception ex)
        {
            WorldImporterStatusMessage = "hmfRwYorGY3R";
yield break;
        }
        MVCubeModelBase targetModel = null;
foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            if (wo != null && wo.HasInteractionFlag(InteractionFlags.IsTerrain))
            {
                targetModel = wo.TryCast<MVCubeModelBase>();
                if (targetModel != null)
                {
break;
                }
            }
        }
        if (targetModel == null)
        {
foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
            {
                if (wo.WorldObjectType == WorldObjectType.CubeModelPrototypeTerrain ||
                    wo.WorldObjectType == WorldObjectType.CubeModelTerrainFineGrained)
                {
                    targetModel = wo.TryCast<MVCubeModelBase>();
break;
                }
            }
        }

        if (targetModel == null)
        {
            WorldImporterStatusMessage = "YnCZc73O1QNl";
yield break;
        }
        int cubesInBatch = 0;

        if (shouldRemoveCubes)
        {
            WorldImporterStatusMessage = "rKCwcWF4xa5Y";
yield return null;

            targetModel.MakeUnique();
            var positionsToRemove = new List<IntVector>();
            if (targetModel.PrototypeCubeModel?.Chunks != null)
            {
                foreach (var chunk in targetModel.PrototypeCubeModel.Chunks.Values)
                {
                    if (chunk?.cells != null)
                    {
                        foreach (var key in chunk.cells.Keys)
                            positionsToRemove.Add(key);
                    }
                }
            }

            int processedThisTick = 0;
            for (int i = 0; i < positionsToRemove.Count; i++)
            {
                targetModel.RemoveCube(positionsToRemove[i]);
                processedThisTick++;
                if (processedThisTick >= cubesPerTick)
                {
                    targetModel.HandleDelta();
                    WorldImporterStatusMessage = $"Lr1dA2H4GO0B";
                    yield return new WaitForSeconds(tickDelay);
                    processedThisTick = 0;
                }
            }
            if (processedThisTick > 0)
            {
                targetModel.HandleDelta();
                yield return new WaitForSeconds(tickDelay);
            }

            WorldImporterStatusMessage = "ZPLdn2X6zVTg";
            yield return new WaitForSeconds(importPauseDelay);
        }

        if (importData.WorldObjects.Count > 0)
        {
            WorldImporterStatusMessage = "py5N3V6Jg9ZH";
yield return null;

            var protoLookup = new Dictionary<int, PrototypeData>();
            foreach (var p in importData.CustomPrototypes)
                protoLookup[p.PrototypeID] = p;

            byte[] identityCorners = CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);
            int processedThisTick = 0;

            foreach (var woData in importData.WorldObjects)
            {
                if (woData.Data != null &&
                    (woData.Data.TryGetValue("AdR4pf4ikStF", out var pidObj) || woData.Data.TryGetValue(1, out pidObj)))
                {
                    int pid = Convert.ToInt32(pidObj);
                    if (protoLookup.TryGetValue(pid, out var protoData))
                    {
                        foreach (var pair in protoData.Cubes)
                        {
                            var localPos = pair.Key;
                            var cube = pair.Value;
                            Vector3 globalPos = woData.Position + new Vector3(localPos.x, localPos.y, localPos.z);
                            IntVector terrainPos =
                                new IntVector((short)Mathf.Round(globalPos.x), (short)Mathf.Round(globalPos.y),
                                              (short)Mathf.Round(globalPos.z));

                            targetModel.AddCube(terrainPos, new Cube(identityCorners, cube.FaceMaterials));

                            processedThisTick++;
                            if (processedThisTick >= cubesPerTick)
                            {
                                targetModel.HandleDelta();
                                yield return new WaitForSeconds(tickDelay);
                                processedThisTick = 0;
                            }
                        }
                    }
                }
            }
            if (processedThisTick > 0)
            {
                targetModel.HandleDelta();
                yield return new WaitForSeconds(tickDelay);
            }
        }

        if (importData.TerrainCubes.Count > 0)
        {
            WorldImporterStatusMessage = "P9j7wtbnttxR";
int processedThisTick = 0;
            int total = 0;

            foreach (var pair in importData.TerrainCubes)
            {
                targetModel.AddCube(pair.Key, pair.Value);
                processedThisTick++;
                total++;

                if (processedThisTick >= cubesPerTick)
                {
                    targetModel.HandleDelta();
                    WorldImporterStatusMessage = $"C4xwDLJ4nmrb";
                    yield return new WaitForSeconds(tickDelay);
                    processedThisTick = 0;
                }
            }
            if (processedThisTick > 0)
            {
                targetModel.HandleDelta();
            }
        }
        else
        {
}
WorldImporterStatusMessage = "fhz9U6O8m8uC";
    }

    public static IEnumerator CreatePersistentCollidingCube()
    {
        if (MVGameControllerBase.GameMode != MVGameMode.CharacterEditor)
        {
yield break;
        }

        var desktopCtrls = GameObject.FindObjectsOfType<DesktopAvatarEditModeController>();
        if (desktopCtrls == null || desktopCtrls.Count == 0)
        {
yield break;
        }
        var controllerInstance = desktopCtrls[0];

        MVBody currentBody = null;
        bool callbackFinished = false;

        ExecuteEvents.ExecuteHierarchy(
            controllerInstance.gameObject, null,
            (ExecuteEvents.EventFunction<IGetCurrentBody>)delegate(IGetCurrentBody handler, BaseEventData eventData) {
                handler.GetCurrentBody((Action<MVBody>)delegate(MVBody body) {
                    currentBody = body;
                    callbackFinished = true;
                });
            });

        float timeout = Time.time + 5f;
        while (!callbackFinished && Time.time < timeout)
        {
            yield return null;
        }

        if (!callbackFinished || currentBody == null)
        {
yield break;
        }

        int avatarId = currentBody.Id;
_responseReceived = false;
        _newWoId = -1;
        World world = MVGameControllerBase.Game.World;

        Il2CppSystem.EventHandler<InitializedGameQueryDataEventArgs> handler = null;
        handler = new Action<Il2CppSystem.Object, InitializedGameQueryDataEventArgs>(
            (sender, e) =>
            {
                if (MVGameControllerBase.Game.LocalPlayer.ActorNr == e.InstigatorActorNumber)
                {
                    _newWoId = e.RootWO.Id;
                    _responseReceived = true;
if (handler != null)
                    {
                        world.InitializedGameQueryData -= handler;
                    }
                }
            });

        world.InitializedGameQueryData += handler;

        var data = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
        data.Add(1, 1f);
        data.Add(2, 21);
        data.Add(3, MVGameControllerBase.Game.LocalPlayer.ProfileID);

        var playerBody = MVGameControllerBase.Game.LocalPlayer.Body?.GameObject.transform;
        var pos = (playerBody?.position ?? Vector3.zero) + (playerBody?.forward ?? Vector3.forward) * 10f;

        MVGameControllerBase.OperationRequests.RequestBuiltInItem(BuiltInItem.CubeModel, avatarId, data, pos,
                                                                  Quaternion.identity, Vector3.one, true, false);
float responseTimeout = Time.time + 10f;
        while (!_responseReceived && Time.time < responseTimeout)
        {
            yield return null;
        }

        if (!_responseReceived)
        {
            if (handler != null)
            {
                world.InitializedGameQueryData -= handler;
            }
}

        MVCubeModelInstance newCube = null;
        if (_newWoId != -1)
        {
            newCube = MVGameControllerBase.WOCM.GetWorldObjectClient(_newWoId)?.TryCast<MVCubeModelInstance>();
        }
        if (newCube == null)
        {
yield break;
        }
try
        {
var matId = (byte)21;
            var cube =
                new Cube(CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners), Cube.CreateMaterialArray(matId));
            int size = 5;
            int half = size / 2;
            for (int x = -half; x <= half; x++)
            {
                for (int z = -half; z <= half; z++)
                {
                    newCube.AddCube(new IntVector(x, 0, z), cube);
                }
            }
            newCube.HandleDelta();
}
        catch (Exception ex)
        {
}
    }
}

public static class ModelImporter
{
    public static string ImporterStatusMessage { get; private set; } = "iNFXMGmEEMux";

    private const int CORNERS_ARRAY_SIZE = 8;
    private const int MATERIALS_ARRAY_SIZE = 6;
    private const int CUBES_PER_BATCH = 50;
    private const float BATCH_DELAY_SECONDS = 0.5f;

    public static void StartImport(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            ImporterStatusMessage = "UfnBWjGXKO7Z";
            return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(InjectGeometryAsync(filePath));
    }

    private static IEnumerator InjectGeometryAsync(string filePath)
    {
        if (MVGameControllerBase.GameMode != MVGameMode.CharacterEditor)
        {
            ImporterStatusMessage = "2UFAQSbkMMwP";
            yield break;
        }

        var controllers = GameObject.FindObjectsOfType<DesktopAvatarEditModeController>();
        if (controllers == null || controllers.Count == 0)
        {
            ImporterStatusMessage = "fi6nPucwN4WQ";
            yield break;
        }
        var controller = controllers[0];
        MVBody bodyRef = null;
        bool bodyFound = false;

        ExecuteEvents.ExecuteHierarchy<IGetCurrentBody>(
            controller.gameObject, null,
            (ExecuteEvents.EventFunction<IGetCurrentBody>)delegate(IGetCurrentBody handler, BaseEventData eventData) {
                handler.GetCurrentBody((Il2CppSystem.Action<MVBody>)delegate(MVBody body) {
                    bodyRef = body;
                    bodyFound = true;
                });
            });

        float timeout = Time.realtimeSinceStartup + 2.0f;
        while (!bodyFound && Time.realtimeSinceStartup < timeout)
            yield return null;

        if (bodyRef == null)
        {
            ImporterStatusMessage = "Dh9ATPglFaL8";
            yield break;
        }
        yield return PerformInjection(bodyRef, filePath);
    }

    private static IEnumerator PerformInjection(MVBody localBody, string filePath)
    {
        ImporterStatusMessage = "DMeIuwx5WBAS";
        yield return null;

        List<ImportedAvatarPartData> importedData = ReadModelDataFromFile(filePath);

        if (importedData == null || importedData.Count == 0)
        {
            yield break;
        }

        int totalCubesInjected = 0;
        int fixedCubesCount = 0;

        byte[] knownGoodCorners = CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);

        foreach (ImportedAvatarPartData partData in importedData)
        {
            ImporterStatusMessage = $"OsLpGgPB0Z4F";

            MVCubeModelInstance localModel = null;
            string[] bodyPartNamesToTry = { "YZFTUsPlK2qq", "kFcFy6UV54lv" };

            foreach (var name in bodyPartNamesToTry)
            {
                localModel = localBody.GetBodyPart(name);
                if (localModel != null)
                {
                    break;
                }
            }

            if (localModel == null)
            {
                ImporterStatusMessage = $"py63dnLebr9P";
                continue;
            }

            try
            {
                localModel.MakeUnique();
                var positionsToRemove = new List<IntVector>();
                foreach (var chunk in localModel.PrototypeCubeModel.Chunks.Values)
                {
                    foreach (var key in chunk.cells.Keys)
                    {
                        positionsToRemove.Add(key);
                    }
                }
                foreach (IntVector pos in positionsToRemove)
                {
                    localModel.RemoveCube(pos);
                }

                localModel.HandleDelta();
            }
            catch (Exception ex)
            {
                ImporterStatusMessage = $"ejjTIspX9Vva";
                continue;
            }

            float waitEnd = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < waitEnd)
                yield return null;

            ImporterStatusMessage = "JXC8lFgsopur";

            int cubesInCurrentBatch = 0;
            foreach (var entry in partData.ImportedCubes)
            {
                Cube originalCube = entry.Value;
                Cube cubeToInject;

                bool isCollapsed = true;
                foreach (byte b in originalCube.ByteCorners)
                {
                    if (b != 0)
                    {
                        isCollapsed = false;
                        break;
                    }
                }

                byte originalMaterialId = originalCube.FaceMaterials[0];
                bool ownsMaterial = true;
                var materialObj = MVGameControllerBase.Game.MaterialRepository.GetMaterial(originalMaterialId);
                if (materialObj != null)
                {
                    ownsMaterial = materialObj.isUnlocked;
                }

                byte[] safeMaterials = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    safeMaterials[i] = originalCube.FaceMaterials[i];
                }

                if (!ownsMaterial)
                {
                    safeMaterials[1] = 21;
                }

                if (isCollapsed)
                {
                    fixedCubesCount++;
                    cubeToInject = new Cube(knownGoodCorners, safeMaterials);
                }
                else
                {
                    cubeToInject = new Cube(originalCube.ByteCorners, safeMaterials);
                }

                localModel.AddCube(entry.Key, cubeToInject);
                totalCubesInjected++;
                cubesInCurrentBatch++;

                if (cubesInCurrentBatch >= CUBES_PER_BATCH)
                {
                    localModel.HandleDelta();
                    ImporterStatusMessage = $"crwVZrac6uQ6";

                    waitEnd = Time.realtimeSinceStartup + BATCH_DELAY_SECONDS;
                    while (Time.realtimeSinceStartup < waitEnd)
                        yield return null;

                    cubesInCurrentBatch = 0;
                }
            }

            localModel.HandleDelta();
        }

        ImporterStatusMessage = "tbqUJ2zcvcCm";

        float finalWait = Time.realtimeSinceStartup + 0.2f;
        while (Time.realtimeSinceStartup < finalWait)
            yield return null;

        try
        {
            MVGameControllerBase.OperationRequests.SetActiveAvatar(localBody.Id);
            ImporterStatusMessage = $"Cf2a8jFN7mkD";
        }
        catch (Exception ex)
        {
            ImporterStatusMessage = $"Fw34ZKNyQpDn";
        }
    }

    private static int ReadAndCorrectInt32(BytePacker bp)
    {
        int originalValue = bp.ReadInt32();
        if (originalValue > 65536)
        {
            byte[] bytes = System.BitConverter.GetBytes(originalValue);
            System.Array.Reverse(bytes);
            int correctedValue = System.BitConverter.ToInt32(bytes, 0);
            if (System.Math.Abs(correctedValue) < System.Math.Abs(originalValue))
            {
return correctedValue;
            }
        }
        return originalValue;
    }

    private static List<ImportedAvatarPartData> ReadModelDataFromFile(string filePath)
    {
        try
        {
string base64Data = File.ReadAllText(filePath);
            byte[] bytePackage = Convert.FromBase64String(base64Data);
var bp = new BytePacker(bytePackage);
            int totalParts = ReadAndCorrectInt32(bp);
if (totalParts <= 0 || totalParts > 100)
            {
throw new InvalidDataException($"MhRqCQPzXjba");
            }

            var importedData = new List<ImportedAvatarPartData>();
            for (int i = 0; i < totalParts; i++)
            {
int nameLength = ReadAndCorrectInt32(bp);
if (nameLength <= 0 || nameLength > 1000)
                {
throw new InvalidDataException($"nMjTDYzoKhlH");
                }

                byte[] nameBytes = bp.ReadBytes(nameLength);
                string partName = System.Text.Encoding.UTF8.GetString(nameBytes);
int prototypeId = ReadAndCorrectInt32(bp);
int cubeCount = ReadAndCorrectInt32(bp);
if (cubeCount < 0 || cubeCount > 150000)
                {
throw new InvalidDataException($"r3jfx335fWoa");
                }

                var cubesDict = new System.Collections.Generic.Dictionary<IntVector, Cube>();
                for (int j = 0; j < cubeCount; j++)
                {
                    var pos = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                    var corners = bp.ReadBytes(CORNERS_ARRAY_SIZE);
                    var materials = bp.ReadBytes(MATERIALS_ARRAY_SIZE);
                    cubesDict[pos] = new Cube(corners, materials);
                }
                importedData.Add(new ImportedAvatarPartData { PartName = partName, PrototypeID = prototypeId,
                                                              ImportedCubes = cubesDict });
}
ImporterStatusMessage = "JwWD5Nrj7hig";
            return importedData;
        }
        catch (Exception ex)
        {
            ImporterStatusMessage = $"aTey2DXyiX7I";
return null;
        }
    }
}

internal class GameMetricsUpdater : MonoBehaviour
{
    const float updateInterval = 0.5f;

    void Awake()
    {
        InvokeRepeating(nameof(UpdateMetrics), 1f, updateInterval);
    }

    private void UpdateMetrics()
    {
        GameInfo.UpdateMetrics();
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameInfo.aimbotOn = !GameInfo.aimbotOn;
            GameInfo.resetAimTgt();
        }

        if (GameInfo.aimbotOn)
        {
            GameInfo.doAim();
        }

        if (GameInfo.triggerbotOn)
        {
            GameInfo.doTrigger();
        }

        TestMod.Features.SmartBuilder.OnUpdate();
    }
}
public static class FlyMode
{
    public static bool IsEnabled = false;
    public static float FlySpeed = 25.0f;
    public static float FastMultiplier = 4.0f;

    private static MVAvatarLocal _cachedAvatar;

    public static void ToggleFlyMode()
    {
        IsEnabled = !IsEnabled;
if (!IsEnabled)
        {
            RestorePhysics();
        }
    }

    public static void Update()
    {
        
        if (MVGameControllerBase.Game == null || MVGameControllerBase.JoinState != MVJoinState.Playing)
            return;

        var player = MVGameControllerBase.Game.LocalPlayer;

        
        
        if (player == null || !player.IsReady || player.AvatarLocal == null)
            return;

        bool inLobby = MVGameControllerBase.PlayModeUI != null && MVGameControllerBase.PlayModeUI.IsInLobby;
        bool isDead = MVGameControllerBase.SpawnRoleDataMediatorLocal.SpawnRoleMode.Value == SpawnRoleModeType.Dead;
        bool isHidden = MVGameControllerBase.SpawnRoleDataMediatorLocal.SpawnRoleMode.Value == SpawnRoleModeType.Hidden;

        if (IsEnabled && (inLobby || isDead || isHidden))
        {
            IsEnabled = false;
RestorePhysics();
            return;
        }

        if (!IsEnabled)
            return;

        if (_cachedAvatar == null || _cachedAvatar.Id != player.WoId)
        {
            var woc = MVGameControllerBase.WOCM?.GetWorldObjectClient(player.WoId);
            _cachedAvatar = woc?.TryCast<MVAvatarLocal>();
        }

        if (_cachedAvatar == null || _cachedAvatar.GameObject == null)
            return;

        GameObject avatarGo = _cachedAvatar.GameObject;

        
        var motor = avatarGo.GetComponent<AvatarMotor>();
        if (motor != null && motor.enabled)
        {
            motor.enabled = false;
            motor.Reset();
        }

        
        var rb = avatarGo.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        var cam = MVGameControllerBase.MainCameraManager?.MainCamera?.transform;
        if (cam == null)
            return;

        
        
        
        
        
        float camYaw = cam.rotation.eulerAngles.y;

        if (TestMod.Features.RotationCheats.spin)
        {
            avatarGo.transform.Rotate(Vector3.up, TestMod.Features.RotationCheats.spd, Space.Self);
        }
        else if (TestMod.Features.RotationCheats.back)
        {
            avatarGo.transform.rotation = Quaternion.Euler(0f, camYaw + 180f, 0f);
        }
        else if (TestMod.Features.RotationCheats.flip)
        {
            avatarGo.transform.rotation = Quaternion.Euler(180f, camYaw, 0f);
        }
        else
        {
            
            avatarGo.transform.rotation = Quaternion.Euler(0f, camYaw, 0f);
        }
        

        Vector3 moveDir = Vector3.zero;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        if (Input.GetKey(KeyCode.W))
            moveDir += forward;
        if (Input.GetKey(KeyCode.S))
            moveDir -= forward;
        if (Input.GetKey(KeyCode.D))
            moveDir += right;
        if (Input.GetKey(KeyCode.A))
            moveDir -= right;
        if (Input.GetKey(KeyCode.Space))
            moveDir += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl))
            moveDir -= Vector3.up;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            float speed = FlySpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                speed *= FastMultiplier;
            avatarGo.transform.position += moveDir.normalized * speed * Time.deltaTime;
        }
    }

    private static void RestorePhysics()
    {
        if (_cachedAvatar != null && _cachedAvatar.GameObject != null)
        {
            GameObject avatarGo = _cachedAvatar.GameObject;

            
            
            bool isPlaying =
                MVGameControllerBase.SpawnRoleDataMediatorLocal.SpawnRoleMode.Value == SpawnRoleModeType.Playing;
            if (isPlaying)
            {
                _cachedAvatar.WorldPosition = avatarGo.transform.position;
                _cachedAvatar.SyncPos = avatarGo.transform.position;
            }

            
            if (_cachedAvatar.RigidBody != null)
            {
                _cachedAvatar.RigidBody.Reset();
                _cachedAvatar.RigidBody.IsMovementLocked = false;
            }

            
            var motor = avatarGo.GetComponent<AvatarMotor>();
            if (motor != null)
            {
                motor.enabled = true;
            }

            var rb = avatarGo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                
                
                
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
            }
        }
    }
}

--- FILE: Features\AntiAfkPatch.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace TestMod.Features
{
    public static class AntiAfkPatch
    {
        public static void Initialize(HarmonyLib.Harmony harmony)
        {
harmony.PatchAll(typeof(AntiAfkPatch));
        }
        [HarmonyPatch(typeof(AwayMonitor), nameof(AwayMonitor.Update))]
        public static class AwayMonitor_Update_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(AwayMonitor), nameof(AwayMonitor.UpdateMobile))]
        public static class AwayMonitor_UpdateMobile_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(AwayMonitor), "ovNZVYrREitL")]
        public static class AwayMonitor_CheckKick_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(AwayMonitor), "8jvwYsyCMNrO")]
        public static class AwayMonitor_GetIdleKickEnabled_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(AwayMonitor), "ziZX7qYjufeF")]
        public static class AwayMonitor_SetIdleKickEnabled_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool value)
            {
                value = false;
                return true;
            }
        }
    }
}

--- FILE: Features\AntiCrashPrototypes.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppMV.WorldObject;
using MelonLoader;
using System;

namespace TestMod.Fixes
{
    public static class AntiCrashPrototypes
    {
        
        
        
        [HarmonyPatch(typeof(MVWorldInventory), nameof(MVWorldInventory.RemovePrototype))]
        public static class RemovePrototype_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(MVWorldInventory __instance, int id)
            {
                
                
                if (__instance.runtimePrototypes != null && !__instance.runtimePrototypes.ContainsKey(id))
                {
return false; 
                }
                return true;
            }
        }

        
        
        
        
        [HarmonyPatch(typeof(MVNetworkGame.EventHandling), nameof(MVNetworkGame.EventHandling.HandleEvent))]
        public static class HandleEvent_SafetyNet
        {
            [HarmonyFinalizer]
            public static Exception Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
return null; 
                }
                return null;
            }
        }
    }
}
--- FILE: Features\AntiCrash_AvatarInit.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppExitGames.Client.Photon;
using MelonLoader;
using System;
using UnityEngine;

namespace TestMod.Features
{
public static class AntiCrash_AvatarInit
{
    public static int LastInstigatorActorNr = -1;

    [HarmonyPatch(typeof(MVNetworkGame), nameof(MVNetworkGame.OnGetGameBatch))]
    public static class Patch_OnGetGameBatch
    {
        [HarmonyPrefix]
        public static void Prefix(EventData eventData)
        {
            try
            {
                if (eventData != null && eventData.Parameters.ContainsKey((byte)254))
                {
                    LastInstigatorActorNr = eventData.Parameters[(byte)254].Unbox<int>();
                }
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch(typeof(MVAvatarRemote), nameof(MVAvatarRemote.Initialize))]
    public static class Patch_MVAvatarRemote
    {
        [HarmonyPrefix]
        public static bool Prefix(MVAvatarRemote __instance)
        {
            return ValidateAvatarOwner(__instance);
        }
    }

    [HarmonyPatch(typeof(MVBuildModeAvatarRemote), nameof(MVBuildModeAvatarRemote.Initialize))]
    public static class Patch_MVBuildModeAvatarRemote
    {
        [HarmonyPrefix]
        public static bool Prefix(MVBuildModeAvatarRemote __instance)
        {
            return ValidateAvatarOwner(__instance);
        }
    }

    private static bool ValidateAvatarOwner(MVWorldObjectClient avatarInstance)
    {
        if (avatarInstance == null)
            return false;

        if (avatarInstance.OwnerActorNr == -1)
            return true;

        var game = MVGameControllerBase.Game;
        if (game != null && game.MVPlayerContainer != null)
        {
            try
            {
                var player = game.MVPlayerContainer.GetPlayerUnsafe(avatarInstance.OwnerActorNr);
                if (player == null)
                {
                    string exploiterName = "765WQFbmynoH";
                    int exploiterProfileId = -1;

                    if (LastInstigatorActorNr != -1 &&
                        game.MVPlayerContainer.TryGetValue(LastInstigatorActorNr, out var exploiter))
                    {
                        exploiterName = exploiter.UserProfileData.UserName;
                        exploiterProfileId = exploiter.ProfileID;
                    }





                    UnityEngine.Object.Destroy(avatarInstance.gameObject);
                    return false;
                }
                return true;
            }
            catch
            {
                UnityEngine.Object.Destroy(avatarInstance.gameObject);
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(typeof(MVWorldInventory), nameof(MVWorldInventory.RemovePrototype))]
    public static class RemovePrototype_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(MVWorldInventory __instance, int id)
        {
            try
            {
                if (__instance.runtimePrototypes == null)
                    return false;
                if (!__instance.runtimePrototypes.ContainsKey(id))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(MVNetworkGame.EventHandling), nameof(MVNetworkGame.EventHandling.HandleEvent))]
    public static class HandleEvent_SafetyNet
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                return null;
            }
            return null;
        }
    }
}
}

--- FILE: Features\AntiCrash_CubeLimit.cs ---
﻿using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppMV.WorldObject;
using MelonLoader;

namespace TestMod.Features
{
public static class AntiCrash_CubeLimit
{
    public const int MAX_AVATAR_CUBES = 900;
    public const int MAX_WORLD_CUBES = 25000000;
    [HarmonyPatch(typeof(RuntimePrototypeCubeModel), "oec4TG2A7RjA")]
    public static class LimitAvatarCubes_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(RuntimePrototypeCubeModel __instance, BytePacker bp)
        {
            int num = bp.ReadInt32();
            int currentLimit = MAX_WORLD_CUBES;
            bool isAvatar = false;

            try
            {
                if (MVGameControllerBase.JoinState == MVJoinState.Playing && __instance.Scale < 0.99f)
                {
                    isAvatar = true;
                    currentLimit = MAX_AVATAR_CUBES;
                }
            }
            catch
            {
            }
            if (num <= currentLimit)
            {
                bp.Position -= 4;
                return true;
            }

            string modelType = isAvatar ? "NkJKBWrD1BcQ" : "kAzS6hpZLjUf";
int totalCubesGenerated = 0;
            for (int i = 0; i < num; i++)
            {
                short x = bp.ReadInt16();
                short y = bp.ReadInt16();
                short z = bp.ReadInt16();
                IntVector intVector = new IntVector(x, y, z);
                byte b = bp.ReadByte();
                Cube cube = new Cube(bp, b);

                int cubesInRow = CubeDataPacker.GetCubesInRow(b);
                if (totalCubesGenerated < currentLimit)
                {
                    __instance.AddCubeNetworkUpdate(intVector, cube, MeshGeneratePriority.None);
                    totalCubesGenerated++;
                    for (int j = 1; j < cubesInRow; j++)
                    {
                        if (totalCubesGenerated < currentLimit)
                        {
                            IntVector rowPos = new IntVector((short)(intVector.x + j), intVector.y, intVector.z);
                            __instance.AddCubeNetworkUpdate(rowPos, Cube.Clone(cube), MeshGeneratePriority.None);
                            totalCubesGenerated++;
                        }
                    }
                }
            }
            return false;
        }
    }
}
}

--- FILE: Features\AreaEditorTool.cs ---
﻿using Il2Cpp;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using ImGuiNET;
using MelonLoader;
using System;
using TestMod.Helpers;
using UnityEngine;

namespace TestMod.Features
{
    public class AreaEditorTool : MonoBehaviour
    {
        public AreaEditorTool(IntPtr ptr) : base(ptr) { }

        public static AreaEditorTool Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public enum ToolState { Idle, SelectingFirst, SelectingSecond, AreaSelected }
        public ToolState CurrentState = ToolState.Idle;

        private IntVector startPos;
        private IntVector endPos;
        private MVCubeModelBase targetModel;

        private GameObject selectionBoxObj;
        private SelectionBox selectionBoxScript;

        private int selectedMaterialId = 1;
        private bool showGui = false;

        void Update()
        {
            if (MVGameControllerBase.GameSessionData == null) return;
            if (MVGameControllerBase.GameMode != MVGameMode.Edit) return;

            if (MVGameControllerBase.EditModeUI == null) return;
            var editUI = MVGameControllerBase.EditModeUI.TryCast<DesktopEditModeController>();
            if (editUI == null) return;

            var stateMachine = editUI.EditModeStateMachine;
            if (stateMachine == null) return;

            var modelingMachine = stateMachine.CubeModelingStateMachine;
            if (modelingMachine == null) return;

            var pickInfo = modelingMachine.SelectedCube;

            if (pickInfo != null && pickInfo.cube != null)
            {
                MVCubeModelBase hoveredModel = modelingMachine.TargetCubeModel;
                IntVector hoveredPos = pickInfo.iLocalPos;

                byte hoveredMat = 1;
                if (pickInfo.cube.FaceMaterials != null && pickInfo.cube.FaceMaterials.Length > 0)
                {
                    hoveredMat = pickInfo.cube.FaceMaterials[(int)pickInfo.pickedFace];
                }

                if (Input.GetMouseButtonDown(1) && !ImGui.GetIO().WantCaptureMouse)
                {
                    HandleClick(hoveredModel, hoveredPos, hoveredMat);
                }

                if (CurrentState == ToolState.SelectingSecond && hoveredModel == targetModel)
                {
                    UpdateVisuals(startPos, hoveredPos);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetTool();
            }
        }

        private void HandleClick(MVCubeModelBase model, IntVector pos, byte materialID)
        {
            if (CurrentState == ToolState.Idle || CurrentState == ToolState.AreaSelected)
            {
                targetModel = model;
                startPos = pos;
                CurrentState = ToolState.SelectingSecond;

                selectedMaterialId = materialID;
CreateVisuals(model);
                UpdateVisuals(startPos, startPos);
                showGui = true;
            }
            else if (CurrentState == ToolState.SelectingSecond)
            {
                if (model == targetModel)
                {
                    endPos = pos;
                    CurrentState = ToolState.AreaSelected;
                    UpdateVisuals(startPos, endPos);
}
                else
                {
ResetTool();
                }
            }
        }

        public void DrawImGuiMenu()
        {
            if (!showGui) return;
            if (MVGameControllerBase.GameMode != MVGameMode.Edit) return;

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 220), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("75wEubDF4Qgt", ref showGui))
            {
                ImGui.Text($"pc5rHqE1FoZ4");

                if (CurrentState == ToolState.AreaSelected)
                {
                    int dx = Mathf.Abs(startPos.x - endPos.x) + 1;
                    int dy = Mathf.Abs(startPos.y - endPos.y) + 1;
                    int dz = Mathf.Abs(startPos.z - endPos.z) + 1;
                    ImGui.Text($"wj50qqOJFa30");
                }

                ImGui.Separator();
                ImGui.InputInt("wyGjezn4qNu6", ref selectedMaterialId);
                if (selectedMaterialId < 1) selectedMaterialId = 1;
                if (selectedMaterialId > 63) selectedMaterialId = 63;

                if (ImGui.Button("rUNSRxRX9BS4"))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        PerformAction(ActionType.Fill);
                    });
                }

                ImGui.SameLine();
                if (ImGui.Button("5PrSzJmJDfpa"))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        PerformAction(ActionType.Clear);
                    });
                }

                if (ImGui.Button("xczZaCQxfInS"))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        PerformAction(ActionType.Replace);
                    });
                }

                ImGui.Separator();
                if (ImGui.Button("eJ2HaxwHlON9"))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        ResetTool();
                    });
                }

                ImGui.End();
            }

            if (!showGui)
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    ResetTool();
                });
            }
        }

        enum ActionType { Fill, Clear, Replace }

        void PerformAction(ActionType action)
        {
            if (targetModel == null) return;

            int minX = Mathf.Min(startPos.x, endPos.x); int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y); int maxY = Mathf.Max(startPos.y, endPos.y);
            int minZ = Mathf.Min(startPos.z, endPos.z); int maxZ = Mathf.Max(startPos.z, endPos.z);

            targetModel.MakeUnique();

            int operations = 0;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        IntVector pos = new IntVector((short)x, (short)y, (short)z);
                        var existingCube = targetModel.GetCube(pos);

                        if (action == ActionType.Clear)
                        {
                            if (existingCube != null)
                            {
                                targetModel.RemoveCube(pos);
                                operations++;
                            }
                        }
                        else if (action == ActionType.Fill)
                        {
                            if (existingCube == null)
                            {
                                Cube newCube = new Cube(
                                    CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners),
                                    Cube.CreateMaterialArray((byte)selectedMaterialId)
                                );
                                targetModel.AddCube(pos, newCube);
                                operations++;
                            }
                        }
                        else if (action == ActionType.Replace)
                        {
                            if (existingCube != null)
                            {
                                Cube existing = targetModel.GetCube(pos);
                                Cube newCube = new Cube(
                                    existing.ByteCorners,
                                    Cube.CreateMaterialArray((byte)selectedMaterialId)
                                );
                                targetModel.RemoveCube(pos);
                                targetModel.AddCube(pos, newCube);
                                operations++;
                            }
                        }
                    }
                }
            }

            if (operations > 0)
            {
                targetModel.HandleDelta();
}
            else
            {
}
        }

        void CreateVisuals(MVCubeModelBase model)
        {
            if (selectionBoxObj != null) Destroy(selectionBoxObj);

            selectionBoxObj = new GameObject("1z6xPCJf6mPW");
            selectionBoxObj.transform.parent = model.GameObject.transform;
            selectionBoxObj.transform.localPosition = Vector3.zero;
            selectionBoxObj.transform.localRotation = Quaternion.identity;
            selectionBoxObj.transform.localScale = Vector3.one;

            selectionBoxScript = selectionBoxObj.AddComponent<SelectionBox>();
        }

        void UpdateVisuals(IntVector s, IntVector e)
        {
            if (selectionBoxScript == null) return;

            int minX = Mathf.Min(s.x, e.x); int maxX = Mathf.Max(s.x, e.x);
            int minY = Mathf.Min(s.y, e.y); int maxY = Mathf.Max(s.y, e.y);
            int minZ = Mathf.Min(s.z, e.z); int maxZ = Mathf.Max(s.z, e.z);

            Vector3 center = new Vector3((minX + maxX) / 2f + 0.5f, (minY + maxY) / 2f + 0.5f, (minZ + maxZ) / 2f + 0.5f);
            Vector3 size = new Vector3(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);

            Bounds bounds = new Bounds(center, size);

            Vector3[] corners = SharedCubeFunctions.GetCorners(bounds);

            selectionBoxScript.FadeIn(0.5f, PrefabPool.Instance.SelectBoxMaterial, corners);
        }

        void ResetTool()
        {
            if (selectionBoxObj != null) Destroy(selectionBoxObj);
            CurrentState = ToolState.Idle;
            targetModel = null;
            showGui = false;
        }
    }
}
--- FILE: Features\AvatarBlinkControl.cs ---
﻿using System;
using System.Collections;
using ImGuiNET;
using UnityEngine;
using Il2Cpp;
using Il2CppMV.Common;
using MelonLoader;
using TestMod.Helpers;

namespace TestMod.Features
{
public static class NetworkedEffectsControl
{
    private static string _status = "Sbycj1l6U8Pa";
    private static bool _infiniteMode = false;
    private static float _loopDelay = 0.5f;
    private static object _activeLoopToken = null;
    public static void RenderTab()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "cwEUAkCns2P5");
        ImGui.Text($"isx8GLczW5kH");
        ImGui.Separator();

        if (MVGameControllerBase.LocalPlayer == null)
        {
            ImGui.Text("jIror08p3OcX");
            return;
        }
        ImGui.Checkbox("TFDisiXW3aLb", ref _infiniteMode);
        if (_infiniteMode)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.SliderFloat("TcR9XMnKHlwT", ref _loopDelay, 0.1f, 2.0f);

            if (_activeLoopToken != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("sEovwHbW4edY"))
                {
                    _activeLoopToken = null;
                    _status = "2CXZ2n0bgTAZ";
                }
            }
        }
        ImGui.Separator();
        ImGui.Text("hBwJlKvmgfaF");
        if (ImGui.Button("GNTacfclZp6D"))
            ExecuteEffect(TriggerDamage);
        ImGui.SameLine();
        if (ImGui.Button("Kh51bWtLagvW"))
            ExecuteEffect(TriggerHeal);
        ImGui.SameLine();
        if (ImGui.Button("hOkHl2Xaxulf"))
            ExecuteEffect(TriggerShield);

        ImGui.Separator();
        ImGui.Text("W2CTtJQhOYkT");
        if (ImGui.Button("WBFUuRaO7iKd"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Enlarged));

        ImGui.SameLine();
        if (ImGui.Button("HBel6kYVRm5t"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Shrunken));

        ImGui.Separator();
        ImGui.Text("W0jaRrCN9LEk");
        if (ImGui.Button("E73I0rIrjXy4"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Fire));
        ImGui.SameLine();
        if (ImGui.Button("zhWknfvSCYlk"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Poison));
        ImGui.SameLine();
        if (ImGui.Button("nrG8Yh8iSywi"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Frozen));

        if (ImGui.Button("R1f8InbuGK12"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Lethal));
        ImGui.SameLine();
        if (ImGui.Button("FOx814UPWsHn"))
            ExecuteEffect(() => TriggerModifier(AvatarModifierPackageType.Mutant));

        ImGui.Separator();

        if (ImGui.Button("QOJZjz9vg86q"))
        {
            _activeLoopToken = null;
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       {
                                                           var player = MVGameControllerBase.LocalPlayer;
                                                           if (player != null)
                                                               player.AvatarLocal.InteractableLocal.ClearModifiers();
                                                       });
            _status = "C4eN9nGjX2rz";
        }
    }
    private static void ExecuteEffect(Action effectAction)
    {
        if (_infiniteMode)
        {
            object token = new object();
            _activeLoopToken = token;
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       { MelonCoroutines.Start(EffectLoop(token, effectAction)); });

            _status = "bYBdCNTIXfjy";
        }
        else
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       { effectAction(); });
            _status = "DovKQxECVIa4";
        }
    }
    private static IEnumerator EffectLoop(object token, Action effectAction)
    {
        while (_activeLoopToken == token)
        {
            if (MVGameControllerBase.LocalPlayer == null)
                break;

            try
            {
                effectAction();
            }
            catch (Exception e)
            {
_activeLoopToken = null;
            }

            yield return new WaitForSeconds(_loopDelay);
        }
    }
    private static void TriggerDamage()
    {
        var p = MVGameControllerBase.LocalPlayer;
        if (p == null)
            return;
        p.AvatarLocal.InteractableLocal.TakeDamage(0.0f, p, PlayerKilledByType.Environmental);
    }

    private static void TriggerHeal()
    {
        var p = MVGameControllerBase.LocalPlayer;
        if (p == null)
            return;
        p.AvatarLocal.InteractableLocal.Heal(0.0f, p);
    }

    private static void TriggerShield()
    {
        var p = MVGameControllerBase.LocalPlayer;
        if (p == null)
            return;
        p.AvatarLocal.InteractableLocal.HealOverTime(AvatarModifierPackageType.Shielded, p);
    }

    private static void TriggerModifier(AvatarModifierPackageType type)
    {
        var p = MVGameControllerBase.LocalPlayer;
        if (p == null)
            return;
        p.AvatarLocal.InteractableLocal.TakeDamageOverTime(type, p, PlayerKilledByType.Environmental);
    }
}
}

--- FILE: Features\AvatarCollisionMod.cs ---
﻿using Il2Cpp;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using System;
using UnityEngine;


namespace TestMod.Features
{
    public static class AvatarCollisionMod
    {
        public static void InjectCollisionCube()
        {
            if (MVGameControllerBase.GameMode != MVGameMode.CharacterEditor)
            {
                Debug.LogError("NZsGUPJbJS2X");
                return;
            }
            var bodyController = UnityEngine.Object.FindObjectOfType<AvatarEditModeBodyController>();
            if (bodyController == null) return;
            MVBody currentBody = bodyController.CurrentBody;
            if (currentBody == null) return;
            MVCubeModelInstance torsoModel = currentBody.GetBodyPart("Kv7sgs0tQ2z5");
            if (torsoModel == null)
            {
                Debug.LogError("zSBGSXQqFn1k");
                return;
            }
            IntVector internalPos = new IntVector(0, 4, 0);
            byte solidMaterialId = 21;
            Cube collisionCube = new Cube(
                CubeBase.IdentityByteCorners,
                Cube.CreateMaterialArray(solidMaterialId)
            );
            torsoModel.AddCube(internalPos, collisionCube);
            torsoModel.HandleDelta();

            Debug.Log("PW80HkwgFcJC" + internalPos);
        }
    }
}

--- FILE: Features\AvatarPreviewUI.cs ---
﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using ImGuiNET;
using MelonLoader;
using UnityEngine;
using TestMod.Helpers;
using Il2CppMV.WorldObject;

namespace TestMod.Features
{
public static class NativeAvatarPreviewUI
{
    private static string _exportDirectory = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "kRB2k0gdPVpg");
    private static string _currentlyPreviewingName = "XP5FW2guMg4Q";
    private static string _activeAvatarName = "lHwAcCOqSgQy";

    private static Camera _previewCam = null;
    private static GameObject _currentAvatarObj = null;
    private static List<RuntimePrototypeCubeModel> _activePreviewModels = new List<RuntimePrototypeCubeModel>();

    private static List<ImportedAvatarPartData> _currentEditingData = null;
    private static Dictionary<string, int[]> _partOffsets = new Dictionary<string, int[]>();
    private static bool _isEditing = false;
    private static string _currentEditFilePath = null;

    public static void RenderUI()
    {
        ImGui.Text($"68b45exd83cN");

        if (_currentAvatarObj != null)
        {
            if (ImGui.Button("sOeEV5FzLV6g", new System.Numerics.Vector2(-1, 40)))
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                           { ClosePreview(); });
            }
            ImGui.Separator();
        }

        if (ImGui.Button("bkM8VK1DN9D9", new System.Numerics.Vector2(-1, 40)))
        {
            string[] currentFiles = Directory.GetFiles(_exportDirectory, "jPg74EnSdqBK");
            UnityMainThreadDispatcher.Instance.Enqueue(
                () =>
                { MelonCoroutines.Start(GenerateRandomAvatarAsync(currentFiles)); });
        }

        if (_isEditing)
        {
            ImGui.Spacing();
            ImGui.TextColored(new System.Numerics.Vector4(0f, 1f, 1f, 1f), "1uTjJdTfnBAD");

            if (ImGui.Button("ogGrNEZFS7UW", new System.Numerics.Vector2(-1, 30)))
            {
                ReRandomizeColors();
            }

            ImGui.Spacing();
            ImGui.Text("GX8WyD7eHy0h");

            string[] allParts = { "5TRIKve08Xb0",    "XKqUmpiZwrmj",  "Avv6G38qAoly",    "H8TsraLG6iKd",  "D3d8Vnu9ihqs",
                                  "xhVoUHWKwL5c", "6ksce7FjivDz", "WKGoZJb0NTun", "MuFA9WwMKWCZ", "ynduNabNt7l8" };

            foreach (string p in allParts)
            {
                if (!_partOffsets.ContainsKey(p))
                    _partOffsets[p] = new int[] { 0, 0, 0 };

                int[] off = _partOffsets[p];

                ImGui.PushItemWidth(80f);
                ImGui.Text(p);
                ImGui.SameLine(100f);
                ImGui.SliderInt($"II4g6bpMNeD3", ref off[0], -15, 15);
                ImGui.SameLine();
                ImGui.SliderInt($"9w1NWQozLM4i", ref off[1], -15, 15);
                ImGui.SameLine();
                ImGui.SliderInt($"NVdTALn1Ghkt", ref off[2], -15, 15);
                ImGui.PopItemWidth();
            }

            if (ImGui.Button("G7n2F25jZYzF", new System.Numerics.Vector2(-1, 30)))
            {
                UnityMainThreadDispatcher.Instance.Enqueue(
                    () =>
                    {
                        MelonCoroutines.Start(BuildPreviewFromDataAsync(_currentEditingData, _currentlyPreviewingName));
                    });
            }

            ImGui.Separator();

            string saveText = _currentEditFilePath == null ? "Y616dQbrdoa2" : "gViyeJwNC0qO";
            ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 0f, 1f), "EVJiww8B4KLf");

            if (ImGui.Button(saveText, new System.Numerics.Vector2(180, 30)))
            {
                SaveEditedAvatar();
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                           { ClosePreview(); });
            }
            ImGui.SameLine();
            if (ImGui.Button("9drZTTi4E5tF", new System.Numerics.Vector2(140, 30)))
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                           { ClosePreview(); });
            }
            ImGui.Separator();
        }

        if (!Directory.Exists(_exportDirectory))
        {
            Directory.CreateDirectory(_exportDirectory);
        }

        string[] files = Directory.GetFiles(_exportDirectory, "p0JkThL9TrtV");

        if (files.Length == 0)
        {
            ImGui.TextColored(new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1f), "HQQn7zmfQbka");
        }

        ImGui.BeginChild("E4oC72ENdjcy", new System.Numerics.Vector2(0, 0), true);

        foreach (var filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (fileName == _activeAvatarName)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0f, 1f, 0f, 1f), fileName);
            }
            else
            {
                ImGui.Text(fileName);
            }

            ImGui.SameLine(ImGui.GetWindowWidth() - 210f);

            if (ImGui.Button($"uJhSU4uts35A"))
            {
                _activeAvatarName = fileName;
                UnityMainThreadDispatcher.Instance.Enqueue(
                    () =>
                    { MelonCoroutines.Start(LoadAndBuildPreviewAsync(filePath, fileName)); });
            }

            ImGui.SameLine();

            if (ImGui.Button($"eOfTK0MFAcZ8"))
            {
                _activeAvatarName = fileName;
                UnityMainThreadDispatcher.Instance.Enqueue(
                    () =>
                    { MelonCoroutines.Start(LoadForEditingAsync(filePath, fileName)); });
            }

            ImGui.SameLine();

            if (ImGui.Button($"XqV1Y4B6MttX"))
            {
                _activeAvatarName = fileName;
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                           { GameInfo.StartImportAvatarGeometry(filePath); });
            }
        }

        ImGui.EndChild();
    }

    private static System.Collections.IEnumerator LoadForEditingAsync(string filePath, string fileName)
    {
        ClosePreview();
        _currentlyPreviewingName = "fqZSrfOxAEkK";
        yield return null;

        List<ImportedAvatarPartData> importedParts = null;

        try
        {
            importedParts = GameInfo.ReadAvatarGeometryFromBase64(filePath);
        }
        catch (Exception ex)
        {
}

        if (importedParts == null || importedParts.Count == 0)
        {
            _currentlyPreviewingName = "jYfSYxhxlLNc";
            yield break;
        }

        _currentEditingData = importedParts;
        _currentEditFilePath = filePath;
        _isEditing = true;
        _partOffsets.Clear();

        MelonCoroutines.Start(BuildPreviewFromDataAsync(_currentEditingData, fileName));
    }

    private static void ReRandomizeColors()
    {
        if (_currentEditingData == null || _currentEditingData.Count == 0)
            return;

        System.Random rnd = new System.Random();
        List<byte> validMaterials = new List<byte>();
        int totalMaterials = MVGameControllerBase.Game.MaterialRepository.MaterialCount;

        for (byte m = 1; m < totalMaterials; m++)
        {
            var mat = MVGameControllerBase.Game.MaterialRepository.GetMaterial(m);
            if (mat != null && mat.Mesh != null)
            {
                validMaterials.Add(m);
            }
        }
        if (validMaterials.Count == 0)
            validMaterials.Add(21);

        string[] colorTargets = { "YJwrdxiBqC9W", "eRYZ5FeRzhzz" };

        foreach (var part in _currentEditingData)
        {
            if (colorTargets.Contains(part.PartName) && part.ImportedCubes.Count > 0)
            {
                byte randomNewMat = validMaterials[rnd.Next(validMaterials.Count)];

                Dictionary<byte, int> matFrequencies = new Dictionary<byte, int>();
                foreach (var cube in part.ImportedCubes.Values)
                {
                    byte primaryMat = cube.FaceMaterials[0];
                    if (!matFrequencies.ContainsKey(primaryMat))
                        matFrequencies[primaryMat] = 0;
                    matFrequencies[primaryMat]++;
                }

                byte majorityMat = matFrequencies.OrderByDescending(x => x.Value).First().Key;

                foreach (var cube in part.ImportedCubes.Values)
                {
                    if (cube.FaceMaterials[0] == majorityMat)
                    {
                        for (int i = 0; i < 6; i++)
                            cube.FaceMaterials[i] = randomNewMat;
                    }
                }
            }
        }

        UnityMainThreadDispatcher.Instance.Enqueue(
            () =>
            { MelonCoroutines.Start(BuildPreviewFromDataAsync(_currentEditingData, _currentlyPreviewingName)); });
    }

    private static System.Collections.IEnumerator GenerateRandomAvatarAsync(string[] availableFiles)
    {
        if (availableFiles.Length < 1)
        {
            _currentlyPreviewingName = "Ckw5iWDNhB5A";
            yield break;
        }

        ClosePreview();
        _currentlyPreviewingName = "0KO4YmsBWCge";
        yield return null;

        System.Random rnd = new System.Random();
        _currentEditingData = new List<ImportedAvatarPartData>();
        _partOffsets.Clear();
        _currentEditFilePath = null;

        string[][] targetPartGroups =
            new string[][] { new string[] { "3NQYvEZ9nrQI" }, new string[] { "uAt6sLQoKo13" }, new string[] { "WtfmC1LTmUBF", "MZvCAMyEr1xg" },
                             new string[] { "P1brG64j1Isc", "7fjrtyPcimfR", "w366beb35qt7", "TwFjCdMimrQ3", "b0ew0UmkSEd3", "rnEQRA7ddb27" } };

        foreach (string[] partGroup in targetPartGroups)
        {
            string randomFile = availableFiles[rnd.Next(availableFiles.Length)];
            List<ImportedAvatarPartData> fileParts = null;

            try
            {
                fileParts = GameInfo.ReadAvatarGeometryFromBase64(randomFile);
            }
            catch
            {
                continue;
            }

            if (fileParts == null)
                continue;

            foreach (string partName in partGroup)
            {
                var sourcePart = fileParts.FirstOrDefault(p => p.PartName == partName);
                if (sourcePart != null)
                {
                    _currentEditingData.Add(sourcePart);
                }
            }
            yield return null;
        }

        ReRandomizeColors();
        _isEditing = true;
    }

    private static void SaveEditedAvatar()
    {
        if (_currentEditingData == null || _currentEditingData.Count == 0)
            return;

        string filePath = _currentEditFilePath;
        string fileName = "72rSKPuds9Tq";

        if (string.IsNullOrEmpty(filePath))
        {
            fileName = "PDFC8lbR6tyD" + DateTime.Now.ToString("jro1mFombtNA") + "C7s5dHtH6pMe";
            filePath = Path.Combine(_exportDirectory, fileName);
        }
        else
        {
            fileName = Path.GetFileName(filePath);
        }

        try
        {
            BytePacker bp = new BytePacker();
            bp.Write(_currentEditingData.Count);

            foreach (var part in _currentEditingData)
            {
                byte[] nameBytes = Encoding.UTF8.GetBytes(part.PartName);
                bp.Write(nameBytes.Length);
                bp.Write(nameBytes);
                bp.Write(part.PrototypeID);
                bp.Write(part.ImportedCubes.Count);

                int offsetX = _partOffsets.ContainsKey(part.PartName) ? _partOffsets[part.PartName][0] : 0;
                int offsetY = _partOffsets.ContainsKey(part.PartName) ? _partOffsets[part.PartName][1] : 0;
                int offsetZ = _partOffsets.ContainsKey(part.PartName) ? _partOffsets[part.PartName][2] : 0;

                foreach (var kvp in part.ImportedCubes)
                {
                    bp.Write((short)(kvp.Key.x + offsetX));
                    bp.Write((short)(kvp.Key.y + offsetY));
                    bp.Write((short)(kvp.Key.z + offsetZ));
                    bp.Write(kvp.Value.ByteCorners);

                    for (int i = 0; i < 6; i++)
                        bp.Write(kvp.Value.FaceMaterials[i]);
                }
            }

            File.WriteAllText(filePath, Convert.ToBase64String(bp.ToArray()));
_currentlyPreviewingName = "RDoZqRpGMb0i" + fileName;
        }
        catch (Exception ex)
        {
}
    }

    public static System.Collections.IEnumerator LoadAndBuildPreviewAsync(string filePath, string fileName)
    {
        ClosePreview();
        _currentlyPreviewingName = "1tbcqQdtjzx3";
        yield return null;

        List<ImportedAvatarPartData> importedParts = null;
        try
        {
            importedParts = GameInfo.ReadAvatarGeometryFromBase64(filePath);
        }
        catch (Exception ex)
        {
}

        if (importedParts == null || importedParts.Count == 0)
        {
            _currentlyPreviewingName = "wcBQ65BmYwFN";
            yield break;
        }

        MelonCoroutines.Start(BuildPreviewFromDataAsync(importedParts, fileName));
    }

    private static System.Collections.IEnumerator BuildPreviewFromDataAsync(List<ImportedAvatarPartData> importedParts,
                                                                            string fileName)
    {
        if (_previewCam != null)
            UnityEngine.Object.Destroy(_previewCam.gameObject);
        if (_currentAvatarObj != null)
            UnityEngine.Object.Destroy(_currentAvatarObj);

        foreach (var rpcm in _activePreviewModels)
            if (rpcm != null)
                rpcm.Destroy();
        _activePreviewModels.Clear();

        _currentAvatarObj = new GameObject($"czwUC0rDbXY6");
        _currentAvatarObj.transform.position = new Vector3(5000f, 5000f, 5000f);

        GameObject rotateRoot = new GameObject("uME9HfhdxRZI");
        rotateRoot.transform.SetParent(_currentAvatarObj.transform);
        rotateRoot.transform.localPosition = Vector3.zero;

        int previewLayer = LayerMask.NameToLayer("J8d4vaLDXTCR");

        BodyData bodyData = MVGameControllerBase.Game.LocalPlayer.Body.BodyData;
        Transform bodyRoot = MVGameControllerBase.Game.LocalPlayer.Body.Transform;

        foreach (var part in importedParts)
        {
            GameObject dummyBone = new GameObject(part.PartName + "T8oVrMfu3bkY");
            dummyBone.transform.SetParent(rotateRoot.transform, false);

            if (bodyData != null && bodyRoot != null)
            {
                Transform liveBone = bodyData.GetPartBone(part.PartName);
                if (liveBone != null)
                {
                    dummyBone.transform.localPosition = bodyRoot.InverseTransformPoint(liveBone.position) * 10f;
                    dummyBone.transform.localRotation = Quaternion.Inverse(bodyRoot.rotation) * liveBone.rotation;
                }
            }

            BytePacker bp = new BytePacker();
            bp.Write(part.ImportedCubes.Count);

            int offsetX = _partOffsets.ContainsKey(part.PartName) ? _partOffsets[part.PartName][0] : 0;
            int offsetY = _partOffsets.ContainsKey(part.PartName) ? _partOffsets[part.PartName][1] : 0;
            int offsetZ = _partOffsets.ContainsKey(part.PartName) ? _partOffsets[part.PartName][2] : 0;

            foreach (var kvp in part.ImportedCubes)
            {
                int visualX = kvp.Key.x + offsetX;
                int visualY = kvp.Key.y + offsetY;
                int visualZ = kvp.Key.z + offsetZ;

                CubeDataPacker.WriteCompressedCube(bp, (short)visualX, (short)visualY, (short)visualZ,
                                                   kvp.Value.ByteCorners, kvp.Value.FaceMaterials);
            }

            RuntimePrototypeCubeModel rpcm = new RuntimePrototypeCubeModel(-1, 0, 1f, bp.ToArray());
            _activePreviewModels.Add(rpcm);

            GameObject partObj = rpcm.GetMesh();
            partObj.name = part.PartName;

            partObj.transform.SetParent(dummyBone.transform, false);
            partObj.transform.localPosition = Vector3.zero;
            partObj.transform.localRotation = Quaternion.identity;

            if (bodyData != null)
            {
                Vector3 boneSpacePos = bodyData.GetPartBoneSpacePosition(part.PartName);
                Quaternion targetRotation = Quaternion.identity;

                switch (part.PartName)
                {
                case "blHBYBmOP14X":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.right, dummyBone.transform.up);
                    boneSpacePos.y -= 0.8f;
                    break;
                case "86foMXAIVB6p":
                    targetRotation = Quaternion.LookRotation(-dummyBone.transform.right, dummyBone.transform.up);
                    break;
                case "yqAsp8CK1sEP":
                    targetRotation = Quaternion.LookRotation(-dummyBone.transform.up, -dummyBone.transform.right);
                    break;
                case "npIMiLRFWf7m":
                    targetRotation = Quaternion.LookRotation(-dummyBone.transform.up, dummyBone.transform.right);
                    break;
                case "j5YtM4U6Rydd":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.forward, dummyBone.transform.up);
                    break;
                case "wUATSLaZnq1R":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.right, dummyBone.transform.up);
                    break;
                case "hYPBqH4eF0Gl":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.forward, dummyBone.transform.up);
                    break;
                case "bOxYAJX1OctK":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.right, dummyBone.transform.up);
                    break;
                case "fOyvUjbgmiTk":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.forward, dummyBone.transform.up);
                    break;
                case "zDuJt3I3FWZq":
                    targetRotation = Quaternion.LookRotation(dummyBone.transform.forward, dummyBone.transform.up);
                    break;
                }

                partObj.transform.rotation = targetRotation;
                partObj.transform.Translate(boneSpacePos);
            }

            foreach (var mr in partObj.GetComponentsInChildren<MeshRenderer>())
            {
                mr.gameObject.layer = previewLayer;
            }

            yield return null;
        }

        Renderer[] renderers = rotateRoot.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(rotateRoot.transform.position, Vector3.zero);

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        GameObject pivotObj = new GameObject("WAj7XOZex3v6");
        pivotObj.transform.position = bounds.center;
        pivotObj.transform.SetParent(_currentAvatarObj.transform);
        rotateRoot.transform.SetParent(pivotObj.transform);

        GameObject camObj = new GameObject("ktpBsUQ17HbU");
        camObj.transform.SetParent(_currentAvatarObj.transform);

        _previewCam = camObj.AddComponent<Camera>();
        _previewCam.clearFlags = CameraClearFlags.SolidColor;
        _previewCam.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        _previewCam.depth = 100;
        _previewCam.cullingMask = 1 << previewLayer;
        _previewCam.rect = new Rect(0.45f, 0.1f, 0.5f, 0.8f);

        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float camDist = (maxDim / 2f) / Mathf.Tan((_previewCam.fieldOfView / 2f) * Mathf.Deg2Rad);

        _previewCam.transform.position = pivotObj.transform.position + new Vector3(0, 0, -(camDist + 2f));
        _previewCam.transform.LookAt(pivotObj.transform.position);

        Light dirLight = camObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.cullingMask = 1 << previewLayer;
        dirLight.intensity = 1.1f;
        dirLight.color = Color.white;

        MelonCoroutines.Start(RotatePreviewRoutine(pivotObj));

        _currentlyPreviewingName = fileName;
    }

    private static System.Collections.IEnumerator RotatePreviewRoutine(GameObject pivotObj)
    {
        while (pivotObj != null)
        {
            pivotObj.transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);
            yield return null;
        }
    }

    public static void ClosePreview()
    {
        _isEditing = false;

        if (_previewCam != null)
        {
            UnityEngine.Object.Destroy(_previewCam.gameObject);
            _previewCam = null;
        }
        if (_currentAvatarObj != null)
        {
            UnityEngine.Object.Destroy(_currentAvatarObj);
            _currentAvatarObj = null;
        }

        foreach (var rpcm in _activePreviewModels)
        {
            if (rpcm != null)
                rpcm.Destroy();
        }
        _activePreviewModels.Clear();

        _currentlyPreviewingName = "09NZeNaGQIOu";
    }
}
}

--- FILE: Features\ChatAntiCrash.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TestMod.Features
{
internal class ChatAntiCrash
{
    public static bool AntiCrashEnabled = true;

    public static class AntiChatCrash
    {
        private static readonly Regex CrashTags =
            new Regex(@"ylE72dSSjWo4", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static bool IsCrashPayload(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            if (text.IndexOf('<') == -1)
                return false;
            if (CrashTags.IsMatch(text))
                return true;
            if (text.Split('<').Length > 45)
                return true;

            return false;
        }
        public static string Sanitize(string text)
        {
            if (!IsCrashPayload(text))
                return text;
            return CrashTags.Replace(text, "sywd6XnQ6ZDN");
        }
        [HarmonyPatch(typeof(ChatControllerBase), "B0ERlEpgWiGn")]
        public static class ChatControllerBase_ReceiveMessage_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(
                Il2CppMV.Common.MVGameMsgType msgType,
                Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> message)
            {
                if (!AntiCrashEnabled || message == null)
                    return true;

                try
                {
                    foreach (var val in message.Values)
                    {
                        if (val == null)
                            continue;

                        string str = val.ToString();
                        if (IsCrashPayload(str))
                        {
return false;
                        }
                    }
                }
                catch (Exception e)
                {
}

                return true;
            }
        }
        [HarmonyPatch(typeof(ChatBubbleController), nameof(ChatBubbleController.ShowChatBubble))]
        public static class ChatBubble_CrashFix
        {
            [HarmonyPrefix]
            public static bool Prefix(ref string text, ref string senderName)
            {
                if (IsCrashPayload(text))
                {
text = "vuqkvEz2hrN2";
                    return true;
                }

                if (IsCrashPayload(senderName))
                {
                    senderName = "NjEF7lUxyo58";
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(Il2CppTMPro.TMP_Text), "uoGz8D8V4eIX")]
        public static class GlobalTMP_Safety_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(ref string value)
            {
                if (string.IsNullOrEmpty(value))
                    return;
                if (value.IndexOf("G07TFisRw9WB", StringComparison.Ordinal) != -1)
                {
                    if (IsCrashPayload(value))
                    {
                        value = "U88zwrZojX15";
                    }
                }
            }
        }
        [HarmonyPatch(typeof(AvatarUIHandlerRemote), nameof(AvatarUIHandlerRemote.UpdateNameTag))]
        public static class NameTag_Safety_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(AvatarUIHandlerRemote __instance)
            {
                if (__instance == null || __instance.ownerActorName == null)
                    return;

                if (IsCrashPayload(__instance.ownerActorName))
                {
                    __instance.ownerActorName = "fGIPbM4947h0";
                }
            }
        }
    }
}
}

--- FILE: Features\ChatTextStylizer.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using ImGuiNET;
using MelonLoader;
using UnityEngine;
using System.Text;
using System;

namespace TestMod.Features
{
public static class ChatStyle
{
    public static bool on = true;
    public static bool wavy = false;

    public static bool rot = false;
    public static bool ital = false;
    public static bool bld = true;
    public static bool und = false;
    public static bool strk = false;
    public static bool hgl = false;
    public static bool crash = false;

    public static bool useSpc = false;
    public static bool useVof = false;
    public static bool useCtag = false;
    public static bool useAlf = false;

    public static float spc = 0f;
    public static float vof = 0f;
    public static string ctag = "AF2WY9k3m5Ln";
    public static string alf = "jDtqKq9IYBuV";

    public static int idx = 9;
    public static int widx = 0;
    public static int sz = 35;
    public static float rotv = 5f;

    public static string[] names =
        new string[] { "Cozn91qvx1C3", "OItA0qyM7cPH", "cUNgP2cALhvb", "H7OrmTA5RMHp", "h0mU7V7QB5HL", "zhKoI5x3bcha", "cijs7zbg6eRG", "YMexmOezAVGa", "BHwocokO8POl", "ZvKhLMD8w5he" };

    public static string[] wnames =
        new string[] { "qu3jgF3HeV2W", "gKsZLxJ8DGt1", "FxhRmFDH7Aqt", "nODdzIOZ6F4s", "yTnMAtxd8xIM", "Rb2wh70t48nQ", "1kkj9wzdHX7L" };

    private static string[] codes = new string[] { "ao9mRuzP0zag", "sKdCDK34nbDd", "4PpDgRGrr4gt", "8wDiMY7MoTgm", "ZUQRCE1mMNYM",
                                                   "5TnaPA2Lwcuh", "TNQkLMjZrCSq", "QrzTKk4IZ81t", "1boKG7AvWIvZ", "L3z1uMCzaxrb" };

    public static void ui()
    {
        ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "rYYzXpEqyWwi");
        ImGui.Separator();
        ImGui.Checkbox("R5KJeRdhY6Da", ref on);

        if (on)
        {
            ImGui.SliderInt("1JwBol7AwGmA", ref sz, 10, 960);

            ImGui.Checkbox("VznvA2LUidMm", ref useSpc);
            if (useSpc)
            {
                ImGui.SameLine();
                ImGui.SliderFloat("VjEYle6Od7pl", ref spc, -10f, 50f);
            }

            ImGui.Checkbox("7mRVE20lmYt8", ref useVof);
            if (useVof)
            {
                ImGui.SameLine();
                ImGui.SliderFloat("W0w5mJIWDYlU", ref vof, -50f, 50f);
            }

            ImGui.Checkbox("lMPPp8LJ6ciO", ref bld);
            ImGui.SameLine();
            ImGui.Checkbox("T31ajKZJiuME", ref ital);

            ImGui.Checkbox("QlmEavWljVg7", ref und);
            ImGui.SameLine();
            ImGui.Checkbox("O6CKmoqjW4Ae", ref strk);
            ImGui.SameLine();
            ImGui.Checkbox("SWSq04aCZ57e", ref hgl);

            ImGui.Checkbox("64NMyzY7JTWA", ref rot);
            if (rot)
            {
                ImGui.SameLine();
                ImGui.SliderFloat("J0jLXxTQZ8F1", ref rotv, -360f, 360f);
            }

            ImGui.Checkbox("lcHMJ7MhJhN1", ref useCtag);
            if (useCtag)
            {
                ImGui.SameLine();
                ImGui.InputText("lEqySJ1aqpeb", ref ctag, 100);
            }

            ImGui.Checkbox("ZNDrGRv7i5Ko", ref useAlf);
            if (useAlf)
            {
                ImGui.SameLine();
                ImGui.InputText("PzoSuvW5oWPv", ref alf, 2);
            }

            ImGui.Separator();

            ImGui.Checkbox("rngvFRXU8PQm", ref crash);

            ImGui.Separator();

            ImGui.Checkbox("49i1NQhng1nz", ref wavy);
            if (wavy)
                ImGui.Combo("Lr3deNcTavbi", ref widx, wnames, wnames.Length);
            else
                ImGui.Combo("svbLoQ0d0MlP", ref idx, names, names.Length);
        }
    }

    private static string hex(Color c)
    {
        int r = Mathf.Clamp((int)(c.r * 255), 0, 255);
        int g = Mathf.Clamp((int)(c.g * 255), 0, 255);
        int b = Mathf.Clamp((int)(c.b * 255), 0, 255);
        return $"lAnqFfwA8cDy";
    }

    private static Color grad(int p, float t)
    {
        switch (p)
        {
        case 0:
            return rainbow(t);
        case 1:
            return Color.Lerp(new Color(1f, 0.4f, 0.7f), new Color(0.4f, 1f, 1f), t);
        case 2:
            return Color.Lerp(new Color(0.5f, 0f, 0.5f), new Color(1f, 0.5f, 0f), t);
        case 3:
            return Color.Lerp(Color.green, new Color(0.6f, 0f, 1f), t);
        case 4:
            return Color.Lerp(Color.red, Color.yellow, t);
        case 5:
            return Color.Lerp(Color.blue, Color.cyan, t);
        case 6:
            return Color.Lerp(new Color(1f, 0.4f, 0.7f), new Color(0.6f, 0f, 1f), t);
        default:
            return Color.white;
        }
    }

    private static Color rainbow(float t)
    {
        float r = Mathf.Abs(t * 6f - 3f) - 1f;
        float g = 2f - Mathf.Abs(t * 6f - 2f);
        float b = 2f - Mathf.Abs(t * 6f - 4f);
        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
    }

    [HarmonyPatch(typeof(SendMessageControl), "tTkn3i01bsh1")]
    public static class SanitizePatch
    {
        [HarmonyPrefix]
        private static bool Pre(ref string message, string tagToSanitize)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(SendMessageControl), "VhVxL7uOsvIx")]
    public static class MsgPatch
    {
        [HarmonyPrefix]
        private static void Pre(ref string chatMsg)
        {
            if (!on || string.IsNullOrEmpty(chatMsg))
                return;

            if (crash)
            {
                chatMsg = "bqvEL24OXojb";
                return;
            }

            if (chatMsg.StartsWith("IoLBgvEFSNlb") || chatMsg.StartsWith("z5bzQFMaeq2D"))
                return;

            string plain = chatMsg;
            StringBuilder sb = new StringBuilder();

            sb.Append($"qjKbPl7cF33a");

            if (rot)
                sb.Append($"X91F23IrNy8V");
            if (useSpc)
                sb.Append($"bFPO57zAoIpU");
            if (useVof)
                sb.Append($"b0Ciy7cKP11f");

            if (bld)
                sb.Append("5aDW2nojZB8s");
            if (ital)
                sb.Append("PhXSjaMeTkxo");
            if (und)
                sb.Append("MXqMgofyfPEq");
            if (strk)
                sb.Append("HWYfJHHT6NtR");
            if (hgl)
                sb.Append("BsRR59V8qKgV");

            if (useCtag && !string.IsNullOrEmpty(ctag))
                sb.Append(ctag);

            string alphaHex = "e5dvcy8kVuwI";
            if (useAlf && alf.Length == 2)
                alphaHex = alf;

            if (wavy)
            {
                int len = plain.Length;
                for (int i = 0; i < len; i++)
                {
                    char c = plain[i];
                    if (c == ' ')
                    {
                        sb.Append("ulY5PMJiLPni");
                        continue;
                    }

                    float t = (float)i / (float)Mathf.Max(len - 1, 1);
                    float wt = Mathf.PingPong(t * 2, 1);
                    if (widx == 0)
                        wt = Mathf.Repeat(t + 0.2f, 1f);

                    Color col = grad(widx, wt);
                    sb.Append($"zvyQ1PUOC4oO");
                }
            }
            else
            {
                string h = codes[idx];
                sb.Append($"BDLsgMfffvGh");
            }

            if (hgl)
                sb.Append("44yCTsizh5Ec");
            if (strk)
                sb.Append("9ZtcgJqjbLPP");
            if (und)
                sb.Append("yX0VwipYvd1D");
            if (ital)
                sb.Append("MWbN3P3F1QGj");
            if (bld)
                sb.Append("kjoMFM0hi7s6");

            if (useVof)
                sb.Append("bJea9SWCU7hN");
            if (useSpc)
                sb.Append("p0LbIuhUvqBI");
            if (rot)
                sb.Append("QshazZHtptyw");

            sb.Append("E1f8jBIHlkHu");

            chatMsg = sb.ToString();
        }
    }
}
}

--- FILE: Features\CubeSpawn.cs ---
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using UnityEngine;
using Il2Cpp;
using MelonLoader;
using Il2CppMV.WorldObject;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Newtonsoft.Json;
using TestMod.Helpers;

namespace TestMod.Features
{

public class CubeSpawn
{
    private static string _modelName = "gZBZil5tkzKy";
    private static string _importFile = "rfdTPk4M6m0X";
    private static bool _isWorking = false;
    private static string[] _fileList = null;
    private static int _pickIdx = 0;
    private static int _batchSize = 300;
    private static float _batchDelay = 5.0f;
    private static int _cubesPlacedInBatch = 0;

    public static void RenderUI()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1, 0, 1, 1), "dkXWWGBzz7SJ");
        ImGui.Text($"LjWovK5l6aDK"BUSY..."3lC8vNNAY78R"READY"DDtRBUBaqI8i");
        ImGui.Separator();
        ImGui.InputText("fnwdAJ6VEvpV", ref _modelName, 64);
        ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "g0iFNcOZkZps");
        ImGui.InputInt("UiZGNTLa1ELM", ref _batchSize);
        ImGui.InputFloat("u6cVUKOOnyug", ref _batchDelay);

        if (_fileList == null)
            RefreshFiles();

        if (ImGui.Button("q8JjPrgzaDz8"))
            RefreshFiles();

        if (_fileList != null && _fileList.Length > 0)
        {
            if (ImGui.Combo("chExfCdtKNso", ref _pickIdx, _fileList, _fileList.Length))
            {
                _importFile = _fileList[_pickIdx];
            }
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), $"Y2Wiz2SWRWRq");
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "xgndxz5hLXYw");
        }

        if (!_isWorking && !string.IsNullOrEmpty(_importFile))
        {
            ImGui.Separator();
            if (ImGui.Button("EYiUFgtNO4WZ"))
            {
                MelonCoroutines.Start(ImportJob());
            }
        }
    }

    private static void RefreshFiles()
    {
        string dir = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "JUqQg0jrrTw2");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            _fileList = new string[0];
            return;
        }

        _fileList = Directory.GetFiles(dir, "ZsZW6VFuShTE").Select(Path.GetFileName).ToArray();

        if (_fileList.Length > 0)
        {
            _pickIdx = 0;
            _importFile = _fileList[0];
        }
        else
        {
            _importFile = "MTnzDGqgAvy7";
        }
    }

    private static EditorStateMachine GetStateMachine()
    {
        var editUI = MVGameControllerBase.EditModeUI;
        if (editUI == null)
            return null;
        var desktopController = editUI.TryCast<DesktopEditModeController>();
        if (desktopController == null)
            return null;
        return desktopController.EditModeStateMachine;
    }

    private static IEnumerator ImportJob()
    {
        _isWorking = true;
string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "RyXrIzsTLZZy", _importFile);

        if (!File.Exists(path))
        {
_isWorking = false;
            yield break;
        }

        var jsonStr = File.ReadAllText(path);
        var models = JsonConvert.DeserializeObject<List<JsonModelData>>(jsonStr);
foreach (var m in models)
        {
            yield return SpawnOne(m);
            yield return new WaitForSeconds(1.0f);
        }
_isWorking = false;
    }

    private static IEnumerator SpawnOne(JsonModelData data)
    {
var sm = GetStateMachine();
        if (sm == null)
        {
yield break;
        }

        var editUI = MVGameControllerBase.EditModeUI;
        var desktop = editUI.TryCast<DesktopEditModeController>();
        var creator = desktop.editorWorldObjectCreation;

        float s = 1.0f;
        if (data.Scale != null && data.Scale.Length > 0)
            s = data.Scale[0];
creator.OnAddNewPrototype(_modelName, s);

        MVWorldObjectClient target = null;
        float t = Time.time + 4.0f;

        while (Time.time < t)
        {
            var sel = sm.SingleSelectedWO;
            if (sel != null)
            {
                target = sel;
                break;
            }
            yield return null;
        }

        if (target == null)
        {
yield break;
        }
desktop.SetState(EditorEvent.ObjectSelected);
        yield return null;

        Vector3 finalPos = Vector3.zero;
        if (data.Pos != null && data.Pos.Length == 3)
            finalPos = new Vector3(data.Pos[0], data.Pos[1], data.Pos[2]);

        if (data.Rot != null && data.Rot.Length == 4)
            target.WorldRotation = new Quaternion(data.Rot[0], data.Rot[1], data.Rot[2], data.Rot[3]);

        if (data.Scale != null && data.Scale.Length == 3)
            target.transform.localScale = new Vector3(data.Scale[0], data.Scale[1], data.Scale[2]);

        target.WorldPosition = finalPos;

        var cm = target.TryCast<MVCubeModelInstance>();
        if (cm == null)
        {
yield break;
        }

        yield return new WaitForSeconds(0.2f);
        yield return MelonCoroutines.Start(PlaceImportedCubesAsync(cm, data, finalPos));
    }

    public static IEnumerator PlaceImportedCubesAsync(MVCubeModelInstance targetModel, JsonModelData data,
                                                      Vector3 finalPos)
    {
        if (targetModel == null)
            yield break;
        _cubesPlacedInBatch = 0;

        int totalProcessed = 0;
        foreach (var c in data.Cubes)
        {
            var pos = new IntVector((short)c.x, (short)c.y, (short)c.z);
            byte[] corners = c.corners ?? CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);
            byte m = c.mat;
            byte[] mats = new byte[] { m, m, m, m, m, m };
            mats[1] = 21;

            var newCube = new Cube(corners, mats);
            targetModel.AddCube(pos, newCube);

            _cubesPlacedInBatch++;
            totalProcessed++;

            if (totalProcessed % 5 == 0)
            {
                yield return null;
            }

            if (_cubesPlacedInBatch >= _batchSize)
            {
targetModel.HandleDelta();
                targetModel.WorldPosition = finalPos;
yield return new WaitForSeconds(_batchDelay);
yield return MelonCoroutines.Start(FixModelPositionAsync(targetModel));

                _cubesPlacedInBatch = 0;
            }
        }

        if (_cubesPlacedInBatch > 0)
        {
targetModel.HandleDelta();
            yield return new WaitForSeconds(0.5f);
yield return MelonCoroutines.Start(FixModelPositionAsync(targetModel));
        }
}

    public static IEnumerator FixModelPositionAsync(MVCubeModelInstance model)
    {
if (model == null || model.WasCollected || model.Pointer == IntPtr.Zero)
        {
yield break;
        }

        var editModeController = MVGameControllerBase.EditModeUI?.TryCast<DesktopEditModeController>();
        if (editModeController == null || editModeController.EditModeStateMachine == null)
        {
yield break;
        }

        var esm = editModeController.EditModeStateMachine;
esm.DeSelectAll();
        esm.SelectWO(model.Id, false, true);

        yield return new WaitForSeconds(0.2f);

        var networkSelector = esm.NetworkSelector;
        if (networkSelector != null && !networkSelector.WasCollected && networkSelector.Pointer != IntPtr.Zero)
        {
            var idSet = new Il2CppSystem.Collections.Generic.HashSet<int>();
            idSet.Add(model.Id);
            networkSelector.RequestOwnership(idSet);
        }

        yield return new WaitForSeconds(0.3f);

        if (model == null || model.WasCollected || model.Pointer == IntPtr.Zero)
            yield break;

        Vector3 originalPos = model.transform.position;
        Vector3 targetOffset = new Vector3(-0.5f, 0f, 0f);
float timer = 0f;
        while (timer < 0.2f)
        {
            Vector3 curPos = Vector3.Lerp(originalPos, originalPos + targetOffset, timer / 0.2f);
            model.transform.position = curPos;
            model.WorldPosition = curPos;
            model.SyncPos = curPos;

            var gem = MVGameControllerBase.GameEventManager;
            if (gem != null && gem.AvatarCommandsBuildMode != null && gem.AvatarCommandsBuildMode.LaserCommands != null)
            {
                gem.AvatarCommandsBuildMode.LaserCommands.UpdatePosition(model.WorldPivot);
            }

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
try
        {
            model.HandleDelta();
        }
        catch
        {
        }

        yield return new WaitForSeconds(0.1f);

        if (model == null || model.WasCollected || model.Pointer == IntPtr.Zero)
            yield break;
timer = 0f;
        while (timer < 0.2f)
        {
            Vector3 curPos = Vector3.Lerp(originalPos + targetOffset, originalPos, timer / 0.2f);
            model.transform.position = curPos;
            model.WorldPosition = curPos;
            model.SyncPos = curPos;

            var gem = MVGameControllerBase.GameEventManager;
            if (gem != null && gem.AvatarCommandsBuildMode != null && gem.AvatarCommandsBuildMode.LaserCommands != null)
            {
                gem.AvatarCommandsBuildMode.LaserCommands.UpdatePosition(model.WorldPivot);
            }

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
model.transform.position = originalPos;
        model.WorldPosition = originalPos;
        model.SyncPos = originalPos;

        try
        {
            model.HandleDelta();
        }
        catch
        {
        }

        yield return new WaitForSeconds(0.2f);
try
        {
            esm.DeSelectAll();
        }
        catch (Exception ex)
        {
}
}
}
}

--- FILE: Features\CustomGunHeadReplacer.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMV.Common;
using Il2CppSystem.Collections.Generic;
using ImGuiNET;
using MelonLoader;
using System;
using TestMod.Helpers;
using UnityEngine;

namespace TestMod.Features
{
public static class CustomGunHeadReplacer
{
    public static bool Enabled = false;
    public static int SelectedPlayerActorNr = -1;

    public static System.Collections.Generic.List<System.Tuple<int, string>> CachedPlayers =
        new System.Collections.Generic.List<System.Tuple<int, string>>();
    private static DateTime _nextFetch = DateTime.MinValue;
    private static bool _isFetching = false;

    private static MVPlayer GetSafePlayer(int actorNr)
    {
        if (actorNr == -1)
            return null;

        var game = MVGameControllerBase.Game;
        if (game == null || game.playerContainer == null)
            return null;

        var enumerator = game.playerContainer.Values.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current != null && enumerator.Current.ActorNr == actorNr)
            {
                return enumerator.Current;
            }
        }
        return null;
    }

    public static void RenderUI()
    {
        if (ImGui.BeginTabItem("dEhIdqssfDuU"))
        {
            ImGui.Checkbox("36d5T5twJeLC", ref Enabled);

            if (DateTime.UtcNow > _nextFetch && !_isFetching)
            {
                _isFetching = true;
                TestMod.Helpers.UnityMainThreadDispatcher.Instance.Enqueue(
                    () =>
                    {
                        var temp = new System.Collections.Generic.List<System.Tuple<int, string>>();
                        var game = MVGameControllerBase.Game;
                        if (game != null && game.playerContainer != null)
                        {
                            var enumerator = game.playerContainer.Values.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                var p = enumerator.Current;
                                if (p != null && p.UserProfileData != null)
                                {
                                    temp.Add(new System.Tuple<int, string>(p.ActorNr, p.UserProfileData.UserName));
                                }
                            }
                        }
                        CachedPlayers = temp;
                        _nextFetch = DateTime.UtcNow.AddSeconds(1);
                        _isFetching = false;
                    });
            }

            if (CachedPlayers.Count > 0)
            {
                string preview = "SjEsscAqA6Sx";
                foreach (var cp in CachedPlayers)
                {
                    if (cp.Item1 == SelectedPlayerActorNr)
                    {
                        preview = cp.Item2;
                        break;
                    }
                }

                if (ImGui.BeginCombo("9vOmabC5KNUN", preview))
                {
                    try
                    {
                        foreach (var cp in CachedPlayers)
                        {
                            if (ImGui.Selectable(cp.Item2, SelectedPlayerActorNr == cp.Item1))
                            {
                                SelectedPlayerActorNr = cp.Item1;
                            }
                        }
                    }
                    finally
                    {
                        ImGui.EndCombo();
                    }
                }
            }
            else
            {
                ImGui.Text("mLobUqNDZnrR");
            }

            ImGui.EndTabItem();
        }
    }

    public static int GetSelectedPlayerHeadId()
    {
        var targetPlayer = GetSafePlayer(SelectedPlayerActorNr);
        if (targetPlayer == null)
            return 0;

        var wo = MVGameControllerBase.WOCM.GetWorldObjectClient(targetPlayer.WoId);
        if (wo == null)
            return 0;

        var avatar = wo.TryCast<MVAvatar>();
        if (avatar == null || avatar.Body == null)
            return 0;

        var headModel = avatar.Body.GetBodyPart("myUuO9fG4ABb");
        if (headModel == null)
            return 0;

        return headModel.Id;
    }

    [HarmonyPatch(typeof(MVPickupOwner), nameof(MVPickupOwner.CreateAndEquipNewItem))]
    public static class SwapCostumeToGunPatch
    {
        public static void Prefix(Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> newState)
        {
            if (!Enabled || newState == null)
                return;

            int headId = GetSelectedPlayerHeadId();
            if (headId == 0)
                return;

            var typeKey = Il2CppDictionaryHelper.GetKeyByName(newState, "t8Kw9GVPPiL7") ??
                          Il2CppDictionaryHelper.GetKeyByName(newState, "LtZ7FURMD5s1");

            if (typeKey != null && newState.ContainsKey(typeKey))
            {
                var typeObj = newState[typeKey];
                if (typeObj != null)
                {
                    unsafe
                    {
                        AvatarItemType *currentType = (AvatarItemType *)IL2CPP.il2cpp_object_unbox(typeObj.Pointer);

                        if (*currentType == AvatarItemType.Costume || *currentType == AvatarItemType.CustomGun)
                        {
                            *currentType = AvatarItemType.CustomGun;

                            var variantKey = Il2CppDictionaryHelper.GetKeyByName(newState, "qz7A5yaE2OPY") ??
                                             Il2CppDictionaryHelper.GetKeyByName(newState, "MuEblbNmY0S6");

                            if (variantKey != null)
                            {
                                Il2CppDictionaryHelper.SetIntInPlace(newState, variantKey.ToString(), 12730220);
                            }

                            var itemDataDict = Il2CppDictionaryHelper.TryGetNestedDictionary(newState, "6pCNlnf04zat") ??
                                               Il2CppDictionaryHelper.TryGetNestedDictionary(newState, "jn64nYcYHxEO");

                            if (itemDataDict != null)
                            {
                                Il2CppDictionaryHelper.SetIntInPlace(itemDataDict, "rSNjPn1k7PRV", headId);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(AvatarEquipable), nameof(AvatarEquipable.Equip))]
    public static class ForceWeaponEquipPatch
    {
        public static void Prefix(ref AvatarItemType type, ref AvatarEquipableType equipType,
                                  Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> itemData, ref int variantID)
        {
            if (!Enabled || type == AvatarItemType.Hand)
                return;

            int headId = GetSelectedPlayerHeadId();

            if (headId != 0 && (type == AvatarItemType.Costume || type == AvatarItemType.CustomGun))
            {
                type = AvatarItemType.CustomGun;
                equipType = AvatarEquipableType.Weapon;
                variantID = 12730220;

                if (itemData != null)
                {
                    Il2CppDictionaryHelper.SetIntInPlace(itemData, "mFnVzB3IiO1H", headId);
                }
            }
        }
    }
}
}

--- FILE: Features\EliteFeatures.cs ---
﻿using System;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;

namespace TestMod.Features
{
    public static class EliteVisualsPatch
    {
        private static readonly Color EliteGoldColor = new Color(1f, 0.84f, 0f, 1f);

        public static void Initialize(HarmonyLib.Harmony harmony)
        {
harmony.PatchAll(typeof(EliteVisualsPatch));
        }
        [HarmonyPatch(typeof(ScoreBoardBase.ScoreData), "MCWCUGcOp8tG")]
        public static class Scoreboard_EliteIcon_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerElement), nameof(PlayerElement.Initialize))]
        public static class PlayerElement_Initialize_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerElement __instance)
            {
                if (__instance == null) return;
                __instance.subscriber = true;
                __instance.ActivateSubscriberUI(true);
                if (__instance.playerName != null)
                {
                    __instance.playerName.color = EliteGoldColor;
                }
                if (__instance.memberUI != null)
                {
                    __instance.memberUI.SetActive(true);
                }
                if (__instance.nonMemberUI != null)
                {
                    __instance.nonMemberUI.gameObject.SetActive(false);
                }
            }
        }
        [HarmonyPatch(typeof(LocalPlayerScore), nameof(LocalPlayerScore.Initialize))]
        public static class LocalPlayerScore_Initialize_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(LocalPlayerScore __instance)
            {
                if (__instance == null) return;
                if (__instance.memberUI != null)
                {
                    __instance.memberUI.SetActive(true);
                }
                if (__instance.playerNameText != null)
                {
                    __instance.playerNameText.color = EliteGoldColor;
                }
            }
        }
        [HarmonyPatch(typeof(AvatarUIHandlerRemote), nameof(AvatarUIHandlerRemote.UpdateNameTag))]
        public static class AvatarNameTag_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(AvatarUIHandlerRemote __instance)
            {
                if (__instance == null) return;
                if (__instance.memberFrame != null)
                {
                    __instance.memberFrame.SetActive(true);
                }
            }
        }
        [HarmonyPatch(typeof(ChatControllerBase), "IX9zUxEnSHMR")]
        public static class ChatColor_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result)
            {
            }
        }
    }
}
--- FILE: Features\exportModels.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TestMod.Helpers;
using UnityEngine;

namespace TestMod.Features
{
public static class WorldObjectOperations
{
    public static IEnumerator CloneObject(int srcId, Vector3 p, Quaternion r, Action<int> cb)
    {
        var src = MVGameControllerBase.WOCM.GetWorldObjectClient(srcId);
        if (src == null)
        {
cb?.Invoke(-1);
            yield break;
        }

        bool got = false;
        int nid = -1;

        Action<Il2CppSystem.Object, Il2CppSystem.EventArgs> h = (s, e) =>
        {
            var a = e.TryCast<CloneWorldObjectTreeResponseEventArgs>();
            if (a == null)
                return;
            if (a.Success)
                nid = a.RootId;
            got = true;
        };

        MVGameControllerBase.WOCM.CloneWorldObjectTreeResponse += h;
        MVGameControllerBase.OperationRequests.CloneWorldObjectTreeWithPosition(src, p, r, true, false, false, false);

        float end = Time.time + 10f;
        while (!got && Time.time < end)
            yield return null;

        MVGameControllerBase.WOCM.CloneWorldObjectTreeResponse -= h;

        if (got && nid != -1)
            cb?.Invoke(nid);
        else
        {
cb?.Invoke(-1);
        }
    }
}

[HarmonyPatch(typeof(WorldNetwork), nameof(WorldNetwork.CreateQueryEvent))]
public static class WOReciever
{
    public static Action<int> OnMyObjectCreated;

    [HarmonyPostfix]
    private static void CreateQueryEvent(MVWorldObjectClient root, int instigatorActorNumber)
    {
        if (MVGameControllerBase.IsInitialized &&
            instigatorActorNumber == MVGameControllerBase.Game.LocalPlayer.ActorNr)
        {
            ExportModels.CapturedModelID = root.Id;
            if (OnMyObjectCreated != null)
                OnMyObjectCreated.Invoke(root.Id);
        }
    }
}

public static class ExportModels
{
    public enum ModelType
    {
        Standard,
        Avatar,
        CubeGun,
        All
    }

    public static string ExportPath => Path.Combine(MelonLoader.MelonUtils.GameDirectory, "itjeZdvgVads");
    public static int CapturedModelID = -1;
    private static bool isEx = false;

    private const int B_SZ = 80;
    private const float B_DEL = 0.6f;

    public static void StartExportAll(ModelType targetType)
    {
        if (isEx)
            return;
        UnityMainThreadDispatcher.Instance.Enqueue(RunExportAll(targetType));
    }

    private static IEnumerator RunExportAll(ModelType targetType)
    {
        yield return null;
        isEx = true;

        if (!Directory.Exists(ExportPath))
            Directory.CreateDirectory(ExportPath);

        string ts = DateTime.Now.ToString("NasDSS7X7sIz");
        string fp = Path.Combine(ExportPath, $"FksMm2GqnvbM");

        using (StreamWriter sw = File.CreateText(fp)) using (JsonTextWriter jw = new JsonTextWriter(sw))
        {
            jw.Formatting = Formatting.Indented;
            jw.WriteStartArray();

            var ser = JsonSerializer.CreateDefault();

            
            if (targetType == ModelType.CubeGun)
            {
                
                MVCubeModelBase targetTerrain =
                    MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                        ?.TryCast<MVCubeModelBase>();

                
                if (targetTerrain == null)
                {
                    targetTerrain = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelPrototypeTerrain>()
                                        ?.TryCast<MVCubeModelBase>();
                }

                if (targetTerrain != null)
                {
var data = CaptureModelData(targetTerrain.TryCast<MVWorldObjectClient>(), targetType, null);

                    if (data != null && data.Cubes.Count > 0)
                    {
                        ser.Serialize(jw, data);
}
                    else
                    {
}
                }
                else
                {
}
            }
            
            else
            {
                var vals = MVGameControllerBase.WOCM.worldObjects.Values;
                var safeL = new List<MVWorldObjectClient>();
                foreach (var v in vals)
                    safeL.Add(v);

                MVWorldObjectClient lastCube = null;
                for (int i = safeL.Count - 1; i >= 0; i--)
                {
                    var w = safeL[i];
                    if (w != null && w.WorldObjectType == WorldObjectType.CubeModel &&
                        w.Id != MVGameControllerBase.WOCM.RootGroup.Id)
                    {
                        if (!IsAvatarObj(w))
                        {
                            lastCube = w;
                            break;
                        }
                    }
                }

                int c = 0;
                foreach (var wo in safeL)
                {
                    if (wo == null)
                        continue;
                    try
                    {
                        var d = CaptureModelData(wo, targetType, lastCube);
                        if (d != null)
                        {
                            ser.Serialize(jw, d);
                        }
                    }
                    catch
                    {
                    }

                    c++;
                    if (c % 10 == 0)
                        yield return null;
                }
            }

            jw.WriteEndArray();
        }
isEx = false;
    }

    public static void StartExport(int id)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(RunExportSingle(id));
    }

    private static IEnumerator RunExportSingle(int id)
    {
        yield return null;
        var wo = MVGameControllerBase.WOCM.GetWorldObjectClient(id);
        if (wo == null)
            yield break;

        var l = new List<JsonModelData>();
        var d = CaptureModelData(wo, ModelType.All, null);
        if (d != null)
            l.Add(d);

        string ts = DateTime.Now.ToString("mRAsXHER8oTt");
        Save(l, $"QPKVh54OOimg");
    }

    private static bool IsAvatarObj(MVWorldObjectClient wo)
    {
        try
        {
            string n = wo.gameObject.name.ToLower();
            Transform p = wo.transform.parent;
            while (p != null)
            {
                string pName = p.name.ToLower();
                if (pName.Contains("3ngwRRUEI8NY") || pName.Contains("oh4VnTX7YvdJ") || pName.Contains("wdoVaZAcmp4S"))
                    return true;
                p = p.parent;
            }
            if (n == "9RYI9fQaVI1w" || n == "MBr6925Rjw5e" || n.Contains("luipQJSCxbsU") || n.Contains("GhBFzj0RjM1R"))
                return true;
        }
        catch
        {
        }
        return false;
    }

    private static JsonModelData CaptureModelData(MVWorldObjectClient wo, ModelType targetType,
                                                  MVWorldObjectClient lastCube)
    {
        if (wo == null)
            return null;

        bool isTerrain = wo.WorldObjectType == WorldObjectType.CubeModelTerrainFineGrained ||
                         wo.WorldObjectType == WorldObjectType.CubeModelPrototypeTerrain;
        bool isStandardCube =
            wo.WorldObjectType == WorldObjectType.CubeModel && wo.Id != MVGameControllerBase.WOCM.RootGroup.Id;

        
        if (targetType == ModelType.CubeGun)
        {
            if (!isTerrain)
                return null;
        }
        else if (targetType == ModelType.Avatar)
        {
            if (!isStandardCube || !IsAvatarObj(wo))
                return null;
        }
        else if (targetType == ModelType.Standard)
        {
            if (lastCube != null && wo.Id == lastCube.Id)
                return null;
            if (!isStandardCube || IsAvatarObj(wo))
                return null;
        }
        else if (targetType == ModelType.All)
        {
            if (!isStandardCube && !isTerrain)
                return null;
        }

        var mi = wo.TryCast<MVCubeModelBase>();
        if (mi == null || mi.PrototypeCubeModel == null)
            return null;

        var d = new JsonModelData();
        d.Pos = new float[] { wo.Position.x, wo.Position.y, wo.Position.z };
        d.Rot = new float[] { wo.Rotation.x, wo.Rotation.y, wo.Rotation.z, wo.Rotation.w };
        Vector3 s = wo.transform.localScale;
        d.Scale = new float[] { s.x, s.y, s.z };

        if (mi.PrototypeCubeModel.Chunks != null)
        {
            foreach (var ce in mi.PrototypeCubeModel.Chunks)
            {
                var ch = ce.Value;
                if (ch.cells == null)
                    continue;

                foreach (var cle in ch.cells)
                {
                    IntVector pos = cle.Key;
                    Cube cb = cle.Value.cube;

                    if (cb != null && cb.FaceMaterials != null && cb.FaceMaterials.Length > 0)
                    {
                        byte[] ca = (cb.Corners != null) ? CubeDataPacker.CornersToByteArray(cb.Corners)
                                                         : CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);

                        d.Cubes.Add(
                            new JsonCube { x = pos.x, y = pos.y, z = pos.z, mat = cb.FaceMaterials[0], corners = ca });
                    }
                }
            }
        }

        return d.Cubes.Count > 0 ? d : null;
    }

    private static void Save(List<JsonModelData> d, string f)
    {
        if (!Directory.Exists(ExportPath))
            Directory.CreateDirectory(ExportPath);
        File.WriteAllText(Path.Combine(ExportPath, f), JsonConvert.SerializeObject(d, Formatting.Indented));
}

    public static void StartImport(string f, int del)
    {
        if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
return;
        }
        if (CapturedModelID == -1)
        {
return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(ImportRoutine(f));
    }

    private static IEnumerator ImportRoutine(string p)
    {
        if (!File.Exists(p))
        {
yield break;
        }

        var l = JsonConvert.DeserializeObject<List<JsonModelData>>(File.ReadAllText(p));
foreach (var md in l)
        {
            Vector3 sp = MVGameControllerBase.MainCameraManager.MainCamera.transform.position +
                         MVGameControllerBase.MainCameraManager.MainCamera.transform.forward * 10f;
            if (md.Pos != null && md.Pos.Length >= 3)
                sp = new Vector3(md.Pos[0], md.Pos[1], md.Pos[2]);

            Quaternion sr = Quaternion.identity;
            if (md.Rot != null && md.Rot.Length >= 4)
                sr = new Quaternion(md.Rot[0], md.Rot[1], md.Rot[2], md.Rot[3]);

            int cid = -1;
            yield return WorldObjectOperations.CloneObject(CapturedModelID, sp, sr,
                                                           (id) =>
                                                           { cid = id; });

            if (cid == -1)
                continue;

            yield return new WaitForSeconds(0.5f);

            var nm = MVGameControllerBase.WOCM.GetWorldObjectClient(cid)?.TryCast<MVCubeModelInstance>();
            if (nm == null)
            {
                yield return new WaitForSeconds(0.5f);
                nm = MVGameControllerBase.WOCM.GetWorldObjectClient(cid)?.TryCast<MVCubeModelInstance>();
            }

            if (nm == null)
            {
continue;
            }

            if (md.Scale != null && md.Scale.Length >= 3)
                nm.transform.localScale = new Vector3(md.Scale[0], md.Scale[1], md.Scale[2]);

            nm.MakeUnique();
            yield return new WaitForSeconds(0.2f);

            var rm = new List<IntVector>();
            if (nm.PrototypeCubeModel?.Chunks != null)
            {
                foreach (var ch in nm.PrototypeCubeModel.Chunks.Values)
                    if (ch.cells != null)
                        foreach (var k in ch.cells.Keys)
                            rm.Add(k);
            }

            if (rm.Count > 0)
            {
                int b = 0;
                foreach (var r in rm)
                {
                    nm.RemoveCube(r);
                    b++;
                    if (b >= B_SZ)
                    {
                        nm.HandleDelta();
                        yield return new WaitForSeconds(B_DEL);
                        b = 0;
                    }
                }
                nm.HandleDelta();
                yield return new WaitForSeconds(B_DEL);
            }

            int ab = 0;
            foreach (var c in md.Cubes)
            {
                var pos = new IntVector((short)c.x, (short)c.y, (short)c.z);
                byte m = c.mat;
                byte[] ms = new byte[] { m, m, m, m, m, m };
                ms[1] = 21;

                byte[] cr = (c.corners != null && c.corners.Length == 8)
                                ? c.corners
                                : CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);

                nm.AddCube(pos, new Cube(cr, ms));

                ab++;
                if (ab >= B_SZ)
                {
                    nm.HandleDelta();
                    yield return new WaitForSeconds(B_DEL);
                    ab = 0;
                }
            }
            nm.HandleDelta();
            yield return new WaitForSeconds(1.0f);
        }
}
}

public class JsonModelData
{
    public float[] Pos;
    public float[] Rot;
    public float[] Scale;
    public List<JsonCube> Cubes = new List<JsonCube>();
}

public class JsonCube
{
    public int x, y, z;
    public byte mat;
    public byte[] corners;
}
}

--- FILE: Features\FixHealRay.cs ---
﻿using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace TestMod.Fixes
{
    public static class HealRayExploitFix
    {
        
        
        
        [HarmonyPatch(typeof(PickUpItemHealRay), nameof(PickUpItemHealRay.TriggerEnd))]
        public static class TriggerEndPatch
        {
            
            [HarmonyPrefix]
            private static bool Prefix(PickUpItemHealRay __instance)
            {
                if (__instance == null) return false;

                
                
                
                try
                {
                    if (__instance.audioSource == null || __instance.rayParticles == null)
                    {
                        
                        return false;
                    }
                }
                catch
                {
                    
                    return false;
                }

                return true;
            }

            
            
            
            [HarmonyFinalizer]
            private static Exception Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
return null; 
                }
                return null;
            }
        }

        
        
        
        [HarmonyPatch(typeof(PickUpItemHealRay), nameof(PickUpItemHealRay.OnStateChanged))]
        public static class OnStateChangedPatch
        {
            [HarmonyFinalizer]
            private static Exception Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
return null; 
                }
                return null;
            }
        }
    }
}
--- FILE: Features\ForceAdminMenu.cs ---
﻿using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Il2Cpp;
using System.Reflection;

namespace TestMod.Features
{
    public static class ForceAdminMenu
    {
        [HarmonyPatch(typeof(PlayerSocialPopup), nameof(PlayerSocialPopup.Initialize))]
        public static class AdminButtonVisiblityPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerSocialPopup __instance)
            {
                try
                {
                    if (__instance.manageUserButton != null)
                    {
                        __instance.manageUserButton.gameObject.SetActive(true);
                    }
                }
                catch { }
            }
        }
        [HarmonyPatch(typeof(PlayerSocialPopup), nameof(PlayerSocialPopup.OnOpenAdminController))]
        public static class BypassAdminCheckPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(PlayerSocialPopup __instance)
            {
try
                {
                    var adminPrefab = __instance.adminToolsPrefab;
                    if (adminPrefab == null) return false;
                    GameObject adminObj = Object.Instantiate(adminPrefab.gameObject);
                    var adminToolInstance = adminObj.GetComponent<AdminToolController>();

                    if (adminToolInstance == null)
                    {
return false;
                    }
                    string targetPlayerName = "bCy53G8qJGup";
                    if (__instance.playerName != null)
                    {
                        targetPlayerName = __instance.playerName.text;
                    }
                    adminToolInstance.Initialize(targetPlayerName);
                    UIStack uiStack = GetUIStackSafe();

                    if (uiStack != null)
                    {
                        uiStack.Push(adminObj, UIPushOption.Blocking, null, UIGroupFlags.GameObjectUI);
                    }
                    else
                    {
if (__instance.transform.parent != null)
                        {
                            adminObj.transform.SetParent(__instance.transform.parent, false);
                        }
                        var rect = adminObj.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = Vector2.zero;
                            rect.localScale = Vector3.one;
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                        }

                        adminObj.SetActive(true);
                        adminObj.transform.SetAsLastSibling();
                    }
}
                catch (System.Exception e)
                {
}

                return false;
            }
        }
        private static UIStack GetUIStackSafe()
        {
            try
            {
                var playController = MVGameControllerBase.PlayModeUI;
                if (playController != null)
                {
                    var type = playController.GetType();
                    while (type != null)
                    {
                        var field = type.GetField("6IBdJaTTJSkS", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (field != null) return field.GetValue(playController) as UIStack;
                        type = type.BaseType;
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
--- FILE: Features\Hacks.cs ---
﻿using System.Collections.Generic;
using UnityEngine;
using TestMod.Helpers;

namespace TestMod.Features
{
    public class ESPRenderData
    {
        public System.Numerics.Vector2 screenPos;
        public string name;
        public uint color;
    }

    public static class Hacks
    {
        
        public static bool speedOn = false;
        public static float speedMult = 2.0f;
        public static bool noFrict = false;

        
        public static bool espOn = false;
        public static bool namesOn = false;

        
        public static bool godMode = false;
        public static string chatMsg = "5O9Ttr8KFWLX"; 
        public static bool sendChatTrigger = false;

        
        private static readonly object _dataLock = new object();
        public static List<ESPRenderData> renderCache = new List<ESPRenderData>();

        
        public static void UpdateLogic()
        {
            if (speedOn) applySpeed();
            if (noFrict) applyFrict();
            if (espOn) updateEsp();

            
            if (godMode) NetworkUtils.SetGodMode(true);

            
            if (sendChatTrigger)
            {
                if (!string.IsNullOrEmpty(chatMsg))
                {
                    NetworkUtils.SendChat(chatMsg);
                    chatMsg = "iWsSNEbwfttv"; 
                }
                sendChatTrigger = false;
            }
        }

        static void applySpeed()
        {
            if (GameInstances.LocalMotor != null)
                GameInstances.LocalMotor.speedBoostSetting = speedMult;
        }

        static void applyFrict()
        {
            if (GameInstances.LocalMotor != null)
                GameInstances.LocalMotor.frictionMultiplier = 0f;
        }

        static void updateEsp()
        {
            if (Camera.main == null) return;

            var tempList = new List<ESPRenderData>();
            float h = Screen.height;

            
            var enemies = PlayerUtils.GetEnemies();

            foreach (var p in enemies)
            {
                var woc = PlayerUtils.GetWOC(p);
                if (woc == null) continue;

                
                
                Vector3 pos = woc.Transform.position + Vector3.up * 2f;

                Vector3 sPos = Camera.main.WorldToScreenPoint(pos);

                if (sPos.z > 0)
                {
                    tempList.Add(new ESPRenderData
                    {
                        screenPos = new System.Numerics.Vector2(sPos.x, h - sPos.y),
                        name = "NJ6ShcFlTkyK" + p.ActorNr,
                        color = 0xFF0000FF 
                    });
                }
            }

            lock (_dataLock)
            {
                renderCache = tempList;
            }
        }

        
        public static void DrawOverlay()
        {
            if (!espOn) return;
            var draw = ImGuiNET.ImGui.GetBackgroundDrawList();

            lock (_dataLock)
            {
                foreach (var d in renderCache)
                {
                    draw.AddCircleFilled(d.screenPos, 4f, d.color);
                    if (namesOn) draw.AddText(d.screenPos, d.color, d.name);
                }
            }
        }
    }
}
--- FILE: Features\ImpulseGunCheats.cs ---
﻿using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2Cpp;

namespace TestMod.Features
{
    public static class ImpulseGunCheats
    {
        public static bool EnableImpulseMods = true;
        public static bool LegitMode = true;
        public static bool RainbowMode = true;
        private static float? _defaultHitImpulse;
        private static float? _defaultRecoilImpulse;
        private static AnimationCurve _defaultChargeCurve;
        private static Color? _defaultHitColor;
        private static Color? _defaultMissColor;

        [HarmonyPatch(typeof(PickupItemImpulseGun), "FaHlASi8gH47")]
        public static class Patch_ImpulseGun_TriggerBegin
        {
            [HarmonyPrefix]
            private static void Prefix(PickupItemImpulseGun __instance)
            {
                if (_defaultHitImpulse == null)
                {
                    _defaultHitImpulse = __instance.hitImpulse;
                    _defaultRecoilImpulse = __instance.recoilImpulse;
                    _defaultHitColor = __instance.hitColor;
                    _defaultMissColor = __instance.missColor;

                    if (__instance.chargeCurve != null)
                    {
                        _defaultChargeCurve = new AnimationCurve(__instance.chargeCurve.keys);
                    }
                }

                if (!EnableImpulseMods)
                {
                    RestoreDefaults(__instance);
                    return;
                }

                try
                {
                    if (LegitMode)
                    {
                        __instance.hitImpulse = _defaultHitImpulse.Value * 1.2f;
                        __instance.recoilImpulse = _defaultRecoilImpulse.Value * 1.2f;
                        if (_defaultChargeCurve != null && _defaultChargeCurve.keys.Count > 0)
                        {
                            Keyframe[] oldKeys = _defaultChargeCurve.keys;
                            Keyframe[] newKeys = new Keyframe[oldKeys.Length];

                            for (int i = 0; i < oldKeys.Length; i++)
                            {
                                Keyframe k = oldKeys[i];
                                Keyframe nk = new Keyframe(k.time / 2f, k.value);
                                nk.inTangent = k.inTangent;
                                nk.outTangent = k.outTangent;
                                nk.tangentMode = k.tangentMode;
                                newKeys[i] = nk;
                            }
                            __instance.chargeCurve = new AnimationCurve(newKeys);
                        }
                    }
                    else
                    {
                        __instance.hitImpulse = 10000f;
                        __instance.recoilImpulse = 5000f;
                        AnimationCurve instantCurve = new AnimationCurve();
                        instantCurve.AddKey(0f, 1f);
                        instantCurve.AddKey(1f, 1f);
                        __instance.chargeCurve = instantCurve;
                    }
                }
                catch (Exception e)
                {
}
            }
        }
        [HarmonyPatch(typeof(PickupItemImpulseGun), "wRILtBbPlvic")]
        public static class Patch_ImpulseGun_Update
        {
            [HarmonyPostfix]
            private static void Postfix(PickupItemImpulseGun __instance)
            {
                if (!EnableImpulseMods) return;

                if (RainbowMode)
                {
                    float hue = Mathf.Repeat(Time.time, 1f);
                    Color rainbow = Color.HSVToRGB(hue, 1f, 1f);
                    __instance.hitColor = rainbow;
                    __instance.missColor = rainbow;
                }
                else if (_defaultHitColor != null)
                {
                    __instance.hitColor = _defaultHitColor.Value;
                    __instance.missColor = _defaultMissColor.Value;
                }
            }
        }

        [HarmonyPatch(typeof(PickupItemImpulseGun), "w7yCDgwRQa5e")]
        public static class Patch_ImpulseGun_TriggerEnd
        {
            [HarmonyPrefix]
            private static void Prefix(PickupItemImpulseGun __instance)
            {
                if (!EnableImpulseMods || _defaultHitImpulse == null) return;
                if (LegitMode)
                {
                    __instance.hitImpulse = _defaultHitImpulse.Value * 1.2f;
                    __instance.recoilImpulse = _defaultRecoilImpulse.Value * 1.2f;
                }
                else
                {
                    __instance.hitImpulse = 10000f;
                    __instance.recoilImpulse = 5000f;
                }
            }
        }

        private static void RestoreDefaults(PickupItemImpulseGun gun)
        {
            if (_defaultHitImpulse.HasValue)
            {
                gun.hitImpulse = _defaultHitImpulse.Value;
                gun.recoilImpulse = _defaultRecoilImpulse.Value;
                gun.chargeCurve = _defaultChargeCurve;
                gun.hitColor = _defaultHitColor.Value;
                gun.missColor = _defaultMissColor.Value;
            }
        }
    }
}
--- FILE: Features\ItemExporter.cs ---
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using UnityEngine;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using Il2CppMV.WorldObject;
using Il2CppMV.Common;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;

namespace TestMod.Features
{
public class LogicArchitect
{
    private static string _exportName = "4PNMpJW8EQbF";
    private static string _status = "T3xhAVgfiiV4";
    private static float _importDelay = 2.0f;

    [System.Serializable]
    public class LogicDump
    {
        public List<LogicItemData> Items = new List<LogicItemData>();
    }

    [System.Serializable]
    public class LogicItemData
    {
        public int ItemID;
        public string Name;
        public float[] Pos;
        public float[] Rot;
        public float[] Scale;
    }

    public static void RenderUI()
    {
        ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), "UIaPTtCAcKhz");
        ImGui.Text($"V2DeVBNl0LWk");
        ImGui.Separator();

        ImGui.InputText("EHuakVLs6rOD", ref _exportName, 64);
        ImGui.SliderFloat("D05xW8rfWaM2", ref _importDelay, 0.5f, 5.0f);

        if (ImGui.Button("4pMe2ivFwyAW"))
        {
            if (MVGameControllerBase.GameMode != MVGameMode.Play)
                _status = "Iw3SXl2iOzVz";
            else
                MelonCoroutines.Start(ExportLogicRoutine());
        }

        if (ImGui.Button("NePcCFFFeHQr"))
        {
            if (MVGameControllerBase.GameMode != MVGameMode.Edit)
                _status = "JpVDyIcE3U5p";
            else
                MelonCoroutines.Start(ImportLogicRoutine());
        }
    }

    private static IEnumerator ExportLogicRoutine()
    {
        _status = "QLdjYu4GSKfT";
        yield return null;

        var game = MVGameControllerBase.Game;
        if (game == null)
        {
            _status = "W19A0L9vtpGX";
            yield break;
        }

        var wocm = game.WorldObjectClientManager;
        if (wocm == null)
        {
            _status = "Pb2478138TBO";
            yield break;
        }

        var objectDict = wocm.worldObjects;
        if (objectDict == null || objectDict.Count == 0)
        {
            _status = "DeUmUozqeo1o";
            yield break;
        }

        var dump = new LogicDump();
        int count = 0;
        _status = $"HXz8u8W2XdcJ";

        foreach (var wo in objectDict.Values)
        {
            if (wo == null)
                continue;
            bool isLogic = wo.TryCast<MVLogicObject>() != null;
            bool isPickup = wo.TryCast<MVPickupItemBase>() != null;
            

            

            int id = IdentifyItem(wo);
            if (id == 0)
                continue;

            var entry = new LogicItemData();
            entry.ItemID = id;
            entry.Name = wo.GetIl2CppType().Name;

            Vector3 pos = wo.Position;
            Quaternion rot = wo.Rotation;
            Vector3 scale = wo.Scale;

            entry.Pos = new float[] { pos.x, pos.y, pos.z };
            entry.Rot = new float[] { rot.x, rot.y, rot.z, rot.w };
            entry.Scale = new float[] { scale.x, scale.y, scale.z };

            dump.Items.Add(entry);
            count++;
        }

        string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "uulEeKQf4Paj", _exportName + "FXxnniCohRs9");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonConvert.SerializeObject(dump, Formatting.Indented));

        _status = $"LSqj26uRQEAv";
    }

    private static IEnumerator ImportLogicRoutine()
    {
        string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "ByYAxHzpllag", _exportName + "wZHE8dcYVRMe");
        if (!File.Exists(path))
        {
            _status = "PUe5dw4UVrog";
            yield break;
        }

        var dump = JsonConvert.DeserializeObject<LogicDump>(File.ReadAllText(path));
        var ops = MVGameControllerBase.Game.OperationRequestSender;
        var editUI = MVGameControllerBase.EditModeUI.TryCast<DesktopEditModeController>();
        if (editUI == null)
        {
            _status = "FDyavq8vpN4D";
            yield break;
        }
        int targetGroupID = editUI.EditModeStateMachine.ParentGroupID;
_status = $"p5FiQlYybldw";
        int processed = 0;

        foreach (var item in dump.Items)
        {
            if (item.Pos == null || item.Pos.Length < 3)
                continue;

            Vector3 pos = new Vector3(item.Pos[0], item.Pos[1], item.Pos[2]);

            Quaternion rot = Quaternion.identity;
            if (item.Rot != null && item.Rot.Length >= 4)
            {
                rot = new Quaternion(item.Rot[0], item.Rot[1], item.Rot[2], item.Rot[3]);
            }

            ops.AddItemToWorld(item.ItemID, targetGroupID, pos, rot, true, true, false);

            processed++;
            _status = $"GUbAqYFsoAi4";
            yield return new WaitForSeconds(_importDelay);
        }

        _status = "MPkvB4endcEM";
    }

    private static int IdentifyItem(MVWorldObjectClient wo)
    {
        if (wo.TryCast<MVSpawnPoint>() != null)
            return ItemIdWWW.RED_TEAM_SPAWN_POINT;
        if (wo.TryCast<MVCheckpoint>() != null)
            return ItemIdWWW.CHECKPOINT;
        if (wo.TryCast<MVFlag>() != null)
            return ItemIdWWW.FLAG;
        if (wo.TryCast<MVGameCoin>() != null)
            return ItemIdWWW.COIN;
        if (wo.TryCast<MVTimeTrigger>() != null)
            return ItemIdWWW.DELAY_CUBE;
        if (wo.TryCast<MVToggleBox>() != null)
            return ItemIdWWW.TOGGLE_CUBE;
        if (wo.TryCast<MVAnd>() != null)
            return ItemIdWWW.AND_CUBE;
        if (wo.TryCast<MVNegate>() != null)
            return ItemIdWWW.NEGATE_CUBE;
        if (wo.TryCast<MVTriggerBox>() != null)
            return ItemIdWWW.TRIGGER_CUBE;
        if (wo.TryCast<MVSmoke>() != null)
            return ItemIdWWW.SMOKE_CUBE;
        if (wo.TryCast<MVFire>() != null)
            return ItemIdWWW.FIRE;
        if (wo.TryCast<MVTextMsg>() != null)
            return ItemIdWWW.TEXT;
        if (wo.TryCast<MVSkybox>() != null)
            return ItemIdWWW.SKYBOX_CUBE;
        if (wo.TryCast<MVPressurePlate>() != null)
            return ItemIdWWW.PRESSURE_PLATE;
        if (wo.TryCast<MVCountingCube>() != null)
            return ItemIdWWW.COUNTING_CUBE;
        if (wo.TryCast<MVRandomBox>() != null)
            return ItemIdWWW.RANDOM_CUBE;
        if (wo.TryCast<UseLever>() != null)
            return ItemIdWWW.LEVER;
        if (wo.TryCast<PickupItemShotgun>() != null)
            return ItemIdWWW.SHOTGUN;
        if (wo.TryCast<PickupItemRailGun>() != null)
            return ItemIdWWW.RAIL_GUN;
        if (wo.TryCast<PickupItemImpulseGun>() != null)
            return ItemIdWWW.IMPULSE_GUN;
        if (wo.TryCast<PickupItemBazooka>() != null)
            return ItemIdWWW.BAZOOKA;
        if (wo.TryCast<PickupItemFlamethrower>() != null)
            return ItemIdWWW.FLAMETHROWER;
        if (wo.TryCast<PickUpItemHealRay>() != null)
            return ItemIdWWW.HEAL_RAY;
        if (wo.TryCast<PickupItemCubeGun>() != null)
            return ItemIdWWW.CUBE_GUN;
        if (wo.TryCast<PickupItemGrowthGun>() != null)
            return ItemIdWWW.GROWTH_GUN;
        if (wo.TryCast<PickupItemMouseGun>() != null)
            return ItemIdWWW.MOUSE_GUN;
        if (wo.TryCast<PickupItemMeleeWeapon>() != null)
            return ItemIdWWW.SWORD;
        if (wo.TryCast<MVHoverCraft>() != null)
            return ItemIdWWW.HOVERCART;
        if (wo.TryCast<MVJetPack>() != null)
            return ItemIdWWW.FIREFLY_JETPACK;
        if (wo.TryCast<MVTeleporter>() != null)
            return ItemIdWWW.TELEPORTER;
        if (wo.TryCast<MVSentryGun>() != null)
            return ItemIdWWW.FIRE_SENTRY_TOWER;

        return 0;
    }
}
}

--- FILE: Features\ItemIdWWW.cs ---
﻿namespace TestMod.Features
{
    internal class ItemIdWWW
    {
        
        public const int BAZOOKA = 10355;
        public const int MACHINE_GUN = 10353;
        public const int SHOTGUN = 10360;
        public const int IMPULSE_GUN = 10354;
        public const int FLAMETHROWER = 10359;
        public const int HEAL_RAY = 7690141;
        public const int RAIL_GUN = 10356;
        public const int REVOLVER = 239796;
        public const int DUAL_REVOLVERS = 239800;
        public const int SHURIKEN = 1165835;
        public const int MULTI_SHURIKEN = 1165838;
        public const int SWORD = 12435368;
        public const int CUBE_GUN = 46140;
        public const int HEALTH_PACK = 10352;
        public const int MUTANTO = 10358;
        public const int LIGHTNING_SPEED = 1165831;
        public const int STAR = 91197;
        public const int COIN = 1812739;
        public const int COIN_CHEST = 1812737;
        public const int MOUSE_GUN = 3785736;
        public const int GROWTH_GUN = 3785733;
        public const int MOUSE_PILL = 3785734;
        public const int GROWTH_PILL = 3785732;
        public const int CRYSTAL = 8880186;
        public const int CRYSTAL_VEIN = 8880187;
        public const int COSTUME = 12496365;
        public const int CUSTOM_GUN = 12730220;

        
        public const int TELEPORTER = 13481;
        public const int PLATFORM = 19674;
        public const int VERTICAL_ROTATOR = 20757;
        public const int HORIZONTAL_ROTATOR = 20756;
        public const int FIREFLY_JETPACK = 75226;
        public const int DRAGONFLY_JETPACK = 75227;
        public const int HOVERCART = 61222;
        public const int VEHICLE_ENERGY = 12226728;
        public const int HAMSTER_BALL = 349297;
        public const int OCULUS = 97157;
        public const int GHOST = 17789;
        public const int FIRE_SENTRY_TOWER = 16408;
        public const int FROST_SENTRY_TOWER = 18688;
        public const int CLASS = 9272265;
        public const int COLLECT_AND_DROP = 6225654;
        public const int DOOR = 12305637;
        public const int SLIDING_DOOR = 12324723;
        public const int TRAP_DOOR = 12339395;

        
        public const int ROUND_TIME = 91196;
        public const int WATER_CUBE = 10377;
        public const int SMOKE_CUBE = 10373;
        public const int TEXT = 10374;
        public const int FIRE = 10372;
        public const int CAMERA_CUBE = 2379699;
        public const int TEAM_EDITOR = 9016173;
        public const int SKYBOX_CUBE = 10375;
        public const int TNT = 10371;
        public const int LIGHT = 10350;
        public const int ELIMINATE_THE_OCULUS = 1165828;
        public const int DEATHMATCH = 1165823;
        public const int TIME_ATTACK_FLAG = 8250560;
        public const int CHECKPOINT = 54338;
        public const int FLAG = 10365;
        public const int SPEAKER = 10380;
        public const int GLOBAL_SPEAKER = 7690139;
        public const int WIND_TURBINE = 3785730;
        public const int RED_TEAM_SPAWN_POINT = 10365;
        public const int YELLOW_TEAM_SPAWN_POINT = 10364;
        public const int BLUE_TEAM_SPAWN_POINT = 10361;
        public const int GREEN_TEAM_SPAWN_POINT = 10363;

        
        public const int TARGET_CUBE = 4113945;
        public const int COUNTING_CUBE = 5435011;
        public const int DELAY_CUBE = 10366;
        public const int NEGATE_CUBE = 10368;
        public const int PULSE_CUBE = 10378;
        public const int TOGGLE_CUBE = 10367;
        public const int AND_CUBE = 10369;
        public const int TRIGGER_CUBE = 9016174;
        public const int CUBE_MODEL_HIDER = 10376;
        public const int CUBE_MODEL_TRANSPARENCY = 12532422;
        public const int RANDOM_CUBE = 10379;
        public const int LEVER = 4113944;
        public const int PRESSURE_PLATE = 10370;

        
        public const int MODEL = 75579;
        public const int GROUP = 10348;
    }
}

--- FILE: Features\KillAllFeature.cs ---
﻿using Il2Cpp;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using Il2CppMV.WorldObject.RuntimeEvents;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TestMod.Features
{
public static class KillAllFeature
{
    private static int kIdx = 0;
    private static int tMode = 0;
    private static int pIdx = 0;
    private static string[] pNames = new string[0];
    private static int[] pIds = new int[0];
    private static readonly string[] kList = { "X8HvR7Ks5zMe", "csiRtY0lYdYL", "hscOubmUHOuE",     "9o13Ys9W9Us6",
                                               "8g2lNwl0jfTh",        "oKAcWk5PAGe3",   "6Evdz1Ln9xo7",    "8xboYJehVxa3",
                                               "5HynU2nEuskZ",    "lDTyfknY0mHg",      "bEgqG01zDbFa", "fP2qfUlEfM4g",
                                               "ZgCovIHbT74W",   "qL3j9xRGTM9i",    "QmNYBTVOxjC1",    "eDp44gOtjIm3",
                                               "51YqkQsMp4dU",          "XfEiznVULbNo",    "ixTzMea1wtIy" };

    public static void draw()
    {
        ImGui.Text("J6aHx1o4Udim");
        ImGui.Combo("JtN7I2fplNYA", ref kIdx, kList, kList.Length);

        ImGui.Text("suWCt7ojcMKR");
        ImGui.SameLine();
        if (ImGui.RadioButton("N3UEEEYSlRv0", tMode == 0))
            tMode = 0;
        ImGui.SameLine();
        if (ImGui.RadioButton("zxffLcBM7o0o", tMode == 1))
            tMode = 1;

        if (tMode == 1)
        {
            if (ImGui.Button("w64Q8OGBfWWW"))
                refresh();

            if (pNames.Length > 0)
            {
                ImGui.Combo("LS6qBGv6ANE1", ref pIdx, pNames, pNames.Length);
            }
            else
            {
                ImGui.Text("0OcwxhykoPBR");
            }
        }

        ImGui.Separator();
        if (ImGui.Button("32z84c0gHPCt"))
            run();
    }

    static void refresh()
    {
        var enemies = getEnemies();
        pNames = enemies.Select(x => $"wkkXuDWI6Gx6"Unknown"NAU2NSnsVnac").ToArray();
        pIds = enemies.Select(x => x.ActorNr).ToArray();

        if (pIdx >= pNames.Length)
            pIdx = 0;
    }

    static void run()
    {
        var targets = new List<MVPlayer>();

        if (tMode == 0)
        {
            targets = getEnemies();
        }
        else
        {
            if (pIds.Length == 0)
                return;
            var pid = pIds[pIdx];
            var all = getAll();
            var p = all.FirstOrDefault(x => x.ActorNr == pid);
            if (p != null)
                targets.Add(p);
        }
        foreach (var t in targets)
        {
            send(t);
        }
    }

    static void send(MVPlayer targetPlayer)
    {
        if (targetPlayer == null)
            return;
        var localPlayer = MVGameControllerBase.Game.LocalPlayer;
        if (localPlayer == null || localPlayer.AvatarLocal == null)
            return;

        
        MVWorldObjectClient targetWO = MVGameControllerBase.WOCM.GetWorldObjectClient(targetPlayer.WoId);
        if (targetWO == null)
            return;

        var handler = targetWO.InteractionDataHandlerBase;
        if (handler == null)
            return;

        
        Vector3 myPos = localPlayer.AvatarLocal.Transform.position;
        Vector3 targetPos = targetWO.GetTargetPosition();
        Vector3 direction = (targetPos - myPos).normalized;
        if (direction == Vector3.zero)
            direction = Vector3.up;

        float damage = 1000f;
        float impulseForce = 15500f;
        Vector3 impulseVector = direction * impulseForce;

        
        
        
        if (kIdx == 1) 
        {
            if (MVGameControllerBase.Game.World.RuntimeEventManager != null)
            {
                
                ExplosionEvent explosion = new ExplosionEvent(RuntimeEventType.Bazooka, targetPos, Vector3.up);
                MVGameControllerBase.Game.World.RuntimeEventManager.SendRuntimeEvent(explosion);
            }
        }

        InteractionData killPacket = default(InteractionData);
        bool valid = true;

        
        switch (kIdx)
        {
        case 0:
            killPacket = AdvancedGhostBodyRotateWeaponPackage.Create(damage, impulseVector);
            break;
        case 1: 
            killPacket = ProximityDamageAndImpulse.Create(damage, impulseVector, PlayerKilledByType.Explosive);
            break;
        case 2:
            killPacket = RailgunHitPackage.Create();
            break;
        case 3:
            killPacket = MeleeWeaponHitPackage.Create(1000f, impulseVector);
            break;
        case 4:
            killPacket = ShotgunHitPackage.Create(impulseVector);
            break;
        case 5:
            killPacket = ImpulseHitPackage.Create(impulseVector);
            break;
        case 6:
            killPacket = SixShooterHitPackage.Create(impulseVector);
            break;
        case 7:
            killPacket = DoubleSixShooterHitPackage.Create(impulseVector);
            break;
        case 8:
            killPacket = CenterGunHitPackage.Create(impulseVector);
            break;
        case 9:
            killPacket = SlapGunHitPackage.Create(impulseVector);
            break;
        case 10:
            killPacket = SentryTowerFirePackage.Create(impulseVector);
            break;
        case 11:
            killPacket = SentryTowerIcePackage.Create(impulseVector);
            break;
        case 12:
            killPacket = FlamethrowerHitPackage.Create();
            break;
        case 13:
            killPacket = MutantHitPackage.Create();
            break;
        case 14:
            killPacket = ThrowingStarHitPackage.Create();
            break;
        case 15:
            killPacket = MultiThrowingStarHitPackage.Create();
            break;
        case 16:
            killPacket = MouseGunHitPackage.Create();
            break;
        case 17:
            killPacket = GrowthGunHitPackage.Create();
            break;
        case 18:
            killPacket = HealRayHitPackage.Create();
            break;
        default:
            valid = false;
            break;
        }

        
        if (valid)
        {
            var myPickupOwner = localPlayer.AvatarLocal.PickupOwner;
            handler.HandleInteraction(myPickupOwner, killPacket, false);
        }
    }
    static List<MVPlayer> getAll()
    {
        var list = new List<MVPlayer>();
        var container = MVGameControllerBase.Game?.MVPlayerContainer;

        if (container != null)
        {
            foreach (var p in container.Values)
            {
                if (p != null)
                    list.Add(p);
            }
        }
        return list;
    }
    static List<MVPlayer> getEnemies()
    {
        var list = new List<MVPlayer>();
        var me = MVGameControllerBase.Game?.LocalPlayer;
        var all = getAll();

        foreach (var p in all)
        {
            if (me != null && p.ActorNr == me.ActorNr)
                continue;
            list.Add(p);
        }
        return list;
    }
}

}

--- FILE: Features\KillTest.cs ---
﻿using Il2Cpp;
using Il2CppAssets.Scripts.Pickups;
using Il2CppCodeStage.AntiCheat.ObscuredTypes;
using Il2CppInterop.Runtime;
using Il2CppMV.Common;
using Il2CppRTG;
using System;
using System.Collections.Generic;
using System.Reflection;
using TestMod.Helpers;
using UnityEngine;

namespace TestMod.Features
{
    public static class UniversalKillFeature
    {
        private static IntPtr _customGunHitPtr = IntPtr.Zero;
        private static IntPtr _centerGunHitPtr = IntPtr.Zero;
        private static IntPtr _railGunHitPtr = IntPtr.Zero;
        private static IntPtr _bazookaHitPtr = IntPtr.Zero;
        private static IntPtr _shotgunHitPtr = IntPtr.Zero;
        private static IntPtr _sixShooterHitPtr = IntPtr.Zero;

        public static void RenderUI()
        {
            ImGuiNET.ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "jpwpNXaZ5t5h");
            ImGuiNET.ImGui.Separator();

            if (ImGuiNET.ImGui.Button("6mjbllDrUMj2"))
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    ExecuteOmniKill();
                });
            }
            ImGuiNET.ImGui.TextDisabled("QP2zMI9eUA3p");
            ImGuiNET.ImGui.TextDisabled("OnMH66gxjkQo");
        }

        private static unsafe void ExecuteOmniKill()
        {
            if (MVGameControllerBase.Game == null || MVGameControllerBase.LocalPlayer == null) return;

            var localPlayer = MVGameControllerBase.LocalPlayer;
            var pickupOwner = localPlayer.AvatarLocal.pickupOwner;

            if (pickupOwner == null || pickupOwner.currentItem == null)
            {
return;
            }

            PickupItem weapon = pickupOwner.currentItem;
            AvatarItemType type = weapon.Type;
            float killDamage = 100f;
            float killImpulse = 100f;

            int kills = 0;
            var playerContainer = MVGameControllerBase.Game.MVPlayerContainer;
            ResolvePointers();

            foreach (MVPlayer target in playerContainer.ActivePlayers)
            {
                if (target.Equals(localPlayer) || !target.IsPlayerStateInWorld) continue;

                MVWorldObjectClient targetWO = MVGameControllerBase.WOCM.GetWorldObjectClient(target.WoId);
                if (targetWO == null) continue;
                VoxelHit fakeHit = new VoxelHit();
                fakeHit.woId = target.WoId;
                fakeHit.point = targetWO.transform.position + new Vector3(0, 1.3f, 0);
                fakeHit.normal = (localPlayer.AvatarLocal.transform.position - targetWO.transform.position).normalized;
                Ray fakeRay = new Ray(
                    localPlayer.AvatarLocal.transform.position + new Vector3(0, 1.5f, 0),
                    (targetWO.transform.position - localPlayer.AvatarLocal.transform.position).normalized
                );

                bool success = false;
                if (type == AvatarItemType.MeleeWeapon)
                {
                    var sword = weapon.TryCast<PickupItemMeleeWeapon>();
                    if (sword != null)
                    {
                        float oldDmg = sword.Configuration.damage;
                        float oldImp = sword.Configuration.impulseStrength;

                        sword.Configuration.damage = killDamage;
                        sword.Configuration.impulseStrength = killImpulse;

                        var hitList = new Il2CppSystem.Collections.Generic.List<VoxelHit>();
                        hitList.Add(fakeHit);
                        sword.OnLocalHit(hitList);

                        sword.Configuration.damage = oldDmg;
                        sword.Configuration.impulseStrength = oldImp;
                        success = true;
                    }
                }
                else if (type == AvatarItemType.CustomGun)
                {
                    var gun = weapon.TryCast<PickupItemCustomGun>();
                    if (gun != null)
                    {
                        var config = gun.Configuration;
                        float oldDmg = config.damage;
                        float oldImp = config.impulseStrength;

                        config.damage = killDamage;
                        config.impulseStrength = killImpulse;

                        InvokeHitMethod(_customGunHitPtr, gun, fakeHit, fakeRay);

                        config.damage = oldDmg;
                        config.impulseStrength = oldImp;
                        success = true;
                    }
                }
                else if (type == AvatarItemType.Bazooka)
                {
                    var bazooka = weapon.TryCast<PickupItemBazooka>();
                    if (bazooka != null)
                    {
                        ObscuredFloat oldDmg = bazooka.baseDamage;
                        float oldImp = bazooka.baseImpulse;
                        bazooka.baseDamage = killDamage;
                        bazooka.baseImpulse = killImpulse;

                        InvokeHitMethod(_bazookaHitPtr, bazooka, fakeHit, fakeRay);
                        bazooka.baseDamage = oldDmg;
                        bazooka.baseImpulse = oldImp;
                        success = true;
                    }
                }
                else if (type == AvatarItemType.RailGun)
                {
                    var rail = weapon.TryCast<PickupItemRailGun>();
                    if (rail != null)
                    {
                        ObscuredFloat oldDmg = rail.baseDamage;
                        rail.baseDamage = killDamage;

                        InvokeHitMethod(_railGunHitPtr, rail, fakeHit, fakeRay);

                        rail.baseDamage = oldDmg;
                        success = true;
                    }
                }
                else if (type == AvatarItemType.CenterGun)
                {
                    var center = weapon.TryCast<PickupItemCenterGun>();
                    if (center != null)
                    {
                        float oldDmg = PickupItemCenterGun.damage;
                        float oldImp = center.impulseStrength;

                        PickupItemCenterGun.damage = killDamage;
                        center.impulseStrength = killImpulse;

                        InvokeHitMethod(_centerGunHitPtr, center, fakeHit, fakeRay);

                        PickupItemCenterGun.damage = oldDmg;
                        center.impulseStrength = oldImp;
                        success = true;
                    }
                }
                else if (type == AvatarItemType.Shotgun)
                {
                    var shotty = weapon.TryCast<PickupItemShotgun>();
                    if (shotty != null)
                    {
                        float oldDmg = PickupItemShotgun.hitDamage;
                        float oldImp = shotty.impulseStrength;

                        PickupItemShotgun.hitDamage = killDamage;
                        shotty.impulseStrength = killImpulse;

                        InvokeHitMethod(_shotgunHitPtr, shotty, fakeHit, fakeRay);

                        PickupItemShotgun.hitDamage = oldDmg;
                        shotty.impulseStrength = oldImp;
                        success = true;
                    }
                }

                if (success) kills++;
            }
}
        private static unsafe void InvokeHitMethod(IntPtr methodPtr, UnityEngine.Object instance, VoxelHit hit, Ray ray)
        {
            if (methodPtr == IntPtr.Zero || instance == null) return;

            void** args = stackalloc void*[2];
            args[0] = (void*)IL2CPP.il2cpp_object_unbox(IL2CPP.Il2CppObjectBaseToPtrNotNull(hit));
            args[1] = &ray;

            IntPtr exc = IntPtr.Zero;
            IL2CPP.il2cpp_runtime_invoke(
                methodPtr,
                IL2CPP.Il2CppObjectBaseToPtrNotNull(instance),
                args,
                ref exc
            );
            Il2CppException.RaiseExceptionIfNecessary(exc);
        }

        private static void ResolvePointers()
        {
            if (_customGunHitPtr != IntPtr.Zero) return;
            string voxelHitType = "4ZyFyq2T68k0";
            string rayType = "ts7DhQPTUjvf";
            string[] paramTypes = new string[] { voxelHitType, rayType };
            _customGunHitPtr = IL2CPP.GetIl2CppMethod(
            Il2CppClassPointerStore<PickupItemCustomGun>.NativeClassPtr,
            false,
            "nJn4Urswikpn",
            "9yumQwqZSqPA",
            paramTypes);
            _centerGunHitPtr = IL2CPP.GetIl2CppMethod(
            Il2CppClassPointerStore<PickupItemCenterGun>.NativeClassPtr,
            false,
            "94VfqYNMCTtl",
            "HsbzX7cKUpp2",
            paramTypes);
            _bazookaHitPtr = IL2CPP.GetIl2CppMethod(
            Il2CppClassPointerStore<PickupItemBazooka>.NativeClassPtr,
            false,
            "BaGUvtXZphR9",
            "fkb6J3viJYUA",
            paramTypes);
            _shotgunHitPtr = IL2CPP.GetIl2CppMethod(
            Il2CppClassPointerStore<PickupItemShotgun>.NativeClassPtr,
            false,
            "2Y0nucvpeqx9",
            "MA80Hg7clvKb",
            paramTypes);
        }
    }
}
--- FILE: Features\LogicManager.cs ---
﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppMV.WorldObject;
using Il2CppMV.Common;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;
using ImGuiNET;

using SCG = System.Collections.Generic;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;

namespace TestMod.Features
{
    public class LogicManager : MonoBehaviour
    {
        public static LogicManager Instance { get; private set; }
        public LogicManager(IntPtr ptr) : base(ptr) { }

        private readonly string exPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "ARLeW2TGLY0Z");

        private void Awake()
        {
            Instance = this;
            try { Directory.CreateDirectory(exPath); } catch { }
            updFiles();
        }

        private string _fName = "4eTkFy9JfcjT";
        private string _msg = "ygfTmsh5nBCS";
        private string[] _files = new string[0];
        private int _idx = 0;

        public void DrawImGuiMenu()
        {
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "TWuz3IKLd88u");
            ImGui.Separator();

            ImGui.Text("yvGH0cSkwUtu");
            if (MVGameControllerBase.GameMode != MVGameMode.Play)
            {
                ImGui.TextDisabled("OV6ccoAAiOlq");
            }
            else
            {
                ImGui.InputText("5iibHeZRsVvl", ref _fName, 64);
                if (ImGui.Button("kV5bYMy4Zlcb"))
                {
                    string path = Path.Combine(exPath, _fName + "OdTwubVt3PqD");
                    ExportLogic(path);
                    _msg = $"D5qF5fYY7IkF";
                    updFiles();
                }
            }

            ImGui.Separator();

            ImGui.Text("n9YBNCuPuO3i");
            if (MVGameControllerBase.GameMode != MVGameMode.Edit)
            {
                ImGui.TextDisabled("wcF5nM7Kli2o");
            }
            else
            {
                if (ImGui.Button("WKgPwvGRxWB4")) updFiles();

                if (_files.Length > 0)
                {
                    ImGui.Combo("aYFHEC90ezBz", ref _idx, _files, _files.Length);

                    if (ImGui.Button("mgCUmakpC5T5"))
                    {
                        if (_idx >= 0 && _idx < _files.Length)
                        {
                            string path = Path.Combine(exPath, _files[_idx]);
                            MelonCoroutines.Start(ImportLogicRoutine(path));
                            _msg = "tRGuYcIhKsGb";
                        }
                    }
                }
                else
                {
                    ImGui.Text("pzcNg8BfSyy3");
                }
            }

            ImGui.Separator();
            ImGui.TextWrapped($"GALXDqiw058x");
        }

        private void updFiles()
        {
            if (Directory.Exists(exPath))
            {
                _files = Directory.GetFiles(exPath, "QkfPNsfoiRmU").Select(Path.GetFileName).ToArray();
            }
            else
            {
                _files = new string[0];
            }
        }

        [Serializable]
        public class WorldDump
        {
            public SCG.List<LogicItemData> Items = new SCG.List<LogicItemData>();
            public SCG.List<LinkData> Links = new SCG.List<LinkData>();
            public SCG.List<ObjectLinkData> ObjectLinks = new SCG.List<ObjectLinkData>();
        }

        [Serializable]
        public class LogicItemData
        {
            public int OriginalID;
            public int WorldObjectTypeInt;
            public float[] Position;
            public float[] Rotation;
            public float[] Scale;
            public SCG.Dictionary<string, object> SettingsData;
        }

        [Serializable] public class LinkData { public int OutputID; public int InputID; }
        [Serializable] public class ObjectLinkData { public int ConnectorID; public int TargetID; }

        public void ExportLogic(string filePath)
        {
            try
            {
                WorldDump dump = new WorldDump();
                SCG.HashSet<int> vids = new SCG.HashSet<int>();
                var wos = MVGameControllerBase.WOCM.worldObjects;
                SCG.List<int> allIds = new SCG.List<int>();
                foreach (var k in wos.Keys) allIds.Add(k);

                foreach (int id in allIds)
                {
                    MVWorldObjectClient wo = wos[id];
                    if (wo == null || wo.Pointer == IntPtr.Zero) continue;

                    if (wo.HasInteractionFlag(InteractionFlags.IsTerrain) ||
                        wo.WorldObjectType == WorldObjectType.PlayModeAvatar ||
                        wo.WorldObjectType == WorldObjectType.AvatarSpawnRoleCreator ||
                        wo is MVCubeModelBase) continue;

                    LogicItemData it = new LogicItemData();
                    it.OriginalID = wo.Id;
                    it.WorldObjectTypeInt = (int)wo.WorldObjectType;
                    it.Position = new float[] { wo.Position.x, wo.Position.y, wo.Position.z };
                    it.Rotation = new float[] { wo.Rotation.x, wo.Rotation.y, wo.Rotation.z, wo.Rotation.w };
                    it.Scale = new float[] { wo.Scale.x, wo.Scale.y, wo.Scale.z };
                    it.SettingsData = SanitizeData(wo.Data);

                    dump.Items.Add(it);
                    vids.Add(wo.Id);
                }

                foreach (var it in dump.Items)
                {
                    MVWorldObjectClient wo = MVGameControllerBase.WOCM.GetWorldObjectClient(it.OriginalID);
                    if (wo == null || wo.Pointer == IntPtr.Zero) continue;

                    if (wo.OutputLinkRefs != null)
                    {
                        foreach (Link link in wo.OutputLinkRefs)
                        {
                            if (vids.Contains(link.inputWOID) && vids.Contains(link.outputWOID))
                            {
                                if (!dump.Links.Exists(l => l.OutputID == link.outputWOID && l.InputID == link.inputWOID))
                                    dump.Links.Add(new LinkData { OutputID = link.outputWOID, InputID = link.inputWOID });
                            }
                        }
                    }

                    if (wo.ObjectLinkRefs != null)
                    {
                        foreach (ObjectLink ol in wo.ObjectLinkRefs)
                        {
                            if (vids.Contains(ol.objectConnectorWOID) && vids.Contains(ol.objectWOID))
                                dump.ObjectLinks.Add(new ObjectLinkData { ConnectorID = ol.objectConnectorWOID, TargetID = ol.objectWOID });
                        }
                    }
                }

                File.WriteAllText(filePath, JsonConvert.SerializeObject(dump, Formatting.Indented));
            }
            catch (Exception e) {  _msg = "Kp2Mxpeyj1J8"; }
        }

        public IEnumerator ImportLogicRoutine(string filePath)
        {
            if (!File.Exists(filePath)) yield break;
            WorldDump dump = JsonConvert.DeserializeObject<WorldDump>(File.ReadAllText(filePath));
            SCG.Dictionary<int, int> idMap = new SCG.Dictionary<int, int>();

            int tot = dump.Items.Count;
            int cur = 0;

            foreach (var it in dump.Items)
            {
                cur++;
                if (cur % 5 == 0) _msg = $"LVzlKjPuz5jR";

                int iid = GetItemIDForType((WorldObjectType)it.WorldObjectTypeInt);
                if (iid != -1)
                {
                    int pre = MVGameControllerBase.WOCM.worldObjects.Count;
                    MVGameControllerBase.OperationRequests.AddItemToWorld(iid, MVGameControllerBase.WOCM.RootGroup.Id,
                        new Vector3(it.Position[0], it.Position[1], it.Position[2]),
                        new Quaternion(it.Rotation[0], it.Rotation[1], it.Rotation[2], it.Rotation[3]), true, true, false);

                    float t = Time.time + 2.0f;
                    while (MVGameControllerBase.WOCM.worldObjects.Count == pre && Time.time < t) yield return null;

                    if (MVGameControllerBase.WOCM.worldObjects.Count > pre)
                    {
                        int nid = -1;
                        foreach (int k in MVGameControllerBase.WOCM.worldObjects.Keys) if (k > nid) nid = k;
                        if (nid != -1)
                        {
                            idMap[it.OriginalID] = nid;
                            if (it.SettingsData != null)
                                MVGameControllerBase.OperationRequests.UpdateWorldObjectDataPartial(nid, FromManagedDictionary(it.SettingsData));
                        }
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }

            foreach (var l in dump.Links)
                if (idMap.ContainsKey(l.OutputID) && idMap.ContainsKey(l.InputID))
                    MVGameControllerBase.OperationRequests.AddLink(new Link { outputWOID = idMap[l.OutputID], inputWOID = idMap[l.InputID] });

            foreach (var ol in dump.ObjectLinks)
                if (idMap.ContainsKey(ol.ConnectorID) && idMap.ContainsKey(ol.TargetID))
                    MVGameControllerBase.OperationRequests.AddObjectLink(new ObjectLink { objectConnectorWOID = idMap[ol.ConnectorID], objectWOID = idMap[ol.TargetID] });

            _msg = "eWRkAiltd4wW";
        }

        private int GetItemIDForType(WorldObjectType t)
        {
            try
            {
                object ui = MVGameControllerBase.EditModeUI;
                object repo = ui.GetType().GetProperty("wRVzVd6uHcMc").GetValue(ui);
                MethodInfo m = repo.GetType().GetMethod("1RTnQBWuAsu4");
                object[] a = { InventoryCategoryType.Logic, t, null };
                if ((bool)m.Invoke(repo, a)) return ((InventoryItem)a[2]).itemID;
                object[] b = { InventoryCategoryType.AdvancedLogic, t, null };
                if ((bool)m.Invoke(repo, b)) return ((InventoryItem)b[2]).itemID;
            }
            catch { }
            return -1;
        }

        private static SCG.Dictionary<string, object> SanitizeData(Il2CppGeneric.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> d)
        {
            var r = new SCG.Dictionary<string, object>();
            if (d == null || d.Pointer == IntPtr.Zero) return r;
            try
            {
                var en = d.GetEnumerator();
                while (en.MoveNext())
                {
                    var c = en.Current;
                    if (c.Key == null || c.Key.Pointer == IntPtr.Zero) continue;
                    r[c.Key.ToString()] = (c.Value != null && c.Value.Pointer != IntPtr.Zero) ? c.Value.ToString() : "GGReFt2uErAG";
                }
            }
            catch { }
            return r;
        }

        private static Il2CppGeneric.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> FromManagedDictionary(SCG.Dictionary<string, object> m)
        {
            var r = new Il2CppGeneric.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
            foreach (var kv in m)
                r.Add(new Il2CppSystem.String((Il2CppSystem.ReadOnlySpan<char>)kv.Key),
                      new Il2CppSystem.String((Il2CppSystem.ReadOnlySpan<char>)kv.Value.ToString()));
            return r;
        }
    }
}
--- FILE: Features\MimicBot.cs ---
﻿using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using Il2CppMV.WorldObject.MetaData;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestMod.Features
{
    public class MimicBot : MonoBehaviour
    {
        public static MimicBot Instance;
        public static bool IsActive = false;

        public string BotName = "3zb44vNDFMrK";
        public int BotID = 999999;
        public float MoveSpeed = 6.5f;
        public float AttackRange = 15.0f;
        public float ShootDelay = 0.5f;

        private GameObject _botObject;
        private MVAvatarRemote _avatarRemote;
        private CharacterController _controller;
        private Transform _rightHand;
        private GameObject _currentWeapon;
        private float _lastShotTime;
        private float _verticalSpeed = 0f;

        public static void Toggle()
        {
            if (IsActive) DestroyBot();
            else SpawnBot();
        }

        public static void SpawnBot()
        {
            if (Instance != null) DestroyBot();

            GameObject prefab = PrefabPool.Instance.mvRemoteAvatarPrefab;
            if (prefab == null) return;

            GameObject botGo = Instantiate(prefab);
            botGo.name = "3Y1qlro1Tu1Y";
            DontDestroyOnLoad(botGo);

            var localPlayer = MVGameControllerBase.Game.LocalPlayer;
            if (localPlayer != null)
            {
                botGo.transform.position = localPlayer.AvatarLocal.transform.position + (localPlayer.AvatarLocal.transform.forward * 2f);
                botGo.transform.rotation = localPlayer.AvatarLocal.transform.rotation;
            }

            Instance = botGo.AddComponent<MimicBot>();
            Instance._botObject = botGo;
            IsActive = true;

          

            Instance.Initialize();
        }

        public static void DestroyBot()
        {
            if (Instance != null)
            {
       

                if (MVGameControllerBase.Game.MVPlayerContainer.ContainsKey(Instance.BotID))
                {
                    MVGameControllerBase.Game.MVPlayerContainer.Remove(Instance.BotID);
                }
                if (Instance._botObject != null) Destroy(Instance._botObject);
                Instance = null;
            }
            IsActive = false;
        }

        private static void Instance_OnMelonLog(System.Drawing.Color c1, System.Drawing.Color c2, string header, string msg)
        {
            if (Instance != null && IsActive)
            {
                TestMod.Helpers.UnityMainThreadDispatcher.Instance.Enqueue(() => Instance.ShowBubble(msg));
            }
        }

        private void ShowBubble(string msg)
        {
            if (_botObject == null) return;
            var anchor = _botObject.GetComponentInChildren<ChatAnchor>();
            if (anchor != null) ChatBubbleManager.ShowChatBubble(msg, BotName, BotID, anchor);
        }

        private void Initialize()
        {
            var gameMotor = _botObject.GetComponent<MvCharacterController>();
            if (gameMotor != null) gameMotor.enabled = false;

            _controller = _botObject.AddComponent<CharacterController>();
            _controller.height = 2.0f;
            _controller.radius = 0.4f;
            _controller.center = new Vector3(0, 1, 0);

            _avatarRemote = _botObject.GetComponent<MVAvatarRemote>();

            InjectFakePlayer();

            MelonCoroutines.Start(SetupVisualsRoutine());
        }

        private GameObject SafeInstantiate(GameObject original)
        {
            bool wasActive = original.activeSelf;
            if (wasActive) original.SetActive(false);
            GameObject clone = Instantiate(original);
            if (wasActive) original.SetActive(true);
            return clone;
        }

        private void SanitizeAndEnable(GameObject go)
        {
            var woc = go.GetComponent<MVWorldObjectClient>();
            if (woc != null) DestroyImmediate(woc.Cast<Component>());

            var logic = go.GetComponent<MVLogicObject>();
            if (logic != null) DestroyImmediate(logic.Cast<Component>());

            var interact = go.GetComponent<InteractionDataHandlerBase>();
            if (interact != null) DestroyImmediate(interact.Cast<Component>());

            var pickup = go.GetComponent<PickupItem>();
            if (pickup != null) DestroyImmediate(pickup.Cast<Component>());

            for (int i = 0; i < go.transform.childCount; i++)
            {
                SanitizeAndEnableRecursive(go.transform.GetChild(i).gameObject);
            }

            var body = go.GetComponent<MVBody>();
            if (body != null) body.Visible = true;

            go.layer = 0;
            go.SetActive(true);
        }

        private void SanitizeAndEnableRecursive(GameObject go)
        {
            var woc = go.GetComponent<MVWorldObjectClient>();
            if (woc != null) DestroyImmediate(woc.Cast<Component>());

            var logic = go.GetComponent<MVLogicObject>();
            if (logic != null) DestroyImmediate(logic.Cast<Component>());

            var interact = go.GetComponent<InteractionDataHandlerBase>();
            if (interact != null) DestroyImmediate(interact.Cast<Component>());

            var pickup = go.GetComponent<PickupItem>();
            if (pickup != null) DestroyImmediate(pickup.Cast<Component>());

            go.layer = 0;

            for (int i = 0; i < go.transform.childCount; i++)
            {
                SanitizeAndEnableRecursive(go.transform.GetChild(i).gameObject);
            }
            if (!go.activeSelf) go.SetActive(true);
        }

        [HideFromIl2Cpp]
        private IEnumerator SetupVisualsRoutine()
        {
            yield return new WaitForSeconds(0.2f);

            var localPlayer = MVGameControllerBase.Game.LocalPlayer;
            if (localPlayer == null || localPlayer.Body == null) yield break;

            GameObject bodyClone = SafeInstantiate(localPlayer.Body.gameObject);
            SanitizeAndEnable(bodyClone);

            bodyClone.transform.SetParent(_botObject.transform);
            bodyClone.transform.localPosition = Vector3.zero;
            bodyClone.transform.localRotation = Quaternion.identity;

            var bodyScript = bodyClone.GetComponent<MVBody>();
            if (bodyScript != null && _avatarRemote != null)
            {
                _avatarRemote.AttachBody(bodyScript);
                if (bodyScript.BodyData != null)
                {
                    _rightHand = bodyScript.BodyData.GetPartBone(BodyData.PartIndex.RArm);
                }
            }

            EquipWeapon(AvatarItemType.Bazooka);
        }

        public void EquipWeapon(AvatarItemType type)
        {
            if (_rightHand == null) return;
            if (_currentWeapon != null) Destroy(_currentWeapon);

            GameObject weaponPrefab = null;
            switch (type)
            {
                case AvatarItemType.Bazooka: weaponPrefab = PrefabPool.Instance.avatarItemBazooka; break;
                case AvatarItemType.RailGun: weaponPrefab = PrefabPool.Instance.avatarItemRailGun; break;
                case AvatarItemType.ImpulseGun: weaponPrefab = PrefabPool.Instance.avatarItemImpulseGun; break;
             
                default: weaponPrefab = PrefabPool.Instance.avatarItemCubeGun; break;
            }

            if (weaponPrefab != null)
            {
                _currentWeapon = Instantiate(weaponPrefab);
                var pickup = _currentWeapon.GetComponent<PickupItem>();
                if (pickup != null) DestroyImmediate(pickup.Cast<Component>());

                _currentWeapon.transform.SetParent(_rightHand);
                _currentWeapon.transform.localPosition = Vector3.zero;
                _currentWeapon.transform.localRotation = Quaternion.identity;
                _currentWeapon.transform.localScale = Vector3.one;
                _currentWeapon.SetActive(true);
}
        }

        private void InjectFakePlayer()
        {
            try
            {
                UserProfileData dummyProfile = new UserProfileData();
                dummyProfile.UserName = BotName;

                MVPlayer fakePlayer = new MVPlayer(BotID, BotID, "G5EuZ7iHJRnv", BuildTarget.Android, dummyProfile, true, false);

                var container = MVGameControllerBase.Game.MVPlayerContainer;
                if (!container.ContainsKey(BotID))
                {
                    container.Add(fakePlayer);
                }

                var uiHandler = _botObject.GetComponentInChildren<AvatarUIHandlerRemote>();
                if (uiHandler != null)
                {
                    uiHandler.Initialize(false, _avatarRemote, BotID, null);
                    if (uiHandler.avatarName != null)
                    {
                        uiHandler.avatarName.text = BotName;
                        uiHandler.avatarName.color = Color.cyan;
                    }
                }
            }
            catch { }
        }

        void Update()
        {
            if (_controller == null || !IsActive) return;

            var localPlayer = MVGameControllerBase.Game.LocalPlayer;
            if (localPlayer == null) return;

            Vector3 targetPos = localPlayer.AvatarLocal.transform.position;
            Vector3 myPos = transform.position;

            float dist = Vector3.Distance(new Vector3(myPos.x, 0, myPos.z), new Vector3(targetPos.x, 0, targetPos.z));

            Vector3 lookDir = (targetPos - myPos).normalized;
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);

            Vector3 moveDir = Vector3.zero;

            if (dist > 5.0f)
            {
                moveDir = transform.forward * MoveSpeed;
                if (_avatarRemote.Body != null) _avatarRemote.Body.StartAnimation("rE08t2kwX22E");
            }
            else
            {
                if (_avatarRemote.Body != null) _avatarRemote.Body.StartAnimation("NnYjknqrZ0GF");
            }

            if (!_controller.isGrounded) _verticalSpeed += Physics.gravity.y * Time.deltaTime;
            else _verticalSpeed = -1f;

            moveDir.y = _verticalSpeed;
            _controller.Move(moveDir * Time.deltaTime);

            if (dist < AttackRange && Time.time > _lastShotTime + ShootDelay)
            {
                ShootAtPlayer(localPlayer);
                _lastShotTime = Time.time;
            }
        }

        private void ShootAtPlayer(MVLocalPlayer target)
        {
            if (_avatarRemote.Body != null) _avatarRemote.Body.StartAnimation("ul0ISFngzSi2");

            if (target.AvatarLocal.InteractableLocal != null)
            {
                target.AvatarLocal.InteractableLocal.TakeDamage(10f, null, PlayerKilledByType.BazookaGun);
            }
        }
    }
}
--- FILE: Features\NameTagColorizer.cs ---
﻿using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
namespace TestMod.Features
{
    public static class NameTagColorizer
    {
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}
--- FILE: Features\PlayerScaleCheat.cs ---
﻿using System;
using UnityEngine;
using Il2Cpp;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using MelonLoader;

namespace TestMod.Features
{
    public static class NetworkedResizer
    {
        

        
        
        public static void SetModifierState(AvatarModifierPackageType modifierType)
        {
            var localPlayer = MVGameControllerBase.Game.LocalPlayer;
            if (localPlayer == null || localPlayer.AvatarLocal == null) return;

            var interactable = localPlayer.AvatarLocal.InteractableLocal;
            if (interactable == null) return;
interactable.AddModifier(modifierType, -1, null);
        }

        public static void ClearModifiers()
        {
            var localPlayer = MVGameControllerBase.Game.LocalPlayer;
            if (localPlayer == null) return;

            var interactable = localPlayer.AvatarLocal.InteractableLocal;
            if (interactable == null) return;

            
            interactable.RemoveModifier(AvatarModifierPackageType.Enlarged);
            interactable.RemoveModifier(AvatarModifierPackageType.Shrunken);
}

        
        
        public static void ForceCustomScale(float newScale)
        {
            var localPlayer = MVGameControllerBase.Game.LocalPlayer;
            if (localPlayer == null) return;

            
            int spawnRoleId = localPlayer.SpawnRolesManager.SpawnRoleId;

            if (spawnRoleId <= 0)
            {
return;
            }
MVGameControllerBase.OperationRequests.UpdatePrototypeScale(spawnRoleId, newScale);

            
            
             MVGameControllerBase.OperationRequests.UpdateWorldObjectDataPartial(spawnRoleId, "aerbyPZG6D4y", newScale); 
        }
    }
}
--- FILE: Features\RailGunCheats.cs ---
﻿using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2Cpp;

namespace TestMod.Features
{
    public static class RailGunCheats
    {
        public static bool EnableRailGunMods = true;
        public static bool LegitMode = true;
        public static bool InfiniteAmmo = true;
        private static bool _defaultsCaptured = false;
        private static float _defaultBaseDamage;
        private static float _defaultChargeLength;
        private static int _defaultMaxAmmo;
        private static AnimationCurve _defaultChargeCurve;

        [HarmonyPatch(typeof(PickupItemRailGun), "3sobscsPxI1H")]
        public static class Patch_RailGun_TriggerBegin
        {
            [HarmonyPrefix]
            private static void Prefix(PickupItemRailGun __instance)
            {
                if (!_defaultsCaptured)
                {
                    try
                    {
                        _defaultBaseDamage = __instance.baseDamage;
                        _defaultChargeLength = __instance.curveChargeLength;
                        _defaultMaxAmmo = __instance.maxAmmo;

                        if (__instance.chargeCurve != null)
                        {
                            _defaultChargeCurve = new AnimationCurve(__instance.chargeCurve.keys);
                        }
                        _defaultsCaptured = true;
}
                    catch (Exception e)
                    {
}
                }

                if (!EnableRailGunMods || !_defaultsCaptured)
                {
                    if (_defaultsCaptured) RestoreDefaults(__instance);
                    return;
                }
                try
                {
                    if (InfiniteAmmo)
                    {
                        __instance.currentAmmo = __instance.maxAmmo;
                    }

                    if (LegitMode)
                    {
                        __instance.baseDamage = _defaultBaseDamage * 1.2f;
                        __instance.curveChargeLength = _defaultChargeLength / 2f;

                        if (_defaultChargeCurve != null)
                            __instance.chargeCurve = _defaultChargeCurve;
                    }
                    else
                    {
                        __instance.baseDamage = 1000f;
                        __instance.curveChargeLength = 0.001f;
                        AnimationCurve instantCurve = new AnimationCurve();
                        instantCurve.AddKey(0f, 1f);
                        instantCurve.AddKey(1f, 1f);
                        __instance.chargeCurve = instantCurve;
                    }
                }
                catch (Exception e)
                {
}
            }
        }
        [HarmonyPatch(typeof(PickupItemRailGun), "HCdDuiZaZRh7")]
        public static class Patch_RailGun_TriggerEnd
        {
            [HarmonyPrefix]
            private static void Prefix(PickupItemRailGun __instance)
            {
                if (!EnableRailGunMods || !_defaultsCaptured) return;
                if (LegitMode)
                {
                    __instance.baseDamage = _defaultBaseDamage * 1.2f;
                }
                else
                {
                    __instance.baseDamage = 1000f;
                    __instance.curveChargeLength = 0.001f;
                }
            }

            [HarmonyPostfix]
            private static void Postfix(PickupItemRailGun __instance)
            {
                if (!EnableRailGunMods) return;
                if (InfiniteAmmo)
                {
                    __instance.currentAmmo = __instance.maxAmmo;
                }
            }
        }

        private static void RestoreDefaults(PickupItemRailGun gun)
        {
            gun.baseDamage = _defaultBaseDamage;
            gun.curveChargeLength = _defaultChargeLength;
            if (_defaultChargeCurve != null)
            {
                gun.chargeCurve = _defaultChargeCurve;
            }
        }
    }
}
--- FILE: Features\ReconnectFix.cs ---
﻿using System.Collections;
using System.Reflection;
using MelonLoader;
using Il2Cpp;
using Il2CppMV.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestMod.Features
{
public static class ReconnectFix
{
    public static void ExecuteReconnect()
    {
        if (MVGameControllerBase.instance == null || MVGameControllerBase.Game == null)
            return;
MVGameControllerBase.Game.ConnState = MVConnState.DisconnectedByUser;
        PropertyInfo disconnectProp =
            typeof(MVGameControllerBase).GetProperty("j0yF6AxoJBXb", BindingFlags.Public | BindingFlags.Static);
        if (disconnectProp != null)
            disconnectProp.SetValue(null, true);

        
        MVGameControllerBase.JoinState = MVJoinState.None;
        UpdateController.Clear();

        
        MVGameControllerBase.OnReceivedGameMsg = null;
        MVGameControllerBase.OnReceivedNotification = null;
        MVGameControllerBase.OnJoinStateChanged = null;

        
        
        

        
        GameObject rtgApp = GameObject.Find("SvtAhT9hiQMl");
        if (rtgApp != null)
        {
            Object.Destroy(rtgApp); 
        }

        
        FieldInfo asyncBookkeeping =
            typeof(KoGaMaDataHandler).GetField("TUCFpUFlbdvK", BindingFlags.Static | BindingFlags.NonPublic);
        if (asyncBookkeeping != null)
        {
            asyncBookkeeping.SetValue(null, null);
        }
        

        
        try
        {
            MVGameControllerBase.Game.Cleanup();
        }
        catch
        {
        }

        
        UnloadUIScenes();

        
        MVGameControllerBase.Game.Peer.Disconnect();

        MelonCoroutines.Start(ReconnectRoutine());
    }

    private static void UnloadUIScenes()
    {
        var playScene = SceneManager.GetSceneByName("nNHa1pLs8X3k");
        if (playScene.IsValid() && playScene.isLoaded)
            SceneManager.UnloadSceneAsync("A5uk4pR2rrBR");

        var editScene = SceneManager.GetSceneByName("2tFz2J5O9qKH");
        if (editScene.IsValid() && editScene.isLoaded)
            SceneManager.UnloadSceneAsync("mo83a7tS17h2");

        var avatarScene = SceneManager.GetSceneByName("AgQjM2ZMSJVz");
        if (avatarScene.IsValid() && avatarScene.isLoaded)
            SceneManager.UnloadSceneAsync("PdOcRpuah8lS");
    }

    private static IEnumerator ReconnectRoutine()
    {
        
        while (MVGameControllerBase.Game != null && MVGameControllerBase.Game.ConnState != MVConnState.Disconnected &&
               MVGameControllerBase.Game.ConnState != MVConnState.DisconnectedByUser)
        {
            yield return null;
        }

        
        yield return null;
        yield return null;
MethodInfo startGameMethod =
            typeof(MVGameControllerBase)
                .GetMethod("2ElwYGF7be5Z", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (startGameMethod != null)
        {
            startGameMethod.Invoke(MVGameControllerBase.instance, null);
}
        else
        {
}
    }
}
}

--- FILE: Features\RotationCheats.cs ---
﻿using System;
using UnityEngine;
using Il2Cpp;
using ImGuiNET;
using HarmonyLib;
using System.Collections.Generic;

namespace TestMod.Features
{
[HarmonyPatch(typeof(AvatarMotor), nameof(AvatarMotor.FixedUpdateFunction))]
public static class RotationCheats
{
    public static bool spin;
    public static bool jit;
    public static bool back;
    public static bool look;
    public static bool crazy;
    public static bool flip;
    public static float spd = 15f;

    [HarmonyPrefix]
    private static void Prefix(AvatarMotor __instance, IMotorAPI motorApi)
    {
        if (MVGameControllerBase.Game == null)
            return;

        var lp = MVGameControllerBase.Game.LocalPlayer;
        if (lp == null || !lp.IsReady)
            return;
        var myAvatar = lp.AvatarLocal;
        if (myAvatar == null || __instance != myAvatar.RigidBody)
            return;

        var tr = __instance.transform;
        if (tr == null)
            return;
        Quaternion currentRot = tr.rotation;
        Quaternion targetRot = currentRot;
        bool active = false;
        float step = spd * 10f * Time.fixedDeltaTime;

        if (spin)
        {
            float r = (Time.time * spd * 50f) % 360f;
            targetRot = Quaternion.Euler(0f, r, 0f);
            active = true;
        }

        if (look)
        {
            var camManager = MVGameControllerBase.MainCameraManager;
            if (camManager != null && camManager.MainCamera != null)
            {
                
                float camYaw = camManager.MainCamera.transform.rotation.eulerAngles.y;
                __instance.transform.rotation = Quaternion.Euler(0f, camYaw, 0f);
            }
        }

        else if (jit)
        {
            float r = UnityEngine.Random.Range(0f, 360f);
            targetRot = Quaternion.Euler(0f, r, 0f);
            active = true;
        }
        else if (back)
        {
            var cam = MVGameControllerBase.MainCameraManager?.MainCamera;
            if (cam != null)
            {
                var fwd = cam.transform.forward;
                fwd.y = 0;
                if (fwd != Vector3.zero)
                {
                    targetRot = Quaternion.LookRotation(-fwd);
                    active = true;
                }
            }
        }
        else if (look)
        {
            Vector3 myPos = tr.position;
            MVPlayer target = null;
            float minDist = float.MaxValue;

            foreach (MVPlayer p in MVGameControllerBase.Game.MVPlayerContainer.Values)
            {
                if (p.ActorNr == lp.ActorNr)
                    continue;
                var targetWoc = MVGameControllerBase.WOCM.GetWorldObjectClient(p.SpawnRolesManager.SpawnRoleId);
                if (targetWoc != null)
                {
                    float d = Vector3.Distance(myPos, targetWoc.Transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        target = p;
                    }
                }
            }

            if (target != null)
            {
                var targetWoc = MVGameControllerBase.WOCM.GetWorldObjectClient(target.SpawnRolesManager.SpawnRoleId);
                if (targetWoc != null)
                {
                    var dir = targetWoc.Transform.position - myPos;
                    dir.y = 0;
                    if (dir != Vector3.zero)
                    {
                        targetRot = Quaternion.LookRotation(dir);
                        active = true;
                    }
                }
            }
        }
        else if (crazy)
        {
            targetRot = UnityEngine.Random.rotation;
            active = true;
        }
        else if (flip)
        {
            targetRot = Quaternion.Euler(180f, tr.rotation.eulerAngles.y, 0f);
            active = true;
        }
        if (active)
        {
            Quaternion newRot = Quaternion.RotateTowards(tr.rotation, targetRot, step);
            if (motorApi != null)
            {
                motorApi.Rotation = newRot;
            }
            tr.rotation = newRot;
        }
    }

    public static void draw()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0, 1), "DC5TjvIVDAuT");
        ImGui.Separator();
        if (ImGui.Checkbox("Nm4meOX7V7p3", ref spin))
        {
            if (spin)
                reset(1);
        }
        if (ImGui.Checkbox("eS0VR4KrzKk7", ref jit))
        {
            if (jit)
                reset(2);
        }
        if (ImGui.Checkbox("leLh7fxpcLuW", ref back))
        {
            if (back)
                reset(3);
        }
        if (ImGui.Checkbox("BQiG0km36NdO", ref look))
        {
            if (look)
                reset(4);
        }
        if (ImGui.Checkbox("BMMPw3lc41Za", ref crazy))
        {
            if (crazy)
                reset(5);
        }
        if (ImGui.Checkbox("XQqP8LbmS7su", ref flip))
        {
            if (flip)
                reset(6);
        }

        ImGui.Separator();

        if (spin || jit || back || look || crazy || flip)
        {
            ImGui.SliderFloat("9vX4UkWtnN0O", ref spd, 1.0f, 100.0f);
        }
    }

    static void reset(int idx)
    {
        if (idx != 1)
            spin = false;
        if (idx != 2)
            jit = false;
        if (idx != 3)
            back = false;
        if (idx != 4)
            look = false;
        if (idx != 5)
            crazy = false;
        if (idx != 6)
            flip = false;
    }
}
}

--- FILE: Features\SmartBuilder.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using ImGuiNET;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TestMod.Helpers;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TestMod.Features
{

[HarmonyPatch(typeof(Cube), "jtTJnNGzd1No", new System.Type[] { typeof(Il2CppStructArray<Vector3>) })]
public static class Patch_Cube_IsLegal
{
    [HarmonyPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(MVCubeModelBase), "4AsDC4UiIi67")]
public static class Patch_SmartBuilder_AddCube
{
    [HarmonyPrefix]
    public static bool Prefix(MVCubeModelBase __instance, IntVector __0, CubeBase __1)
    {
        if (!SmartBuilder.SmartBuilderEnabled)
            return true;

        if (SmartBuilder._isModPlacingCube)
            return true;

        if (SmartBuilder._isCurrentlyBuilding)
            return false;

        try
        {
            var lp = MVGameControllerBase.Game?.LocalPlayer;
            if (lp == null || !lp.IsReady || lp.AvatarLocal == null)
                return true;

            SmartBuilder._isCurrentlyBuilding = true;
            SmartBuilder.RecordPlacement();

            byte matId = 21;
            var realCube = __1.TryCast<Cube>();
            if (realCube != null && realCube.FaceMaterials != null && realCube.FaceMaterials.Length > 0)
            {
                matId = realCube.FaceMaterials[0];
            }

            matId = SmartBuilder.GetSafeMaterialId(matId);

            Cube templateCube =
                new Cube(CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners), Cube.CreateMaterialArray(matId));

            UnityMainThreadDispatcher.Instance.Enqueue(
                () =>
                {
                    if (SmartBuilder.buildMode == 5)
                    {
                        if (SmartBuilder.availableImports != null && SmartBuilder.availableImports.Length > 0 &&
                            SmartBuilder.selectedImportIndex < SmartBuilder.availableImports.Length)
                        {
                            string selectedFile = SmartBuilder.availableImports[SmartBuilder.selectedImportIndex];
                            MelonLoader.MelonCoroutines.Start(
                                SmartBuilder.ImportAndBuildStructure(__instance, __0, selectedFile));
                        }
                        else
                        {
                            SmartBuilder._isCurrentlyBuilding = false;
                        }
                    }
                    else
                    {
                        MelonLoader.MelonCoroutines.Start(
                            SmartBuilder.BuildStructureCoroutine(__instance, __0, templateCube));
                    }
                });

            return false;
        }
        catch (Exception ex)
        {
SmartBuilder._isCurrentlyBuilding = false;
            return true;
        }
    }
}

[HarmonyPatch(typeof(GUICellCursor), "qYoenqpsyFkO")]
public static class Patch_SmartBuilder_Preview
{
    public static Dictionary<int, List<GameObject>> extraCursorPools = new Dictionary<int, List<GameObject>>();

    [HarmonyPostfix]
    public static void Postfix(GUICellCursor __instance, IntVector position, GameObject cubeGameObject)
    {
        int instanceId = __instance.gameObject.GetInstanceID();

        if (!SmartBuilder.SmartBuilderEnabled || SmartBuilder._isCurrentlyBuilding || SmartBuilder.buildMode == 4)
        {
            __instance.GetComponent<MeshRenderer>().enabled = true;
            ClearPreviews(instanceId);
            return;
        }

        var woc = MVWorldObjectClientManager.GetMVObject(cubeGameObject.transform);
        var targetModel = woc?.TryCast<MVCubeModelBase>();
        if (targetModel == null)
        {
            targetModel = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                              ?.TryCast<MVCubeModelBase>();
        }

        List<IntVector> buildPositions = SmartBuilder.CalculateBlocksToPlace(position, targetModel);

        if (!extraCursorPools.ContainsKey(instanceId))
        {
            extraCursorPools[instanceId] = new List<GameObject>();
        }

        List<GameObject> pool = extraCursorPools[instanceId];
        int extraNeeded = Mathf.Max(0, buildPositions.Count - 1);

        while (pool.Count < extraNeeded)
        {
            GameObject extraObj = new GameObject($"Vhml5o0AFvmn");
            extraObj.layer = LayerMask.NameToLayer("BdCXtbbIv9NB");

            MeshFilter originalMf = __instance.GetComponent<MeshFilter>();
            MeshRenderer originalMr = __instance.GetComponent<MeshRenderer>();

            MeshFilter mf = extraObj.AddComponent<MeshFilter>();
            MeshRenderer mr = extraObj.AddComponent<MeshRenderer>();

            mf.sharedMesh = originalMf.sharedMesh;
            mr.sharedMaterial = originalMr.sharedMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            pool.Add(extraObj);
        }

        if (buildPositions.Count > 0)
        {
            __instance.GetComponent<MeshRenderer>().enabled = true;
            __instance.transform.position = SharedCubeFunctions.LocalToWorld(cubeGameObject, buildPositions[0]);

            int poolIdx = 0;
            for (int i = 1; i < buildPositions.Count; i++)
            {
                if (poolIdx >= pool.Count)
                    break;
                GameObject preview = pool[poolIdx];
                Vector3 worldPos = SharedCubeFunctions.LocalToWorld(cubeGameObject, buildPositions[i]);

                preview.transform.position = worldPos;
                preview.transform.rotation = cubeGameObject.transform.rotation;
                preview.transform.localScale = cubeGameObject.transform.localScale;
                preview.SetActive(true);
                poolIdx++;
            }

            for (int i = poolIdx; i < pool.Count; i++)
            {
                pool[i].SetActive(false);
            }
        }
        else
        {
            __instance.GetComponent<MeshRenderer>().enabled = false;
            for (int i = 0; i < pool.Count; i++)
            {
                pool[i].SetActive(false);
            }
        }
    }

    public static void ClearPreviews(int instanceId)
    {
        if (extraCursorPools.TryGetValue(instanceId, out var pool))
        {
            foreach (var p in pool)
            {
                if (p != null)
                    p.SetActive(false);
            }
        }
    }
}

[HarmonyPatch(typeof(GUICellCursor), "MEuzGLJghHAw")]
public static class Patch_GUICellCursor_Destroy
{
    [HarmonyPostfix]
    public static void Postfix(GUICellCursor __instance)
    {
        if (__instance == null || __instance.gameObject == null)
            return;

        int instanceId = __instance.gameObject.GetInstanceID();

        if (Patch_SmartBuilder_Preview.extraCursorPools.ContainsKey(instanceId))
        {
            Patch_SmartBuilder_Preview.ClearPreviews(instanceId);

            foreach (var previewObj in Patch_SmartBuilder_Preview.extraCursorPools[instanceId])
            {
                if (previewObj != null)
                {
                    UnityEngine.Object.Destroy(previewObj);
                }
            }

            Patch_SmartBuilder_Preview.extraCursorPools.Remove(instanceId);
        }
    }
}

public static class SmartBuilder
{
    public static bool SmartBuilderEnabled = true;
    public static float cubePlacerDelay = 0.250f;
    public static int buildMode = 0;
    public static string[] buildModes =
        new string[] { "hh1pC01RjgjM", "EC2Z7dE9Zy89", "wOOTvkzuG5PJ", "Mpraxnk9E3es", "XzvzbLPs1B0j", "MYDynnQCkv0W" };

    public static int cubesToPlace = 10;
    public static int placementOffset = 0;
    public static int placementDirIndex = 0;
    public static string[] dirNamesLine = new string[] { "H0ERBlJYigb0", "L0cRpTWt9U2E", "oOjtZSZrCgOz", "2TyXKlfRP4MC", "DbisdDD6kxeX", "Q9Kwd4xgyC93" };

    public static int wallWidth = 5;
    public static int wallHeight = 5;

    public static int stairSteps = 5;
    public static int stairWidth = 2;
    public static int stairDir = 0;
    public static string[] dirNames = new string[] { "HcROvKG6J2BR", "VHikvSWfb9FX", "x6z298PObF28", "PcvmHSQlNScK" };

    public static int trapScale = 1;
    public static int trapHeight = 0;

    public static float antiLimitPauseDuration = 0.5f;
    public static int antiLimitMaxBlocks = 13;

    public static string exportName = "ouTOElMOcjpF";
    public static string importName = "dzQ5SW0zJJ19";

    public static int importRotation = 0;
    public static IntVector importOffset = new IntVector(0, 0, 0);

    private static string[] _playerNames = new string[] { "Ru6JPxcPAazb" };
    private static int[] _playerIds = new int[] { -1 };
    private static int _selectedPlayerIdx = 0;

    private static Mesh _previewMesh;

    public static bool _isCurrentlyBuilding = false;
    public static bool _isModPlacingCube = false;

    public static string CubePlacerStatusMessage = "tZtKICLqfmFY";
    private static Queue<float> _placementHistory = new Queue<float>();

    public static IntVector? exportStartPoint = null;
    public static IntVector? exportEndPoint = null;
    public static MVCubeModelBase exportTargetModel = null;

    public static string[] availableImports = new string[0];
    public static int selectedImportIndex = 0;

    private static string _cachedImportFile = null;
    private static List<IntVector> _cachedImportPositions = new List<IntVector>();

    public static void LoadImportCacheIfNeeded()
    {
        if (availableImports == null || availableImports.Length == 0 || selectedImportIndex >= availableImports.Length)
        {
            _cachedImportPositions.Clear();
            _cachedImportFile = null;
            return;
        }

        string selectedFile = availableImports[selectedImportIndex];
        if (_cachedImportFile == selectedFile)
            return;

        _cachedImportPositions.Clear();
        _cachedImportFile = selectedFile;

        string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "rSTJskBnxjHs", selectedFile + "H6z6sCBeV5tX");
        if (!File.Exists(path))
            return;

        try
        {
            byte[] rawBytes = Convert.FromBase64String(File.ReadAllText(path));
            BytePacker bp = new BytePacker(rawBytes);
            int cubeCount = bp.ReadInt32();

            for (int i = 0; i < cubeCount; i++)
            {
                short lx = bp.ReadInt16();
                short ly = bp.ReadInt16();
                short lz = bp.ReadInt16();
                _cachedImportPositions.Add(new IntVector(lx, ly, lz));
                bp.ReadBytes(8);
                bp.ReadBytes(6);
            }
        }
        catch
        {
        }
    }

    public static void RefreshAvailableImports()
    {
        _cachedImportFile = null;
        string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "bn42wUGSg1nd");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            availableImports = new string[0];
            return;
        }

        string[] files = Directory.GetFiles(path, "YuzxCeLfJ7ZO");
        availableImports = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            availableImports[i] = Path.GetFileNameWithoutExtension(files[i]);
        }

        if (selectedImportIndex >= availableImports.Length)
        {
            selectedImportIndex = 0;
        }
    }

    public static HashSet<byte> GetAvailableMapMaterials()
    {
        HashSet<byte> availableMats = new HashSet<byte>();

        var wocm = MVGameControllerBase.WOCM;
        if (wocm == null || wocm.worldObjects == null)
            return availableMats;

        Il2CppSystem.Object itemDataKey = (Il2CppSystem.Object) "y3aW3xSY1pCF";
        Il2CppSystem.Object matKey = (Il2CppSystem.Object) "MiNdOZLaomnG";

        foreach (var wo in wocm.worldObjects.Values)
        {
            if (wo == null || wo.Data == null)
                continue;

            try
            {
                if (wo.Data.ContainsKey(itemDataKey))
                {
                    var itemDict = wo.Data[itemDataKey]
                                       .TryCast<Il2CppSystem.Collections.Generic
                                                    .Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();

                    if (itemDict != null && itemDict.ContainsKey(matKey))
                    {
                        var matObj = itemDict[matKey];

                        var unboxPtr = IL2CPP.il2cpp_object_unbox(matObj.Pointer);
                        byte matId = Marshal.ReadByte(unboxPtr);

                        availableMats.Add(matId);
                    }
                }
            }
            catch
            {
            }
        }

        availableMats.Add(GameInfo.GetEquippedMaterialId());

        return availableMats;
    }

    public static byte GetSafeMaterialId(byte requestedMatId)
    {
        HashSet<byte> availableMats = GetAvailableMapMaterials();

        if (availableMats.Count == 0)
            return requestedMatId;

        if (availableMats.Contains(requestedMatId))
        {
            return requestedMatId;
        }

        if (availableMats.Contains(21))
        {
            return 21;
        }

        var enumerator = availableMats.GetEnumerator();
        if (enumerator.MoveNext())
        {
            return enumerator.Current;
        }

        return 21;
    }

    public static void RecordPlacement()
    {
        _placementHistory.Enqueue(Time.time);
    }

    public static void AutoCenterImport()
    {
        if (_cachedImportPositions == null || _cachedImportPositions.Count == 0)
            return;

        int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;

        foreach (var pos in _cachedImportPositions)
        {
            int rx = pos.x;
            int ry = pos.y;
            int rz = pos.z;

            if (importRotation == 1)
            {
                rx = -pos.z;
                rz = pos.x;
            }
            else if (importRotation == 2)
            {
                rx = -pos.x;
                rz = -pos.z;
            }
            else if (importRotation == 3)
            {
                rx = pos.z;
                rz = -pos.x;
            }

            if (rx < minX)
                minX = rx;
            if (ry < minY)
                minY = ry;
            if (rz < minZ)
                minZ = rz;

            if (rx > maxX)
                maxX = rx;
            if (ry > maxY)
                maxY = ry;
            if (rz > maxZ)
                maxZ = rz;
        }

        int centerX = -(minX + maxX) / 2;
        int bottomY = -minY;
        int centerZ = -(minZ + maxZ) / 2;

        importOffset = new IntVector((short)centerX, (short)bottomY, (short)centerZ);
    }

    public static void HandleSelectionInput()
    {
        if (!MVGameControllerBase.IsInitialized || MVGameControllerBase.Game == null ||
            MVGameControllerBase.MainCameraManager == null)
            return;

        if (!MVGameControllerBase.MainCameraManager.IsCameraControllerSet())
            return;

        var camManager = MVGameControllerBase.MainCameraManager;
        if (camManager.CurrentCamera == null)
            return;

        var cam = camManager.CurrentCamera.transform;
        if (cam == null)
            return;

        MVCubeModelBase targetModel =
            MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                ?.TryCast<MVCubeModelBase>();
        if (targetModel == null)
            return;

        var lp = MVGameControllerBase.Game.LocalPlayer;
        if (lp != null && lp.IsReady && lp.AvatarLocal != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.RightBracket))
            {
                Vector3 playerPos = lp.AvatarLocal.transform.position;
                IntVector playerLocalPos = SharedCubeFunctions.WorldToLocal(targetModel.gameObject, playerPos, false);

                if (Input.GetKeyDown(KeyCode.LeftBracket))
                {
                    exportStartPoint = playerLocalPos;
                    exportTargetModel = targetModel;
}
                if (Input.GetKeyDown(KeyCode.RightBracket))
                {
                    exportEndPoint = playerLocalPos;
                    exportTargetModel = targetModel;
}
            }

            if (buildMode == 5 && !_isCurrentlyBuilding)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    importRotation = (importRotation + 1) % 4;
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    AutoCenterImport();
                }
            }
        }
    }

    public static void DrawSelectionBox()
    {
        if (exportStartPoint == null || exportEndPoint == null || exportTargetModel == null)
            return;

        if (_previewMesh == null)
        {
            _previewMesh = new Mesh();
            SharedCubeFunctions.AddCubeMesh(_previewMesh, CubeBase.IdentityCorners, true);
        }

        int minX = Mathf.Min(exportStartPoint.Value.x, exportEndPoint.Value.x);
        int minY = Mathf.Min(exportStartPoint.Value.y, exportEndPoint.Value.y);
        int minZ = Mathf.Min(exportStartPoint.Value.z, exportEndPoint.Value.z);

        int maxX = Mathf.Max(exportStartPoint.Value.x, exportEndPoint.Value.x);
        int maxY = Mathf.Max(exportStartPoint.Value.y, exportEndPoint.Value.y);
        int maxZ = Mathf.Max(exportStartPoint.Value.z, exportEndPoint.Value.z);

        Vector3 size = new Vector3(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);

        Vector3 worldMin = SharedCubeFunctions.LocalToWorld(exportTargetModel.gameObject,
                                                            new IntVector((short)minX, (short)minY, (short)minZ));
        Vector3 worldMax = SharedCubeFunctions.LocalToWorld(exportTargetModel.gameObject,
                                                            new IntVector((short)maxX, (short)maxY, (short)maxZ));

        Vector3 realCenter = (worldMin + worldMax) / 2f;

        Vector3 visualScale = Vector3.one;
        Vector3 finalScale = new Vector3(size.x * visualScale.x, size.y * visualScale.y, size.z * visualScale.z);

        Matrix4x4 matrix = Matrix4x4.TRS(realCenter, exportTargetModel.transform.rotation, finalScale);
        Graphics.DrawMesh(_previewMesh, matrix, PrefabPool.Instance.SelectBoxMaterial, 0, Camera.main);
    }

    public static void ExportSelection(string filename)
    {
        if (exportStartPoint == null || exportEndPoint == null || exportTargetModel == null)
            return;

        int minX = Mathf.Min(exportStartPoint.Value.x, exportEndPoint.Value.x);
        int minY = Mathf.Min(exportStartPoint.Value.y, exportEndPoint.Value.y);
        int minZ = Mathf.Min(exportStartPoint.Value.z, exportEndPoint.Value.z);

        int maxX = Mathf.Max(exportStartPoint.Value.x, exportEndPoint.Value.x);
        int maxY = Mathf.Max(exportStartPoint.Value.y, exportEndPoint.Value.y);
        int maxZ = Mathf.Max(exportStartPoint.Value.z, exportEndPoint.Value.z);

        int realMinX = int.MaxValue;
        int realMinY = int.MaxValue;
        int realMinZ = int.MaxValue;

        var rawCubes = new Dictionary<IntVector, Cube>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    IntVector worldGridPos = new IntVector((short)x, (short)y, (short)z);
                    Cube cube = exportTargetModel.GetCube(worldGridPos);

                    if (cube != null)
                    {
                        rawCubes.Add(worldGridPos, cube);
                        if (x < realMinX)
                            realMinX = x;
                        if (y < realMinY)
                            realMinY = y;
                        if (z < realMinZ)
                            realMinZ = z;
                    }
                }
            }
        }

        if (rawCubes.Count == 0)
        {
return;
        }

        var exportedCubes = new Dictionary<IntVector, Cube>();

        foreach (var kvp in rawCubes)
        {
            IntVector relativePos = new IntVector((short)(kvp.Key.x - realMinX), (short)(kvp.Key.y - realMinY),
                                                  (short)(kvp.Key.z - realMinZ));
            exportedCubes.Add(relativePos, Cube.Clone(kvp.Value));
        }

        BytePacker bp = new BytePacker();
        bp.Write(exportedCubes.Count);

        foreach (var kvp in exportedCubes)
        {
            bp.Write(kvp.Key.x);
            bp.Write(kvp.Key.y);
            bp.Write(kvp.Key.z);
            bp.Write(kvp.Value.ByteCorners);
            bp.Write(kvp.Value.FaceMaterials);
        }

        string b64 = Convert.ToBase64String(bp.ToArray());
        string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "fGD89Zl3oXuN", filename + "WSKSxlGyPsIn");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, b64);
}

    public static IEnumerator ImportAndBuildStructure(MVCubeModelBase targetModel, IntVector rootPos, string filename)
    {
        string path = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "UANh5Ua6oaZh", filename + "TO6XZIcGwXTO");
        if (!File.Exists(path))
            yield break;

        byte[] rawBytes = Convert.FromBase64String(File.ReadAllText(path));
        BytePacker bp = new BytePacker(rawBytes);
        int cubeCount = bp.ReadInt32();

        var importedCubes = new Dictionary<IntVector, Cube>();

        for (int i = 0; i < cubeCount; i++)
        {
            IntVector localPos = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
            byte[] corners = bp.ReadBytes(8);
            byte[] materials = bp.ReadBytes(6);

            for (int m = 0; m < materials.Length; m++)
            {
                materials[m] = GetSafeMaterialId(materials[m]);
            }

            importedCubes.Add(localPos, new Cube(corners, materials));
        }

        int cachedRot = importRotation;
        IntVector cachedOffset = importOffset;

        IntVector fwd = new IntVector((short)0, (short)0, (short)1);
        IntVector right = new IntVector((short)1, (short)0, (short)0);
        IntVector up = new IntVector((short)0, (short)1, (short)0);

        Camera cam = MVGameControllerBase.MainCameraManager?.MainCamera;
        if (cam != null && targetModel != null)
        {
            Vector3 camFwd = cam.transform.forward;
            camFwd.y = 0;
            if (camFwd == Vector3.zero)
                camFwd = Vector3.forward;

            Vector3 camRight = cam.transform.right;
            camRight.y = 0;
            if (camRight == Vector3.zero)
                camRight = Vector3.right;

            Vector3 localFwd = targetModel.transform.InverseTransformDirection(camFwd.normalized);
            Vector3 localRight = targetModel.transform.InverseTransformDirection(camRight.normalized);
            Vector3 localUp = targetModel.transform.InverseTransformDirection(Vector3.up);

            fwd = GetDominantAxis(localFwd);
            right = GetDominantAxis(localRight);
            up = GetDominantAxis(localUp);
        }

        float safeDelay = Mathf.Max(cubePlacerDelay, 0.01f);
        int placed = 0;

        foreach (var kvp in importedCubes)
        {
            int rx = kvp.Key.x;
            int ry = kvp.Key.y;
            int rz = kvp.Key.z;

            if (cachedRot == 1)
            {
                rx = -kvp.Key.z;
                rz = kvp.Key.x;
            }
            else if (cachedRot == 2)
            {
                rx = -kvp.Key.x;
                rz = -kvp.Key.z;
            }
            else if (cachedRot == 3)
            {
                rx = kvp.Key.z;
                rz = -kvp.Key.x;
            }

            rx += cachedOffset.x;
            ry += cachedOffset.y;
            rz += cachedOffset.z;

            IntVector finalPos = new IntVector((short)(rootPos.x + right.x * rx + up.x * ry + fwd.x * rz),
                                               (short)(rootPos.y + right.y * rx + up.y * ry + fwd.y * rz),
                                               (short)(rootPos.z + right.z * rx + up.z * ry + fwd.z * rz));

            while (_placementHistory.Count > 0 && Time.time - _placementHistory.Peek() > antiLimitPauseDuration)
            {
                _placementHistory.Dequeue();
            }

            if (_placementHistory.Count >= antiLimitMaxBlocks)
            {
                CubePlacerStatusMessage = "eLWq7B8U9kHL";
                while (_placementHistory.Count >= antiLimitMaxBlocks)
                {
                    if (Time.time - _placementHistory.Peek() > antiLimitPauseDuration)
                    {
                        _placementHistory.Dequeue();
                    }
                    yield return null;
                }
            }

            try
            {
                _isModPlacingCube = true;
                targetModel.AddCube(finalPos, kvp.Value);
                targetModel.HandleDelta();
                _isModPlacingCube = false;
                RecordPlacement();
                placed++;
            }
            catch
            {
                _isModPlacingCube = false;
            }

            CubePlacerStatusMessage = $"94m80VrN9QvF";
            yield return new WaitForSeconds(safeDelay);
        }

        CubePlacerStatusMessage = "kDxMDj4P5wVt";
        _isCurrentlyBuilding = false;
    }

    public static void StartImportAtPlayer()
    {
        if (_isCurrentlyBuilding || availableImports.Length == 0)
            return;

        if (!MVGameControllerBase.IsInitialized || MVGameControllerBase.Game == null)
            return;

        var lp = MVGameControllerBase.Game.LocalPlayer;
        if (lp == null || !lp.IsReady || lp.AvatarLocal == null)
            return;

        MVCubeModelBase targetModel =
            MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                ?.TryCast<MVCubeModelBase>();
        if (targetModel == null)
            return;

        IntVector playerGridPos = SharedCubeFunctions.WorldToLocal(targetModel.gameObject,
                                                                   lp.AvatarLocal.gameObject.transform.position, false);

        _isCurrentlyBuilding = true;

        string selectedFile = availableImports[selectedImportIndex];
        MelonLoader.MelonCoroutines.Start(ImportAndBuildStructure(targetModel, playerGridPos, selectedFile));
    }

    public static void RefreshPlayers()
    {
        if (MVGameControllerBase.Game?.MVPlayerContainer == null)
            return;

        var nameList = new List<string> { "HNFOTStfFvbG" };
        var idList = new List<int> { -1 };

        int myId = MVGameControllerBase.Game.LocalPlayer.ActorNr;

        foreach (var p in MVGameControllerBase.Game.MVPlayerContainer.Values)
        {
            if (p != null && p.ActorNr != myId)
            {
                nameList.Add($"SRlEoSbku2sO");
                idList.Add(p.ActorNr);
            }
        }

        _playerNames = nameList.ToArray();
        _playerIds = idList.ToArray();
        if (_selectedPlayerIdx >= _playerNames.Length)
            _selectedPlayerIdx = 0;
    }

    public static void RenderUI()
    {
        ImGui.Checkbox("4vh2v7EPVOSZ", ref SmartBuilderEnabled);
        ImGui.Text("fC7jNoJE02XK" +
                   "MJ97GVDMyJhW");
        ImGui.Combo("Z8MAIO38seIU", ref buildMode, buildModes, buildModes.Length);

        float dly = cubePlacerDelay;
        if (ImGui.SliderFloat("fejVGVSukXhx", ref dly, 0.01f, 1.0f))
            cubePlacerDelay = Mathf.Max(dly, 0.01f);

        ImGui.SliderFloat("P3N48B1GiUlM", ref antiLimitPauseDuration, 0.5f, 10.0f);
        ImGui.SliderInt("UbAHS8SSJzsL", ref antiLimitMaxBlocks, 5, 50);

        ImGui.Separator();

        if (buildMode == 0)
        {
            ImGui.SliderInt("tWCrb1CxCnE6", ref cubesToPlace, 1, 100);
            ImGui.SliderInt("zTBuOFiEaZAf", ref placementOffset, 0, 10);
            ImGui.Combo("zheTm5cBAV7A", ref placementDirIndex, dirNamesLine, dirNamesLine.Length);
        }
        else if (buildMode == 1)
        {
            ImGui.SliderInt("ml5n8wSBEqHu", ref wallWidth, 1, 30);
            ImGui.SliderInt("uRyjfOOQJiJz", ref wallHeight, 1, 30);
            ImGui.SliderInt("88XXGvjob3vi", ref placementOffset, 0, 10);
        }
        else if (buildMode == 2)
        {
            ImGui.SliderInt("gDTrVW7a5KlU", ref stairSteps, 1, 50);
            ImGui.SliderInt("UaIw0pmjqMFx", ref stairWidth, 1, 10);
            ImGui.SliderInt("Ku6yd3u2Pf9n", ref placementOffset, 0, 10);
            ImGui.Combo("Vm1i5TAe9dGd", ref stairDir, dirNames, dirNames.Length);
        }
        else if (buildMode == 3)
        {
        }
        else if (buildMode == 4)
        {
            ImGui.SliderInt("pooHc6bq9pgg", ref trapScale, 1, 10);
            ImGui.SliderInt("8DWtEjuaSQaJ", ref trapHeight, 0, 10);
            ImGui.Separator();

            if (ImGui.Button("rvjRTx1vSoxU"))
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                           { RefreshPlayers(); });
            }

            if (_playerNames.Length > 0)
            {
                ImGui.Combo("MhBWqtfbOF41", ref _selectedPlayerIdx, _playerNames, _playerNames.Length);
            }
        }
        else if (buildMode == 5)
        {
            ImGui.InputText("eifBzZODdwzC", ref exportName, 64);
            if (ImGui.Button("m7eoEURlMrXt"))
            {
                string safeExportName = exportName;
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                           {
                                                               ExportSelection(safeExportName);
                                                               RefreshAvailableImports();
                                                           });
            }

            ImGui.Separator();

            if (ImGui.Button("h7FBHL8tV8g6"))
            {
                RefreshAvailableImports();
            }

            if (availableImports != null && availableImports.Length > 0)
            {
                ImGui.Combo("QBoQa3qwp0Y5", ref selectedImportIndex, availableImports, availableImports.Length);

                ImGui.Separator();
                ImGui.Text("WsMPatS1KN2J");

                if (ImGui.Button("qbggJk7UGBUm"))
                {
                    AutoCenterImport();
                }
                ImGui.SameLine();
                if (ImGui.Button("yvqdRysMKoxx"))
                {
                    importOffset = new IntVector(0, 0, 0);
                }

                int rot = importRotation;
                if (ImGui.SliderInt("WJfUUBz9rdL7", ref rot, 0, 3))
                    importRotation = rot;

                int ox = importOffset.x;
                int oy = importOffset.y;
                int oz = importOffset.z;

                bool offChanged = false;
                if (ImGui.InputInt("WzWYii21yMlv", ref ox))
                    offChanged = true;
                if (ImGui.InputInt("PDMjHPf7KEci", ref oy))
                    offChanged = true;
                if (ImGui.InputInt("kaq59rVSXNnp", ref oz))
                    offChanged = true;

                if (offChanged)
                {
                    importOffset = new IntVector((short)ox, (short)oy, (short)oz);
                }

                if (ImGui.Button("DLKyxF2YMFgu"))
                {
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                               { StartImportAtPlayer(); });
                }
            }
            else
            {
                ImGui.Text("im3HfUnJCbcV");
            }
        }

        ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), CubePlacerStatusMessage);

        if (buildMode != 5 && ImGui.Button("i1Iv4bcQL0Wr"))
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       { StartBuildingStructureAtPlayer(); });
        }
    }

    private static IntVector GetDominantAxis(Vector3 dir)
    {
        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);
        float az = Mathf.Abs(dir.z);

        if (ax > ay && ax > az)
            return new IntVector((short)Mathf.Sign(dir.x), (short)0, (short)0);
        if (ay > ax && ay > az)
            return new IntVector((short)0, (short)Mathf.Sign(dir.y), (short)0);
        return new IntVector((short)0, (short)0, (short)Mathf.Sign(dir.z));
    }

    public static List<IntVector> CalculateBlocksToPlace(IntVector startPos, MVCubeModelBase targetModel = null)
    {
        var blocksToPlace = new List<IntVector>();

        IntVector fwd = new IntVector((short)0, (short)0, (short)1);
        IntVector right = new IntVector((short)1, (short)0, (short)0);
        IntVector up = new IntVector((short)0, (short)1, (short)0);

        Camera cam = MVGameControllerBase.MainCameraManager?.MainCamera;
        if (cam != null && targetModel != null)
        {
            Vector3 camFwd = cam.transform.forward;
            camFwd.y = 0;
            if (camFwd == Vector3.zero)
                camFwd = Vector3.forward;

            Vector3 camRight = cam.transform.right;
            camRight.y = 0;
            if (camRight == Vector3.zero)
                camRight = Vector3.right;

            Vector3 localFwd = targetModel.transform.InverseTransformDirection(camFwd.normalized);
            Vector3 localRight = targetModel.transform.InverseTransformDirection(camRight.normalized);
            Vector3 localUp = targetModel.transform.InverseTransformDirection(Vector3.up);

            fwd = GetDominantAxis(localFwd);
            right = GetDominantAxis(localRight);
            up = GetDominantAxis(localUp);
        }

        if (buildMode == 0)
        {
            IntVector dirStep = fwd;
            if (placementDirIndex == 1)
                dirStep = new IntVector((short)-fwd.x, (short)-fwd.y, (short)-fwd.z);
            if (placementDirIndex == 2)
                dirStep = right;
            if (placementDirIndex == 3)
                dirStep = new IntVector((short)-right.x, (short)-right.y, (short)-right.z);
            if (placementDirIndex == 4)
                dirStep = up;
            if (placementDirIndex == 5)
                dirStep = new IntVector((short)-up.x, (short)-up.y, (short)-up.z);

            for (int i = 0; i <= cubesToPlace; i++)
            {
                int totalOffset = placementOffset + i;
                var pos = new IntVector((short)(startPos.x + dirStep.x * totalOffset),
                                        (short)(startPos.y + dirStep.y * totalOffset),
                                        (short)(startPos.z + dirStep.z * totalOffset));
                blocksToPlace.Add(pos);
            }
        }
        else if (buildMode == 1)
        {
            for (int h = 0; h < wallHeight; h++)
            {
                for (int w = 0; w < wallWidth; w++)
                {
                    var pos = new IntVector((short)(startPos.x + right.x * w + up.x * h + fwd.x * placementOffset),
                                            (short)(startPos.y + right.y * w + up.y * h + fwd.y * placementOffset),
                                            (short)(startPos.z + right.z * w + up.z * h + fwd.z * placementOffset));
                    blocksToPlace.Add(pos);
                }
            }
        }
        else if (buildMode == 2)
        {
            for (int step = 0; step < stairSteps; step++)
            {
                for (int w = 0; w < stairWidth; w++)
                {
                    var pos = new IntVector(
                        (short)(startPos.x + fwd.x * (step + placementOffset) + right.x * w + up.x * step),
                        (short)(startPos.y + fwd.y * (step + placementOffset) + right.y * w + up.y * step),
                        (short)(startPos.z + fwd.z * (step + placementOffset) + right.z * w + up.z * step));
                    blocksToPlace.Add(pos);
                }
            }
        }
        else if (buildMode == 3)
        {
            var radius = 6f;
            var angleStep = 0.25f;
            for (int i = 0; i <= 70; i++)
            {
                var angle = i * angleStep;
                var x = startPos.x + Mathf.RoundToInt(radius * Mathf.Cos(angle));
                var z = startPos.z + Mathf.RoundToInt(radius * Mathf.Sin(angle));
                var y = startPos.y + i / 4;
                blocksToPlace.Add(new IntVector((short)x, (short)y, (short)z));
            }
        }
        else if (buildMode == 4)
        {
            IntVector bp = startPos;

            if (targetModel != null && MVGameControllerBase.Game?.MVPlayerContainer != null)
            {
                if (_playerIds != null && _playerIds.Length > 0 && _selectedPlayerIdx < _playerIds.Length)
                {
                    int targetId = _playerIds[_selectedPlayerIdx];
                    MVPlayer targetPlayer = null;

                    if (targetId == -1)
                    {
                        var validPlayers = new List<MVPlayer>();
                        int myActorId = MVGameControllerBase.Game.LocalPlayer.ActorNr;

                        foreach (var p in MVGameControllerBase.Game.MVPlayerContainer.Values)
                        {
                            if (p != null && p.ActorNr != myActorId)
                                validPlayers.Add(p);
                        }

                        if (validPlayers.Count > 0)
                            targetPlayer = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
                    }
                    else
                    {
                        if (MVGameControllerBase.Game.MVPlayerContainer.ContainsKey(targetId))
                            targetPlayer = MVGameControllerBase.Game.MVPlayerContainer[targetId];
                    }

                    if (targetPlayer != null && MVGameControllerBase.WOCM != null &&
                        targetPlayer.SpawnRolesManager != null)
                    {
                        var avatarWoc =
                            MVGameControllerBase.WOCM.GetWorldObjectClient(targetPlayer.SpawnRolesManager.SpawnRoleId);
                        if (avatarWoc != null && avatarWoc.GameObject != null)
                        {
                            Vector3 playerPosition = avatarWoc.GameObject.transform.position;
                            bp = SharedCubeFunctions.WorldToLocal(targetModel.gameObject, playerPosition, false);
                        }
                    }
                }
            }

            short s = (short)trapScale;
            short h1 = (short)trapHeight;
            short h2 = (short)(trapHeight * 2);

            blocksToPlace.Add(new IntVector((short)(bp.x + 0 * s), (short)(bp.y + h1), (short)(bp.z + 0 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 3 * s), (short)(bp.y + h1), (short)(bp.z + 0 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 0 * s), (short)(bp.y + h1), (short)(bp.z + 1 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 3 * s), (short)(bp.y + h1), (short)(bp.z + 1 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 2 * s), (short)(bp.y + h1), (short)(bp.z + 2 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 1 * s), (short)(bp.y + h1), (short)(bp.z + 2 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 2 * s), (short)(bp.y + h1), (short)(bp.z - 1 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 1 * s), (short)(bp.y + h1), (short)(bp.z - 1 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 2 * s), (short)(bp.y + h2), (short)(bp.z + 0 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 1 * s), (short)(bp.y + h2), (short)(bp.z + 0 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 2 * s), (short)(bp.y + h2), (short)(bp.z + 1 * s)));
            blocksToPlace.Add(new IntVector((short)(bp.x + 1 * s), (short)(bp.y + h2), (short)(bp.z + 1 * s)));
        }
        else if (buildMode == 5)
        {
            LoadImportCacheIfNeeded();

            foreach (var pos in _cachedImportPositions)
            {
                int rx = pos.x;
                int ry = pos.y;
                int rz = pos.z;

                if (importRotation == 1)
                {
                    rx = -pos.z;
                    rz = pos.x;
                }
                else if (importRotation == 2)
                {
                    rx = -pos.x;
                    rz = -pos.z;
                }
                else if (importRotation == 3)
                {
                    rx = pos.z;
                    rz = -pos.x;
                }

                rx += importOffset.x;
                ry += importOffset.y;
                rz += importOffset.z;

                var finalPos = new IntVector((short)(startPos.x + right.x * rx + up.x * ry + fwd.x * rz),
                                             (short)(startPos.y + right.y * rx + up.y * ry + fwd.y * rz),
                                             (short)(startPos.z + right.z * rx + up.z * ry + fwd.z * rz));
                blocksToPlace.Add(finalPos);
            }
        }

        return blocksToPlace;
    }

    public static void StartBuildingStructureAtPlayer()
    {
        if (_isCurrentlyBuilding)
            return;

        if (!MVGameControllerBase.IsInitialized || MVGameControllerBase.Game == null)
            return;
        var lp = MVGameControllerBase.Game.LocalPlayer;
        if (lp == null || !lp.IsReady || lp.AvatarLocal == null)
            return;

        MVCubeModelBase targetModel =
            MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>()
                ?.TryCast<MVCubeModelBase>();
        if (targetModel == null)
            return;

        IntVector playerGridPos = SharedCubeFunctions.WorldToLocal(targetModel.gameObject,
                                                                   lp.AvatarLocal.gameObject.transform.position, false);

        byte safeMatId = GetSafeMaterialId(21);

        Cube templateCube =
            new Cube(CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners), Cube.CreateMaterialArray(safeMatId));

        _isCurrentlyBuilding = true;
        RecordPlacement();
        MelonLoader.MelonCoroutines.Start(BuildStructureCoroutine(targetModel, playerGridPos, templateCube));
    }

    public static IEnumerator BuildStructureCoroutine(MVCubeModelBase target, IntVector rootPos, CubeBase templateCube)
    {
        List<IntVector> blocksToPlace = CalculateBlocksToPlace(rootPos, target);
        float safeDelay = Mathf.Max(cubePlacerDelay, 0.01f);
        int placed = 0;

        foreach (var pos in blocksToPlace)
        {
            if (buildMode != 4 && buildMode != 5 && pos.x == rootPos.x && pos.y == rootPos.y && pos.z == rootPos.z &&
                placementOffset == 0)
                continue;

            while (_placementHistory.Count > 0 && Time.time - _placementHistory.Peek() > antiLimitPauseDuration)
            {
                _placementHistory.Dequeue();
            }

            if (_placementHistory.Count >= antiLimitMaxBlocks)
            {
                CubePlacerStatusMessage = "bcjK0lfb4rkj";
                while (_placementHistory.Count >= antiLimitMaxBlocks)
                {
                    if (Time.time - _placementHistory.Peek() > antiLimitPauseDuration)
                    {
                        _placementHistory.Dequeue();
                    }
                    yield return null;
                }
            }

            try
            {
                _isModPlacingCube = true;
                target.AddCube(pos, templateCube);
                target.HandleDelta();
                _isModPlacingCube = false;
                RecordPlacement();
                placed++;
            }
            catch
            {
                _isModPlacingCube = false;
            }

            CubePlacerStatusMessage = $"xYCiCMmDledn";
            yield return new WaitForSeconds(safeDelay);
        }

        CubePlacerStatusMessage = "Mhpwmn2UN4fI";
        _isCurrentlyBuilding = false;
    }

    public static void OnUpdate()
    {
        if (MVGameControllerBase.instance == null || MVGameControllerBase.Game == null)
            return;
        if (MVGameControllerBase.JoinState != MVJoinState.Playing)
            return;

        if (MVGameControllerBase.Game.LocalPlayer == null || !MVGameControllerBase.Game.LocalPlayer.IsReady)
            return;
        if (MVGameControllerBase.WOCM == null || MVGameControllerBase.MainCameraManager == null)
            return;

        if (!MVGameControllerBase.MainCameraManager.IsCameraControllerSet())
            return;
        if (MVGameControllerBase.MainCameraManager.CurrentCamera == null)
            return;

        if (!SmartBuilderEnabled)
            return;

        HandleSelectionInput();
        DrawSelectionBox();
    }
}
}

--- FILE: Features\TerrainExportImport.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using ImGuiNET;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TestMod.Helpers;
using UnityEngine;
using BytePacker = Il2CppMV.WorldObject.BytePacker;

namespace TestMod.Features;

public static class TerrainExportImport
{
    public static string status = "1gtjQhkZJOSC";
    public static bool useAltBot = false;

    public static readonly string exportPath = Path.Combine(MelonLoader.MelonUtils.GameDirectory, "yWFxiwa8hmrH");

    private static int newId = -1;
    private static bool gotResp = false;

    public static int cubesPerTick = 1;
    public static float tickDelay = 0.05f;
    public static float importPauseDelay = 5.0f;

    public static bool fastBatchMode = false;
    public static int fastBatchSize = 4000;

    public static void startExp()
    {
if (MVGameControllerBase.GameMode != MVGameMode.Play)
        {
            status = "gsjhJp9uspCD";
return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(runExp());
    }

    public static void startImp(string p, bool w)
    {
if (MVGameControllerBase.GameMode != MVGameMode.Edit)
        {
            status = "Xpz6YxgGlVIg";
return;
        }
        if (!File.Exists(p))
        {
            status = "nwdt4p9N775x";
return;
        }
        UnityMainThreadDispatcher.Instance.Enqueue(runImp(p, w));
    }

    private static IEnumerator runExp()
    {
status = "TLuh5T2vSSH5";
        yield return null;

        var dat = new TEI_CubeData { tCubes = new Dictionary<IntVector, Cube>(), protos = new List<TEI_ProtoData>(),
                                     objs = new List<TEI_ObjData>() };

        try
        {
var terr = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelPrototypeTerrain>();
            MVCubeModelBase terrBase = terr;
            if (terrBase == null)
            {
                terrBase = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>();
            }
            if (terrBase == null)
            {
foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
                {
                    if (wo.HasInteractionFlag(InteractionFlags.IsTerrain))
                    {
                        terrBase = wo.TryCast<MVCubeModelBase>();
                        if (terrBase != null)
                            break;
                    }
                }
            }

            if (terrBase != null)
            {
var d = getDict(terrBase);
                foreach (var kv in d)
                    dat.tCubes.Add(kv.Key, kv.Value);
}
            else
            {
}
var all = MVGameControllerBase.WOCM.worldObjects.Values;
            var up = new HashSet<int>();

            foreach (var wo in all)
            {
                if (wo == null || wo.HasInteractionFlag(InteractionFlags.IsTerrain) ||
                    wo.WorldObjectType != WorldObjectType.CubeModel)
                    continue;
                if (wo.Id == MVGameControllerBase.WOCM.RootGroup.Id)
                    continue;

                var cm = wo.TryCast<MVCubeModelInstance>();
                if (cm?.PrototypeCubeModel == null)
                    continue;

                var o = new TEI_ObjData { oid = wo.Id,
                                          type = (int)wo.WorldObjectType,
                                          gid = wo.GroupId,
                                          pos = wo.Position,
                                          rot = wo.Rotation,
                                          scl = wo.Scale,
                                          data = cleanDict(wo.Data) };
                dat.objs.Add(o);

                int pid = cm.PrototypeCubeModel.PrototypeId;
                if (!up.Contains(pid))
                {
                    up.Add(pid);
                    var pd = new TEI_ProtoData { id = pid, cubes = new Dictionary<IntVector, Cube>() };
                    var raw = getDict(cm);
                    foreach (var kv in raw)
                        pd.cubes.Add(kv.Key, kv.Value);
                    dat.protos.Add(pd);
                }
            }

            var bp = new BytePacker();

            bp.Write(dat.tCubes.Count);
            foreach (var kv in dat.tCubes)
            {
                bp.Write(kv.Key.x);
                bp.Write(kv.Key.y);
                bp.Write(kv.Key.z);
                bp.Write(kv.Value.ByteCorners);
                bp.Write(kv.Value.FaceMaterials);
            }

            bp.Write(dat.protos.Count);
            foreach (var p in dat.protos)
            {
                bp.Write(p.id);
                bp.Write(p.cubes.Count);
                foreach (var kv in p.cubes)
                {
                    bp.Write(kv.Key.x);
                    bp.Write(kv.Key.y);
                    bp.Write(kv.Key.z);
                    bp.Write(kv.Value.ByteCorners);
                    bp.Write(kv.Value.FaceMaterials);
                }
            }

            bp.Write(dat.objs.Count);
            foreach (var o in dat.objs)
            {
                bp.Write(o.oid);
                bp.Write(o.type);
                bp.Write(o.gid);
                bp.Write(o.pos.x);
                bp.Write(o.pos.y);
                bp.Write(o.pos.z);
                bp.Write(o.rot.x);
                bp.Write(o.rot.y);
                bp.Write(o.rot.z);
                bp.Write(o.rot.w);
                bp.Write(o.scl.x);
                bp.Write(o.scl.y);
                bp.Write(o.scl.z);
                writeObj(bp, o.data);
            }

            Directory.CreateDirectory(exportPath);
            string fname = $"py9xxGyUFlvz";
            var f = Path.Combine(exportPath, fname);

            File.WriteAllText(f, Convert.ToBase64String(bp.ToArray()));
            status = $"LDmPdzAz2uMV";
}
        catch (Exception e)
        {
            status = $"l11ujfGH0zVJ";
}
    }

    private static IEnumerator runImp(string p, bool wipe)
    {
status = "7wFKTeTUw8Yy";
        yield return null;

        TEI_CubeData dat;
        try
        {
var b = Convert.FromBase64String(File.ReadAllText(p));
            var bp = new BytePacker(b);
            dat = new TEI_CubeData { tCubes = new Dictionary<IntVector, Cube>(), protos = new List<TEI_ProtoData>(),
                                     objs = new List<TEI_ObjData>() };

            int tc = bp.ReadInt32();
            for (int i = 0; i < tc; i++)
            {
                var iv = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                var c = new Cube(bp.ReadBytes(8), bp.ReadBytes(6));
                dat.tCubes[iv] = c;
            }
int pc = bp.ReadInt32();
            for (int i = 0; i < pc; i++)
            {
                var pr = new TEI_ProtoData { id = bp.ReadInt32(), cubes = new Dictionary<IntVector, Cube>() };
                int cc = bp.ReadInt32();
                for (int j = 0; j < cc; j++)
                {
                    var iv = new IntVector(bp.ReadInt16(), bp.ReadInt16(), bp.ReadInt16());
                    var c = new Cube(bp.ReadBytes(8), bp.ReadBytes(6));
                    pr.cubes[iv] = c;
                }
                dat.protos.Add(pr);
            }
int oc = bp.ReadInt32();
            for (int i = 0; i < oc; i++)
            {
                var o = new TEI_ObjData { oid = bp.ReadInt32(),
                                          type = bp.ReadInt32(),
                                          gid = bp.ReadInt32(),
                                          pos = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                                          rot = new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(),
                                                               bp.ReadSingle()),
                                          scl = new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle()),
                                          data = readObj(bp) as Dictionary<object, object> };
                dat.objs.Add(o);
            }
}
        catch (Exception e)
        {
            status = $"rnIi3VgcKMBB";
yield break;
        }
MVCubeModelBase terr = null;
        foreach (var wo in MVGameControllerBase.WOCM.worldObjects.Values)
        {
            if (wo.HasInteractionFlag(InteractionFlags.IsTerrain))
            {
                terr = wo.TryCast<MVCubeModelBase>();
                if (terr != null)
                    break;
            }
        }

        if (terr == null)
        {
            terr = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelPrototypeTerrain>();
            if (terr == null)
                terr = MVGameControllerBase.WOCM.GetSingletonWorldObject<MVCubeModelFineGrainedTerrain>();
        }

        if (terr == null)
        {
            status = "SIO2SYO9Egqs";
yield break;
        }
terr.MakeUnique();

        if (wipe)
        {
status = "lTGyLPht3tB5";
            var keys = new List<IntVector>();
            if (terr.PrototypeCubeModel?.Chunks != null)
            {
                foreach (var ch in terr.PrototypeCubeModel.Chunks.Values)
                    foreach (var k in ch.cells.Keys)
                        keys.Add(k);
            }
int c = 0;
            for (int i = 0; i < keys.Count; i++)
            {
                terr.RemoveCube(keys[i]);
                c++;

                if (fastBatchMode)
                {
                    if (c >= fastBatchSize)
                    {
                        terr.HandleDelta();
                        status = $"86PgbWlHCpQY";
                        yield return new WaitForSeconds(importPauseDelay);
                        c = 0;
                    }
                }
                else
                {
                    if (c >= cubesPerTick)
                    {
                        terr.HandleDelta();
                        status = $"oncKa8mcM3JT";
                        yield return new WaitForSeconds(tickDelay);
                        c = 0;
                    }
                }
            }
            if (c > 0)
            {
                terr.HandleDelta();
            }

            status = "SeSfWFE6Pj4A";
yield return new WaitForSeconds(importPauseDelay);
        }

        var pLook = new Dictionary<int, TEI_ProtoData>();
        foreach (var pr in dat.protos)
            pLook[pr.id] = pr;
        var world = MVGameControllerBase.Game.World;

        status = "OBkJAIuEQkNR";
int createdCount = 0;
        foreach (var o in dat.objs)
        {
            int pid = -1;
            if (o.data != null)
            {
                if (o.data.ContainsKey("uoNMtXPjZCk4"))
                    pid = Convert.ToInt32(o.data["IF6i16uNpflR"]);
                else if (o.data.ContainsKey(1))
                    pid = Convert.ToInt32(o.data[1]);
            }
            if (pid <= 0 || !pLook.ContainsKey(pid))
                continue;

            gotResp = false;
            newId = -1;
            Il2CppSystem.EventHandler<InitializedGameQueryDataEventArgs> h = null;
            h = new Action<Il2CppSystem.Object, InitializedGameQueryDataEventArgs>(
                (s, e) =>
                {
                    if (e.InstigatorActorNumber == MVGameControllerBase.Game.LocalPlayer.ActorNr)
                    {
                        if (Vector3.Distance(e.RootWO.Position, o.pos) < 0.1f)
                        {
                            newId = e.RootWO.Id;
                            gotResp = true;
                            if (h != null)
                                world.InitializedGameQueryData -= h;
                        }
                    }
                });
            world.InitializedGameQueryData += h;

            var d = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
            d.Add((Il2CppSystem.Object)(byte)1, (Il2CppSystem.Object)o.scl.x);
            d.Add((Il2CppSystem.Object)(byte)2, (Il2CppSystem.Object)(byte)21);
            d.Add((Il2CppSystem.Object)(byte)3, (Il2CppSystem.Object)MVGameControllerBase.Game.LocalPlayer.ProfileID);

            MVGameControllerBase.OperationRequests.RequestBuiltInItem(
                BuiltInItem.CubeModel, MVGameControllerBase.WOCM.RootGroup.Id, d, o.pos, o.rot, o.scl, true, false);

            float to = Time.realtimeSinceStartup + 5f;
            while (!gotResp && Time.realtimeSinceStartup < to)
                yield return null;

            if (!gotResp && h != null)
            {
                world.InitializedGameQueryData -= h;
            }

            if (newId != -1)
            {
                var nm = MVGameControllerBase.WOCM.GetWorldObjectClient(newId)?.TryCast<MVCubeModelInstance>();
                if (nm != null)
                {
                    nm.MakeUnique();
                    var pd = pLook[pid];
                    int cnt = 0;
                    foreach (var kv in pd.cubes)
                    {
                        nm.AddCube(kv.Key, fixCube(kv.Value));
                        cnt++;

                        if (fastBatchMode)
                        {
                            if (cnt >= fastBatchSize)
                            {
                                nm.HandleDelta();
                                yield return new WaitForSeconds(importPauseDelay);
                                cnt = 0;
                            }
                        }
                        else
                        {
                            if (cnt >= cubesPerTick)
                            {
                                nm.HandleDelta();
                                yield return new WaitForSeconds(tickDelay);
                                cnt = 0;
                            }
                        }
                    }
                    if (cnt > 0)
                        nm.HandleDelta();
                    createdCount++;
                }
            }
            yield return null;
        }
status = "6nPAIf2k58nR";
int tcnt = 0;
        int totalPlaced = 0;
        foreach (var kv in dat.tCubes)
        {
            terr.AddCube(kv.Key, fixCube(kv.Value));
            tcnt++;
            totalPlaced++;

            if (fastBatchMode)
            {
                if (tcnt >= fastBatchSize)
                {
                    terr.HandleDelta();
                    status = $"rdkopo6qd61j";
                    yield return new WaitForSeconds(importPauseDelay);
                    tcnt = 0;
                }
            }
            else
            {
                if (tcnt >= cubesPerTick)
                {
                    terr.HandleDelta();
                    status = $"yZsQHlIyKpKu";
                    yield return new WaitForSeconds(tickDelay);
                    tcnt = 0;
                }
            }
        }
        if (tcnt > 0)
        {
            terr.HandleDelta();
        }

        status = "BVht5vAAvYVi";
}

    private static Cube fixCube(Cube c)
    {
        var m = c.FaceMaterials;
        if (m != null && m.Length == 6)
        {
            byte m1 = m[0];
            bool uni = true;
            for (int i = 1; i < 6; i++)
                if (m[i] != m1)
                    uni = false;
            if (uni)
            {
                byte bot = useAltBot ? (m1 == 23 ? (byte)21 : (byte)23) : m1;
                return new Cube(c.ByteCorners, new byte[] { m1, bot, m1, m1, m1, m1 });
            }
        }
        return c;
    }

    private static Il2CppSystem.Collections.Generic.Dictionary<IntVector, Cube> getDict(MVCubeModelBase m)
    {
        var d = new Il2CppSystem.Collections.Generic.Dictionary<IntVector, Cube>();
        if (m == null || m.PrototypeCubeModel == null)
            return d;

        if (m.PrototypeCubeModel.Chunks == null)
            return d;

        foreach (var ch in m.PrototypeCubeModel.Chunks.Values)
        {
            if (ch?.cells == null)
                continue;
            var k = new List<IntVector>();
            foreach (var key in ch.cells.Keys)
                k.Add(key);

            foreach (var p in k)
            {
                try
                {
                    var c = m.PrototypeCubeModel.GetCube(p);
                    if (c != null && !d.ContainsKey(p))
                        d.Add(p, Cube.Clone(c));
                }
                catch
                {
                }
            }
        }
        return d;
    }

    private static Dictionary<object, object> cleanDict(
        Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> src)
    {
        if (src == null)
            return new Dictionary<object, object>();
        var d = new Dictionary<object, object>();
        foreach (var p in src)
        {
            var k = cleanObj(p.Key);
            if (k == null)
                continue;
            var v = cleanObj(p.Value);
            d[k] = v;
        }
        return d;
    }

    private static object cleanObj(object o)
    {
        if (o == null)
            return null;
        if (o is string || o is int || o is float || o is bool || o is byte || o is Vector3 || o is Quaternion ||
            o is IntVector)
            return o;

        var t = o.GetType().FullName;
        if (t.Contains("pdsEMFFLo1V7"))
            return Convert.ToInt32(o);
        if (t.Contains("FPqiVtyNyRRE"))
            return Convert.ToSingle(o);
        if (t.Contains("cNKmqXDfAWsI"))
            return Convert.ToBoolean(o);
        if (t.Contains("jtJzJAde9a1Z"))
            return Convert.ToString(o);
        if (t.Contains("ZWMMEnA66Wop"))
            return Convert.ToByte(o);

        return null;
    }

    private enum DT : byte
    {
        Nul,
        Str,
        Int,
        Flt,
        Bol,
        Vec,
        Qut,
        Dct
    }

    private static void writeObj(BytePacker bp, object o)
    {
        if (o == null)
        {
            bp.Write((byte)DT.Nul);
            return;
        }
        if (o is string s)
        {
            bp.Write((byte)DT.Str);
            var b = System.Text.Encoding.UTF8.GetBytes(s);
            bp.Write(b.Length);
            bp.Write(b);
        }
        else if (o is int i)
        {
            bp.Write((byte)DT.Int);
            bp.Write(i);
        }
        else if (o is float f)
        {
            bp.Write((byte)DT.Flt);
            bp.Write(f);
        }
        else if (o is bool b)
        {
            bp.Write((byte)DT.Bol);
            bp.Write(b ? (byte)1 : (byte)0);
        }
        else if (o is Vector3 v)
        {
            bp.Write((byte)DT.Vec);
            bp.Write(v.x);
            bp.Write(v.y);
            bp.Write(v.z);
        }
        else if (o is Quaternion q)
        {
            bp.Write((byte)DT.Qut);
            bp.Write(q.x);
            bp.Write(q.y);
            bp.Write(q.z);
            bp.Write(q.w);
        }
        else if (o is Dictionary<object, object> d)
        {
            bp.Write((byte)DT.Dct);
            bp.Write(d.Count);
            foreach (var p in d)
            {
                writeObj(bp, p.Key);
                writeObj(bp, p.Value);
            }
        }
        else
            bp.Write((byte)DT.Nul);
    }

    private static object readObj(BytePacker bp)
    {
        var t = (DT)bp.ReadByte();
        switch (t)
        {
        case DT.Str:
            int l = bp.ReadInt32();
            return System.Text.Encoding.UTF8.GetString(bp.ReadBytes(l));
        case DT.Int:
            return bp.ReadInt32();
        case DT.Flt:
            return bp.ReadSingle();
        case DT.Bol:
            return bp.ReadByte() == 1;
        case DT.Vec:
            return new Vector3(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle());
        case DT.Qut:
            return new Quaternion(bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle(), bp.ReadSingle());
        case DT.Dct:
            int c = bp.ReadInt32();
            var d = new Dictionary<object, object>();
            for (int i = 0; i < c; i++)
            {
                var k = readObj(bp);
                var v = readObj(bp);
                if (k != null)
                    d[k] = v;
            }
            return d;
        default:
            return null;
        }
    }
}
[Serializable]
public class TEI_CubeData
{
    public Dictionary<IntVector, Cube> tCubes;
    public List<TEI_ProtoData> protos;
    public List<TEI_ObjData> objs;
}

[Serializable]
public class TEI_ProtoData
{
    public int id;
    public Dictionary<IntVector, Cube> cubes;
}

[Serializable]
public class TEI_ObjData
{
    public int oid;
    public int type;
    public int gid;
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scl;
    public Dictionary<object, object> data;
}

--- FILE: Features\WeaponEquipper.cs ---
﻿using System;
using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using Il2CppMV.WorldObject;
using Il2CppMV.Common;
using TestMod.Helpers;

namespace TestMod.Features
{
public class WeaponForcePlayMode
{
    private static int _selectedWeaponIndex = 0;
    private static string _status = "bGGOmnzRyE5k";

    private struct WeaponOption
    {
        public string Name;
        public AvatarItemType Type;
        public int VariantID;
    }
    private static readonly System.Collections.Generic
        .List<WeaponOption> Weapons = new System.Collections.Generic.List<WeaponOption>() {
        
        new WeaponOption { Name = "TkSnjAaNG2US", Type = AvatarItemType.Bazooka, VariantID = 10355 },        
        new WeaponOption { Name = "TWU8lVAqjFGO", Type = AvatarItemType.CenterGun, VariantID = 10353 },  
        new WeaponOption { Name = "0Pb7kiKI6g5C", Type = AvatarItemType.Shotgun, VariantID = 10360 },        
        new WeaponOption { Name = "iX6gMfX5afJM", Type = AvatarItemType.RailGun, VariantID = 10356 },       
        new WeaponOption { Name = "kJWExMjyIrAC", Type = AvatarItemType.ImpulseGun, VariantID = 10354 }, 
        new WeaponOption { Name = "lZFfIJP0nH03", Type = AvatarItemType.Flamethrower,
                           VariantID = 10359 },                                                       
        new WeaponOption { Name = "xEjGfQmTC89v", Type = AvatarItemType.SixShooter, VariantID = 239796 }, 
        new WeaponOption { Name = "juf1NFdlwm8c", Type = AvatarItemType.DoubleSixShooter,
                           VariantID = 239800 }, 

        
        new WeaponOption { Name = "zCxgwTRuLiR2", Type = AvatarItemType.MeleeWeapon, VariantID = 12435368 }, 
        new WeaponOption { Name = "n2P7oJ636gDM", Type = AvatarItemType.ThrowingStar,
                           VariantID = 1165835 }, 
        new WeaponOption { Name = "nxQfB4NU4pni", Type = AvatarItemType.MultiThrowingStar,
                           VariantID = 1165838 }, 

        
        new WeaponOption { Name = "3uGFRStfsZXV", Type = AvatarItemType.HealRay, VariantID = 7690141 }, 
        new WeaponOption { Name = "aMZ7vmS6qhqP", Type = AvatarItemType.CubeGun,
                           VariantID = 46140 }, 
        new WeaponOption { Name = "b9PY8Xxu2m7w", Type = AvatarItemType.MouseGun, VariantID = 3785736 },   
        new WeaponOption { Name = "MkMoTNizgW47", Type = AvatarItemType.GrowthGun, VariantID = 3785733 }, 

        
        new WeaponOption { Name = "USZPSDH7Ps03", Type = AvatarItemType.CustomGun,
                           VariantID = 12730220 } 
    };

    public static void RenderUI()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "rLn67Trmu2xV");
        ImGui.Text($"zUuc22JW51yh");

        string[] names = new string[Weapons.Count];
        for (int i = 0; i < Weapons.Count; i++)
            names[i] = Weapons[i].Name;

        ImGui.Combo("0WAUF5yPQTIG", ref _selectedWeaponIndex, names, names.Length);

        if (ImGui.Button("LYMEWUesnxCy"))
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                                       { ForceEquip(Weapons[_selectedWeaponIndex]); });
        }
    }

    private static void ForceEquip(WeaponOption weapon)
    {
        var player = MVGameControllerBase.LocalPlayer;
        if (player == null)
            return;

        var equipable = player.AvatarLocal.avatarEquipable;
        if (equipable == null)
        {
            _status = "e6OUa4Xl8QhR";
            return;
        }

        try
        {
            var itemData = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>();
            AvatarEquipableType equipType = AvatarEquipableType.Weapon;
            equipable.Equip(weapon.Type, equipType, null, weapon.VariantID);

            _status = $"ZySVPjsybayD";
        }
        catch (Exception ex)
        {
            _status = "mQRBQNrflbll";
}
    }
}
}

--- FILE: Helpers\AntiBan.cs ---
﻿using HarmonyLib;
using Il2Cpp;
using Il2CppMV.Common;
using System;

namespace TestMod.Helpers
{
    [HarmonyPatch]
    internal static class AntiBan
    {
        
        [HarmonyPatch(typeof(CheatHandling), "gzEw7rzaG2D9")]
        [HarmonyPatch(typeof(CheatHandling), "DsUWXFemnQJW")]
        [HarmonyPatch(typeof(CheatHandling), "4bKVqU3641Ax")]
        [HarmonyPatch(typeof(CheatHandling), "cET2PyFbow2c")]
        [HarmonyPatch(typeof(CheatHandling), "VRR3MzSTnIvf")]
        
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "NOXg4Qi3ztb2", new Type[] { typeof(int), typeof(MVPlayer), typeof(string) })]
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "u7gPlryNh7I8")]
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Kfe1EkdhPV6L")]
        [HarmonyPrefix]
        private static bool noBan()
        {
            return false;
        }
    }
}
--- FILE: Helpers\Il2CppDictionaryHelper.cs ---
﻿using System;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;

public static class Il2CppDictionaryHelper
{
    public static unsafe Il2CppSystem.Object BoxInt(int value)
    {
        IntPtr intClassPtr = IL2CPP.GetIl2CppClass("fYDcKbt6HqVN", "FTA7KABtriWw", "DNz51zXwU7pe");
        IntPtr boxedVal = IL2CPP.il2cpp_value_box(intClassPtr, (IntPtr)(&value));
        return new Il2CppSystem.Object(boxedVal);
    }

    public static unsafe int UnboxInt(Il2CppSystem.Object obj)
    {
        if (obj == null || obj.Pointer == IntPtr.Zero)
            return 0;

        int *ptr = (int *)IL2CPP.il2cpp_object_unbox(obj.Pointer);
        return *ptr;
    }

    public static Il2CppSystem.Object BoxString(string value)
    {
        if (value == null)
            return null;
        return new Il2CppSystem.String(IL2CPP.ManagedStringToIl2Cpp(value));
    }

    public static Il2CppSystem.Object GetKeyByName(Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> dict,
                                                   string targetKey)
    {
        if (dict == null)
            return null;

        var enumerator = dict.Keys.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current != null && enumerator.Current.ToString() == targetKey)
            {
                return enumerator.Current;
            }
        }
        return null;
    }

    public static unsafe void SetIntInPlace(Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> dict, string keyName,
                                            int value)
    {
        if (dict == null)
            return;

        var existingKey = GetKeyByName(dict, keyName);
        if (existingKey != null && dict.ContainsKey(existingKey))
        {
            var targetObj = dict[existingKey];
            if (targetObj != null && targetObj.Pointer != IntPtr.Zero)
            {
                int *ptr = (int *)IL2CPP.il2cpp_object_unbox(targetObj.Pointer);
                *ptr = value;
            }
        }
        else
        {
            dict[BoxString(keyName)] = BoxInt(value);
        }
    }

    public static int GetInt(Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> dict, string keyName,
                             int fallback = 0)
    {
        if (dict == null)
            return fallback;

        var key = GetKeyByName(dict, keyName);
        if (key != null && dict.ContainsKey(key))
        {
            return UnboxInt(dict[key]);
        }
        return fallback;
    }

    public static unsafe void SetEnumInPlace<TEnum>(Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> dict,
                                                    string keyName, TEnum newValue)
        where TEnum : Enum
    {
        if (dict == null)
            return;

        var existingKey = GetKeyByName(dict, keyName);
        if (existingKey != null && dict.ContainsKey(existingKey))
        {
            var targetObj = dict[existingKey];
            if (targetObj != null && targetObj.Pointer != IntPtr.Zero)
            {
                int intValue = (int)(object)newValue;
                int *ptr = (int *)IL2CPP.il2cpp_object_unbox(targetObj.Pointer);
                *ptr = intValue;
            }
        }
    }

    public static Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> TryGetNestedDictionary(
        Dictionary<Il2CppSystem.Object, Il2CppSystem.Object> dict, string keyName)
    {
        if (dict == null)
            return null;

        var key = GetKeyByName(dict, keyName);
        if (key != null && dict.ContainsKey(key))
        {
            var nestedObj = dict[key];
            if (nestedObj != null)
            {
                return nestedObj.TryCast<Dictionary<Il2CppSystem.Object, Il2CppSystem.Object>>();
            }
        }
        return null;
    }
}

--- FILE: Helpers\Il2CppGeneralHelper.cs ---
﻿using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;


namespace System.Runtime.CompilerServices
{
internal sealed class IsUnmanagedAttribute : Attribute
{
}
}

namespace TestMod.Helpers
{
public static unsafe class Il2CppGeneralHelper
{
#region String Marshalling

    
    
    
    public static IntPtr StringToNative(string value)
    {
        if (value == null)
            return IntPtr.Zero;
        return IL2CPP.ManagedStringToIl2Cpp(value);
    }

    
    
    
    public static string StringToManaged(IntPtr il2cppStringPtr)
    {
        if (il2cppStringPtr == IntPtr.Zero)
            return null;
        return IL2CPP.Il2CppStringToManaged(il2cppStringPtr);
    }

#endregion

#region Native Object Instantiation

    
    
    
    public static IntPtr AllocateNativeObject(IntPtr classPointer)
    {
        if (classPointer == IntPtr.Zero)
            throw new ArgumentNullException(nameof(classPointer));
        return IL2CPP.il2cpp_object_new(classPointer);
    }

    
    
    
    public static IntPtr GetClassPointer(string assemblyName, string nameSpace, string className)
    {
        return IL2CPP.GetIl2CppClass(assemblyName, nameSpace, className);
    }

#endregion

#region Unmanaged Memory &Boxing

    
    
    
    public static T Unbox<T>(IntPtr objectPtr)
        where T : unmanaged
    {
        if (objectPtr == IntPtr.Zero)
            return default;
        void *rawPtr = (void *)IL2CPP.il2cpp_object_unbox(objectPtr);
        return *(T *)rawPtr;
    }

    
    
    
    
    public static IntPtr Box<T>(IntPtr classPtr, T value)
        where T : unmanaged
    {
        return IL2CPP.il2cpp_value_box(classPtr, (IntPtr)(&value));
    }

#endregion

#region Field Modification &GC Barriers

    
    
    
    
    
    public static void SetObjectFieldSafe(IntPtr targetObject, int fieldOffset, IntPtr valueToAssign)
    {
        if (targetObject == IntPtr.Zero)
            return;

        
        IntPtr targetAddress = targetObject + fieldOffset;
        IL2CPP.il2cpp_gc_wbarrier_set_field(targetObject, targetAddress, valueToAssign);
    }

    
    
    
    public static T ReadPrimitiveField<T>(IntPtr targetObject, int fieldOffset)
        where T : unmanaged
    {
        if (targetObject == IntPtr.Zero)
            return default;
        IntPtr fieldAddress = targetObject + fieldOffset;
        return *(T *)fieldAddress;
    }

    
    
    
    
    public static void WritePrimitiveField<T>(IntPtr targetObject, int fieldOffset, T value)
        where T : unmanaged
    {
        if (targetObject == IntPtr.Zero)
            return;
        IntPtr fieldAddress = targetObject + fieldOffset;
        *(T *)fieldAddress = value;
    }

#endregion

#region Array Generators

    
    
    
    public static Il2CppReferenceArray<T> CreateReferenceArray<T>(T[] managedArray)
        where T : Il2CppObjectBase
    {
        if (managedArray == null)
            return null;
        var nativeArray = new Il2CppReferenceArray<T>(managedArray.Length);
        for (int i = 0; i < managedArray.Length; i++)
        {
            nativeArray[i] = managedArray[i];
        }
        return nativeArray;
    }

    
    
    
    public static Il2CppStringArray CreateStringArray(string[] managedArray)
    {
        if (managedArray == null)
            return null;
        var nativeArray = new Il2CppStringArray(managedArray.Length);
        for (int i = 0; i < managedArray.Length; i++)
        {
            nativeArray[i] = managedArray[i];
        }
        return nativeArray;
    }

    
    
    
    public static Il2CppStructArray<T> CreateStructArray<T>(T[] managedArray)
        where T : unmanaged
    {
        if (managedArray == null)
            return null;
        var nativeArray = new Il2CppStructArray<T>(managedArray.Length);
        for (int i = 0; i < managedArray.Length; i++)
        {
            nativeArray[i] = managedArray[i];
        }
        return nativeArray;
    }

#endregion
}
}

--- FILE: Helpers\UnityMainThreadDispatcher.cs ---
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace TestMod.Helpers
{
    public class UnityMainThreadDispatcher
    {
        private static readonly UnityMainThreadDispatcher _instance = new UnityMainThreadDispatcher();
        public static UnityMainThreadDispatcher Instance => _instance;

        private readonly Queue<Action> q = new Queue<Action>();
        public void Update()
        {
            lock (q)
            {
                while (q.Count > 0)
                {
                    q.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(IEnumerator action)
        {
            lock (q)
            {
                q.Enqueue(() =>
                {
                    MelonCoroutines.Start(action);
                });
            }
        }

        public void Enqueue(Action action)
        {
            lock (q)
            {
                q.Enqueue(action);
            }
        }
        public Task EnqueueAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        private IEnumerator actWrapper(Action a)
        {
            a();
            yield return null;
        }
    }
}
