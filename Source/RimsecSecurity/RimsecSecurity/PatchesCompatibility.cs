using HarmonyLib;
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
        public static void ExecuteCompatibilityPatches(Harmony harmony)
        {
            var prisonLaborAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith("prisonlabor"));
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

            var saveOurShipAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith("shipshaveinsides"));
            if (saveOurShipAssembly != null)
            {
                Log.Message($"Patching sos2");
                var org = AccessTools.Method(saveOurShipAssembly.GetType("SaveOurShip2.ShipInteriorMod2"), "hasSpaceSuit");
                var postfix = new HarmonyMethod(typeof(SaveOurShip2Patches), nameof(SaveOurShip2Patches.ShipInteriorMod2_hasSpaceSuit_Postfix));
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
            Log.Message($"JobPackage: pawn the error is being thrown for: {pawn.NameShortColored} {pawn.def.defName}");
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return true;
            Log.Message($"Prefixing JobPackage");
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
            Log.Message($"InitWorkSettings: pawn the error is being thrown for: {pawn.NameShortColored} {pawn.def.defName}");
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return true;
            Log.Message($"Prefixing InitWorkSettings");
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
}
