using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Utils;
using VRageMath;

namespace AddMissingSearchBoxes.Patches
{
    [HarmonyPatch(typeof(MyGuiScreenTerminal), "CreateFactionsPageControls")]
    internal static class MyGuiScreenTerminal_CreateFactionsPageControls_Patch
    {
        public static string SearchBoxText = "";

        private static void Postfix(MyGuiScreenTerminal __instance, MyGuiControlTabPage page)
        {
            MyGuiControlSearchBox searchBox = new()
            {
                Position = new Vector2(-0.452f, -0.34f), //Position.Y is inverted, positive numbers move the searchbox down.
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,  
            };
            searchBox.Size = new Vector2(0.29f, searchBox.Size.Y);
            searchBox.m_label.Text = "Search by tag";
            searchBox.OnTextChanged += delegate (string newText)
            {
                SearchBoxText = newText;
                __instance.m_controllerFactions.OnFactionFilterItemSelected();
            };

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
                    var factionsList = (MyGuiControlTable)page.Controls[i];
                    factionsList.Position = new Vector2(-0.452f, combobox.PositionY + 0.04f);
                    factionsList.VisibleRowsCount = 14;
                    continue;
                }
            }
        }
    }
}
