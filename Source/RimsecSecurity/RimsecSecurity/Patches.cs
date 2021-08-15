using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using Verse.AI;
using UnityEngine;
using RimWorld.Planet;
using System.Reflection.Emit;

namespace RimsecSecurity
{
    [StaticConstructorOnStartup]
    public static class Patches
    {
        static Patches()
        {
            var harmony = new Harmony("Shakesthespeare.RimsecSecurity");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PatchesCompatibility.ExecuteCompatibilityPatches(harmony);

            if (ModSettings.hidePeacekeepersFromColonistBar)
            {
                var org_ColonistBar_CheckRecacheEntries = AccessTools.Method(typeof(ColonistBar), "CheckRecacheEntries");
                var post_ColonistBar_CheckRecacheEntries = new HarmonyMethod(typeof(TargettedPatches), nameof(TargettedPatches.ColonistBar_CheckRecacheEntries_Postfix));
                harmony.Patch(org_ColonistBar_CheckRecacheEntries, null, post_ColonistBar_CheckRecacheEntries);
            }

            if (!ModSettings.countPeacekeepersTowardsPopulation)
            {
                var org_StorytellerUtilityPopulation_AdjustedPopulation_get = AccessTools.PropertyGetter(typeof(StorytellerUtilityPopulation), "AdjustedPopulation");
                var post_StorytellerUtilityPopulation_AdjustedPopulation_get = new HarmonyMethod(typeof(TargettedPatches), nameof(TargettedPatches.StorytellerUtilityPopulation_AdjustedPopulation_get_Postfix));
                harmony.Patch(org_StorytellerUtilityPopulation_AdjustedPopulation_get, null, post_StorytellerUtilityPopulation_AdjustedPopulation_get);
            }
        }
    }

    #region UI
    [HarmonyPatch(typeof(Need), "get_LabelCap")]
    public class Need_get_LabelCap
    {
        public static void Postfix(ref string __result, Need __instance, Pawn ___pawn)
        {
            if (__instance.def.defName == "Rest" && PeacekeeperUtility.IsPeacekeeper(___pawn)) __result = "RSEnergyLabel".Translate();
            else if ((new string[] { "Joy", "Comfort", "Beauty", "Outdoors", "Mood" }).Contains(__instance.def.defName) && PeacekeeperUtility.IsPeacekeeper(___pawn)) __result = string.Empty;
        }
    }

    //[HarmonyPatch(typeof(Need_Rest), "GetTipString")]
    //public class Need_Rest_GetTipString
    //{
    //    public static void Postfix(ref string __result, Need __instance, Pawn ___pawn)
    //    {
    //        if (!PeacekeeperUtility.IsPeacekeeper(___pawn)) return;
    //        if (__instance.def.defName == "Rest")
    //        {
    //            __result = string.Concat(new string[]
    //                {
    //                __instance.LabelCap,
    //                ": ",
    //                __instance.CurLevelPercentage.ToStringPercent(),
    //                " (",
    //                __instance.CurLevel.ToString("0.##"),
    //                " / ",
    //                __instance.MaxLevel.ToString("0.##"),
    //                ")\n",
    //                "RSEnergyDesc".Translate()
    //                }
    //            );
    //        }
    //        else if ((new string[] { "Joy", "Comfort", "Beauty", "Outdoors", "Mood" }).Contains(__instance.def.defName)) __result = string.Empty;
    //    }
    //}

