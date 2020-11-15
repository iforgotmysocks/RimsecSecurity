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
            PatchRobotNoFoodAndRecipes();
            PatchStorytellers();
            PatchRemoveRottingFromCorpses();
            PatchDebug();
        }

        // disabled for now -> handled by recipes
        //public static IEnumerable<ThingDef> peacekeeperCorpses = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.race.HasModExtension<RSPeacekeeperModExt>()).Select(def => def.RaceProps.corpseDef);

        public static void PatchRobotNoFoodAndRecipes()
        {
            var robots = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.race.HasModExtension<RSPeacekeeperModExt>());
            var vanResRemove = DefDatabase<RecipeDef>.GetNamedSilentFail("ButcherCorpseFlesh");
            var recipes = DefDatabase<RecipeDef>.AllDefs.Where(def => def.workerClass == typeof(Recipe_AdministerIngestible));
            var traits = DefDatabase<TraitDef>.AllDefs;

            foreach (var robot in robots)
            {
                robot.RaceProps.corpseDef.ingestible.foodType = FoodTypeFlags.None;
                vanResRemove.fixedIngredientFilter.SetAllow(robot.RaceProps.corpseDef, false);

                foreach (var recipe in recipes)
                {
                    recipe.recipeUsers.Remove(robot.race);
                }
            }
        }

        public static void PatchStorytellers()
        {
            foreach (var storyteller in DefDatabase<StorytellerDef>.AllDefs)
            {
                storyteller.comps.Add(new StorytellerCompProperties_OnOffCycle()
                {
                    incident = RSDefOf.RSSRSOrbitalTraderIncident,
                    onDays = 7,
                    offDays = 10,
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

        private static void PatchDebug()
        {
            if (!ModSettings.debugActive) return;
            RSDefOf.RSRobotAssemblyBench.designationCategory = RSDefOf.RSSRSBuildings;
            RSDefOf.RSSRSBuildings.AllResolvedDesignators.Add(new Designator_Build(RSDefOf.RSRobotAssemblyBench));

            var recipes = DefDatabase<RecipeDef>.AllDefs.Where(def => def.products.Any(product => product.thingDef.GetCompProperties<CompProperties_SpawnPeacekeeper>() != null));
            foreach (var recipe in recipes)
            {
                recipe.ingredients = new List<IngredientCount>();
                recipe.workAmount = 10;
            }
        }

    }
}
