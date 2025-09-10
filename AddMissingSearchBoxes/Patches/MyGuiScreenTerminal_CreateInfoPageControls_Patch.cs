using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using VRage.Utils;
using VRageMath;

namespace AddMissingSearchBoxes.Patches
{
    [HarmonyPatch(typeof(MyGuiScreenTerminal), "CreateInfoPageControls")]
    internal static class MyGuiScreenTerminal_CreateInfoPageControls_Patch
    {
        public static string SearchBoxText = "";

        private static void Postfix(MyGuiControlTabPage infoPage)
        {
            if (MyGuiScreenTerminal.InteractedEntity != null)
            {
                return;
            }

            MyGuiControlSearchBox searchBox = new()
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

            MyTerminalInfoController.RequestServerLimitInfo();
        }
    }
}
