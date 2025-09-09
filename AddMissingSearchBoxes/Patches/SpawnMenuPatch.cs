using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using System.Linq;
using System;
using System.Reflection;
using VRage.Utils;
using VRageMath;
using static Sandbox.Graphics.GUI.MyGuiControlListbox;
using Sandbox.Definitions;
using System.Text;
using System.Collections.Generic;
using VRage.Game;
using Sandbox.Game.Localization;
using VRage;
using Sandbox.Game.Entities.Planet;

namespace ClientPlugin.Patches
{
    [HarmonyPatch]
    internal static class SpawnMenuPatch
    {
        private static List<Item> ItemSpawnMenuItems = null;
        private static List<Item> SpawnableAsteroids = null;
        private static List<Item> VoxelMaterials = null;
        private static List<Item> SpawnablePlanets = null;

        private static MethodInfo TargetMethod()
        {
            return AccessTools.Method("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:CreateMenu");
        }

        private static void Postfix(MyGuiScreenDebugBase __instance)
        {
            int m_selectedScreen = (int)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_selectedScreen").GetValue(__instance);

            switch (m_selectedScreen)
            {
                case 1:
                    {
                        MyGuiControlListbox m_asteroidTypeListbox = (MyGuiControlListbox)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_asteroidTypeListbox").GetValue(__instance);

                        MyGuiControlSearchBox searchBox = new MyGuiControlSearchBox
                        {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            Position = m_asteroidTypeListbox.Position + new Vector2(0, -0.04f)
                        };
                        searchBox.Size = new Vector2(m_asteroidTypeListbox.Size.X, searchBox.Size.Y);
                        searchBox.OnTextChanged += AsteroidTypeSearchbox_TextChanged;

                        MyGuiControlLabel searchBoxText = (MyGuiControlLabel)AccessTools.Field(typeof(MyGuiControlSearchBox), "m_label").GetValue(searchBox);
                        searchBoxText.Text = "Search or select type";

                        __instance.Controls.Add(searchBox);

                        MyGuiControlListbox m_materialTypeListbox = (MyGuiControlListbox)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_materialTypeListbox").GetValue(__instance);

                        MyGuiControlSearchBox searchBoxMaterial = new MyGuiControlSearchBox
                        {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            Position = m_materialTypeListbox.Position + new Vector2(0, -0.04f)
                        };
                        searchBoxMaterial.Size = new Vector2(m_materialTypeListbox.Size.X, searchBoxMaterial.Size.Y);
                        searchBoxMaterial.OnTextChanged += AsteroidMaterialSearchbox_TextChanged;

                        MyGuiControlLabel searchBoxMaterialText = (MyGuiControlLabel)AccessTools.Field(typeof(MyGuiControlSearchBox), "m_label").GetValue(searchBoxMaterial);
                        searchBoxMaterialText.Text = "Search or select material";

                        __instance.Controls.Add(searchBoxMaterial);
                        break;
                    }
                case 3:
                    {
                        MyGuiControlListbox m_planetListbox = (MyGuiControlListbox)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_planetListbox").GetValue(__instance);

                        MyGuiControlSearchBox searchBox = new MyGuiControlSearchBox
                        {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            Position = m_planetListbox.Position + new Vector2(0, -0.04f)
                        };
                        searchBox.Size = new Vector2(m_planetListbox.Size.X, searchBox.Size.Y);
                        searchBox.OnTextChanged += PlanetSearchbox_TextChanged;

                        __instance.Controls.Add(searchBox);

                        break;
                    }
            }
        }