    [HarmonyPatch(typeof(Need), "GetTipString")]
    public class Need_GetTipString
    {
        public static void Postfix(ref string __result, Need __instance, Pawn ___pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(___pawn)) return;
            if (__instance.def.defName == "Rest")
            {
                __result = string.Concat(new string[]
                    {
                    __instance.LabelCap,
                    ": ",
                    __instance.CurLevelPercentage.ToStringPercent(),
                    " (",
                    __instance.CurLevel.ToString("0.##"),
                    " / ",
                    __instance.MaxLevel.ToString("0.##"),
                    ")\n",
                    "RSEnergyDesc".Translate()
                    }
                );
            }
            else if ((new string[] { "Joy", "Comfort", "Beauty", "Outdoors", "Mood" }).Contains(__instance.def.defName)) __result = string.Empty;
        }
    }

    [HarmonyPatch(typeof(Need), "DrawOnGUI")]
    public class Need_DrawOnGUI
    {
        public static bool Prefix(Need __instance, Pawn ___pawn)
        {
            if (!(new string[] { "Joy", "Comfort", "Beauty", "Outdoors", "Mood" }).Contains(__instance.def.defName) || !PeacekeeperUtility.IsPeacekeeper(___pawn)) return true;
            return false;
        }
    }

    //[HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    //public class Need_Food_NeedInterval
    //{
    //    public static bool Prefix(Need_Food __instance, Pawn ___pawn, ref int ___lastNonStarvingTick)
    //    {
    //        if (!PeacekeeperUtility.IsPeacekeeper(___pawn)) return true;
    //        // todo add class to hediff that removes the hediff when energy is available
    //        var isFrozen = (bool)Traverse.Create(__instance).Property("IsFrozen").GetValue<bool>();
    //        var malnutritionSeverity = (float)Traverse.Create(__instance).Property("MalnutritionSeverityPerInterval").GetValue<float>();

    //        if (!isFrozen)
    //        {
    //            __instance.CurLevel -= __instance.FoodFallPerTick * 150f;
    //        }
    //        if (!__instance.Starving)
    //        {
    //            ___lastNonStarvingTick = Find.TickManager.TicksGame;
    //        }
    //        if (!isFrozen)
    //        {
    //            if (__instance.Starving)
    //            {
    //                HealthUtility.AdjustSeverity(___pawn, RSDefOf.RSEnergyShortage, malnutritionSeverity * ModSettings.Get().energyShortageSeverityMult);
    //                return false;
    //            }
    //            HealthUtility.AdjustSeverity(___pawn, RSDefOf.RSEnergyShortage, -malnutritionSeverity);
    //        }

    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(Need_Rest), "NeedInterval")]
    public class Need_Rest_NeedInterval
    {
        private static float MalnutritionSeverityPerInterval(Pawn pawn)
        {
            return 0.00113333331f * Mathf.Lerp(0.8f, 1.2f, Rand.ValueSeeded(pawn.thingIDNumber ^ 2551674));
        }
        public static bool Prefix(Need_Rest __instance, Pawn ___pawn, ref int ___ticksAtZero)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(___pawn)) return true;

            // todo add class to hediff that removes the hediff when energy is available
            var isFrozen = (bool)Traverse.Create(__instance).Property("IsFrozen").GetValue<bool>();
            var malnutInt = MalnutritionSeverityPerInterval(___pawn);

            if (!isFrozen)
            {
                __instance.CurLevel -= __instance.RestFallPerTick * 150f;
            }
            if (__instance.CurLevel < 0.0001f)
            {
                ___ticksAtZero += 150;
            }
            else
            {
                ___ticksAtZero = 0;
            }
            if (___ticksAtZero > 1000)
            {
                HealthUtility.AdjustSeverity(___pawn, RSDefOf.RSEnergyShortage, malnutInt * ModSettings.energyShortageSeverityMult);
            }
            else HealthUtility.AdjustSeverity(___pawn, RSDefOf.RSEnergyShortage, -malnutInt);

            return false;
        }
    }

    [HarmonyPatch(typeof(PawnCapacityDef), "GetLabelFor", new Type[] { typeof(Pawn) })]
    public class PawnCapacityDef_GetLabelFor
    {
        public static void Postfix(ref string __result, PawnCapacityDef __instance, Pawn pawn)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            if (__instance == PawnCapacityDefOf.BloodFiltration) __result = "RSCoolantFiltration".Translate();
            if (__instance == PawnCapacityDefOf.BloodPumping) __result = "RSCoolantCirculation".Translate();
        }
    }

    //[HarmonyPatch(typeof(PawnCapacityUtility), "BodyCanEverDoCapacity")]
    //public class PawnCapacityUtility_BodyCanEverDoCapacity
    //{
    //    public static void Postfix(ref bool __result, BodyDef bodyDef, PawnCapacityDef capacity)
    //    {
    //        if (bodyDef != RSDefOf.RSPeacekeeperBody) return;
    //        if (capacity == PawnCapacityDefOf.BloodFiltration) __result = false;
    //        if (capacity == PawnCapacityDefOf.BloodPumping) __result = false;
    //    }
    //}

    [HarmonyPatch(typeof(RimWorld.HealthCardUtility), "DrawOverviewTab")]
    public class HealthCardUtility_DrawOverviewTab
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var getLabelForInfo = AccessTools.Method(typeof(PawnCapacityDef), "GetLabelFor", new Type[] { typeof(bool), typeof(bool) });
#pragma warning disable CS0252
            var idx = codes.FindIndex(code => code.operand == getLabelForInfo);
