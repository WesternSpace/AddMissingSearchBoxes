using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using System;
using System.Linq;
using System.Reflection;
using VRage;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyGuiScreenTerminal), "CreateFactionsPageControls")]
    internal static class FactionsMenuPatch
    {
        private static MyGuiControlTable factionsList;

        private static MyGuiScreenTerminal instance;

        private static string searchBoxtext = "";
        private static void Postfix(MyGuiScreenTerminal __instance, MyGuiControlTabPage page)
        {
            if (!Plugin.Instance.Config.FactionsSearchboxEnabled)
            {
                return;
            }

            instance = __instance;
            MyGuiControlSearchBox searchBox = new MyGuiControlSearchBox
            {
                Position = new Vector2(-0.452f, -0.34f), //Position.Y is inverted, positive numbers move the searchbox down.
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,  
            };
            searchBox.Size = new Vector2(0.29f, searchBox.Size.Y);
            MyGuiControlLabel searchBoxText = (MyGuiControlLabel)AccessTools.Field(typeof(MyGuiControlSearchBox), "m_label").GetValue(searchBox);
            searchBoxText.Text = "Search by tag";
            searchBox.OnTextChanged += SearchBox_OnTextChanged;

            page.Controls.Add(searchBox);

            //Searches for the control elements

            MyGuiControlCombobox combobox = null;

            for (int i = 0; i < page.Controls.Count; i++)
            {
                //Shift the combobox down
                if (page.Controls[i].Name == "FactionFilters")
                {
                    page.Controls[i].Position = new Vector2(-0.452f, searchBox.PositionY + 0.04f);
                    combobox = (MyGuiControlCombobox)page.Controls[i];
                    continue;
                }

                //We hide the "Factions:" header background by moving it offscreen
                if (page.Controls[i].GetType() == typeof(MyGuiControlPanel))
                {
                    if (page.Controls[i].Name != "panelFactionMembersNamePanel")
                    {
                        page.Controls[i].Position = new Vector2(2f, 2f);
                    }  
                    continue;
                }

                //Hide the header text
                if (page.Controls[i].GetType() == typeof(MyGuiControlLabel))
                {
                    MyGuiControlLabel label = (MyGuiControlLabel)page.Controls[i];
                    if (label.Text == MyTexts.GetString(MySpaceTexts.TerminalTab_FactionsTableLabel))
                    {
                        label.Text = "";
                    }
                }

                //Shift the list down
                if (page.Controls[i].Name == "FactionsTable")
                {
                    factionsList = (MyGuiControlTable)page.Controls[i];
                    factionsList.Position = new Vector2(-0.452f, combobox.PositionY + 0.04f);
                    factionsList.VisibleRowsCount = 14;
                    continue;
                }
            }
        }

        private static void SearchBox_OnTextChanged(string newText)
        {
            searchBoxtext = newText;
            FieldInfo factionControllerField = AccessTools.Field(typeof(MyGuiScreenTerminal), "m_controllerFactions");
            object controller = factionControllerField.GetValue(instance);

            MethodInfo filterSelectedMethod = AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalFactionController"), "OnFactionFilterItemSelected");
            filterSelectedMethod.Invoke(controller, null);
        }

        public static bool Prefix_AddFaction(IMyFaction faction)
        {
            string[] subStrings = searchBoxtext.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (subStrings.All(s => faction.Tag.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return false;
            }

            return true;
        }
    }
}