        private static void AsteroidTypeSearchbox_TextChanged(string newText)
        {
            MyGuiControlListbox m_asteroidTypeListbox = null;

            foreach (MyGuiScreenBase screen in MyScreenManager.Screens)
            {
                if (screen.GetType() == AccessTools.TypeByName("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu"))
                {
                    m_asteroidTypeListbox = (MyGuiControlListbox)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_asteroidTypeListbox").GetValue(screen);
                    break;
                }
            }

            if (SpawnableAsteroids == null)
            {
                SpawnableAsteroids = GetSpawnableAsteroids();
            }

            m_asteroidTypeListbox.Items.Clear();

            string[] subStrings = newText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (Item item in SpawnableAsteroids)
            {
                if (subStrings.All(s => item.Text.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    continue;
                }

                m_asteroidTypeListbox.Items.Add(item);
            }

        }


        private static List<Item> GetSpawnableAsteroids()
        {
            var items = new List<Item>();
            foreach (MyVoxelMapStorageDefinition item3 in MyDefinitionManager.Static.GetVoxelMapStorageDefinitions().OrderBy(delegate (MyVoxelMapStorageDefinition e)
            {
                MyStringHash subtypeId3 = e.Id.SubtypeId;
                return subtypeId3.ToString();
            }))
            {
                MyStringHash subtypeId = item3.Id.SubtypeId;
                string text = subtypeId.ToString();
                items.Add(new Item(new StringBuilder(text), text, null, text));
            }

            return items;
        }

        private static void AsteroidMaterialSearchbox_TextChanged(string newText)
        {
            MyGuiControlListbox m_materialTypeListbox = null;

            foreach (MyGuiScreenBase screen in MyScreenManager.Screens)
            {
                if (screen.GetType() == AccessTools.TypeByName("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu"))
                {
                    m_materialTypeListbox = (MyGuiControlListbox)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_materialTypeListbox").GetValue(screen);
                    break;
                }
            }

            if (VoxelMaterials == null)
            {
                VoxelMaterials = GetVoxelMaterials();
            }

            m_materialTypeListbox.Items.Clear();

            string[] subStrings = newText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            m_materialTypeListbox.Add(new Item(MyTexts.Get(MySpaceTexts.SpawnMenu_KeepOriginalMaterial), MyTexts.GetString(MySpaceTexts.SpawnMenu_KeepOriginalMaterial_Tooltip)));

            foreach (Item item in VoxelMaterials)
            {
                if (subStrings.All(s => item.Text.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    continue;
                }

                m_materialTypeListbox.Items.Add(item);
            }
        }

        private static List<Item> GetVoxelMaterials()
        {
            var items = new List<Item>();
            foreach (MyVoxelMaterialDefinition item4 in MyDefinitionManager.Static.GetVoxelMaterialDefinitions().OrderBy(delegate (MyVoxelMaterialDefinition e)
            {
                MyStringHash subtypeId2 = e.Id.SubtypeId;
                return subtypeId2.ToString();
            }))
            {
                MyStringHash subtypeId = item4.Id.SubtypeId;
                string text2 = subtypeId.ToString();
                items.Add(new Item(new StringBuilder(text2), text2, null, text2));
            }

            return items;
        }

        private static void PlanetSearchbox_TextChanged(string newText)
        {
            MyGuiControlListbox m_planetListbox = null;

            foreach (MyGuiScreenBase screen in MyScreenManager.Screens)
            {
                if (screen.GetType() == AccessTools.TypeByName("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu"))
                {
                    m_planetListbox = (MyGuiControlListbox)AccessTools.Field("Sandbox.Game.Gui.MyGuiScreenDebugSpawnMenu:m_planetListbox").GetValue(screen);
                    break;
                }
            }

            if (SpawnablePlanets == null)
            {
                SpawnablePlanets = GetPlanets();
            }

            m_planetListbox.Items.Clear();

            string[] subStrings = newText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (Item item in SpawnablePlanets)
            {
                if (subStrings.All(s => item.Text.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    continue;
                }

                m_planetListbox.Items.Add(item);
            }
        }

        private static List<Item> GetPlanets()
        {
            var items = new List<Item>();
            foreach (MyPlanetGeneratorDefinition item in MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions().OrderBy(delegate (MyPlanetGeneratorDefinition e)
            {
                MyStringHash subtypeId2 = e.Id.SubtypeId;
                return subtypeId2.ToString();
            }))
            {
                MyStringHash subtypeId = item.Id.SubtypeId;
                string text = subtypeId.ToString();
                Vector4? colorMask = null;
                string toolTip = text;
                if (!MyPlanets.Static.CanSpawnPlanet(item, register: false, 1f, out var errorMessage))
                {
                    toolTip = errorMessage;
                    colorMask = MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER;
                }
                items.Add(new MyGuiControlListbox.Item(new StringBuilder(text), toolTip, null, text)
                {
                    ColorMask = colorMask
                });
            }

            return items;
        }
    }
}
