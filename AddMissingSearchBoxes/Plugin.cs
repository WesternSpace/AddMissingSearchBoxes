using System;
using System.IO;
using ClientPlugin.GUI;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using ClientPlugin.Config;
using ClientPlugin.Logging;
using ClientPlugin.Patches;
using VRage.FileSystem;
using VRage.Plugins;
using System.Reflection;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, IDisposable
    {
        public const string Name = "AddMissingSearchBoxes";
        public static Plugin Instance { get; private set; }

        public long Tick { get; private set; }

        public IPluginLogger Log => Logger;
        private static readonly IPluginLogger Logger = new PluginLogger(Name);

        public IPluginConfig Config => config?.Data;
        private PersistentConfig<PluginConfig> config;
        private static readonly string ConfigFileName = $"{Name}.cfg";

        private static bool initialized;
        private static bool failed;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;

            Log.Info("Loading");

            if(!Directory.Exists(Path.Combine(MyFileSystem.UserDataPath, "Storage\\PluginData")))
            {
                Directory.CreateDirectory(Path.Combine(MyFileSystem.UserDataPath, "Storage\\PluginData"));
            }

            var configPath = Path.Combine(MyFileSystem.UserDataPath, "Storage\\PluginData", ConfigFileName);
            config = PersistentConfig<PluginConfig>.Load(Log, configPath);

#if DEBUG
            Harmony.DEBUG = true;
#endif
            Log.Debug("Applying Harmony patches");
            Harmony patcher = new Harmony(Name);

            try
            {
                patcher.PatchAll(Assembly.GetExecutingAssembly());
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalFactionController"), "AddFaction"), new HarmonyMethod(typeof(FactionsMenuPatch), nameof(FactionsMenuPatch.Prefix_AddFaction)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshPlayerChatHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshPlayerChatHistory)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshFactionChatHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshFactionChatHistory)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshGlobalChatHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshGlobalChatHistory)));
                patcher.Patch(AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "RefreshChatBotHistory"), new HarmonyMethod(typeof(TerminalChatMenuPatch), nameof(TerminalChatMenuPatch.Prefix_RefreshChatBotHistory)));
            }
            catch (Exception ex)
            {
                Log.Debug("Error while patching game code.");
                failed = true;
                throw ex;
            }

            Log.Debug("Successfully loaded");
        }

        public void Dispose()
        {
            Instance = null;
        }

        public void Update()
        {
            
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyPluginConfigDialog());
        }
    }
}