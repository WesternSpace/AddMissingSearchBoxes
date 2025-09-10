using HarmonyLib;
using Sandbox.Game.Gui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddMissingSearchBoxes.Patches
{
    [HarmonyPatch("Sandbox.Game.Gui.MyTerminalInfoController", "ServerLimitInfo_Received")]
    public static class MyTerminalInfoController_ServerLimitInfo_Received_Patch
    {
        private static bool Prefix(ref List<MyTerminalInfoController.GridBuiltByIdInfo> gridsWithBuiltById)
        {
            string[] subStrings = MyGuiScreenTerminal_CreateInfoPageControls_Patch.SearchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            gridsWithBuiltById.RemoveAll(x => !subStrings.All(s => x.GridName.Contains(s, StringComparison.OrdinalIgnoreCase)));

            return true;
        }
    }
}