#pragma warning restore CS0252 

            if (idx == -1)
            {
                Log.Warning($"Could not find GetLabelFor code instruction; skipping changes");
                return instructions;
            }

            codes[idx].operand = AccessTools.Method(typeof(PawnCapacityDef), "GetLabelFor", new Type[] { typeof(Pawn) });
            codes.RemoveRange(idx - 7, 7);

            return codes;
        }
    }
    #endregion

    #region medical
    [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
    public class RestUtility_IsValidBedFor
    {
        // todo - may need some revision
        public static void Postfix(ref bool __result, Thing bedThing, Pawn sleeper)
        {
            if (PeacekeeperUtility.IsPeacekeeper(sleeper) && bedThing.def == RSDefOf.RSChargeStation) __result = true;
            else if (PeacekeeperUtility.IsPeacekeeper(sleeper) && bedThing.def != RSDefOf.RSChargeStation) __result = false;
            else if (bedThing.def == RSDefOf.RSChargeStation) __result = false;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_Tend), "HasJobOnThing")]
    public class WorkGiver_Tend_HasJobOnThing
    {
        public static void Postfix(ref bool __result, Pawn pawn, Thing t, bool forced = false)
        {
            if (!__result) return;
            Pawn pawn2 = t as Pawn;
            if (!PeacekeeperUtility.IsPeacekeeper(pawn2)) return;
            __result = false;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_DiseaseHuman), "PotentialVictimCandidates")]
    public class IncidentWorker_DiseaseHuman_PotentialVictimCandidates
    {
        public static void Postfix(ref IEnumerable<Pawn> __result, IIncidentTarget target)
        {
            var newList = __result.ToList();
            newList.RemoveAll(pawn => PeacekeeperUtility.IsPeacekeeper(pawn));
            __result = newList;
        }
    }

    // todo - remove patch and set graphic in new thingclass
    //[HarmonyPatch(typeof(Thing), "get_Graphic")]
    //public class Building_Bed_get_Graphic
    //{
    //    public static void Postfix(ref Graphic __result, ThingWithComps __instance)
    //    {
    //        if (__instance.def != DefsOf.RSChargeStation) return;
    //        __result = PeacekeeperUtility.CreateMaintenanceStationGraphic(__instance);
    //    }
    //}

    [HarmonyPatch(typeof(WorkGiver_RescueDowned), "HasJobOnThing")]
    public class WorkGiver_RescueDowned_HasJobOnThing
    {
        public static void Postfix(ref bool __result, Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (!PeacekeeperUtility.IsPeacekeeper(pawn2)) return;
            if (!pawn2.Downed || pawn2.Faction != Faction.OfPlayer || PeacekeeperUtility.IsInChargeStation(pawn2) || !pawn.CanReserve(pawn2) || GenAI.EnemyIsNear(pawn2, 40f))
            {
                __result = false;
                return;
            }
            var chargeStation = PeacekeeperUtility.GetEmptyChargeStation(pawn2);
            __result = chargeStation != null && pawn2.CanReserve(chargeStation, 1, -1, null, false);
        }
    }

    [HarmonyPatch(typeof(WorkGiver_RescueDowned), "JobOnThing")]
    public class WorkGiver_RescueDowned_JobOnThing
    {
        public static void Postfix(ref Job __result, Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (!PeacekeeperUtility.IsPeacekeeper(pawn2) || PeacekeeperUtility.IsInChargeStation(pawn2) || !pawn.CanReserve(t)) return;

            Thing t2 = PeacekeeperUtility.GetEmptyChargeStation(pawn2);
            if (t2 == null) return;
            Job job = JobMaker.MakeJob(RSDefOf.RSRescueToChargeStation, pawn2, t2);
            job.count = 1;
            __result = job;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    public class FloatMenuMakerMap_ChoicesAtFor
    {
        public static void Postfix(ref List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
        {
            var order = __result.FirstOrDefault(cur => cur.Priority == MenuOptionPriority.RescueOrCapture && PeacekeeperUtility.IsPeacekeeper(cur.revalidateClickTarget as Pawn));
            if (order != null)
            {
                var victim = order.revalidateClickTarget as Pawn;
                if (!pawn.CanReserve(victim)) return;
                order.action = delegate ()
                {
                    var building_Bed = PeacekeeperUtility.GetEmptyChargeStation(victim);
                    if (building_Bed == null)
                    {
                        string t5;
                        if (victim.RaceProps.Animal)
                        {
                            t5 = "NoAnimalBed".Translate();
                        }
                        else
                        {
                            t5 = "NoNonPrisonerBed".Translate();
                        }
                        Messages.Message("CannotRescue".Translate() + ": " + t5, victim, MessageTypeDefOf.RejectInput, false);
                        return;
                    }
                    Job job = JobMaker.MakeJob(RSDefOf.RSRescueToChargeStation, victim, building_Bed);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
                };
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_RescueNearby), "TryGiveJob")]
    public class JobGiver_RescueNearby_TryGiveJob
    {
        public static void Postfix(ref Job __result, Pawn pawn, float ___radius)
        {
            if (__result != null) return;
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = (Pawn)t;
                return pawn3.Downed && pawn3.Faction == pawn.Faction && !pawn3.InBed() && pawn.CanReserve(pawn3, 1, -1, null, false) && !pawn3.IsForbidden(pawn) && !GenAI.EnemyIsNear(pawn3, 25f);
            };
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), ___radius, validator, null, 0, -1, false, RegionType.Set_Passable, false);
            if (pawn2 == null) return;
            if (!PeacekeeperUtility.IsPeacekeeper(pawn2) || PeacekeeperUtility.IsInChargeStation(pawn2)) return;

            var building_Bed = PeacekeeperUtility.GetEmptyChargeStation(pawn2);
            if (building_Bed == null || !pawn2.CanReserve(building_Bed, 1, -1, null, false) || building_Bed.IsForbidden(pawn)) return;
            Job job = JobMaker.MakeJob(RSDefOf.RSRescueToChargeStation, pawn2, building_Bed);
            job.count = 1;
            __result = job;
        }
    }

    [HarmonyPatch(typeof(Alert_ColonistNeedsRescuing), "NeedsRescue")]
    public class Alert_ColonistNeedsRescuing_NeedsRescue
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (!__result || !PeacekeeperUtility.IsPeacekeeper(p)) return;
            if (PeacekeeperUtility.IsInChargeStation(p)) __result = false;
        }
    }

    // todo rare nre seems to happen here for some reason -> figure out why
    //[HarmonyPatch(typeof(Alert_NeedDoctor), "get_Patients")]
    //public class Alert_NeedDoctor_get_Patients
    //{
    //    public static void Postfix(ref bool __result, Pawn p)
    //    {
    //        Log.Message($"")

    //        if (!__result || !PeacekeeperUtility.IsPeacekeeper(p)) return;
    //        if (PeacekeeperUtility.IsInChargeStation(p)) __result = false;
    //    }
    //}

    // disabled, lets try to do this by patching needs rescue and needs doctor
    /* 
    [HarmonyPatch(typeof(RestUtility), "InBed")]
    public class RestUtility_InBed
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(p)) return;
            if (PeacekeeperUtility.IsInChargeStation(p)) __result = true;
            else __result = false;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "CurrentBed")]
    public class RestUtility_CurrentBed
    {
        private static Building_Bed cachedBed = null;
        public static void Postfix(ref Building_Bed __result, Pawn p)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(p) || !PeacekeeperUtility.IsInChargeStation(p)) return;
            cachedBed = cachedBed ?? (cachedBed = ThingMaker.MakeThing(ThingDefOf.Bed, ThingDefOf.WoodLog) as Building_Bed);
            cachedBed.Medical = true;
            __result = cachedBed;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_VisitSickPawn), "HasJobOnThing")]
    public class WorkGiver_VisitSickPawn_HasJobOnThing
    {
        public static void Postfix(ref bool __result, Pawn pawn, Thing t, bool forced)
        {
            var robot = t as Pawn;
            if (robot == null || !PeacekeeperUtility.IsPeacekeeper(robot)) return;
            __result = false;
        }
    }
    */
    #endregion

    #region thoughts / memories
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
    public class PawnDiedOrDownedThoughtsUtility_AppendThoughts_ForHumanlike
    {
        public static bool Prefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(victim)) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Relations")]
    public class PawnDiedOrDownedThoughtsUtility_AppendThoughts_Relations
    {
        public static bool Prefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(victim)) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn), "ButcherProducts")]
    public class Pawn_ButcherProducts
    {
        public static void Postfix(Pawn __instance, Pawn butcher, float efficiency)
        {
            ModSettings.butcheredPeacekeeper = PeacekeeperUtility.IsPeacekeeper(__instance);
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[] { typeof(ThoughtDef), typeof(Pawn), typeof(Precept) })]
    public class MemoryThoughtHandler_TryGainMemory
    {
        public static ThoughtDef[] badThoughts = new[] { ThoughtDefOf.ButcheredHumanlikeCorpse, ThoughtDefOf.KnowButcheredHumanlikeCorpse };
        public static bool Prefix(MemoryThoughtHandler __instance, ThoughtDef def, Pawn otherPawn = null)
        {
            if (ModSettings.butcheredPeacekeeper && badThoughts.Contains(def)) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Corpse), "GiveObservedThought")]
    public class Corpse_GiveObservedThought
    {
        public static void Postfix(ref Thought_Memory __result, Corpse __instance)
        {
            if (__result != null && PeacekeeperUtility.IsPeacekeeper(__instance.InnerPawn)) __result = null;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity), "ShouldHaveThought")]
    public class ThoughtWorker_Precept_IdeoDiversity_ShouldHaveThought
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (!ModSettings.removeIdeologyImpact) return codes;
            var idx = codes.FindIndex(code => code.operand != null && code.operand.ToString().Contains("get_IsPrisoner"));
            if (idx == -1)
            {
                Log.Warning($"Could not find get_IsPrisoner code instruction; skipping changes");
                return instructions;
            }
            codes.Insert(idx + 2, new CodeInstruction(OpCodes.Ldloc_2));
            codes.Insert(idx + 3, new CodeInstruction(OpCodes.Ldloc_3));
            codes.Insert(idx + 4, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Pawn>), "get_Item", new Type[] { typeof(Int32) })));
            codes.Insert(idx + 5, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PeacekeeperUtility), nameof(PeacekeeperUtility.IsPeacekeeper), new Type[] { typeof(Pawn) })));
            codes.Insert(idx + 6, new CodeInstruction(OpCodes.Brtrue_S, codes[idx + 1].operand));

            return codes;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity_Uniform), "ShouldHaveThought")]
    public class ThoughtWorker_Precept_IdeoDiversity_Uniform_ShouldHaveThought
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (!ModSettings.removeIdeologyImpact) return codes;
            var idx = codes.FindIndex(code => code.operand != null && code.operand.ToString().Contains("IsQuestLodger"));
            if (idx == -1)
            {
                Log.Warning($"Could not find IsQuestLodger code instruction; skipping changes");
                return instructions;
            }
            codes.Insert(idx + 2, new CodeInstruction(OpCodes.Ldloc_0));
            codes.Insert(idx + 3, new CodeInstruction(OpCodes.Ldloc_2));
            codes.Insert(idx + 4, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Pawn>), "get_Item", new Type[] { typeof(Int32) })));
            codes.Insert(idx + 5, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PeacekeeperUtility), nameof(PeacekeeperUtility.IsPeacekeeper), new Type[] { typeof(Pawn) })));
            codes.Insert(idx + 6, new CodeInstruction(OpCodes.Brtrue_S, codes[idx + 1].operand));

            return codes;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_IdeoDiversity_Social), "ShouldHaveThought")]
    public class ThoughtWorker_Precept_IdeoDiversity_Social_ShouldHaveThought
    {
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (!ModSettings.removeIdeologyImpact || __result.StageIndex == ThoughtState.Inactive.StageIndex) return;
            if (PeacekeeperUtility.IsPeacekeeper(p) || PeacekeeperUtility.IsPeacekeeper(otherPawn)) __result = false;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_AnyBodyPartCovered_Social), "ShouldHaveThought")]
    public class ThoughtWorker_Precept_AnyBodyPartCovered_Social_ShouldHaveThought
    {
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (!ModSettings.removeIdeologyImpact || __result.StageIndex == ThoughtState.Inactive.StageIndex) return;
            if (PeacekeeperUtility.IsPeacekeeper(p) || PeacekeeperUtility.IsPeacekeeper(otherPawn)) __result = false;
        }
    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_GroinUncovered_Social), "ShouldHaveThought")]
    public class ThoughtWorker_Precept_GroinUncovered_Social_ShouldHaveThought
    {
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (!ModSettings.removeIdeologyImpact || __result.StageIndex == ThoughtState.Inactive.StageIndex) return;
            if (PeacekeeperUtility.IsPeacekeeper(p) || PeacekeeperUtility.IsPeacekeeper(otherPawn)) __result = false;
        }
    }
    #endregion

    #region social
    [HarmonyPatch(typeof(InteractionUtility), "CanInitiateInteraction")]
    public class InteractionUtility_CanInitiateInteraction
    {
        public static void Postfix(ref bool __result, Pawn pawn, InteractionDef interactionDef = null)
        {
            if (!__result) return;
            if (PeacekeeperUtility.IsPeacekeeper(pawn)) __result = false;
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), "CanReceiveInteraction")]
    public class InteractionUtility_CanReceiveInteraction
    {
        public static void Postfix(ref bool __result, Pawn pawn, InteractionDef interactionDef = null)
        {
            if (!__result) return;
            if (PeacekeeperUtility.IsPeacekeeper(pawn)) __result = false;
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), "CanInitiateRandomInteraction")]
    public class InteractionUtility_CanInitiateRandomInteraction
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (!__result) return;
            if (PeacekeeperUtility.IsPeacekeeper(p)) __result = false;
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), "CanReceiveRandomInteraction")]
    public class InteractionUtility_CanReceiveRandomInteraction
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (!__result) return;
            if (PeacekeeperUtility.IsPeacekeeper(p)) __result = false;
        }
    }

    [HarmonyPatch(typeof(GatheringsUtility), "ShouldGuestKeepAttendingGathering")]
    public class GatheringsUtility_ShouldGuestKeepAttendingGathering
    {
        public static bool Prefix(ref bool __result, Pawn p)
        {
            if (!PeacekeeperUtility.IsPeacekeeper(p)) return true;
            __result = false;
            return false;
        }
    }

    #endregion

    #region butchering/food
    // disabled for now -> handled by recipes

    //[HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    //public class WorkGiver_DoBill_TryFindBestBillIngredients
    //{
    //    public static void Postfix(WorkGiver_DoBill __instance, ref bool __result, Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
    //    {
    //        if (!__result || bill.recipe.defName != "ButcherCorpseFlesh") return;
    //        if (chosen.Any(item => item.Thing is Corpse pk && CustomPatches.peacekeeperCorpses.Contains(pk.def))) __result = false;
    //    }
    //}
    #endregion

    #region caravan / travel
    [HarmonyPatch(typeof(Caravan), "get_NightResting")]
    public class Caravan_NightResting
    {
        public static void Postfix(ref bool __result, Caravan __instance)
        {
            if (!__result) return;
            var peaceKeeperCount = __instance.pawns.InnerListForReading.Where(pawn => PeacekeeperUtility.IsPeacekeeper(pawn)).Count();
            if (peaceKeeperCount == __instance.pawns.Count) __result = false;
        }
    }

    [HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
    public class JobGiver_GetRest_TryGiveJob
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result == null) return;
            if (PeacekeeperUtility.IsPeacekeeper(pawn)) __result = null;
        }
    }

    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
    public class Caravan_NeedsTracker_TrySatisfyPawnNeeds
    {
        public static void Postfix(Pawn pawn, Caravan ___caravan)
        {
            if (pawn.Dead || !PeacekeeperUtility.IsPeacekeeper(pawn)) return;
            PeacekeeperUtility.RefuelPawnOnCaravan(pawn, ___caravan);
        }
    }
    #endregion

    #region weapons / equipment
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public class EquipmentUtility_CanEquip
    {
        public static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            if (!__result) return;
            var weaponModExt = thing.def.GetModExtension<RSPeacekeeperWeaponModExt>();
            if (weaponModExt == null || weaponModExt.weightType != "heavy") return;
            var robotModExt = pawn.def.GetModExtension<RSPeacekeeperModExt>();
            if (robotModExt != null && robotModExt.gunDef == thing.def) return;
            cantReason = "RSGunTooHeavy".Translate();
            __result = false;
        }
    }
    #endregion

    #region skills
    [HarmonyPatch(typeof(Pawn_SkillTracker), "SkillsTick")]
    public class Pawn_SkillTracker_SkillsTick
    {
        public static bool Prefix(Pawn ___pawn)
        {
            if (PeacekeeperUtility.IsPeacekeeper(___pawn)) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_SkillTracker), "Learn")]
    public class Pawn_SkillTracker_Learn
    {
        public static bool Prefix(Pawn ___pawn)
        {
            if (PeacekeeperUtility.IsPeacekeeper(___pawn)) return false;
            return true;
        }
    }
    #endregion

    public class TargettedPatches
    {
        public static void ColonistBar_CheckRecacheEntries_Postfix(List<ColonistBar.Entry> ___cachedEntries)
        {
            ___cachedEntries.RemoveAll(entry => PeacekeeperUtility.IsPeacekeeper(entry.pawn));
        }

        public static void StorytellerUtilityPopulation_AdjustedPopulation_get_Postfix(ref float __result)
        {
            var robotCount = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists?.Where(colonist => PeacekeeperUtility.IsPeacekeeper(colonist))?.Count();
            if (robotCount == null) return;
            __result -= (int)robotCount;
        }
    }
}
