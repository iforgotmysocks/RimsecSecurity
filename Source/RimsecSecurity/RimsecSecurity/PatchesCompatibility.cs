using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimsecSecurity
{
    static class PatchesCompatibility
    {
        public static Assembly hygieneAssembly;
        public static void ExecuteCompatibilityPatches(Harmony harmony)
        {
            var prisonLaborAssembly = PeacekeeperUtility.GetAssemblyFromString("prisonlabor");
            if (prisonLaborAssembly != null)
            {
                Log.Message($"Patching prison labor");
                //var org = AccessTools.Method(prisonLaborAssembly.GetType("PrisonLabor.Core.AI.JobGivers.JobGiver_BedTime"), "TryGiveJob");
                //var postfix = new HarmonyMethod(typeof(PrisonLaberPatches), nameof(PrisonLaberPatches.JobGiver_Bedtime_TryGiveJob_Postfix));
                //harmony.Patch(org, null, postfix);

                //org = AccessTools.Method(prisonLaborAssembly.GetType("PrisonLabor.Core.AI.JobGivers.JobGiver_Diet"), "TryGiveJob");
                //postfix = new HarmonyMethod(typeof(PrisonLaberPatches), nameof(PrisonLaberPatches.JobGiver_Diet_TryGiveJob_Postfix));
                //harmony.Patch(org, null, postfix);

                //org = AccessTools.Method(prisonLaborAssembly.GetType("PrisonLabor.Core.AI.JobGivers.JobGiver_Labor"), "TryIssueJobPackage");
                //var prefix = new HarmonyMethod(typeof(PrisonLaberPatches), nameof(PrisonLaberPatches.JobGiver_Labor_TryIssueJobPackage_Prefix));
                //harmony.Patch(org, prefix, null);

                //org = AccessTools.Method(prisonLaborAssembly.GetType("PrisonLabor.Core.LaborWorkSettings.WorkSettings"), "InitWorkSettings");
                //prefix = new HarmonyMethod(typeof(PrisonLaberPatches), nameof(PrisonLaberPatches.WorkSettings_InitWorkSettings_Prefix));
                //harmony.Patch(org, prefix, null);

                // only patch for pl that is required yet.... unless robos are actually generated as prisoners
                var org = AccessTools.Method(prisonLaborAssembly.GetType("PrisonLabor.Core.Needs.Need_Treatment"), "NeedInterval");
                var prefix = new HarmonyMethod(typeof(PrisonLaberPatches), nameof(PrisonLaberPatches.Need_Treatment_NeedInterval_Prefix));
                harmony.Patch(org, prefix, null);
            }

            var saveOurShipAssembly = PeacekeeperUtility.GetAssemblyFromString("shipshaveinsides");
            if (saveOurShipAssembly != null)
            {
                Log.Message($"Patching sos2");
                var org = AccessTools.Method(saveOurShipAssembly.GetType("SaveOurShip2.ShipInteriorMod2"), "hasSpaceSuit");
                var postfix = new HarmonyMethod(typeof(SaveOurShip2Patches), nameof(SaveOurShip2Patches.ShipInteriorMod2_hasSpaceSuit_Postfix));
                harmony.Patch(org, null, postfix);
            }

            var guardsForMeAssembly = PeacekeeperUtility.GetAssemblyFromString("guardsforme");
            if (guardsForMeAssembly != null)
            {
                Log.Message($"Patching Gaurds for me");
                var org = AccessTools.Method(guardsForMeAssembly.GetType("aRandomKiwi.GFM.Utils"), "guardNeedFood");
                var postfix = new HarmonyMethod(typeof(GuardsForMePatches), nameof(GuardsForMePatches.guardNeedFood_Postfix));
                harmony.Patch(org, null, postfix);

                org = AccessTools.Method(guardsForMeAssembly.GetType("aRandomKiwi.GFM.Utils"), "guardNeedJoy");
                postfix = new HarmonyMethod(typeof(GuardsForMePatches), nameof(GuardsForMePatches.guardNeedJoy_Postfix));
                harmony.Patch(org, null, postfix);

                org = AccessTools.Method(guardsForMeAssembly.GetType("aRandomKiwi.GFM.Utils"), "guardNeedMood");
                postfix = new HarmonyMethod(typeof(GuardsForMePatches), nameof(GuardsForMePatches.guardNeedMood_Postfix));
                harmony.Patch(org, null, postfix);

                org = AccessTools.Method(guardsForMeAssembly.GetType("aRandomKiwi.GFM.Utils"), "guardNeedHygiene");
                postfix = new HarmonyMethod(typeof(GuardsForMePatches), nameof(GuardsForMePatches.guardNeedHygiene_Postfix));
                harmony.Patch(org, null, postfix);

                org = AccessTools.Method(guardsForMeAssembly.GetType("aRandomKiwi.GFM.Utils"), "guardNeedBladder");
                postfix = new HarmonyMethod(typeof(GuardsForMePatches), nameof(GuardsForMePatches.guardNeedBladder_Postfix));
                harmony.Patch(org, null, postfix);
            }

            hygieneAssembly = PeacekeeperUtility.GetAssemblyFromString("badhygiene");
            if (hygieneAssembly != null)
            {
                var org = AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed");
                var postfix = new HarmonyMethod(typeof(DubsHygienePatches), nameof(DubsHygienePatches.Pawn_NeedsTracker_ShouldHaveNeed_Postfix));
                harmony.Patch(org, null, postfix);
            }
        }
    }

    static class PrisonLaberPatches
    {
        public static void JobGiver_Bedtime_TryGiveJob_Postfix(ref Job __result, Pawn pawn)
        {
            if (__result == null || !PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = null;
        }

        public static void JobGiver_Diet_TryGiveJob_Postfix(ref Job __result, Pawn pawn)
        {
            if (__result == null || !PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = null;
        }

        public static bool JobGiver_Labor_TryIssueJobPackage_Prefix(ref ThinkResult __result, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return true;
            __result = ThinkResult.NoJob;
            return false;
        }

        //public static bool JobGiver_Labor__TryIssueJobPackage_Postfix(ref ThinkResult __result, Pawn pawn)
        //{
        //    if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return true;
        //    __result = ThinkResult.NoJob;
        //    return false;
        //}

        public static bool WorkSettings_InitWorkSettings_Prefix(Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return true;
            if (pawn?.playerSettings?.AreaRestriction != null) pawn.playerSettings.AreaRestriction = null;
            return false;
        }

        public static bool Need_Treatment_NeedInterval_Prefix(Pawn ___pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(___pawn)) return true;
            return false;
        }

    }

    static class SaveOurShip2Patches 
    {
        public static void ShipInteriorMod2_hasSpaceSuit_Postfix(ref bool __result, Pawn pawn)
        {
            if (__result == true || !PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = true;
        }
    }

    static class GuardsForMePatches
    {
        public static void guardNeedFood_Postfix(ref bool __result, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = false;
        }

        public static void guardNeedJoy_Postfix(ref bool __result, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = false;
        }

        public static void guardNeedMood_Postfix(ref bool __result, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = false;
        }
        public static void guardNeedHygiene_Postfix(ref bool __result, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = false;
        }
        public static void guardNeedBladder_Postfix(ref bool __result, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            __result = false;
        }
    }

    static class DubsHygienePatches
    {
        public static void Pawn_NeedsTracker_ShouldHaveNeed_Postfix(Pawn_NeedsTracker __instance, ref bool __result, Pawn ___pawn, NeedDef nd)
        {
            if (!__result) return;
            if (!PeacekeeperUtility.IsPeacekeeper(___pawn)) return;

            var t = PatchesCompatibility.hygieneAssembly.GetType("DubsBadHygiene.NeedsUtil");
            var checkInfo = t.GetMethod("IsHygieneNeed", new[] { typeof(NeedDef) });
            var isHygieneNeed = (bool)checkInfo.Invoke(null, new object[] { nd } );

            if (!isHygieneNeed) return;
            __result = false;
        }
    }
}
