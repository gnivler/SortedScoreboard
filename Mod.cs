using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global  
// ReSharper disable UnusedMember.Local  
// ReSharper disable InconsistentNaming

namespace SortedScoreboard
{
    public class Mod : MBSubModuleBase
    {
        private static void Log(object input)
        {
            //FileLog.Log($"[SortedScoreboard][{DateTime.Now:G}] {input ?? "null"}");
        }

        private static readonly Harmony harmony = new Harmony("ca.gnivler.bannerlord.SortedScoreboard");
        private static readonly List<PropertyInfo> PlayerOrderedPropertyInfos = new List<PropertyInfo>();
        private static readonly List<PropertyInfo> EnemyOrderedPropertyInfos = new List<PropertyInfo>();
        private static Settings Settings;

        // mod uses this map, with the player config, to populate two lists of PropertyInfos
        private static readonly Dictionary<string, PropertyInfo> propertyInfoMap = new Dictionary<string, PropertyInfo>
        {
            {"GainedSkills", AccessTools.Property(typeof(SPScoreboardUnitVM), "GainedSkills")},
            {"ReadyToUpgrade", AccessTools.Property(typeof(SPScoreboardStatsVM), "ReadyToUpgrade")},
            {"Killed", AccessTools.Property(typeof(SPScoreboardStatsVM), "Dead")},
            {"Wounded", AccessTools.Property(typeof(SPScoreboardStatsVM), "Wounded")},
            {"Kills", AccessTools.Property(typeof(SPScoreboardStatsVM), "Kill")},
        };

