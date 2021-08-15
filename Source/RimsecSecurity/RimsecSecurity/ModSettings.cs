using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimsecSecurity
{
    class ModSettings : Verse.ModSettings
    {
        public static int peacekeeperNumber = 1;
        public static bool butcheredPeacekeeper;
        public static float energyShortageSeverityMult = 10;
        public static bool hidePeacekeepersFromColonistBar = false;
        public static bool debugActive = false;
        public static bool countPeacekeepersTowardsPopulation = true;
        public static float fuelConsumptionRate;
        public static float daysPauseBetweenTradeShips = 15;
        public static bool allowClothing = false;
        public static bool removeIdeologyImpact = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref peacekeeperNumber, "peacekeeperNumber", 1);
            Scribe_Values.Look(ref energyShortageSeverityMult, "energyShortageSeverityMult", 0);
            Scribe_Values.Look(ref hidePeacekeepersFromColonistBar, "hidePeacekeepersFromColonistBar", false);
            Scribe_Values.Look(ref countPeacekeepersTowardsPopulation, "countPeacekeepersTowardsPopulation", true);
            Scribe_Values.Look(ref fuelConsumptionRate, "fuelConsumptionRate", 0.5f);
            Scribe_Values.Look(ref debugActive, "debugActive", false);
            Scribe_Values.Look(ref daysPauseBetweenTradeShips, "daysPauseBetweenTradeShips", 15);
            Scribe_Values.Look(ref allowClothing, "allowClothing", false);
            Scribe_Values.Look(ref removeIdeologyImpact, "removeIdeologyImpact", true);
        }
        
        public void DoWindowContents(Rect rect)
        {
            var options = new Listing_Standard();
            options.Begin(rect);
            options.Label("All changes (except maint. station fuel consumption) require a restart to take effect.");
            options.Gap(24f);
            options.CheckboxLabeled("Hide peacekeeper robots from colonist bar", ref hidePeacekeepersFromColonistBar);
            options.CheckboxLabeled("Count peacekeeper robots towards colonist population", ref countPeacekeepersTowardsPopulation);
            options.CheckboxLabeled("Allow robots to wear clothes? (clothes may not completely fit but that won't be addressed)", ref allowClothing);
            options.Gap(24f);
            options.Label($"Maintenance station: Component fuel consumption rate per day: {Math.Round(fuelConsumptionRate, 2)}");
            fuelConsumptionRate = options.Slider(fuelConsumptionRate, 0.01f, 2f);
            options.Label($"Interval of days between SRS trade ships (+2 days on which the event can happen): {Math.Round(daysPauseBetweenTradeShips, 1)}");
            daysPauseBetweenTradeShips = options.Slider(daysPauseBetweenTradeShips, 1f, 60f);
            options.CheckboxLabeled("Remove ideology diversity impact", ref ModSettings.removeIdeologyImpact);

            options.Gap(24f);
            if (options.ButtonTextLabeled("Spawn random test robot at random colonist location", "Spawn")) PeacekeeperUtility.SpawnRandomRobot(); 

            options.End();
        }

    }
}
