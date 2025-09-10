using System;
using HarmonyLib;
using VRage.Plugins;
using System.Reflection;
using AddMissingSearchBoxes.Logging;
using AddMissingSearchBoxes.Patches;

namespace AddMissingSearchBoxes
{
    public class Plugin : IPlugin, IDisposable
    {
        public const string Name = "AddMissingSearchBoxes";

        public IPluginLogger Log => Logger;
        private static readonly IPluginLogger Logger = new PluginLogger(Name);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Log.Info("Loading");
//#if DEBUG
            Harmony.DEBUG = true;
//#endif
            Log.Debug("Applying Harmony patches");
            Harmony patcher = new(Name);

            try
            {
                patcher.PatchAll(Assembly.GetExecutingAssembly());
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshPlayerChatHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshPlayerChatHistory)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshFactionChatHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshFactionChatHistory)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshGlobalChatHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshGlobalChatHistory)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshChatBotHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshChatBotHistory)));
            }
            catch (Exception ex)
            {
                Log.Debug("Error while patching game code.");
                throw ex;
            }

            Log.Debug("Successfully loaded");
        }

        public void Dispose()
        {

        }

        public void Update()
        {
            
        }
    }
}