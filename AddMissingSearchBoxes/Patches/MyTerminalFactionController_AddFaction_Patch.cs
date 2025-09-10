using HarmonyLib;
using Sandbox.Game.Gui;
using System;
using System.Linq;
using VRage.Game.ModAPI;

namespace AddMissingSearchBoxes.Patches
{
    [HarmonyPatch(typeof(MyTerminalFactionController), "AddFaction")]
    internal static class MyTerminalFactionController_AddFaction_Patch
    {
        public static bool Prefix(IMyFaction faction)
        {
            string[] subStrings = MyGuiScreenTerminal_CreateFactionsPageControls_Patch.SearchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            if (subStrings.All(s => faction.Tag.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return false;
            }

            return true;
        }
    }
}