        protected override void OnSubModuleLoad()
        {
            Log("Startup");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void ConfigureSettings()
        {
            try
            {
                Settings = new Settings(JsonConvert.DeserializeObject<Dictionary<string, List<string>>>
                    (File.ReadAllText("..\\..\\Modules\\SortedScoreboard\\mod_settings.json")));
            }
            catch (Exception ex)
            {
                Log(ex);
                Settings = new Settings();
            }

            // create a new list of Comparers from the input
            PlayerOrderedPropertyInfos.Clear();
            EnemyOrderedPropertyInfos.Clear();
            foreach (var column in Settings.PlayerSortOrder)
            {
                PlayerOrderedPropertyInfos.Add(propertyInfoMap[column]);
            }

            foreach (var column in Settings.EnemySortOrder)
            {
                EnemyOrderedPropertyInfos.Add(propertyInfoMap[column]);
            }
        }

        private static CustomBattleCombatant playerCombatant;

        [HarmonyPatch(typeof(CustomBattleMenuVM), "PrepareBattleData")]
        public class CustomBattleMenuVMPrepareBattleDataPatch
        {
            private static void Postfix(CustomBattleData __result)
            {
                playerCombatant = __result.PlayerParty;
            }
        }

        [HarmonyPatch(typeof(SPScoreboardPartyVM), "UpdateScores")]
        public class SPScoreboardPartyVMUpdateScoresPatch
        {
            private static void Postfix(SPScoreboardPartyVM __instance)
            {
                ConfigureSettings();
                if (__instance.BattleCombatant is CustomBattleCombatant customBattleCombatant)
                {
                    if (customBattleCombatant == playerCombatant)
                    {
                        __instance.Members.Sort(new MultiComparer(PlayerOrderedPropertyInfos));
                    }
                    else
                    {
                        __instance.Members.Sort(new MultiComparer(EnemyOrderedPropertyInfos));
                    }
                }
                else if (__instance.BattleCombatant is PartyBase partyBase)
                {
                    if (partyBase.LeaderHero == null ||
                        !Clan.PlayerClan.WarParties.Contains(partyBase.MobileParty))
                    {
                        __instance.Members.Sort(new MultiComparer(EnemyOrderedPropertyInfos));
                    }
                    else
                    {
                        __instance.Members.Sort(new MultiComparer(PlayerOrderedPropertyInfos));
                    }
                }
            }

            private class MultiComparer : Comparer<SPScoreboardUnitVM>
            {
                private readonly List<PropertyInfo> orderedPropertyInfos;

                public MultiComparer(List<PropertyInfo> propertyInfos)
                {
                    orderedPropertyInfos = propertyInfos;
                }

                public override int Compare(SPScoreboardUnitVM x, SPScoreboardUnitVM y)
                {
                    var result = 0;

                    if (x == null || y == null)
                    {
                        return 0;
                    }

                    // the source data comes from two types so this is really verbose
                    if (orderedPropertyInfos[0].Name == "GainedSkills")
                    {
                        var X = ((IList) orderedPropertyInfos[0].GetValue(x)).Count;
                        var Y = ((IList) orderedPropertyInfos[0].GetValue(y)).Count;
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        for (var i = 1; i < 5; i++)
                        {
                            X = (int) orderedPropertyInfos[i].GetValue(x.Score);
                            Y = (int) orderedPropertyInfos[i].GetValue(y.Score);
                            result = Helper(X, Y);
                            if (result != 0)
                            {
                                return result;
                            }
                        }
                    }
                    else if (orderedPropertyInfos[1].Name == "GainedSkills")
                    {
                        var X = (int) orderedPropertyInfos[0].GetValue(x.Score);
                        var Y = (int) orderedPropertyInfos[0].GetValue(y.Score);
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        X = ((IList) orderedPropertyInfos[1].GetValue(x)).Count;
                        Y = ((IList) orderedPropertyInfos[1].GetValue(y)).Count;
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        for (var i = 2; i < 5; i++)
                        {
                            X = (int) orderedPropertyInfos[i].GetValue(x.Score);
                            Y = (int) orderedPropertyInfos[i].GetValue(y.Score);
                            result = Helper(X, Y);
                            if (result != 0)
                            {
                                return result;
                            }
                        }
                    }
                    else if (orderedPropertyInfos[2].Name == "GainedSkills")
                    {
                        var X = (int) orderedPropertyInfos[0].GetValue(x.Score);
                        var Y = (int) orderedPropertyInfos[0].GetValue(y.Score);
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        X = (int) orderedPropertyInfos[1].GetValue(x.Score);
                        Y = (int) orderedPropertyInfos[1].GetValue(y.Score);
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        X = ((IList) orderedPropertyInfos[2].GetValue(x)).Count;
                        Y = ((IList) orderedPropertyInfos[2].GetValue(y)).Count;
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        X = (int) orderedPropertyInfos[3].GetValue(x.Score);
                        Y = (int) orderedPropertyInfos[3].GetValue(y.Score);
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        X = (int) orderedPropertyInfos[4].GetValue(x.Score);
                        Y = (int) orderedPropertyInfos[4].GetValue(y.Score);
                        result = Helper(X, Y);
                    }
                    else if (orderedPropertyInfos[3].Name == "GainedSkills")
                    {
                        int X, Y;
                        for (var i = 0; i < 3; i++)
                        {
                            X = (int) orderedPropertyInfos[i].GetValue(x.Score);
                            Y = (int) orderedPropertyInfos[i].GetValue(y.Score);
                            result = Helper(X, Y);
                            if (result != 0)
                            {
                                return result;
                            }
                        }

                        X = ((IList) orderedPropertyInfos[3].GetValue(x)).Count;
                        Y = ((IList) orderedPropertyInfos[3].GetValue(y)).Count;
                        result = Helper(X, Y);
                        if (result != 0)
                        {
                            return result;
                        }

                        X = (int) orderedPropertyInfos[4].GetValue(x.Score);
                        Y = (int) orderedPropertyInfos[4].GetValue(y.Score);
                        result = Helper(X, Y);
                    }
                    else if (orderedPropertyInfos[4].Name == "GainedSkills")
                    {
                        int X, Y;
                        for (var i = 0; i < 4; i++)
                        {
                            X = (int) orderedPropertyInfos[0].GetValue(x.Score);
                            Y = (int) orderedPropertyInfos[0].GetValue(y.Score);
                            result = Helper(X, Y);
                            if (result != 0)
                            {
                                return result;
                            }
                        }

                        X = ((IList) orderedPropertyInfos[4].GetValue(x)).Count;
                        Y = ((IList) orderedPropertyInfos[4].GetValue(y)).Count;
                        result = Helper(X, Y);
                    }

                    static int Helper(int X, int Y)
                    {
                        var num = 0;
                        if (X > Y)
                        {
                            num = -1;
                        }

                        if (X < Y)
                        {
                            num = 1;
                        }

                        return num;
                    }

                    return result;
                }
            }
        }
    }
}
