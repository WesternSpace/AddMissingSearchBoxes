using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using System.Linq;
using System;
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
using DirectShowLib.BDA;

namespace AddMissingSearchBoxes.Patches
{
    [HarmonyPatch(typeof(MyGuiScreenDebugSpawnMenu), "CreateMenu")]
    internal static class MyGuiScreenDebugSpawnMenu_CreateMenu_Patch
    {
        private static List<Item> SpawnableAsteroids = null;
        private static List<Item> VoxelMaterials = null;
        private static List<Item> SpawnablePlanets = null;

        private static void Postfix(MyGuiScreenDebugSpawnMenu __instance)
        {
            switch (MyGuiScreenDebugSpawnMenu.m_selectedScreen)
            {
                case 1:
                    {
                        MyGuiControlSearchBox searchBox = new()
                        {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            Position = __instance.m_asteroidTypeListbox.Position + new Vector2(0, -0.04f)
                        };
                        searchBox.Size = new Vector2(__instance.m_asteroidTypeListbox.Size.X, searchBox.Size.Y);
                        searchBox.OnTextChanged += AsteroidTypeSearchbox_TextChanged;
                        searchBox.m_label.Text = "Search or select type";

                        __instance.Controls.Add(searchBox);

                        MyGuiControlSearchBox searchBoxMaterial = new()
                        {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            Position = __instance.m_materialTypeListbox.Position + new Vector2(0, -0.04f)
                        };
                        searchBoxMaterial.Size = new Vector2(__instance.m_materialTypeListbox.Size.X, searchBoxMaterial.Size.Y);
                        searchBoxMaterial.OnTextChanged += AsteroidMaterialSearchbox_TextChanged;
                        searchBoxMaterial.m_label.Text = "Search or select material";

                        __instance.Controls.Add(searchBoxMaterial);
                        break;
                    }
                case 3:
                    {
                        MyGuiControlSearchBox searchBox = new()
                        {
                            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                            Position = __instance.m_planetListbox.Position + new Vector2(0, -0.04f)
                        };
                        searchBox.Size = new Vector2(__instance.m_planetListbox.Size.X, searchBox.Size.Y);
                        searchBox.OnTextChanged += PlanetSearchbox_TextChanged;

                        __instance.Controls.Add(searchBox);

                        break;
                    }
            }
        }

        private static void AsteroidTypeSearchbox_TextChanged(string newText)
        {
            SpawnableAsteroids ??= GetSpawnableAsteroids();

            MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_asteroidTypeListbox.Items.Clear();

            string[] subStrings = newText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            foreach (Item item in SpawnableAsteroids)
            {
                if (subStrings.All(s => item.Text.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    continue;
                }

                MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_asteroidTypeListbox.Items.Add(item);
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
            VoxelMaterials ??= GetVoxelMaterials();

            MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_materialTypeListbox.Items.Clear();

            string[] subStrings = newText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_materialTypeListbox.Add(new Item(MyTexts.Get(MySpaceTexts.SpawnMenu_KeepOriginalMaterial), MyTexts.GetString(MySpaceTexts.SpawnMenu_KeepOriginalMaterial_Tooltip)));

            foreach (Item item in VoxelMaterials)
            {
                if (subStrings.All(s => item.Text.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    continue;
                }

                MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_materialTypeListbox.Items.Add(item);
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
            SpawnablePlanets ??= GetPlanets();

            MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_planetListbox.Items.Clear();

            string[] subStrings = newText.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            foreach (Item item in SpawnablePlanets)
            {
                if (subStrings.All(s => item.Text.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    continue;
                }

                MyScreenManager.GetFirstScreenOfType<MyGuiScreenDebugSpawnMenu>().m_planetListbox.Items.Add(item);
            }
        }

        private static List<Item> GetPlanets()
        {
            var items = new List<Item>();
            foreach (MyPlanetGeneratorDefinition item in from e in MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions()
                                                         orderby e.Id.SubtypeId.ToString()
                                                         select e)
            {
                if (!item.Public)
                {
                    continue;
                }

                MyStringHash subtypeId = item.Id.SubtypeId;
                string text = subtypeId.ToString();
                Vector4? colorMask = null;
                string toolTip = text;
                if (!MyPlanets.Static.CanSpawnPlanet(item, register: false, 1f, out var errorMessage))
                {
                    toolTip = errorMessage;
                    colorMask = MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER;
                }
                items.Add(new Item(new StringBuilder(text), toolTip, null, text)
                {
                    ColorMask = colorMask
                });
            }

            return items;
        }
    }
}
