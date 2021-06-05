using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using AlienRace;

namespace RimsecSecurity
{
    [StaticConstructorOnStartup]
    static class CustomPatches
    {
        static CustomPatches()
        {
            PeacekeeperUtility.RunSavely(PatchRobotNoFoodAndRecipes);
            PeacekeeperUtility.RunSavely(PatchStorytellers);
            PeacekeeperUtility.RunSavely(PatchRemoveRottingFromCorpses);
            PeacekeeperUtility.RunSavely(PatchFuelConsumption);
            PeacekeeperUtility.RunSavely(PatchRobotClothing);
        }

        private static void PatchFuelConsumption()
        {
            var benchDef = RSDefOf.RSChargeStation;
            var fuelComp = benchDef.GetCompProperties<CompProperties_Refuelable>();
            if (fuelComp == null) return;
            fuelComp.fuelConsumptionRate = ModSettings.fuelConsumptionRate;
        }

        public static void PatchRobotNoFoodAndRecipes()
        {
            var robots = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def?.race?.HasModExtension<RSPeacekeeperModExt>() == true);
            var vanResRemove = DefDatabase<RecipeDef>.GetNamedSilentFail("ButcherCorpseFlesh");
            if (vanResRemove == null) Log.Error($"ButcherCorpseFlesh recipe not found and null");
            var recipes = DefDatabase<RecipeDef>.AllDefs.Where(def => def.workerClass == typeof(Recipe_AdministerIngestible));

            foreach (var robot in robots)
            {
                robot.RaceProps.corpseDef.ingestible.foodType = FoodTypeFlags.None;
                if (vanResRemove != null) vanResRemove.fixedIngredientFilter.SetAllow(robot.RaceProps.corpseDef, false);

                foreach (var recipe in recipes)
                {
                    if (recipe.recipeUsers == null) continue;
                    recipe.recipeUsers.Remove(robot.race);
                }
            }
        }

        public static void PatchRobotClothing()
        {
            if (!ModSettings.allowClothing) return;
            var robots = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def?.race?.HasModExtension<RSPeacekeeperModExt>() == true);
            foreach (var robot in robots)
            {
                var alienRace = robot.race as ThingDef_AlienRace;
                alienRace.alienRace.raceRestriction.onlyUseRaceRestrictedApparel = false;
            }
        }

        public static void PatchStorytellers()
        {
            foreach (var storyteller in DefDatabase<StorytellerDef>.AllDefs)
            {
                storyteller.comps.Add(new StorytellerCompProperties_OnOffCycle()
                {
                    incident = RSDefOf.RSSRSOrbitalTraderIncident,
                    onDays = 2,
                    offDays = ModSettings.daysPauseBetweenTradeShips,
                    numIncidentsRange = new FloatRange(1, 1)
                }); 
            }
        }

        private static void PatchRemoveRottingFromCorpses()
        {
            var robots = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.race.HasModExtension<RSPeacekeeperModExt>());

            foreach (var robot in robots)
            {
                var rottableComp = robot.RaceProps.corpseDef.GetCompProperties<CompProperties_Rottable>();
                if (rottableComp != null) robot.RaceProps.corpseDef.comps.Remove(rottableComp);
            }
        }

    }
}
