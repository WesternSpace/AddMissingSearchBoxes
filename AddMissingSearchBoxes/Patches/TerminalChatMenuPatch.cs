using HarmonyLib;
using Sandbox.Engine.Multiplayer;
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

namespace AddMissingSearchBoxes.Patches
{
    [HarmonyPatch(typeof(MyGuiScreenTerminal), "CreateChatPageControls")]
    internal static class TerminalChatMenuPatch
    {
        private static string searchBoxText = "";

        private static MyGuiScreenTerminal terminalInstance = null;

        private static void Postfix(MyGuiScreenTerminal __instance, MyGuiControlTabPage chatPage)
        {
            terminalInstance = __instance;

            MyGuiControlSearchBox searchBox = new()
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

            MyGuiScreenTerminal.m_instance.m_controllerChat.m_playerList_ItemsSelected(null);
            MyGuiScreenTerminal.m_instance.m_controllerChat.m_factionList_ItemsSelected(null);
        }

        public static bool Prefix_RefreshPlayerChatHistory(MyTerminalChatController __instance, MyIdentity playerIdentity)
        {
            if (playerIdentity == null || MySession.Static.ChatSystem == null)
            {
                return false;
            }

            __instance.m_chatHistory.Clear();
            List<MyUnifiedChatItem> list = [];
            MySession.Static.ChatSystem.ChatHistory.GetPrivateHistory(ref list, playerIdentity.IdentityId);
            foreach (MyUnifiedChatItem item in list)
            {
                if (item != null)
                {
                    string[] subStrings = searchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId);
                    if (myIdentity != null)
                    {
                        Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                        Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                        __instance.m_chatHistory.AppendText(myIdentity.DisplayName, "White", __instance.m_chatHistory.TextScale, relationColor);
                        __instance.m_chatHistory.AppendText(": ", "White", __instance.m_chatHistory.TextScale, relationColor);
                        __instance.m_chatHistory.AppendText(item.Text, "White", __instance.m_chatHistory.TextScale, channelColor);
                        __instance.m_chatHistory.AppendLine();
                    }
                }
            }
            __instance.m_factionList.SelectedItems.Clear();
            __instance.m_chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }

        public static bool Prefix_RefreshFactionChatHistory(MyTerminalChatController __instance, MyFaction faction)
        { 
            __instance.m_chatHistory.Clear();
            if (MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId) == null && !MySession.Static.IsUserAdmin(Sync.MyId))
            {
                return false;
            }

            List<MyUnifiedChatItem> list = [];
            MySession.Static.ChatSystem.ChatHistory.GetFactionHistory(ref list, faction.FactionId);
            foreach (MyUnifiedChatItem item in list)
            {
                MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId);

                if (myIdentity != null)
                {
                    string[] subStrings = searchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    string playerName = myIdentity.DisplayName;

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => playerName.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                    Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                    __instance.m_chatHistory.AppendText(playerName, "White", __instance.m_chatHistory.TextScale, relationColor);
                    __instance.m_chatHistory.AppendText(": ", "White", __instance.m_chatHistory.TextScale, relationColor);
                    __instance.m_chatHistory.AppendText(item.Text, "White", __instance.m_chatHistory.TextScale, channelColor);
                    __instance.m_chatHistory.AppendLine();
                }
            }

            __instance.m_playerList.SelectedItems.Clear();
            __instance.m_chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }

        public static bool Prefix_RefreshGlobalChatHistory(MyTerminalChatController __instance)
        {
            __instance.m_chatHistory.Clear();
            List<MyUnifiedChatItem> list = [];

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
                    string[] subStrings = searchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => item.CustomAuthor.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    Color relationColor = MyChatSystem.GetRelationColor(item.SenderId);
                    Color channelColor = MyChatSystem.GetChannelColor(item.Channel);
                    if (item.CustomAuthor.Length > 0)
                    {
                        __instance.m_chatHistory.AppendText(item.CustomAuthor + ": ", item.AuthorFont, __instance.m_chatHistory.TextScale, relationColor);
                    }
                    else
                    {
                        __instance.m_chatHistory.AppendText(MyTexts.GetString(MySpaceTexts.ChatBotName) + ": ", item.AuthorFont, __instance.m_chatHistory.TextScale, relationColor);
                    }
                    __instance.m_chatHistory.AppendText(item.Text, "White", __instance.m_chatHistory.TextScale, channelColor);
                    __instance.m_chatHistory.AppendLine();
                }
                else if (item.Channel == ChatChannel.Global)
                {
                    MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId);

                    if (myIdentity != null)
                    {
                        string[] subStrings = searchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                        string playerName = myIdentity.DisplayName;

                        if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => playerName.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                        {
                            continue;
                        }

                        Color relationColor2 = MyChatSystem.GetRelationColor(item.SenderId);
                        Color channelColor2 = MyChatSystem.GetChannelColor(item.Channel);
                        __instance.m_chatHistory.AppendText(playerName, "White", __instance.m_chatHistory.TextScale, relationColor2);
                        __instance.m_chatHistory.AppendText(": ", "White", __instance.m_chatHistory.TextScale, relationColor2);
                        __instance.m_chatHistory.AppendText(item.Text, "White", __instance.m_chatHistory.TextScale, channelColor2);
                        __instance.m_chatHistory.AppendLine();
                    }
                }
            }

            __instance.m_factionList.SelectedItems.Clear();
            __instance.m_chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }

        public static bool Prefix_RefreshChatBotHistory(MyTerminalChatController __instance)
        {
            __instance.m_chatHistory.Clear();
            List<MyUnifiedChatItem> list = [];
            MySession.Static.ChatSystem.ChatHistory.GetChatbotHistory(ref list);
            foreach (MyUnifiedChatItem item in list)
            {
                MyIdentity myIdentity = MySession.Static.Players.TryGetIdentity(item.SenderId != 0L ? item.SenderId : item.TargetId);
                if (myIdentity != null)
                {
                    Vector4 one = Vector4.One;
                    Color white = Color.White;
                    string text = item.CustomAuthor.Length > 0 ? item.CustomAuthor : myIdentity.DisplayName;

                    string[] subStrings = searchBoxText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                    if (subStrings.All(s => item.Text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false && subStrings.All(s => text.Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    __instance.m_chatHistory.AppendText(text, "White", __instance.m_chatHistory.TextScale, one);
                    __instance.m_chatHistory.AppendText(": ", "White", __instance.m_chatHistory.TextScale, one);
                    __instance.m_chatHistory.Parse(item.Text, "White", __instance.m_chatHistory.TextScale, white);
                    __instance.m_chatHistory.AppendLine();
                }
            }

            __instance.m_factionList.SelectedItems.Clear();
            __instance.m_chatHistory.ScrollbarOffsetV = 1f;
            return false;
        }
    }
}
