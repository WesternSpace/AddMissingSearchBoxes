using HarmonyLib;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GameSystems.Chat;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRage;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Patches
{
    //[HarmonyPatch(typeof(MyGuiScreenTerminal), "CreateChatPageControls")]
    internal static class TerminalChatMenuPatch
    {
        private static string searchBoxText = "";

        private static MyGuiScreenTerminal terminalInstance = null;

        private static void Postfix(MyGuiScreenTerminal __instance, MyGuiControlTabPage chatPage)
        {
            if (!Plugin.Instance.Config.ChatSearchboxEnabled)
            {
                return;
            }

            terminalInstance = __instance;

            MyGuiControlSearchBox searchBox = new MyGuiControlSearchBox
            {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Position = new Vector2(-0.125f, -0.332f),
            };
            searchBox.Size = new Vector2(0.581f, searchBox.Size.Y);
            searchBox.OnTextChanged += SearchBox_OnTextChanged;

            chatPage.Controls.Add(searchBox);

            for (int i = 0; i < chatPage.Controls.Count; i++)
            {
                if (chatPage.Controls[i].GetType() == typeof(MyGuiControlPanel))
                {
                    if (chatPage.Controls[i].Position == new Vector2(-0.125f, -0.332000017f))
                    {
                        chatPage.Controls[i].Position = new Vector2(-0.125f, -0.332f + searchBox.Size.Y);
                        chatPage.Controls[i].Size = new Vector2(chatPage.Controls[i].Size.X, chatPage.Controls[i].Size.Y - searchBox.Size.Y);
                    }
                    continue;
                }

                if (chatPage.Controls[i].Name == "ChatHistory")
                {
                    chatPage.Controls[i].PositionY = -0.29f;
                    chatPage.Controls[i].Size = new Vector2(chatPage.Controls[i].Size.X, 0.575f);
                }
            }
        }

        private static void SearchBox_OnTextChanged(string newText)
        {
            searchBoxText = newText;
            FieldInfo chatControllerField = AccessTools.Field(typeof(MyGuiScreenTerminal), "m_controllerChat");
            object controllerInstance = chatControllerField.GetValue(terminalInstance);
            AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_playerList_ItemsSelected").Invoke(controllerInstance, new object[] { null });
            AccessTools.Method(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_factionList_ItemsSelected").Invoke(controllerInstance, new object[] { null });
        }

        public static bool Prefix_RefreshPlayerChatHistory(object __instance, MyIdentity playerIdentity)
        {
            if (playerIdentity == null || MySession.Static.ChatSystem == null)
            {
                return false;
            }

            FieldInfo chatHistoryField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_chatHistory");
            MyGuiControlMultilineText chatHistory = (MyGuiControlMultilineText)chatHistoryField.GetValue(__instance);

            FieldInfo factionListField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_factionList");
            MyGuiControlListbox factionList = (MyGuiControlListbox)factionListField.GetValue(__instance);

            chatHistory.Clear();
            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
            MySession.Static.ChatSystem.ChatHistory.GetPrivateHistory(ref list, playerIdentity.IdentityId);
            foreach (MyUnifiedChatItem item in list)
            {
                if (item != null)
                {
                    string[] subStrings = searchBoxText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId);
                    if (myIdentity != null)
                    {
                        Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                        Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                        chatHistory.AppendText(myIdentity.DisplayName, "White", chatHistory.TextScale, relationColor);
                        chatHistory.AppendText(": ", "White", chatHistory.TextScale, relationColor);
                        chatHistory.AppendText(item.Text, "White", chatHistory.TextScale, channelColor);
                        chatHistory.AppendLine();
                    }
                }
            }
            factionList.SelectedItems.Clear();
            chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }

        public static bool Prefix_RefreshFactionChatHistory(object __instance, MyFaction faction)
        {
            FieldInfo chatHistoryField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_chatHistory");
            MyGuiControlMultilineText chatHistory = (MyGuiControlMultilineText)chatHistoryField.GetValue(__instance);

            chatHistory.Clear();
            if (MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId) == null && !MySession.Static.IsUserAdmin(Sync.MyId))
            {
                return false;
            }

            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
            MySession.Static.ChatSystem.ChatHistory.GetFactionHistory(ref list, faction.FactionId);
            foreach (MyUnifiedChatItem item in list)
            {
                MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId);

                if (myIdentity != null)
                {
                    string[] subStrings = searchBoxText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    string playerName = myIdentity.DisplayName;

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => playerName.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                    Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                    chatHistory.AppendText(playerName, "White", chatHistory.TextScale, relationColor);
                    chatHistory.AppendText(": ", "White", chatHistory.TextScale, relationColor);
                    chatHistory.AppendText(item.Text, "White", chatHistory.TextScale, channelColor);
                    chatHistory.AppendLine();
                }
            }

            FieldInfo playerListField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_playerList");
            MyGuiControlListbox playerList = (MyGuiControlListbox)playerListField.GetValue(__instance);

            playerList.SelectedItems.Clear();
            chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }

        public static bool Prefix_RefreshGlobalChatHistory(object __instance)
        {
            FieldInfo chatHistoryField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_chatHistory");
            MyGuiControlMultilineText chatHistory = (MyGuiControlMultilineText)chatHistoryField.GetValue(__instance);

            chatHistory.Clear();
            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();

            bool allowPlayerDrivenChat = MyMultiplayer.Static?.IsTextChatAvailable ?? true;
            if (allowPlayerDrivenChat)
            {
                MySession.Static.ChatSystem.ChatHistory.GetGeneralHistory(ref list);
            }
            else
            {
                list.Add(new MyUnifiedChatItem
                {
                    Channel = ChatChannel.GlobalScripted,
                    Text = MyTexts.GetString(MyCommonTexts.ChatRestricted),
                    AuthorFont = "White"
                });
            }
            foreach (MyUnifiedChatItem item in list)
            {
                if (item.Channel == ChatChannel.GlobalScripted)
                {
                    string[] subStrings = searchBoxText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => item.CustomAuthor.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                    Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                    if (item.CustomAuthor.Length > 0)
                    {
                        chatHistory.AppendText(item.CustomAuthor + ": ", item.AuthorFont, chatHistory.TextScale, relationColor);
                    }
                    else
                    {
                        chatHistory.AppendText(MyTexts.GetString(MySpaceTexts.ChatBotName) + ": ", item.AuthorFont, chatHistory.TextScale, relationColor);
                    }
                    chatHistory.AppendText(item.Text, "White", chatHistory.TextScale, channelColor);
                    chatHistory.AppendLine();
                }
                else if (item.Channel == ChatChannel.Global)
                {
                    MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId);

                    if (myIdentity != null)
                    {
                        string[] subStrings = searchBoxText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        string playerName = myIdentity.DisplayName;

                        if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => playerName.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                        {
                            continue;
                        }

                        Color relationColor2 = MyChatSystem.GetRelationColor(item.SenderId);
                        Color channelColor2 = MyChatSystem.GetChannelColor(item.Channel);
                        chatHistory.AppendText(playerName, "White", chatHistory.TextScale, relationColor2);
                        chatHistory.AppendText(": ", "White", chatHistory.TextScale, relationColor2);
                        chatHistory.AppendText(item.Text, "White", chatHistory.TextScale, channelColor2);
                        chatHistory.AppendLine();
                    }
                }
            }

            FieldInfo factionListField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_factionList");
            MyGuiControlListbox factionList = (MyGuiControlListbox)factionListField.GetValue(__instance);

            factionList.SelectedItems.Clear();
            chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }

        public static bool Prefix_RefreshChatBotHistory(object __instance)
        {
            FieldInfo chatHistoryField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_chatHistory");
            MyGuiControlMultilineText chatHistory = (MyGuiControlMultilineText)chatHistoryField.GetValue(__instance);

            chatHistory.Clear();
            List<MyUnifiedChatItem> list = new List<MyUnifiedChatItem>();
            MySession.Static.ChatSystem.ChatHistory.GetChatbotHistory(ref list);
            foreach (MyUnifiedChatItem item in list)
            {
                MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity((item.SenderId != 0L) ? item.SenderId : item.TargetId);
                if (myIdentity != null)
                {
                    Vector4 one = Vector4.One;
                    Color white = Color.White;
                    string text = ((item.CustomAuthor.Length > 0) ? item.CustomAuthor : myIdentity.DisplayName);

                    string[] subStrings = searchBoxText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    chatHistory.AppendText(text, "White", chatHistory.TextScale, one);
                    chatHistory.AppendText(": ", "White", chatHistory.TextScale, one);
                    chatHistory.Parse(item.Text, "White", chatHistory.TextScale, white);
                    chatHistory.AppendLine();
                }
            }

            FieldInfo factionListField = AccessTools.Field(AccessTools.TypeByName("Sandbox.Game.Gui.MyTerminalChatController"), "m_factionList");
            MyGuiControlListbox factionList = (MyGuiControlListbox)factionListField.GetValue(__instance);

            factionList.SelectedItems.Clear();
            chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }
    }
}
