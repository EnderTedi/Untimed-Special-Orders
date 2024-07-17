using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Extensions;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;
using StardewValley;
using StardewModdingAPI.Events;

namespace UntimedSpecialOrders
{
    internal class ModEntry : Mod
    {
#pragma warning disable CS8618
        public static IModHelper helper;
        public static IMonitor monitor;
#pragma warning restore CS8618
        public static string? ModID;

        public override void Entry(IModHelper Helper)
        {
            helper = Helper;
            monitor = Monitor;
            ModID = ModManifest.UniqueID;

            Helper.Events.Content.AssetRequested += OnAssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(typeof(ModEntry).Assembly);
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"{ModID}/UntimedOrders"))
                e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Low);
        }

        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.IsTimedQuest))]
        public static class SpecialOrderUntimedQuestsPatch1
        {
            public static void Postfix(SpecialOrder __instance, ref bool __result)
            {
                if (__instance is not null) 
                {
                    SpecialOrderData? data = __instance?.GetData();
                    if (data is not null && data.CustomFields is not null && data.CustomFields.TryGetValue("Untimed", out string? _))
                    {
                        __result = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.UpdateAvailableSpecialOrders))]
        public static class SpecialOrderAvailabilityPatch
        {
            public static void Postfix(SpecialOrder __instance, bool forceRefresh)
            {
                if (__instance is not null)
                {
                    SpecialOrderData? data = __instance?.GetData();
                    if (data is not null && data.CustomFields is not null && data.CustomFields.TryGetValue("Untimed", out string? _) && forceRefresh)
                    {
                        UpdateAvailability(data);
                    }
                }
            }

            private static void UpdateAvailability(SpecialOrderData data)
            {
                bool forceRefresh = true;
                foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
                {
                    if ((order.questDuration.Value == QuestDuration.TwoDays || order.questDuration.Value == QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
                    {
                        order.SetDuration(order.questDuration.Value);
                    }
                }
                if (!forceRefresh)
                {
                    foreach (SpecialOrder availableSpecialOrder in Game1.player.team.availableSpecialOrders)
                    {
                        if (availableSpecialOrder.orderType.Value == data.OrderType)
                        {
                            return;
                        }
                    }
                }
                SpecialOrder.RemoveAllSpecialOrders(data.OrderType);
                List<string> keyQueue = new();
                foreach (KeyValuePair<string, SpecialOrderData> pair in DataLoader.SpecialOrders(Game1.content))
                {
                    if (pair.Value.OrderType == data.OrderType && SpecialOrder.CanStartOrderNow(pair.Key, pair.Value))
                    {
                        keyQueue.Add(pair.Key);
                    }
                }
                List<string> keysIncludingCompleted = new(keyQueue);
                Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 1.3);
                for (int i = 0; i < 2; i++)
                {
                    if (keyQueue.Count == 0)
                    {
                        if (keysIncludingCompleted.Count == 0)
                        {
                            break;
                        }
                        keyQueue = new List<string>(keysIncludingCompleted);
                    }
                    string key = r.ChooseFrom(keyQueue);
                    Game1.player.team.availableSpecialOrders.Add(SpecialOrder.GetSpecialOrder(key, r.Next()));
                    keyQueue.Remove(key);
                    keysIncludingCompleted.Remove(key);
                }
            }
        }

        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.SetDuration))]
        public static class SpecialOrderUntimedQuestsPatch2
        {
            public static bool Prefix(SpecialOrder __instance)
            {

                if (__instance is not null)
                {
                    SpecialOrderData? data = __instance?.GetData();
                    if (__instance is not null && data is not null && data.CustomFields is not null && data.CustomFields.TryGetValue("Untimed", out string? _))
                    {
                        __instance.dueDate.Value = Game1.Date.TotalDays + 999;
                        return false;
                    }
                    return true;
                }
                return true;
            }
        }


        /*[HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.IsTimedQuest))]
        public static class SpecialOrderUntimedQuestsPatch1
        {
            public static void Postfix(SpecialOrder __instance, ref bool __result)
            {
                if (__instance.questKey.Value.EndsWith("_Untimed"))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.UpdateAvailableSpecialOrders))]
        public static class SpecialOrderAvailabilityPatch
        {
            public static void Postfix(string orderType, bool forceRefresh)
            {
                if (orderType == "" && forceRefresh)
                {
                    UpdateAvailability();
                }
            }

            private static void UpdateAvailability()
            {
                string orderType = "UntimedOrder";
                bool forceRefresh = true;

                foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
                {
                    if ((order.questDuration.Value == QuestDuration.TwoDays || order.questDuration.Value == QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
                    {
                        order.SetDuration(order.questDuration.Value);
                    }
                }
                if (!forceRefresh)
                {
                    foreach (SpecialOrder availableSpecialOrder in Game1.player.team.availableSpecialOrders)
                    {
                        if (availableSpecialOrder.orderType.Value == orderType)
                        {
                            return;
                        }
                    }
                }
                SpecialOrder.RemoveAllSpecialOrders(orderType);
                List<string> keyQueue = new();
                foreach (KeyValuePair<string, SpecialOrderData> pair in DataLoader.SpecialOrders(Game1.content))
                {
                    if (pair.Value.OrderType == orderType && SpecialOrder.CanStartOrderNow(pair.Key, pair.Value))
                    {
                        keyQueue.Add(pair.Key);
                    }
                }
                List<string> keysIncludingCompleted = new(keyQueue);
                if (orderType == "")
                {
                    //keyQueue.RemoveAll((string id) => Game1.player.team.completedSpecialOrders.Contains(id));
                }
                Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 1.3);
                for (int i = 0; i < 2; i++)
                {
                    if (keyQueue.Count == 0)
                    {
                        if (keysIncludingCompleted.Count == 0)
                        {
                            break;
                        }
                        keyQueue = new List<string>(keysIncludingCompleted);
                    }
                    string key = r.ChooseFrom(keyQueue);
                    Game1.player.team.availableSpecialOrders.Add(SpecialOrder.GetSpecialOrder(key, r.Next()));
                    keyQueue.Remove(key);
                    keysIncludingCompleted.Remove(key);
                }
            }
        }

        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.SetDuration))]
        public static class SpecialOrderUntimedQuestsPatch2
        {
            public static bool Prefix(SpecialOrder __instance)
            {
                // I don't know why this is necessary
                if (__instance.questKey.Value.EndsWith("_Untimed"))
                {
                    __instance.dueDate.Value = Game1.Date.TotalDays + 999;
                    return false;
                }
                return true;
            }
        }*/
    }
}
