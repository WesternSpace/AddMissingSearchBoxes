using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyGuiScreenTerminal), "CreateInfoPageControls")]
    internal static class GridInfoMenuPatch
    {
        public static string SearchBoxText = "";

        private static void Postfix(MyGuiControlTabPage infoPage)
        {
            if (MyGuiScreenTerminal.InteractedEntity != null)
            {
                return;
            }

            MyGuiControlSearchBox searchBox = new MyGuiControlSearchBox
            {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Position = new Vector2(-0.452f, -0.332f),
            };
            searchBox.Size = new Vector2(0.29f, searchBox.Size.Y);
            searchBox.OnTextChanged += SearchBox_OnTextChanged;

            infoPage.Controls.Add(searchBox);
        }

        private static void SearchBox_OnTextChanged(string newText)
        {
            SearchBoxText = newText;

            AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalInfoController"), "RequestServerLimitInfo").Invoke(null, null);
        }
    }
}
