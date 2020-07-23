using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global  
// ReSharper disable UnusedMember.Local  
// ReSharper disable InconsistentNaming

namespace SortedScoreboard
{
    public class Mod : MBSubModuleBase
    {
        private static readonly Harmony harmony = new Harmony("ca.gnivler.bannerlord.SortedScoreboard");
        private static readonly List<PropertyInfo> orderedPropertyInfos = new List<PropertyInfo>();

        private static List<string> configOrder = new List<string>
        {
            "GainedSkills",
            "ReadyToUpgrade",
            "Killed",
            "Wounded",
            "Kills"
        };

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
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.PrintError($"SortedScoreboard encountered an exception: {ex}");
            }
        }

        private static void ConfigureSettings()
        {
            var readList = File.ReadAllLines("..\\..\\Modules\\SortedScoreboard\\mod_settings.txt").ToList();
            // fall back to hard coded values if deserialized to null?
            configOrder = readList.Count == 0 ? configOrder : readList;
            // create a new list of Comparers from the string input
            orderedPropertyInfos.Clear();
            foreach (var config in configOrder)
            {
                orderedPropertyInfos.Add(propertyInfoMap[config]);
            }
        }

        [HarmonyPatch(typeof(SPScoreboardPartyVM), "UpdateScores")]
        public class SPScoreboardPartyVMRefreshPowerPatch
        {
            private static void Postfix(SPScoreboardPartyVM __instance)
            {
                ConfigureSettings();
                __instance.Members.Sort(new MultiComparer());
            }
        }

        private class MultiComparer : Comparer<SPScoreboardUnitVM>
        {
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
